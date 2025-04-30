using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Constants;
using ProjectManager.Data;
using ProjectManager.Models.CapitalManagement;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels.CapitalManagement
{
    public class BusinessBalanceViewModel : BaseViewModel
    {
        private readonly AppDbContext _context;

        private BusinessBalanceModel _businessBalance;
        public BusinessBalanceModel BusinessBalance
        {
            get => _businessBalance;
            set { _businessBalance = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CashFlowModel> CashFlows { get; set; } = new();
        public ObservableCollection<LoanModel> Loans { get; set; } = new();
        public ObservableCollection<IdentityUser> Users { get; set; } = new();

        public IdentityUser SelectedUser { get; set; }

        private LoanModel _selectedLoan;
        public LoanModel SelectedLoan
        {
            get => _selectedLoan;
            set { _selectedLoan = value; OnPropertyChanged(); }
        }
        private double _netBalance;
        public double NetBalance
        {
            get { return _netBalance; }
            set { _netBalance = value; OnPropertyChanged(); }
        }
        private double _emergencyReserveAdjustment;
        public double EmergencyReserveAdjustment
        {
            get { return _emergencyReserveAdjustment; }
            set { _emergencyReserveAdjustment = value; OnPropertyChanged(); }
        }

        private bool _isIncomeFormVisible;
        public bool IsIncomeFormVisible
        {
            get => _isIncomeFormVisible;
            set { _isIncomeFormVisible = value; OnPropertyChanged(); }
        }

        private bool _isExpenseFormVisible;
        public bool IsExpenseFormVisible
        {
            get => _isExpenseFormVisible;
            set { _isExpenseFormVisible = value; OnPropertyChanged(); }
        }

        private bool _isLoanFormVisible;
        public bool IsLoanFormVisible
        {
            get => _isLoanFormVisible;
            set { _isLoanFormVisible = value; OnPropertyChanged(); }
        }

        public decimal NewIncomeAmount { get; set; }
        public string NewIncomeDescription { get; set; }
        public decimal NewExpenseAmount { get; set; }
        public string NewExpenseDescription { get; set; }
        public decimal NewLoanAmount { get; set; }
        public DateTime? NewLoanPaybackDate { get; set; }

        public decimal EmergencyReserveAmount { get; set; }

        public ICommand RefreshBalanceCommand { get; }
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

            LoadBusinessBalance();
            LoadUsers();

            RefreshBalanceCommand = new RelayCommand(LoadBusinessBalance);
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
        }

        private void LoadUsers()
        {
            Users = new ObservableCollection<IdentityUser>(_context.Users.ToList());
            OnPropertyChanged(nameof(Users));
        }

        private void LoadBusinessBalance()
        {
            BusinessBalance = _context.BusinessBalances
                .Include(bb => bb.CashFlows)
                .Include(bb => bb.Loans)
                .FirstOrDefault();

            if (BusinessBalance == null)
            {
                BusinessBalance = new BusinessBalanceModel
                {
                    CashOnHand = 0,
                    TotalSalesIncome = 0,
                    TotalExpenses = 0,
                    TotalOutstandingLoans = 0,
                    EmergencyReserve = 0,
                    CashFlows = new List<CashFlowModel>(),
                    Loans = new List<LoanModel>()
                };
                _context.BusinessBalances.Add(BusinessBalance);
                _context.SaveChanges();
            }

            CashFlows = new ObservableCollection<CashFlowModel>(BusinessBalance.CashFlows.OrderByDescending(x => x.TransactionDate));
            Loans = new ObservableCollection<LoanModel>(BusinessBalance.Loans.Where(x => !x.IsRepaid).OrderByDescending(x => x.LoanDate));

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

            var cashFlow = new CashFlowModel
            {
                Amount = NewIncomeAmount,
                Description = NewIncomeDescription,
                TransactionDate = DateTime.Now,
                Type = TransactionType.Income
            };

            BusinessBalance.CashFlows.Add(cashFlow);
            BusinessBalance.TotalSalesIncome += NewIncomeAmount;
            BusinessBalance.CashOnHand += NewIncomeAmount;

            _context.CashFlows.Add(cashFlow);
            SaveBusinessBalance();

            NewIncomeAmount = 0;
            NewIncomeDescription = string.Empty;
            HideAllForms();
        }

        private void SaveExpense()
        {
            if (NewExpenseAmount <= 0 || string.IsNullOrWhiteSpace(NewExpenseDescription))
                return;

            if (NewExpenseAmount >= BusinessBalance.CashOnHand)
            {
                MessageBox.Show("There is no enough available cash", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var cashFlow = new CashFlowModel
            {
                Amount = NewExpenseAmount,
                Description = NewExpenseDescription,
                TransactionDate = DateTime.Now,
                Type = TransactionType.Expense
            };

            BusinessBalance.CashFlows.Add(cashFlow);
            BusinessBalance.TotalExpenses += NewExpenseAmount;
            BusinessBalance.CashOnHand -= NewExpenseAmount;

            _context.CashFlows.Add(cashFlow);
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

        public void MarkLoanAsRepaid()
        {
            if (SelectedLoan == null || SelectedLoan.IsRepaid)
                return;

            if (SelectedLoan.AmountLoaned >= BusinessBalance.CashOnHand)
            {
                MessageBox.Show("There is no enough available cash", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedLoan.IsRepaid = true;
            SelectedLoan.OutstandingBalance = 0;
            BusinessBalance.TotalOutstandingLoans -= SelectedLoan.AmountLoaned;

            decimal repaidAmountLoaned = SelectedLoan.AmountLoaned;
            string repaidLenderName = SelectedLoan.LenderName;

            _context.Loans.Update(SelectedLoan);
            SaveBusinessBalance();

            NewExpenseAmount = repaidAmountLoaned;
            NewExpenseDescription = $"Repaid loan to {repaidLenderName}";
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
            OnPropertyChanged(nameof(EmergencyReserveAmount));
        }

        private void ReleaseEmergencyReserve()
        {
            if (EmergencyReserveAmount <= 0 || BusinessBalance.EmergencyReserve < EmergencyReserveAmount)
                return;

            BusinessBalance.EmergencyReserve -= EmergencyReserveAmount;
            BusinessBalance.CashOnHand += EmergencyReserveAmount;
            SaveBusinessBalance();
            EmergencyReserveAmount = 0;
            OnPropertyChanged(nameof(EmergencyReserveAmount));
        }

        private void HideAllForms()
        {
            IsIncomeFormVisible = false;
            IsExpenseFormVisible = false;
            IsLoanFormVisible = false;
        }

        private void ShowIncomeForm()
        {
            HideAllForms();
            IsIncomeFormVisible = true;
        }

        private void ShowExpenseForm()
        {
            HideAllForms();
            IsExpenseFormVisible = true;
        }

        private void ShowLoanForm()
        {
            HideAllForms();
            IsLoanFormVisible = true;
        }
    }
}
