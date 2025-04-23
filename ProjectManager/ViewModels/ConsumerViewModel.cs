using Microsoft.EntityFrameworkCore;
using ProjectManager.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace ProjectManager.ViewModels
{
    public class ConsumerViewModel : BaseViewModel, IDataErrorInfo
    {
        private AppDbContext _context;
        private bool _isBusy;
        private string _searchText = string.Empty;

        public ObservableCollection<Consumer> Consumers { get; set; } = new();
        public ObservableCollection<Consumer> FilteredConsumers { get; set; } = new();

        public Dictionary<string, string> ErrorCollection =>
            new Dictionary<string, string>
            {
                [nameof(Phone)] = this[nameof(Phone)],
                [nameof(CommercialRegisterNo)] = this[nameof(CommercialRegisterNo)]
            };

        public ConsumerViewModel()
        {
            _context = new AppDbContext();

            // Initialize commands first
            SaveCommand = new RelayCommand(
            _ => SaveConsumer(),
            _ => !IsBusy && !HasErrors);

            DeleteCommand = new RelayCommand(
                _ => DeleteConsumer(),
                _ => !IsBusy && CanDelete);

            RefreshCommand = new RelayCommand(
                _ => LoadData(),
                _ => !IsBusy);

            SearchCommand = new RelayCommand(
                _ => FilterConsumers(),
                _ => !IsBusy);

            AddNewCommand = new RelayCommand(
                _ => AddNewConsumer(),
                _ => !IsBusy);

            EditCommand = new RelayCommand(
                consumer => SelectedConsumer = consumer as Consumer,
                _ => !IsBusy);

            ClearCommand = new RelayCommand(
                _ => ClearFields(),
                _ => !IsBusy);

            LoadData();


        }
        bool firstTimeFlag = true;
        #region Properties

        private Consumer _selectedConsumer;
        public Consumer SelectedConsumer
        {
            get => _selectedConsumer;
            set
            {
                _selectedConsumer = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanDelete));
                if (value != null)
                {
                    Phone = value.Phone;
                    CommercialRegisterNo = value.CommercialRegisterNo;
                }
            }
        }

        private string _phone = string.Empty;
        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                firstTimeFlag = false;
                OnPropertyChanged();
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private string _commercialRegisterNo = string.Empty;
        public string CommercialRegisterNo
        {
            get => _commercialRegisterNo;
            set
            {
                _commercialRegisterNo = value;
                firstTimeFlag = false;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterConsumers();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();

                // Check if commands are initialized before raising can execute changed
                SaveCommand?.RaiseCanExecuteChanged();
                DeleteCommand?.RaiseCanExecuteChanged();
                RefreshCommand?.RaiseCanExecuteChanged();
                SearchCommand?.RaiseCanExecuteChanged();
            }
        }

        public bool CanDelete => SelectedConsumer != null;

        #endregion

        #region Commands

        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand SearchCommand { get; private set; }
        public RelayCommand AddNewCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand ClearCommand { get; private set; }

        #endregion

        #region Methods

        private void LoadData()
        {
            try
            {
                IsBusy = true;
                _context = new AppDbContext();

                var consumers = _context.Consumers.ToList();
                Consumers = new ObservableCollection<Consumer>(consumers);
                FilteredConsumers = new ObservableCollection<Consumer>(consumers);

                OnPropertyChanged(nameof(Consumers));
                OnPropertyChanged(nameof(FilteredConsumers));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void FilterConsumers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredConsumers = new ObservableCollection<Consumer>(Consumers);
            }
            else
            {
                var searchLower = SearchText.ToLower();
                FilteredConsumers = new ObservableCollection<Consumer>(
                    Consumers.Where(c =>
                        c.Phone.ToLower().Contains(searchLower) ||
                        (c.CommercialRegisterNo != null &&
                         c.CommercialRegisterNo.ToLower().Contains(searchLower)))
                );
            }
            OnPropertyChanged(nameof(FilteredConsumers));
        }

        private void AddNewConsumer()
        {
            SelectedConsumer = null;
            Phone = string.Empty;
            CommercialRegisterNo = string.Empty;
        }

        private void SaveConsumer()
        {
            try
            {
                IsBusy = true;

                if (HasErrors)
                {
                    MessageBox.Show("Please fix validation errors before saving.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedConsumer == null)
                {
                    // Add new consumer
                    var existing = _context.Consumers.Find(Phone);
                    if (existing != null)
                    {
                        MessageBox.Show("A consumer with this phone number already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var consumer = new Consumer
                    {
                        Phone = Phone,
                        CommercialRegisterNo = CommercialRegisterNo
                    };

                    _context.Consumers.Add(consumer);
                    Consumers.Add(consumer);
                }
                else
                {
                    // Update existing consumer
                    SelectedConsumer.CommercialRegisterNo = CommercialRegisterNo;
                    _context.Entry(SelectedConsumer).State = EntityState.Modified;
                }

                _context.SaveChanges();
                FilterConsumers();
                MessageBox.Show("Consumer saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving consumer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void DeleteConsumer()
        {
            try
            {
                if (SelectedConsumer == null) return;

                var result = MessageBox.Show(
                    "Are you sure you want to delete this consumer? This action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;

                IsBusy = true;

                _context.Consumers.Remove(SelectedConsumer);
                _context.SaveChanges();
                Consumers.Remove(SelectedConsumer);
                FilterConsumers();
                ClearFields();

                MessageBox.Show("Consumer deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting consumer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ClearFields()
        {
            SelectedConsumer = null;
            Phone = string.Empty;
            CommercialRegisterNo = string.Empty;
        }

        #endregion

        #region Validation

        public string Error => string.Empty;

        private string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Phone):
                        if (string.IsNullOrWhiteSpace(Phone) && !firstTimeFlag)
                            return "Phone number is required";
                        if (Phone.Length > 20)
                            return "Phone number cannot exceed 20 characters";
                        break;


                    case nameof(CommercialRegisterNo):
                        if (!string.IsNullOrWhiteSpace(CommercialRegisterNo) && CommercialRegisterNo.Length > 50)
                            return "Commercial register number cannot exceed 50 characters";
                        break;
                }

                return string.Empty;
            }
        }


        public bool HasErrors
        {
            get
            {
                if (firstTimeFlag)
                    return true;
                return !string.IsNullOrEmpty(this[nameof(Phone)]) ||
                       !string.IsNullOrEmpty(this[nameof(CommercialRegisterNo)]);
            }
        }

        #endregion
    }
}