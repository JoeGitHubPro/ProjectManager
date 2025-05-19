using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Constants;
using ProjectManager.Data;
using ProjectManager.Models.CapitalManagement;
using ProjectManager.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels.CapitalManagement
{
    public class BusinessBalanceViewModel : BaseViewModel
    {
        private static readonly Lazy<BusinessBalanceViewModel> _instance =
        new Lazy<BusinessBalanceViewModel>(() => new BusinessBalanceViewModel());

        public static BusinessBalanceViewModel Instance => _instance.Value;

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

        private BusinessBalanceViewModel()
        {
            _context = new AppDbContext();

            RefreshCommand = new AsyncRelayCommand(LoadBusinessBalanceAsync);
            ShowIncomeFormCommand = new RelayCommand(ShowIncomeForm);
            ShowExpenseFormCommand = new RelayCommand(ShowExpenseForm);
            ShowLoanFormCommand = new RelayCommand(ShowLoanForm);
            CancelFormCommand = new RelayCommand(HideAllForms);
            SaveIncomeCommand = new AsyncRelayCommand(SaveIncomeAsync);
            SaveExpenseCommand = new AsyncRelayCommand(SaveExpenseAsync);
            SaveLoanCommand = new AsyncRelayCommand(SaveLoanAsync);
            MarkAsRepaidCommand = new AsyncRelayCommand(MarkLoanAsRepaidAsync);
            AddReserveCommand = new AsyncRelayCommand(AddEmergencyReserveAsync);
            ReleaseReserveCommand = new AsyncRelayCommand(ReleaseEmergencyReserveAsync);

            _ = LoadInitialDataAsync(); // Fire-and-forget async initialization
        }

        public async Task LoadInitialDataAsync()
        {
            await LoadBusinessBalanceAsync();
            await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Users = new ObservableCollection<IdentityUser>(users);
                    OnPropertyChanged(nameof(Users));
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error loading users", ex.Message);
            }
        }

        private async Task LoadBusinessBalanceAsync()
        {
            try
            {
                var businessBalance = await _context.BusinessBalances
                    .Include(bb => bb.CashFlows)
                    .Include(bb => bb.Loans)
                    .FirstOrDefaultAsync();

                if (businessBalance == null)
                {
                    businessBalance = new BusinessBalanceModel();
                    await _context.BusinessBalances.AddAsync(businessBalance);
                    await _context.SaveChangesAsync();
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    BusinessBalance = businessBalance;

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
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error loading balance", ex.Message);
            }
        }

        private async Task SaveBusinessBalanceAsync()
        {
            try
            {
                _context.BusinessBalances.Update(BusinessBalance);
                await _context.SaveChangesAsync();
                await LoadBusinessBalanceAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error saving balance", ex.Message);
            }
        }
        public async Task SaveIncomeAsync(decimal amount, string description)
        {
            try
            {
                if (amount <= 0 || string.IsNullOrWhiteSpace(description))
                {
                    await ShowWarningAsync("Validation Error", "Amount and description are required");
                    return;
                }

                var income = new CashFlowModel
                {
                    Amount = amount,
                    Description = description,
                    TransactionDate = DateTime.Now,
                    Type = TransactionType.Income
                };

                BusinessBalance.CashFlows.Add(income);
                BusinessBalance.TotalSalesIncome += amount;
                BusinessBalance.CashOnHand += amount;

                await _context.CashFlows.AddAsync(income);
                await SaveBusinessBalanceAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    NewIncomeAmount = 0;
                    NewIncomeDescription = string.Empty;
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error saving income", ex.Message);
            }
        }
        public async Task SaveIncomeAsync()
        {
            try
            {
                if (NewIncomeAmount <= 0 || string.IsNullOrWhiteSpace(NewIncomeDescription))
                {
                    await ShowWarningAsync("Validation Error", "Amount and description are required");
                    return;
                }

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

                await _context.CashFlows.AddAsync(income);
                await SaveBusinessBalanceAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    NewIncomeAmount = 0;
                    NewIncomeDescription = string.Empty;
                    HideAllForms();
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error saving income", ex.Message);
            }
        }

        private async Task SaveExpenseAsync()
        {
            try
            {
                if (NewExpenseAmount <= 0 || string.IsNullOrWhiteSpace(NewExpenseDescription))
                {
                    await ShowWarningAsync("Validation Error", "Amount and description are required");
                    return;
                }

                if (NewExpenseAmount > BusinessBalance.CashOnHand)
                {
                    await ShowWarningAsync("Insufficient Funds", "Not enough available cash.");
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

                await _context.CashFlows.AddAsync(expense);
                await SaveBusinessBalanceAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    NewExpenseAmount = 0;
                    NewExpenseDescription = string.Empty;
                    HideAllForms();
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error saving expense", ex.Message);
            }
        }

        private async Task SaveLoanAsync()
        {
            try
            {
                if (NewLoanAmount <= 0 || SelectedUser == null || NewLoanPaybackDate == null)
                {
                    await ShowWarningAsync("Validation Error", "Please fill all loan details");
                    return;
                }

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

                await _context.Loans.AddAsync(loan);
                await SaveBusinessBalanceAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    NewLoanAmount = 0;
                    NewLoanPaybackDate = null;
                    SelectedUser = null;
                    HideAllForms();
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error saving loan", ex.Message);
            }
        }

        private async Task MarkLoanAsRepaidAsync()
        {
            try
            {
                if (SelectedLoan == null || SelectedLoan.IsRepaid)
                    return;

                if (SelectedLoan.AmountLoaned > BusinessBalance.CashOnHand)
                {
                    await ShowWarningAsync("Insufficient Funds", "Not enough available cash.");
                    return;
                }

                SelectedLoan.IsRepaid = true;
                SelectedLoan.OutstandingBalance = 0;
                BusinessBalance.TotalOutstandingLoans -= SelectedLoan.AmountLoaned;

                var repaidLoan = SelectedLoan;

                _context.Loans.Update(SelectedLoan);
                await SaveBusinessBalanceAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    NewExpenseAmount = repaidLoan.AmountLoaned;
                    NewExpenseDescription = $"Repaid loan to {repaidLoan.LenderName}";
                });

                await SaveExpenseAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error marking loan as repaid", ex.Message);
            }
        }

        private async Task AddEmergencyReserveAsync()
        {
            try
            {
                if (EmergencyReserveAmount <= 0)
                {
                    await ShowWarningAsync("Validation Error", "Amount must be positive");
                    return;
                }

                if (BusinessBalance.CashOnHand < EmergencyReserveAmount)
                {
                    await ShowWarningAsync("Insufficient Funds", "Not enough available cash.");
                    return;
                }

                BusinessBalance.EmergencyReserve += EmergencyReserveAmount;
                BusinessBalance.CashOnHand -= EmergencyReserveAmount;

                await SaveBusinessBalanceAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    EmergencyReserveAmount = 0;
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error adding reserve", ex.Message);
            }
        }

        private async Task ReleaseEmergencyReserveAsync()
        {
            try
            {
                if (EmergencyReserveAmount <= 0)
                {
                    await ShowWarningAsync("Validation Error", "Amount must be positive");
                    return;
                }

                if (BusinessBalance.EmergencyReserve < EmergencyReserveAmount)
                {
                    await ShowWarningAsync("Insufficient Reserve", "Not enough reserve funds.");
                    return;
                }

                BusinessBalance.EmergencyReserve -= EmergencyReserveAmount;
                BusinessBalance.CashOnHand += EmergencyReserveAmount;

                await SaveBusinessBalanceAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    EmergencyReserveAmount = 0;
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error releasing reserve", ex.Message);
            }
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

        #region Dialog Helpers
        private async Task ShowErrorAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new ErrorDialog(title, message);
            });
        }

        private async Task ShowWarningAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new WarningDialog($"{title}\t{message}");
            });
        }

        private async Task ShowInfoAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new InformationDialog($"{title}\t{message}");
            });
        }
        #endregion
    }
}