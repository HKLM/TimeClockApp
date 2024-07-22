using System.Diagnostics.CodeAnalysis;
using CsvHelper;
using TimeClockApp.Shared;

namespace TimeClockApp.ViewModels
{
    public partial class ExportPageViewModel : TimeStampViewModel
    {
        private readonly ExportDataService dataService = new();
        private readonly FileService fhs = new();
        internal string CSVFilePath { get; set; }

        [ObservableProperty]
        private string exportLog = string.Empty;

        //TODO FIX when adding to data fails due to existing data at same rowId.
        /// <summary>
        /// During import of Data should the TimeCards and Expenses data be overwritten or only added to (with possible duplicates)
        /// </summary>
        [ObservableProperty]
        private bool overwriteData = true;

        [ObservableProperty]
        private bool warningBoxVisible = false;

        private enum SQLTables
        {
            TimeCard = 0,
            Employee = 1,
            Project = 2,
            Phase = 3,
            Config = 4,
            Expense = 5,
            ExpenseType = 6,
            Version = 7
        }

        public ExportPageViewModel()
        {
            dataService = new();
            fhs = new();
        }

        public void OnAppearing()
        {
            ExportLog = string.Empty;
            Loading = false;
        }

        [RelayCommand]
        private async Task ImportData()
        {
            WarningBoxVisible = false;
            Loading = true;
            HasError = false;
            try
            {
                ExportLog = "Preparing for Import ... \n";

                FilePickerFileType customFileType = new(new Dictionary<DevicePlatform, IEnumerable<string>>
                            {
                                { DevicePlatform.Android, new[] { "application/zip" } },
                            });

                FileResult result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "ZIP file to import",
                    FileTypes = customFileType,

                });
                string fileCopyName = String.Empty;
                if (result != null)
                {
                    if (File.Exists(result.FullPath))
                        fileCopyName = result.FullPath;
                }
                //fall back to default if no user selection
                if (fileCopyName == String.Empty)
                    fileCopyName = Path.Combine(fhs.GetDownloadPath(), "TimeCardApp.zip");

                //  Create a location for our data
                string UnZipPath = fhs.GetLocalFilePath("TimeClockAppUNZIP");

                if (Directory.Exists(UnZipPath))
                {
                    Directory.Delete(UnZipPath, true);
                }

                Directory.CreateDirectory(UnZipPath);
                ExportLog += "Start Unzip file...\n";
                ImportDataModel dataModel = new();

                //TODO - add some sort of version check to ensure the files we are importing are even compatible with the current version

                dataModel = Utilities.ExportUtilities.UnzipArchive(fileCopyName, UnZipPath);
                if (dataModel.IsAnyTrue())
                {
                    ExportLog += "Done!...\n";
                    ExportLog += dataService.ImportData(dataModel, ExportLog, OverwriteData);
                    int icd = dataModel.DeleteCachedFile();
                    ExportLog += "\nCleaned up " + icd + " temp files\n";
                }
                else
                {
                    ExportLog += "No valid files found for import!\nABORTING\n";
                    dataModel.DeleteCachedFile();
                    Loading = false;
                    return;
                }

                ExportLog += "Done!\n";
            }
            catch (Exception ex)
            {
                HasError = true;
                Trace.WriteLine("\nEXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException);
                //await dataService.ShowPopupErrorAsync("EXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            finally
            {
                Loading = false;
            }
        }

        [RelayCommand]
        private async Task ExportData()
        {
            Loading = true;
            HasError = false;
            try
            {
                ExportLog = "Preparing for export ... \n";

                //  Create a location for our data
                CSVFilePath = Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, "TimeClockApp", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
                try
                {
                    if (Directory.Exists(CSVFilePath))
                    {
                        Directory.Delete(CSVFilePath, true);
                    }
                }
                catch (Exception ex)
                {
                    // Log things and move on, don't want to fail just because of a left over lock or something
                    Debug.WriteLine(ex);
                    ExportLog += ex.Message + "\n";
                }

                Directory.CreateDirectory(CSVFilePath);
                ExportLog += "Start Database export\n";
                Task<string> taskA = Task.Run(() => MakeCSVFilesAsync());

                //  Zip everything and present a share dialog to the user
                ExportLog += "Zipping ... \n";
                await taskA.ContinueWith(async antecedent => await Utilities.ExportUtilities.CompressAndExportFolder((await antecedent))).Unwrap();

                ExportLog += "Done!\n";
            }
            catch (Exception ex)
            {
                HasError = true;
                Trace.WriteLine(ex);
                //await dataService.ShowPopupErrorAsync("EXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            finally
            {
                Loading = false;
            }
        }

        private async Task<string> MakeCSVFilesAsync()
        {
            List<Task> tableTaskList = [];
            for (int i = 0; i < Enum.GetNames(typeof(SQLTables)).Length; i++)
            {
                SQLTables sQL = (SQLTables)i;
                ExportLog += "Exporting " + sQL.ToString() + " table\n";
                tableTaskList.Add(CreateFileAsync(sQL, CSVFilePath, sQL.ToString() + ".csv"));
            }
            await Task.WhenAll(tableTaskList);
            ExportLog += "Table export complete\n";

            return CSVFilePath;
        }

        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        private async Task CreateFileAsync(SQLTables table, string filePath, string fileName)
        {
            string file = Path.Combine(filePath, fileName);
            switch (table)
            {
                case SQLTables.TimeCard:
                    List<TimeCard> w = await dataService.BackupGetTimeCard();
                    await using (StreamWriter writer = new(file))
                    await using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<TimeCardMap>();
                        await csv.WriteRecordsAsync(w);
                        await csv.FlushAsync();
                    }
                    return;
                case SQLTables.Employee:
                    List<Employee> emp = await dataService.BackupGetEmployee();
                    await using (StreamWriter writer = new(file))
                    await using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<EmployeeMap>();
                        await csv.WriteRecordsAsync(emp);
                        await csv.FlushAsync();
                    }
                    return;
                case SQLTables.Project:
                    List<Project> pr = await dataService.BackupGetProject();
                    await using (StreamWriter writer = new(file))
                    await using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<ProjectMap>();
                        await csv.WriteRecordsAsync(pr);
                        await csv.FlushAsync();
                    }
                    return;
                case SQLTables.Phase:
                    List<Phase> ph = await dataService.BackupGetPhase();
                    await using (StreamWriter writer = new(file))
                    await using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<PhaseMap>();
                        await csv.WriteRecordsAsync(ph);
                        await csv.FlushAsync();
                    }
                    return;
                case SQLTables.Config:
                    List<Config> c = await dataService.BackupGetConfig();
                    await using (StreamWriter writer = new(file))
                    await using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<ConfigMap>();
                        await csv.WriteRecordsAsync(c);
                        await csv.FlushAsync();
                    }
                    return;
                case SQLTables.Expense:
                    List<Expense> e = await dataService.BackupGetExpense();
                    await using (StreamWriter writer = new(file))
                    await using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<ExpenseMap>();
                        await csv.WriteRecordsAsync(e);
                        await csv.FlushAsync();
                    }
                    return;
                case SQLTables.ExpenseType:
                    List<ExpenseType> et = await dataService.BackupGetExpenseType();
                    await using (StreamWriter writer = new(file))
                    await using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<ExpenseTypeMap>();
                        await csv.WriteRecordsAsync(et);
                        await csv.FlushAsync();
                    }
                    return;
                case SQLTables.Version:
                    using (StreamWriter outputFile = new StreamWriter(Path.Combine(filePath, "FILE_ID.DIZ")))
                    {
                        await outputFile.WriteAsync(dataService.GetVERSIONCHECKNUMBER.ToString());
                    }
                    return;

                default:
                    break;
            }

            return;
        }

        [RelayCommand]
        private async Task OnBackupDBRequest()
        {
            Loading = true;
            HasError = false;
            try
            {
                ExportLog = "START BACKUP DATABASE: \n";

                string dbFileName = "database-bk.db3";
                string file = Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, dbFileName);
                if (File.Exists(file))
                {
                    ExportLog += "File Exists, Deleting filename: " + file + "\n";
                    File.Delete(file);
                }
                ExportLog += "Begin backing up...\n";

                await dataService.BackupDatabase(file);

                // Copy file to public accessable download folder, outside of app sandbox
                string filePublic = Path.Combine(fhs.GetDownloadPath(), dbFileName);
                if (File.Exists(filePublic))
                {
                    File.Delete(filePublic);
                }
                byte[] bytes = await File.ReadAllBytesAsync(file);
                ExportLog += "Copy backup database file to: " + filePublic + "\n";
                await File.WriteAllBytesAsync(filePublic, bytes);

                ExportLog += "Back up complete, start share file...\n";
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "TimeClock App SQLite3 Database",
                    File = new ShareFile(filePublic),
                });

                ExportLog += "FINISHED\n";
            }
            catch
            {
                HasError = true;
            }
            finally
            {
                Loading = false;
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }

        [RelayCommand]
        private void OnToggleWarningBox()
        {
            WarningBoxVisible = !WarningBoxVisible;
        }

        [RelayCommand]
        private void OnCloseWarningBox()
        {
            WarningBoxVisible = false;
        }
    }
}
