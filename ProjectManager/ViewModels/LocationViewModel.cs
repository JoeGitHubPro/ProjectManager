using Microsoft.EntityFrameworkCore;
using ProjectManager.Data;
using ProjectManager.Models;
using ProjectManager.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels
{
    public class LocationViewModel : BaseViewModel, IDisposable
    {
        private AppDbContext _context;
        private string _searchText = string.Empty;
        private Location _selectedLocation;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private bool _isNewRecord = true;
        private Dictionary<string, string> _errorCollection = new();
        private bool _isDisposed;
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

        public LocationViewModel()
        {
            InitializeCommands();
            InitializeContext();
            _ = LoadDataAsync();
        }

        private void InitializeContext()
        {
            _context = new AppDbContext();
        }

        public ObservableCollection<Location> Locations { get; private set; } = new();
        public ObservableCollection<Location> FilteredLocations { get; private set; } = new();

        public string Name
        {
            get => _name;
            set
            {
                _name = value ?? string.Empty;
                OnPropertyChanged();
                ValidateProperty(nameof(Name));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public Location SelectedLocation
        {
            get => _selectedLocation;
            set
            {
                _selectedLocation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanEditOrDelete));

                if (value != null)
                {
                    _isNewRecord = false;
                    Name = value.Name;
                    Description = value.Description ?? string.Empty;
                }
                else
                {
                    ClearFields();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value ?? string.Empty;
                OnPropertyChanged();
                FilterLocations();
            }
        }

        private bool _isFormVisible;
        public bool IsFormVisible
        {
            get => _isFormVisible;
            set
            {
                _isFormVisible = value;
                OnPropertyChanged();
            }
        }

        public bool CanEditOrDelete => SelectedLocation != null;

        public ICommand SaveCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand AddNewCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }

        private void InitializeCommands()
        {
            SaveCommand = new AsyncRelayCommand(SaveLocationAsync, () => !HasErrors);
            DeleteCommand = new AsyncRelayCommand(DeleteLocationAsync, () => CanEditOrDelete);
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            SearchCommand = new RelayCommand(FilterLocations);
            AddNewCommand = new RelayCommand(AddNewLocation);
            EditCommand = new RelayCommand(() => IsFormVisible = true);
            ClearCommand = new RelayCommand(ClearFields);
        }

        private async Task LoadDataAsync()
        {
            if (_isDisposed) return;

            await _asyncLock.WaitAsync();
            try
            {
                ClearFields();

                // Ensure context is initialized
                if (_context == null)
                {
                    InitializeContext();
                }

                var data = await _context.Locations.ToListAsync();

                await ExecuteOnUiThreadAsync(() =>
                {
                    Locations = new ObservableCollection<Location>(data);
                    FilteredLocations = new ObservableCollection<Location>(data);
                    OnPropertyChanged(nameof(Locations));
                    OnPropertyChanged(nameof(FilteredLocations));
                });
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Error loading data: {ex.Message}");
            }
            finally
            {
                _asyncLock.Release();
            }
        }

        private async Task SaveLocationAsync()
        {
            if (_isDisposed) return;

            if (HasErrors || string.IsNullOrWhiteSpace(Name))
            {
                await ShowWarningAsync("Please fix validation errors before saving.");
                return;
            }

            await _asyncLock.WaitAsync();
            try
            {
                // Ensure context is initialized
                if (_context == null)
                {
                    InitializeContext();
                }

                Location savedLocation = null;

                if (_isNewRecord)
                {
                    // Check for existing location with same name
                    var existing = await _context.Locations
                        .AsNoTracking()
                        .FirstOrDefaultAsync(l => l.Name == Name);

                    if (existing != null)
                        throw new InvalidOperationException("A location with this name already exists.");

                    // Create and add new location
                    var newLocation = new Location { Name = Name, Description = Description };
                    _context.Locations.Add(newLocation); // Using synchronous version for better tracking
                    await _context.SaveChangesAsync();

                    // Store the saved location with generated ID
                    savedLocation = newLocation;
                }
                else if (SelectedLocation != null)
                {
                    // Ensure the entity is tracked before modifying
                    var locationToUpdate = await _context.Locations
                        .FirstOrDefaultAsync(l => l.Id == SelectedLocation.Id);

                    if (locationToUpdate != null)
                    {
                        locationToUpdate.Name = Name;
                        locationToUpdate.Description = Description;
                        await _context.SaveChangesAsync();
                        savedLocation = locationToUpdate;
                    }
                    else
                    {
                        throw new InvalidOperationException("The selected location no longer exists in the database.");
                    }
                }

                await ExecuteOnUiThreadAsync(() =>
                {
                    // Update the collections without reloading everything
                    if (_isNewRecord && savedLocation != null)
                    {
                        // Add the new location to collections
                        Locations.Add(savedLocation);

                        // Update filtered collection based on current search
                        if (string.IsNullOrWhiteSpace(SearchText) ||
                            savedLocation.Name.ToLower().Contains(SearchText.ToLower()))
                        {
                            FilteredLocations.Add(savedLocation);
                        }

                        // Select the newly added location
                        _selectedLocation = savedLocation; // Directly set the field to avoid recursion
                        OnPropertyChanged(nameof(SelectedLocation));
                        OnPropertyChanged(nameof(CanEditOrDelete));
                    }
                    else if (!_isNewRecord && savedLocation != null)
                    {
                        // Update the item in the collections
                        int locIndex = Locations.IndexOf(Locations.FirstOrDefault(l => l.Id == savedLocation.Id));
                        if (locIndex >= 0)
                        {
                            Locations[locIndex] = savedLocation;
                        }

                        int filteredIndex = FilteredLocations.IndexOf(FilteredLocations.FirstOrDefault(l => l.Id == savedLocation.Id));
                        if (filteredIndex >= 0)
                        {
                            FilteredLocations[filteredIndex] = savedLocation;
                        }
                    }

                    IsFormVisible = false;
                    _isNewRecord = false;
                });

                await ShowInfoAsync("Location saved successfully.");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Error saving location: {ex.Message}");
            }
            finally
            {
                _asyncLock.Release();
                await LoadDataAsync();
            }
        }

        private async Task DeleteLocationAsync()
        {
            if (_isDisposed || SelectedLocation == null) return;

            await _asyncLock.WaitAsync();
            try
            {
                bool confirmed = await ShowConfirmationDialogAsync("Are you sure you want to delete this location?");
                if (!confirmed) return;

                // Ensure context is initialized
                if (_context == null)
                {
                    InitializeContext();
                }

                // Get a fresh reference to avoid tracking issues
                var locationToDelete = await _context.Locations
                    .FirstOrDefaultAsync(l => l.Id == SelectedLocation.Id)
                    .ConfigureAwait(false);

                if (locationToDelete == null)
                {
                    throw new InvalidOperationException("The selected location no longer exists in the database.");
                }

                _context.Locations.Remove(locationToDelete);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                await ExecuteOnUiThreadAsync(() =>
                {
                    Locations.Remove(SelectedLocation);
                    FilteredLocations.Remove(SelectedLocation);
                    ClearFields();
                });

                await ShowInfoAsync("Location deleted successfully.");
            }
            catch (DbUpdateException dbEx)
            {
                await ShowErrorAsync($"Cannot delete this location because it's being used by other records. {dbEx.Message}");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Error deleting location: {ex.Message}");
            }
            finally
            {
                _asyncLock.Release();
            }
        }

        private void FilterLocations()
        {
            if (_isDisposed) return;

            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    FilteredLocations = new ObservableCollection<Location>(Locations);
                }
                else
                {
                    var search = SearchText.ToLower();
                    FilteredLocations = new ObservableCollection<Location>(
                        Locations.Where(l => l.Name?.ToLower().Contains(search) == true)
                    );
                }

                OnPropertyChanged(nameof(FilteredLocations));
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Error filtering locations: {ex.Message}");
            }
        }

        private void AddNewLocation()
        {
            // Reset fields without causing recursion
            Name = string.Empty;
            Description = string.Empty;
            _isNewRecord = true;
            IsFormVisible = true;

            // Manually clear the selected location without causing recursion
            _selectedLocation = null;
            OnPropertyChanged(nameof(SelectedLocation));
            OnPropertyChanged(nameof(CanEditOrDelete));

            // Clear validation errors
            _errorCollection.Clear();
            OnPropertyChanged(nameof(ErrorCollection));
            OnPropertyChanged(nameof(HasErrors));
        }

        private void ClearFields()
        {
            // Don't set SelectedLocation to null here, as it would cause recursion
            _isNewRecord = true;
            IsFormVisible = false;

            // Only set these if they're not already empty to avoid notification loops
            if (!string.IsNullOrEmpty(Name)) Name = string.Empty;
            if (!string.IsNullOrEmpty(Description)) Description = string.Empty;

            _errorCollection.Clear();
            OnPropertyChanged(nameof(ErrorCollection));
            OnPropertyChanged(nameof(HasErrors));
        }

        public string this[string propertyName]
        {
            get
            {
                string error = string.Empty;

                if (propertyName == nameof(Name))
                {
                    if (string.IsNullOrWhiteSpace(Name))
                        error = "Name is required.";
                    else if (Name.Length > 100)
                        error = "Name cannot exceed 100 characters.";
                }

                return error;
            }
        }

        public Dictionary<string, string> ErrorCollection => _errorCollection;

        private void ValidateProperty(string propertyName)
        {
            string error = this[propertyName] ?? string.Empty;

            if (_errorCollection.ContainsKey(propertyName))
                _errorCollection[propertyName] = error;
            else
                _errorCollection.Add(propertyName, error);

            OnPropertyChanged(nameof(ErrorCollection));
            OnPropertyChanged(nameof(HasErrors));
        }

        public bool HasErrors => _errorCollection.Values.Any(e => !string.IsNullOrEmpty(e));

        private async Task ShowErrorAsync(string message)
        {
            await ExecuteOnUiThreadAsync(() => new ErrorDialog("Error", message));
        }

        private async Task<bool> ShowConfirmationDialogAsync(string message)
        {
            bool? result = await ExecuteOnUiThreadAsync(() =>
            {
                var dialog = new ConfirmationDialog(message);
                return dialog.ShowDialog();
            });

            return result == true;
        }

        private async Task ShowWarningAsync(string message)
        {
            await ExecuteOnUiThreadAsync(() => new WarningDialog(message));
        }

        private async Task ShowInfoAsync(string message)
        {
            await ExecuteOnUiThreadAsync(() => new InformationDialog(message));
        }

        private async Task<T> ExecuteOnUiThreadAsync<T>(Func<T> action)
        {
            if (Application.Current == null || Application.Current.Dispatcher == null)
                return default;

            return await Application.Current.Dispatcher.InvokeAsync(action);
        }

        private async Task ExecuteOnUiThreadAsync(Action action)
        {
            if (Application.Current == null || Application.Current.Dispatcher == null)
                return;

            await Application.Current.Dispatcher.InvokeAsync(action);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _context?.Dispose();
                _asyncLock?.Dispose();
            }

            _context = null;
            _isDisposed = true;
        }

        ~LocationViewModel()
        {
            Dispose(false);
        }
    }
}