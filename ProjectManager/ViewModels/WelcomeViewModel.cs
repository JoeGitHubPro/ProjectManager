using Microsoft.EntityFrameworkCore;
using ProjectManager.Constants;
using ProjectManager.Data;
using ProjectManager.Views.Dialogs;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels
{
    public class WelcomeViewModel : BaseViewModel
    {
        #region Private Fields
        private string _welcomeMessage;
        private int _projectCount;
        private int _clientCount;
        private int _teamCount;
        private readonly AppDbContext _context = new();
        #endregion

        #region Public Properties
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        public string Username => $"Hi {UserSession.Username} 👋";

        public int ProjectCount
        {
            get => _projectCount;
            set => SetProperty(ref _projectCount, value);
        }

        public int ClientCount
        {
            get => _clientCount;
            set => SetProperty(ref _clientCount, value);
        }

        public int TeamCount
        {
            get => _teamCount;
            set => SetProperty(ref _teamCount, value);
        }
        #endregion

        #region Commands
        public ICommand NavigateToProjectsCommand { get; }
        public ICommand ShowTutorialCommand { get; }
        public ICommand RefreshDataCommand { get; }
        #endregion

        public WelcomeViewModel()
        {
            NavigateToProjectsCommand = new RelayCommand(ExecuteNavigateToProjects);
            ShowTutorialCommand = new RelayCommand(ExecuteShowTutorial);
            RefreshDataCommand = new AsyncRelayCommand(LoadInitialDataAsync);

            _ = LoadInitialDataAsync(); // Fire-and-forget async initialization
        }

        private void ExecuteShowTutorial()
        {
            // Implementation would go here
            // Consider making this async if it involves I/O operations
            new InformationDialog("Tutorial This feature is coming soon!");
        }

        private void ExecuteNavigateToProjects()
        {
            // Implementation would go here
            // This would typically trigger navigation which should be handled by your navigation service
        }

        public async Task LoadInitialDataAsync()
        {
            try
            {
                // Set welcome message on UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    WelcomeMessage = $"Welcome back, {UserSession.Username}! Here's your project management overview for today.";
                });

                // Initialize default values
                int projects = 0;
                int clients = 0;
                int users = 0;

                // Load data with individual error handling
                try
                {
                    projects = await GetProjectCountAsync();
                }
                catch (Exception ex)
                {
                    new ErrorDialog("Error loading projects: ", $"{ex.Message}");
                }

                try
                {
                    clients = await GetClientCountAsync();
                }
                catch (Exception ex)
                {
                    new ErrorDialog("Error loading clients:", $"{ex.Message}");
                }

                try
                {
                    users = await GetTeamCountAsync();
                }
                catch (Exception ex)
                {
                    new ErrorDialog("Error loading users: ", $"{ex.Message}");
                }

                // Update UI with results
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ProjectCount = projects;
                    ClientCount = clients;
                    TeamCount = users;
                });
            }
            catch (Exception ex)
            {
                new ErrorDialog("Critical error in LoadInitialDataAsync: ", $"{ex}");
                await HandleLoadErrorAsync(ex);
            }
        }

        private async Task HandleLoadErrorAsync(Exception ex)
        {
            // Log the error if you have logging infrastructure
            // Logger.LogError(ex, "Error loading welcome page data");

            await ShowErrorAsync("Loading Error", "Could not load dashboard data");

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                WelcomeMessage = "Welcome to your project management dashboard.";
                ProjectCount = 0;
                ClientCount = 0;
                TeamCount = 0;
            });
        }

        private async Task<int> GetProjectCountAsync()
        {
            return await _context.Projects.CountAsync();
        }

        private async Task<int> GetClientCountAsync()
        {
            return await _context.Consumers.CountAsync();
        }

        private async Task<int> GetTeamCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        private async Task ShowErrorAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                new ErrorDialog(title, message);
            });
        }
    }
}