using ProjectManager.Constants;
using ProjectManager.Data;
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
        private AppDbContext _context = new();


        #endregion

        #region Public Properties

        /// <summary>
        /// Dynamic welcome message that can be personalized
        /// </summary>
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set
            {
                if (_welcomeMessage != value)
                {
                    _welcomeMessage = value;
                    OnPropertyChanged();
                }
            }
        }
        public string? Username
        {
            get
            {
                return $"Hi {UserSession.Username} 👋";
            }

        }

        /// <summary>
        /// Number of active projects
        /// </summary>
        public int ProjectCount
        {
            get => _projectCount;
            set
            {
                if (_projectCount != value)
                {
                    _projectCount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of clients in the system
        /// </summary>
        public int ClientCount
        {
            get => _clientCount;
            set
            {
                if (_clientCount != value)
                {
                    _clientCount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of team members in the system
        /// </summary>
        public int TeamCount
        {
            get => _teamCount;
            set
            {
                if (_teamCount != value)
                {
                    _teamCount = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        /// <summary>
        /// Command to navigate to the Projects view
        /// </summary>
        public ICommand NavigateToProjectsCommand { get; private set; }

        /// <summary>
        /// Command to show the app tutorial/demo
        /// </summary>
        public ICommand ShowTutorialCommand { get; private set; }



        /// <summary>
        /// Default constructor
        /// </summary>
        public WelcomeViewModel()
        {
            // Initialize commands
            NavigateToProjectsCommand = new RelayCommand(ExecuteNavigateToProjects);
            ShowTutorialCommand = new RelayCommand(ExecuteShowTutorial);

            // Load data
            LoadInitialData();
        }

        private void ExecuteShowTutorial()
        {
            throw new NotImplementedException();
        }

        private void ExecuteNavigateToProjects()
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Loads initial data for the view
        /// </summary>
        private void LoadInitialData()
        {
            try
            {
                // Get the user's name for personalized welcome message
                var currentUserName = UserSession.Username;
                WelcomeMessage = $"Welcome back, {currentUserName}! Here's your project management overview for today.";

                // Load statistics
                ProjectCount = _context.Projects.Count();
                ClientCount = _context.Consumers.Count();
                TeamCount = _context.Users.Count();
            }
            catch (Exception ex)
            {
                // Handle exceptions - log and set default values
                // Logger.LogError(ex, "Error loading welcome page data");
                WelcomeMessage = "Welcome to your project management dashboard.";
                ProjectCount = 0;
                ClientCount = 0;
                TeamCount = 0;
            }
        }
    }
}


