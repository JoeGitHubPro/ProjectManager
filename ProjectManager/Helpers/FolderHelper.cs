using ProjectManager.Config;
using ProjectManager.Constants;
using ProjectManager.Views.Dialogs;
using System.Diagnostics;
using System.IO;

namespace ProjectManager.Helpers
{
    public static class FolderHelper
    {
        public static string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dir");

        public static async Task<string> CreateUniqueFolder()
        {
            var config = await ConfigHelper.LoadConfigAsync<AppConfig>("jsconfig.json");

            // Ensure the base folder (e.g., \Dir) exists in the application root
            Directory.CreateDirectory(baseDir);

            // Build unique folder name
            string guid = Guid.NewGuid().ToString("N");
            string folderName = $"{UserSession.Username}-{guid}";
            string fullPath = Path.Combine(baseDir, folderName);

            // Create the main folder
            Directory.CreateDirectory(fullPath);

            // Define subfolders to create
            List<string> subfolders = config.Subfolders ?? new List<string> { "Contract", "Set-Up", "Source-Code", "Drivers" };

            foreach (var subfolder in subfolders)
            {
                string subfolderPath = Path.Combine(fullPath, subfolder);
                Directory.CreateDirectory(subfolderPath);
            }

            return folderName;
        }

        public static void OpenFolderByName(string FolderName)
        {
            string targetPath = Path.Combine(baseDir, FolderName);

            if (Directory.Exists(targetPath))
            {
                Process.Start("explorer.exe", targetPath);
            }
            else
            {
                new ErrorDialog("Error", "Folder not found!");
            }
        }

        public static void DeleteFolder(string FolderName)
        {
            try
            {
                string targetPath = Path.Combine(baseDir, FolderName);

                // Check if the directory exists
                if (Directory.Exists(targetPath)) // Use targetPath instead
                {
                    // Delete the directory and all its contents
                    Directory.Delete(targetPath, true);
                    new InformationDialog($"Folder '{targetPath}' deleted successfully.");
                }
                else
                {
                    new WarningDialog($"Folder '{targetPath}' does not exist.");
                }
            }
            catch (Exception ex)
            {
                new ErrorDialog($"An error occurred: ", $"{ex.Message}");
            }
        }
    }
}