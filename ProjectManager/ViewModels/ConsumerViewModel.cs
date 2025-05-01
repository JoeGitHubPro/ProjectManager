using ProjectManager.Data;
using ProjectManager.Models;
using ProjectManager.Views.Dialogs;
using System.Collections.ObjectModel;

namespace ProjectManager.ViewModels
{
    public class ConsumerViewModel : BaseViewModel
    {
        private AppDbContext _context;
        private string _searchText = string.Empty;
        private Consumer _selectedConsumer;
        private string _phone = string.Empty;
        private string _commercialRegisterNo = string.Empty;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private string _whatsapp = string.Empty;
        private string _facebook = string.Empty;
        private string _tikTok = string.Empty;
        private string _x = string.Empty;
        private string _instagram = string.Empty;
        private string _site = string.Empty;
        private string _notes = string.Empty;
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
                    IsFormVisible = true;

                    Phone = value.Phone;
                    CommercialRegisterNo = value.CommercialRegisterNo;
                    Name = value.Name;
                    Description = value.Description;
                    Whatsapp = value.Whatsapp;
                    Facebook = value.Facebook;
                    TikTok = value.TikTok;
                    X = value.X;
                    Instagram = value.Instagram;
                    Site = value.Site;
                    Notes = value.Notes;
                }
            }
        }

        private bool _isFormVisible = false;
        public bool IsFormVisible
        {
            get => _isFormVisible;
            set
            {
                _isFormVisible = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => SetField(ref _description, value);
        }

        public string Whatsapp
        {
            get => string.IsNullOrEmpty(_whatsapp) ? Phone : _whatsapp;
            set => SetField(ref _whatsapp, value);
        }

        public string Facebook
        {
            get => _facebook;
            set => SetField(ref _facebook, value);
        }

        public string TikTok
        {
            get => _tikTok;
            set => SetField(ref _tikTok, value);
        }

        public string X
        {
            get => _x;
            set => SetField(ref _x, value);
        }

        public string Instagram
        {
            get => _instagram;
            set => SetField(ref _instagram, value);
        }

        public string Site
        {
            get => _site;
            set => SetField(ref _site, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetField(ref _notes, value);
        }

        public string Phone
        {
            get => _phone;
            set => ValidateAndNotify(ref _phone, value, nameof(Phone));
        }

        public string CommercialRegisterNo
        {
            get => _commercialRegisterNo;
            set => ValidateAndNotify(ref _commercialRegisterNo, value, nameof(CommercialRegisterNo));
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
            SaveCommand = new RelayCommand(SaveConsumer, () => !HasErrors);
            DeleteCommand = new RelayCommand(DeleteConsumer);
            RefreshCommand = new RelayCommand(LoadData);
            SearchCommand = new RelayCommand(FilterConsumers);
            AddNewCommand = new RelayCommand(AddNewConsumer);
            EditCommand = new RelayCommand(SaveConsumer);
            ClearCommand = new RelayCommand(ClearFields);
        }

        #endregion

        #region Methods

        private void LoadData()
        {
            ClearFields();

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
                var search = SearchText.ToLower();
                FilteredConsumers = new ObservableCollection<Consumer>(
                    Consumers.Where(c =>
                        (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.Phone) && c.Phone.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.Whatsapp) && c.Whatsapp.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.Facebook) && c.Facebook.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.TikTok) && c.TikTok.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.X) && c.X.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.Instagram) && c.Instagram.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.Site) && c.Site.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.Notes) && c.Notes.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.CommercialRegisterNo) && c.CommercialRegisterNo.ToLower().Contains(search))
                    ));
            }

            OnPropertyChanged(nameof(FilteredConsumers));
        }

        private void AddNewConsumer()
        {
            ClearFields();
            _isNewRecord = true;
            IsFormVisible = true;
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
                if (_context == null) _context = new AppDbContext();

                if (_isNewRecord)
                    SaveNewConsumer();
                else
                    UpdateExistingConsumer();

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
            if (_context.Consumers.Any(c => c.Phone == Phone))
                throw new InvalidOperationException("A consumer with this phone number already exists.");

            var consumer = new Consumer
            {
                Phone = Phone,
                CommercialRegisterNo = CommercialRegisterNo,
                Name = Name,
                Description = Description,
                Whatsapp = Whatsapp,
                Facebook = Facebook,
                TikTok = TikTok,
                X = X,
                Instagram = Instagram,
                Site = Site,
                Notes = Notes
            };

            _context.Consumers.Add(consumer);
            _context.SaveChanges();

            Consumers.Add(consumer);
            FilteredConsumers.Add(consumer);
        }

        private void UpdateExistingConsumer()
        {
            if (SelectedConsumer == null || _context == null) return;

            SelectedConsumer.Phone = Phone;
            SelectedConsumer.CommercialRegisterNo = CommercialRegisterNo;
            SelectedConsumer.Name = Name;
            SelectedConsumer.Description = Description;
            SelectedConsumer.Whatsapp = Whatsapp;
            SelectedConsumer.Facebook = Facebook;
            SelectedConsumer.TikTok = TikTok;
            SelectedConsumer.X = X;
            SelectedConsumer.Instagram = Instagram;
            SelectedConsumer.Site = Site;
            SelectedConsumer.Notes = Notes;

            _context.Consumers.Update(SelectedConsumer);
            _context.SaveChanges();
        }

        private void DeleteConsumer()
        {
            if (SelectedConsumer == null || _context == null) return;

            var result = new ConfirmationDialog("Are you sure you want to delete this consumer? This action cannot be undone.");
            result.ShowDialog();

            if (!result.IsConfirmed) return;

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
            Phone = CommercialRegisterNo = Name = Description = Whatsapp =
                Facebook = TikTok = X = Instagram = Site = Notes = string.Empty;

            _isNewRecord = true;
            IsFormVisible = false;
        }

        #endregion

        #region Validation

        public string this[string propertyName]
        {
            get
            {
                return propertyName switch
                {
                    nameof(Phone) => string.IsNullOrWhiteSpace(Phone)
                        ? "Phone number is required"
                        : Phone.Length > 20
                            ? "Phone number cannot exceed 20 characters"
                            : string.Empty,

                    nameof(CommercialRegisterNo) => !string.IsNullOrWhiteSpace(CommercialRegisterNo) && CommercialRegisterNo.Length > 50
                        ? "Commercial register number cannot exceed 50 characters"
                        : string.Empty,

                    _ => string.Empty
                };
            }
        }

        private readonly Dictionary<string, string> _errorCollection = new();
        public Dictionary<string, string> ErrorCollection => _errorCollection;

        private void ValidateProperty(string propertyName)
        {
            string error = this[propertyName];
            _errorCollection[propertyName] = error;
            OnPropertyChanged(nameof(ErrorCollection));
            OnPropertyChanged(nameof(HasErrors));
        }

        private void ValidateAndNotify(ref string backingField, string value, string propertyName)
        {
            backingField = value;
            OnPropertyChanged(propertyName);
            ValidateProperty(propertyName);
        }

        public bool HasErrors => _errorCollection.Values.Any(error => !string.IsNullOrWhiteSpace(error));

        #endregion

        #region Helper Methods

        private void SetField(ref string field, string value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            field = value;
            OnPropertyChanged(propertyName);
        }

        private void ShowError(string message) =>
        new ErrorDialog("Error", message);

        private void ShowWarning(string message) =>
        new WarningDialog(message);

        private void ShowInfo(string message) =>
        new InformationDialog(message);

        #endregion
    }
}
