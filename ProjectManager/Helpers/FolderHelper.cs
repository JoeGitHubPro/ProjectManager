using ProjectManager.Config;
using System.Diagnostics;
using System.IO;

namespace ProjectManager.Helpers
{
    public static class FolderHelper
    {
        public static string CreateUniqueFolder()
        {
            var config = ConfigHelper.LoadConfig<AppConfig>("jsconfig.json");

            // Ensure the base folder (e.g., \Dir) exists in the application root
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.MainFolder is not null ? config.MainFolder : "Dir");
            Directory.CreateDirectory(baseDir);

            // Build unique folder name
            string guid = Guid.NewGuid().ToString("N");

            string folderName = guid;
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
        public static void OpenFolderByName(string FolderName)
        {
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dir");
            string targetPath = Path.Combine(baseDir, FolderName);

            if (Directory.Exists(targetPath))
            {
                Process.Start("explorer.exe", targetPath);
            }
            else
            {
                System.Windows.MessageBox.Show("Folder not found!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}

