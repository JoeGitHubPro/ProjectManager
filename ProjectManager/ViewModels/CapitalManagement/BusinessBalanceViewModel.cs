using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Constants;
using ProjectManager.Data;
using ProjectManager.Models.CapitalManagement;
using ProjectManager.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProjectManager.ViewModels.CapitalManagement
{
    public class BusinessBalanceViewModel : BaseViewModel
    {
        private readonly AppDbContext _context;
        private LoanModel? selectedLoan;

        public BusinessBalanceModel BusinessBalance { get; set; } = new();
        public ObservableCollection<CashFlowModel> CashFlows { get; set; } = new();
        public ObservableCollection<LoanModel> Loans { get; set; } = new();
        public ObservableCollection<IdentityUser> Users { get; set; } = new();

        public IdentityUser? SelectedUser { get; set; }
        public LoanModel? SelectedLoan
        {
            get => selectedLoan;
            set => selectedLoan = value;
        }

        public decimal NewIncomeAmount { get; set; }
        public string NewIncomeDescription { get; set; } = string.Empty;

        public decimal NewExpenseAmount { get; set; }
        public string NewExpenseDescription { get; set; } = string.Empty;

        public decimal NewLoanAmount { get; set; }
        public DateTime? NewLoanPaybackDate { get; set; }

        public decimal EmergencyReserveAmount { get; set; }

        public bool IsIncomeFormVisible { get; set; }
        public bool IsExpenseFormVisible { get; set; }
        public bool IsLoanFormVisible { get; set; }

        public ICommand RefreshCommand { get; }
        public ICommand ShowIncomeFormCommand { get; }
        public ICommand ShowExpenseFormCommand { get; }
        public ICommand ShowLoanFormCommand { get; }
        public ICommand CancelFormCommand { get; }
        public ICommand SaveIncomeCommand { get; }
        public ICommand SaveExpenseCommand { get; }
        public ICommand SaveLoanCommand { get; }
        public ICommand MarkAsRepaidCommand { get; }
        public ICommand AddReserveCommand { get; }
        public ICommand ReleaseReserveCommand { get; }

        public BusinessBalanceViewModel()
        {
            _context = new AppDbContext();

            RefreshCommand = new RelayCommand(LoadBusinessBalance);
            ShowIncomeFormCommand = new RelayCommand(ShowIncomeForm);
            ShowExpenseFormCommand = new RelayCommand(ShowExpenseForm);
            ShowLoanFormCommand = new RelayCommand(ShowLoanForm);
            CancelFormCommand = new RelayCommand(HideAllForms);
            SaveIncomeCommand = new RelayCommand(SaveIncome);
            SaveExpenseCommand = new RelayCommand(SaveExpense);
            SaveLoanCommand = new RelayCommand(SaveLoan);
            MarkAsRepaidCommand = new RelayCommand(MarkLoanAsRepaid);
            AddReserveCommand = new RelayCommand(AddEmergencyReserve);
            ReleaseReserveCommand = new RelayCommand(ReleaseEmergencyReserve);

            LoadInitialData();
        }

        private void LoadInitialData()
        {
            LoadBusinessBalance();
            Users = new ObservableCollection<IdentityUser>(_context.Users.ToList());
            OnPropertyChanged(nameof(Users));
        }

        private void LoadBusinessBalance()
        {
            BusinessBalance = _context.BusinessBalances
                .Include(bb => bb.CashFlows)
                .Include(bb => bb.Loans)
                .FirstOrDefault() ?? new BusinessBalanceModel();

            if (BusinessBalance.Id == 0)
            {
                _context.BusinessBalances.Add(BusinessBalance);
                _context.SaveChanges();
            }

            CashFlows = new ObservableCollection<CashFlowModel>(
                (BusinessBalance.CashFlows ?? new List<CashFlowModel>())
                .OrderByDescending(x => x.TransactionDate));

            Loans = new ObservableCollection<LoanModel>(
                (BusinessBalance.Loans ?? new List<LoanModel>())
                .Where(x => !x.IsRepaid)
                .OrderByDescending(x => x.LoanDate));

            OnPropertyChanged(nameof(BusinessBalance));
            OnPropertyChanged(nameof(CashFlows));
            OnPropertyChanged(nameof(Loans));
        }


        private void SaveBusinessBalance()
        {
            _context.BusinessBalances.Update(BusinessBalance);
            _context.SaveChanges();
            LoadBusinessBalance();
        }

        public void SaveIncome()
        {
            if (NewIncomeAmount <= 0 || string.IsNullOrWhiteSpace(NewIncomeDescription))
                return;

            var income = new CashFlowModel
            {
                Amount = NewIncomeAmount,
                Description = NewIncomeDescription,
                TransactionDate = DateTime.Now,
                Type = TransactionType.Income
            };

            BusinessBalance.CashFlows.Add(income);
            BusinessBalance.TotalSalesIncome += NewIncomeAmount;
            BusinessBalance.CashOnHand += NewIncomeAmount;

            _context.CashFlows.Add(income);
            SaveBusinessBalance();

            NewIncomeAmount = 0;
            NewIncomeDescription = string.Empty;
            HideAllForms();
        }

        private void SaveExpense()
        {
            if (NewExpenseAmount <= 0 || string.IsNullOrWhiteSpace(NewExpenseDescription))
                return;

            if (NewExpenseAmount > BusinessBalance.CashOnHand)
            {
                new WarningDialog("Not enough available cash.");
                return;
            }

            var expense = new CashFlowModel
            {
                Amount = NewExpenseAmount,
                Description = NewExpenseDescription,
                TransactionDate = DateTime.Now,
                Type = TransactionType.Expense
            };

            BusinessBalance.CashFlows.Add(expense);
            BusinessBalance.TotalExpenses += NewExpenseAmount;
            BusinessBalance.CashOnHand -= NewExpenseAmount;

            _context.CashFlows.Add(expense);
            SaveBusinessBalance();

            NewExpenseAmount = 0;
            NewExpenseDescription = string.Empty;
            HideAllForms();
        }

        private void SaveLoan()
        {
            if (NewLoanAmount <= 0 || SelectedUser == null || NewLoanPaybackDate == null)
                return;

            var loan = new LoanModel
            {
                AmountLoaned = NewLoanAmount,
                OutstandingBalance = NewLoanAmount,
                LoanDate = DateTime.Now,
                LenderId = SelectedUser.Id,
                LenderName = SelectedUser.UserName,
                DueDate = NewLoanPaybackDate.Value,
                IsRepaid = false
            };

            BusinessBalance.Loans.Add(loan);
            BusinessBalance.TotalOutstandingLoans += NewLoanAmount;
            BusinessBalance.CashOnHand += NewLoanAmount;

            _context.Loans.Add(loan);
            SaveBusinessBalance();

            NewLoanAmount = 0;
            NewLoanPaybackDate = null;
            SelectedUser = null;
            HideAllForms();
        }

        private void MarkLoanAsRepaid()
        {
            if (SelectedLoan == null || SelectedLoan.IsRepaid)
                return;

            if (SelectedLoan.AmountLoaned > BusinessBalance.CashOnHand)
            {
                new WarningDialog("Not enough available cash.");
                return;
            }

            SelectedLoan.IsRepaid = true;
            SelectedLoan.OutstandingBalance = 0;
            BusinessBalance.TotalOutstandingLoans -= SelectedLoan.AmountLoaned;

            LoanModel Repaidloan = SelectedLoan;

            _context.Loans.Update(SelectedLoan);
            SaveBusinessBalance();

            NewExpenseAmount = Repaidloan.AmountLoaned;
            NewExpenseDescription = $"Repaid loan to {Repaidloan.LenderName}";
            SaveExpense();
        }

        private void AddEmergencyReserve()
        {
            if (EmergencyReserveAmount <= 0 || BusinessBalance.CashOnHand < EmergencyReserveAmount)
                return;

            BusinessBalance.EmergencyReserve += EmergencyReserveAmount;
            BusinessBalance.CashOnHand -= EmergencyReserveAmount;

            SaveBusinessBalance();
            EmergencyReserveAmount = 0;
        }

        private void ReleaseEmergencyReserve()
        {
            if (EmergencyReserveAmount <= 0 || BusinessBalance.EmergencyReserve < EmergencyReserveAmount)
                return;

            BusinessBalance.EmergencyReserve -= EmergencyReserveAmount;
            BusinessBalance.CashOnHand += EmergencyReserveAmount;

            SaveBusinessBalance();
            EmergencyReserveAmount = 0;
        }

        private void HideAllForms()
        {
            IsIncomeFormVisible = IsExpenseFormVisible = IsLoanFormVisible = false;
            OnPropertyChanged(nameof(IsIncomeFormVisible));
            OnPropertyChanged(nameof(IsExpenseFormVisible));
            OnPropertyChanged(nameof(IsLoanFormVisible));
        }

        private void ShowIncomeForm()
        {
            HideAllForms();
            IsIncomeFormVisible = true;
            OnPropertyChanged(nameof(IsIncomeFormVisible));
        }

        private void ShowExpenseForm()
        {
            HideAllForms();
            IsExpenseFormVisible = true;
            OnPropertyChanged(nameof(IsExpenseFormVisible));
        }

        private void ShowLoanForm()
        {
            HideAllForms();
            IsLoanFormVisible = true;
            OnPropertyChanged(nameof(IsLoanFormVisible));
        }
    }
}
