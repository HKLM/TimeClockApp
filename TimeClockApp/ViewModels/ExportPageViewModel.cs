using CsvHelper;

namespace TimeClockApp.ViewModels
{
    public partial class ExportPageViewModel : TimeStampViewModel
    {
        private readonly ExportDataService dataService = new();
        private readonly FileHelperService fhs = new();
        internal string CSVFilePath { get; set; }

        [ObservableProperty]
        private string exportLog = string.Empty;
        [ObservableProperty]
        private bool isBusy = false;

        private enum SQLTables
        {
            TimeCard = 0,
            Employee = 1,
            Wages = 2,
            Project = 3,
            Phase = 4,
            Config = 5,
            Expense = 6
        }

        public ExportPageViewModel()
        {
            dataService = new();
            fhs = new();
        }

        public void OnAppearing()
        {
            ExportLog = string.Empty;
        }

        [RelayCommand]
        private async Task ImportData()
        {
            IsBusy = true;
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

                //TODO - popup question box. Do you want to import this data? Existing data will be overwritten and can not be undone.

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

                dataModel = Utilities.ExportUtilties.UnzipArchive(fileCopyName, UnZipPath);
                if (dataModel.IsAnyTrue())
                {
                    ExportLog += "Done!...\n";
                    ExportLog += dataService.ImportData(dataModel, ExportLog);
                    int icd = dataModel.DeleteCachedFile();
                    ExportLog += "\nCleaned up " + icd + " temp files\n";
                }
                else
                {
                    ExportLog += "No valid files found for import!\nABORTING\n";
                    dataModel.DeleteCachedFile();
                    IsBusy = false;
                    return;
                }

                ExportLog += "Done!\n";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\nEXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException);
                await dataService.ShowPopupErrorAsync("EXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ExportData()
        {
            IsBusy = true;
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
                await taskA.ContinueWith(async antecedent => await Utilities.ExportUtilties.CompressAndExportFolder(antecedent.Result)).Unwrap();

                ExportLog += "Done!\n";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await dataService.ShowPopupErrorAsync("EXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<string> MakeCSVFilesAsync()
        {
            List<Task> tableTaskList = new();
            for (int i = 0; i < 7; i++)
            {
                SQLTables sQL = (SQLTables)i;
                ExportLog += "Exporting " + sQL.ToString() + " table\n";
                tableTaskList.Add(CreateFileAsync(sQL, CSVFilePath, sQL.ToString() + ".csv"));
            }
            await Task.WhenAll(tableTaskList);
            ExportLog += "Table export complete\n";

            return CSVFilePath;
        }

        private async Task CreateFileAsync(SQLTables table, string filePath, string fileName)
        {
            string file = Path.Combine(filePath, fileName);
            switch (table)
            {
                case SQLTables.TimeCard:
                    List<TimeCard> w = dataService.BackupGetTimeCard();
                    await using (StreamWriter writer = new(file))
                    await using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<TimeCardMap>();
                        await csv.WriteRecordsAsync(w);
                        await csv.FlushAsync();
                    }
                    return;
                case SQLTables.Employee:
                    List<Employee> emp = dataService.BackupGetEmployee();
                    await using (StreamWriter writer = new(file))
                    await using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<EmployeeMap>();
                        await csv.WriteRecordsAsync(emp);
                        await csv.FlushAsync();
                    }
                    return;
                case SQLTables.Wages:
                    List<Wages> wg = dataService.BackupGetWages();
                    await using (StreamWriter writer = new(file))
                    await using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<WagesMap>();
                        await csv.WriteRecordsAsync(wg);
                        await csv.FlushAsync();
                    }
                    return;
                case SQLTables.Project:
                    List<Project> pr = dataService.BackupGetProject();
                    await using (StreamWriter writer = new(file))
                    await using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<ProjectMap>();
                        await csv.WriteRecordsAsync(pr);
                        await csv.FlushAsync();
                    }
                    return;
                case SQLTables.Phase:
                    List<Phase> ph = dataService.BackupGetPhase();
                    await using (StreamWriter writer = new(file))
                    await using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<PhaseMap>();
                        await csv.WriteRecordsAsync(ph);
                        await csv.FlushAsync();
                    }
                    return;
                case SQLTables.Config:
                    List<Config> c = dataService.BackupGetConfig();
                    await using (StreamWriter writer = new(file))
                    await using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<ConfigMap>();
                        await csv.WriteRecordsAsync(c);
                        await csv.FlushAsync();
                    }
                    return;
                case SQLTables.Expense:
                    List<Expense> e = dataService.BackupGetExpense();
                    await using (StreamWriter writer = new(file))
                    await using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<ExpenseMap>();
                        await csv.WriteRecordsAsync(e);
                        await csv.FlushAsync();
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
            IsBusy = true;

            ExportLog = "START BACKUP DATABASE: \n";

            string dbFileName = "database.db3";
            string file = Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, dbFileName);
            if (File.Exists(file))
            {
                ExportLog += "File Exists, Deleting filename: " + file + "\n";
                File.Delete(file);
            }
            ExportLog += "Begin backing up...\n";

            dataService.BackupDatabase(file);

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
            IsBusy = false;
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                dataService.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            base.Dispose();
        }
    }
}
