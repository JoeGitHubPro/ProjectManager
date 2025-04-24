using Microsoft.EntityFrameworkCore;
using ProjectManager.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace ProjectManager.ViewModels
{
    public class ConsumerViewModel : BaseViewModel
    {
        private AppDbContext _context;
        private string _searchText = string.Empty;
        private Consumer _selectedConsumer;
        private string _phone = string.Empty;
        private string _commercialRegisterNo = string.Empty;
        private bool _isNewRecord = true;

        public ConsumerViewModel()
        {
            InitializeCommands();
            LoadData();
        }

        #region Properties

        public ObservableCollection<Consumer> Consumers { get; private set; } = new();
        public ObservableCollection<Consumer> FilteredConsumers { get; private set; } = new();

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
                    _isNewRecord = false;
                    Phone = value.Phone;
                    CommercialRegisterNo = value.CommercialRegisterNo;
                }
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged();
                ValidateProperty(nameof(Phone));
            }
        }

        public string CommercialRegisterNo
        {
            get => _commercialRegisterNo;
            set
            {
                _commercialRegisterNo = value;
                OnPropertyChanged();
                ValidateProperty(nameof(CommercialRegisterNo));
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

        private void InitializeCommands()
        {
            SaveCommand = new RelayCommand(
                 SaveConsumer,
                () => !HasErrors);

            DeleteCommand = new RelayCommand(
              DeleteConsumer);

            RefreshCommand = new RelayCommand(
                LoadData);

            SearchCommand = new RelayCommand(
               FilterConsumers);

            AddNewCommand = new RelayCommand(
                AddNewConsumer);

            EditCommand = new RelayCommand(
                SaveConsumer);

            ClearCommand = new RelayCommand(
                ClearFields);
        }

        #endregion

        #region Methods

        private void LoadData()
        {
            try
            {
                _context = new AppDbContext();

                var consumers = _context.Consumers.ToList();
                Consumers = new ObservableCollection<Consumer>(consumers);
                FilteredConsumers = new ObservableCollection<Consumer>(consumers);

                OnPropertyChanged(nameof(Consumers));
                OnPropertyChanged(nameof(FilteredConsumers));
            }
            catch (Exception ex)
            {
                ShowError($"Error loading data: {ex.Message}");
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
            ClearFields();
            _isNewRecord = true;
        }

        private void SaveConsumer()
        {
            if (HasErrors)
            {
                ShowWarning("Please fix validation errors before saving.");
                return;
            }

            try
            {

                if (_isNewRecord)
                {
                    SaveNewConsumer();
                }
                else
                {
                    UpdateExistingConsumer();
                }

                _context.SaveChanges();
                FilterConsumers();
                ShowInfo("Consumer saved successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error saving consumer: {ex.Message}");
            }
        }

        private void SaveNewConsumer()
        {
            var existing = _context.Consumers.Find(Phone);
            if (existing != null)
            {
                throw new InvalidOperationException("A consumer with this phone number already exists.");
            }

            var consumer = new Consumer
            {
                Phone = Phone,
                CommercialRegisterNo = CommercialRegisterNo
            };

            _context.Consumers.Add(consumer);
            Consumers.Add(consumer);
        }

        private void UpdateExistingConsumer()
        {
            if (SelectedConsumer == null) return;

            SelectedConsumer.CommercialRegisterNo = CommercialRegisterNo;
            _context.Entry(SelectedConsumer).State = EntityState.Modified;
        }

        private void DeleteConsumer()
        {
            if (SelectedConsumer == null) return;

            var result = MessageBox.Show(
                "Are you sure you want to delete this consumer? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                _context.Consumers.Remove(SelectedConsumer);
                _context.SaveChanges();
                Consumers.Remove(SelectedConsumer);
                FilteredConsumers.Remove(SelectedConsumer);
                ClearFields();

                ShowInfo("Consumer deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting consumer: {ex.Message}");
            }

        }

        private void ClearFields()
        {
            SelectedConsumer = null;
            Phone = string.Empty;
            CommercialRegisterNo = string.Empty;
            _isNewRecord = true;
        }

        #endregion

        #region Validation

        public string this[string propertyName]
        {
            get
            {
                string error = string.Empty;

                switch (propertyName)
                {
                    case nameof(Phone):
                        if (string.IsNullOrWhiteSpace(Phone))
                            error = "Phone number is required";
                        else if (Phone.Length > 20)
                            error = "Phone number cannot exceed 20 characters";
                        break;

                    case nameof(CommercialRegisterNo):
                        if (!string.IsNullOrWhiteSpace(CommercialRegisterNo) && CommercialRegisterNo.Length > 50)
                            error = "Commercial register number cannot exceed 50 characters";
                        break;
                }

                return error;
            }
        }

        private void ValidateProperty(string propertyName)
        {
            string error = this[propertyName];
            ErrorCollection[propertyName] = error;
            OnPropertyChanged(nameof(ErrorCollection));
            OnPropertyChanged(nameof(HasErrors));
        }
        public Dictionary<string, string> ErrorCollection =>
           new Dictionary<string, string>
           {
               [nameof(Phone)] = this[nameof(Phone)],
               [nameof(CommercialRegisterNo)] = this[nameof(CommercialRegisterNo)]
           };

        public bool HasErrors => ErrorCollection.Values.Any(error => !string.IsNullOrEmpty(error));

        #endregion

        #region Helper Methods

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowWarning(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}