using System.Diagnostics;
using System.IO.Compression;

using TimeClockApp.Models;
using TimeClockApp.Services;

namespace TimeClockApp.Utilities
{
    public static class ExportUtilties
    {
        private static readonly FileHelperService fhs = new();
        public static async Task<bool> CompressAndExportFolder(string folderToZipPath)
        {
            // Get a temporary cache directory
            string exportZipTempDirectory = Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, "Export");

            // Delete folder incase anything from previous exports, it will be recreated later anyway
            try
            {
                if (Directory.Exists(exportZipTempDirectory))
                    Directory.Delete(exportZipTempDirectory, true);
            }
            catch (Exception ex)
            {
                // Log things and move on, don't want to fail just because of a left over lock or something
                System.Diagnostics.Debug.WriteLine(ex);
            }

            // Get a timestamped filename
            string exportZipFilename = $"MyAppData_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.zip";
            Directory.CreateDirectory(exportZipTempDirectory);

            string exportZipFilePath = Path.Combine(exportZipTempDirectory, exportZipFilename);
            if (File.Exists(exportZipFilePath))
            {
                File.Delete(exportZipFilePath);
            }

            // Zip everything up
            ZipFile.CreateFromDirectory(folderToZipPath, exportZipFilePath, CompressionLevel.Fastest, false);

            // Copy zip file to public accessable download folder, outside of app sandbox
            string filePublic = Path.Combine(fhs.GetDownloadPath(), exportZipFilename);
            if (File.Exists(filePublic))
            {
                File.Delete(filePublic);
            }
            byte[] bytes = await File.ReadAllBytesAsync(exportZipFilePath);
            await File.WriteAllBytesAsync(filePublic, bytes);

            // Give the user the option to share this using whatever medium they like
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "TimeClock App Export Data",
                File = new ShareFile(filePublic),
            });

            return true;
        }

        public static ImportDataModel UnzipArchive(string fileToUNZipPath, string unZipTempDirectory)
        {
            ImportDataModel dataModel = new();
            try
            {
                // UnZip everything 
                ZipFile.ExtractToDirectory(fileToUNZipPath, unZipTempDirectory, true);

                string timecardFile = Path.Combine(unZipTempDirectory, "TimeCard.csv");
                dataModel.bTimeCard = File.Exists(timecardFile);
                dataModel.FileTimeCard = timecardFile;
                string wagesFile = Path.Combine(unZipTempDirectory, "Wages.csv");
                dataModel.bWages = File.Exists(wagesFile);
                dataModel.FileWages = wagesFile;
                string employeeFile = Path.Combine(unZipTempDirectory, "Employee.csv");
                dataModel.bEmployee = File.Exists(employeeFile);
                dataModel.FileEmployee = employeeFile;
                string projectFile = Path.Combine(unZipTempDirectory, "Project.csv");
                dataModel.bProject = File.Exists(projectFile);
                dataModel.FileProject = projectFile;
                string phaseFile = Path.Combine(unZipTempDirectory, "Phase.csv");
                dataModel.bPhase = File.Exists(phaseFile);
                dataModel.FilePhase = phaseFile;
                string configFile = Path.Combine(unZipTempDirectory, "Config.csv");
                dataModel.bConfig = File.Exists(configFile);
                dataModel.FileConfig = configFile;
                string expenseFile = Path.Combine(unZipTempDirectory, "Expense.csv");
                dataModel.bExpense = File.Exists(expenseFile);
                dataModel.FileExpense = expenseFile;
            }
            catch (Exception ex)
            {
                string ExportLog = "\nEXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException;
                Debug.WriteLine(ExportLog);
                SqliteDataStore dataService = new();
                dataService.ShowPopupError(ExportLog, "ABORTING DUE TO ERROR");
            }
            return dataModel;
        }
    }
}
