using ProjectManager.Config;
using ProjectManager.Helpers;
using ProjectManager.Models;
using ProjectManager.Screens;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ProjectManager.ViewModels
{
    public class AddProjectViewModel : BaseViewModel
    {
        private string _projectDetails;
        private decimal? _projectPrice;
        private string _folderSerial;
        private string _ownerName;
        private string _ownerCommercialRegister;
        private string _ownerPhone;
        private string _ownerSystemDetails;
        private string _notes;

        public string ProjectDetails
        {
            get => _projectDetails;
            set { _projectDetails = value; OnPropertyChanged(); }
        }

        public decimal? ProjectPrice
        {
            get => _projectPrice;
            set { _projectPrice = value; OnPropertyChanged(); }
        }

        public string FolderSerial
        {
            get => _folderSerial;
            set { _folderSerial = value; OnPropertyChanged(); }
        }

        public string OwnerName
        {
            get => _ownerName;
            set { _ownerName = value; OnPropertyChanged(); }
        }

        public string OwnerCommercialRegister
        {
            get => _ownerCommercialRegister;
            set { _ownerCommercialRegister = value; OnPropertyChanged(); }
        }

        public string OwnerPhone
        {
            get => _ownerPhone;
            set { _ownerPhone = value; OnPropertyChanged(); }
        }

        public string OwnerSystemDetails
        {
            get => _ownerSystemDetails;
            set { _ownerSystemDetails = value; OnPropertyChanged(); }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        private readonly AppDbContext _context;

        public AddProjectViewModel()
        {
            _context = new AppDbContext();

            SaveCommand = new RelayCommand(SaveProject);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void SaveProject()
        {
            string folderName = CreateUniqueFolder();

            var newProject = new ProjectInfo
            {
                ProjectDetails = ProjectDetails,
                ProjectPrice = ProjectPrice,
                FolderSerial = folderName,
                OwnerName = OwnerName,
                OwnerCommercialRegister = OwnerCommercialRegister,
                OwnerPhone = OwnerPhone,
                OwnerSystemDetails = OwnerSystemDetails,
                Notes = Notes,
                CreatedDate = DateTime.Now
            };

            _context.ProjectInfos.Add(newProject);
            _context.SaveChanges();

            // Show success message
            MessageBox.Show("Project saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Cancel();
        }
        private void Cancel()
        {
            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w is AddProjectScreen)?
                .Close();
        }
        public static string CreateUniqueFolder()
        {
            var config = ConfigHelper.LoadConfig<AppConfig>("jsconfig.json");

            // Ensure the base folder (e.g., \Dir) exists in the application root
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.MainFolder is not null ? config.MainFolder : "Dir");
            Directory.CreateDirectory(baseDir);

            // Build unique folder name
            string datePart = DateTime.Now.ToString("dd-MM-yyyy");
            string guid = Guid.NewGuid().ToString("N");

            string folderName = $"{datePart}-{guid}";
            string fullPath = Path.Combine(baseDir, folderName);

            // Create the main folder
            Directory.CreateDirectory(fullPath);

            // Define subfolders to create
            List<string> subfolders = config.Subfolders is not null ? config.Subfolders : ["Contract", "Set-Up", "Source-Code", "Drivers"];

            foreach (var subfolder in subfolders)
            {
                string subfolderPath = Path.Combine(fullPath, subfolder);
                Directory.CreateDirectory(subfolderPath);
            }

            return folderName;
        }
    }

}
