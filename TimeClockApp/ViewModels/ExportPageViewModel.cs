using System.Diagnostics.CodeAnalysis;
using CsvHelper;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;
using TimeClockApp.Shared;
using TimeClockApp.Utilities;

namespace TimeClockApp.ViewModels
{
    public partial class ExportPageViewModel : BaseViewModel
    {
        private readonly ExportDataService dataService = new();
        private readonly FileService fhs = new();
        private readonly ExportUtilities util = new();
        internal string CSVFilePath { get; set; }

        [ObservableProperty]
        public partial string ExportLog { get; set; } = string.Empty;

        //TODO FIX when adding to data fails due to existing data at same rowId.
        /// <summary>
        /// During import of Data should the TimeCards and Expenses data be overwritten or only added to (with possible duplicates)
        /// </summary>
        [ObservableProperty]
        public partial bool OverwriteData { get; set; } = true;

        [ObservableProperty]
        public partial bool WarningBoxVisible { get; set; } = false;

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
                                { DevicePlatform.WinUI, new[] { ".zip" } },
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

                dataModel = util.UnzipArchive(fileCopyName, UnZipPath);
                if (dataModel.IsAnyTrue())
                {
                    ExportLog += "Done!...\n";
                    ExportLog += dataService.ImportData(dataModel, ExportLog, OverwriteData);
                    App.NoticeProjectHasChanged = true;
                    App.NoticePhaseHasChanged = true;
                    App.NoticeUserHasChanged = true;
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
            catch (AggregateException ax)
            {
                HasError = true;
                Log.WriteLine("ImportData Exception =================\n");
                string f = TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateExceptionForPopup(ax, "ExportPageViewModel");
                await dataService.ShowPopupErrorAsync(f, "ABORTING DUE TO ERROR");
            }
            catch (Exception ex)
            {
                HasError = true;
                Log.WriteLine("\nEXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException, "ExportPageViewModel");
                await dataService.ShowPopupErrorAsync("EXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
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
                    Log.WriteLine("\nEXPECTED POSSIBLE EXCEPTION\n" + ex.Message + "\n" + ex.InnerException);
                    ExportLog += ex.Message + "\n";
                }

                Directory.CreateDirectory(CSVFilePath);
                ExportLog += "Start Database export\n";
                Task<string> taskA = Task.Run(() => MakeCSVFilesAsync());

                //  Zip everything and present a share dialog to the user
                ExportLog += "Zipping ... \n";
                await taskA.ContinueWith(async antecedent => await util.CompressAndExportFolder((await antecedent))).Unwrap();

                ExportLog += "Done!\n";
            }
            catch (AggregateException ax)
            {
                HasError = true;
                string f = TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateExceptionForPopup(ax, "ExportPageViewModel");
                await dataService.ShowPopupErrorAsync(f, "ABORTING DUE TO ERROR");
            }
            catch (Exception ex)
            {
                HasError = true;
                Log.WriteLine("\nEXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException);
                await dataService.ShowPopupErrorAsync("EXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
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

                string dbFileName = "timeclock_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".db3";
                string file = Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, dbFileName);
                bool bFileDeleteError = false;
                try
                {
                    if (File.Exists(file))
                    {
                        ExportLog += "Old Temp File Exists, Deleting filename: " + file + "\n";
                        File.Delete(file);
                        ExportLog += "Old Temp File Deleted.\n";
                    }
                }
                catch (IOException iox)
                {
                    bFileDeleteError = true;
                    HasError = true;
                    Log.WriteLine(iox.Message + "\n" + iox.InnerException + "\n", "Backup");
                }
                if (!bFileDeleteError)
                {
                    ExportLog += "Begin backing up...\n";
                    await dataService.BackupDatabase(file);
#if WINDOWS
                    await Share.Default.RequestAsync(new ShareFileRequest
                    {
                        Title = "TimeClock App SQLite3 Database",
                        File = new ShareFile(file),
                    });

#else
                    // Copy file to public accessible download folder, outside of app sandbox
                    string filePublic = Path.Combine(fhs.GetDownloadPath(), dbFileName);
                    if (File.Exists(filePublic))
                    {
                        ExportLog += "Old Backup File Exists, Deleting filename: " + file + "\n";
                        File.Delete(filePublic);
                        ExportLog += "Old Backup File Deleted.\n";
                    }
                    byte[] bytes = await File.ReadAllBytesAsync(file);
                    ExportLog += "Copy backup database file to: " + filePublic + "\n";
                    await File.WriteAllBytesAsync(filePublic, bytes);
                    ExportLog += "Back up complete, start share file...\n";
                    await Share.Default.RequestAsync(new ShareFileRequest
                    {
                        Title = "TimeClock App SQLite3 Database",
                        File = new ShareFile(filePublic),
                    });
#endif
                    ExportLog += "FINISHED\n";
                }
                else
                {
                    ExportLog += "ABORT due to cant delete temp file. Database NOT backed up.\n";
                }
            }
            catch (IOException io)
            {
                HasError = true;
                Log.WriteLine(io.Message + "\n" + io.InnerException + "\n", "Backup");
                await dataService.ShowPopupErrorAsync(io.Message + "\n" + io.InnerException, "BACKUP1 ABORTING BACKUP DUE TO ERROR");
            }
            catch (AggregateException ax)
            {
                HasError = true;
                string f = TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateExceptionForPopup(ax, "ExportPageViewModel");
                await dataService.ShowPopupErrorAsync(f, "BACKUP2 ABORTING DUE TO ERROR");
            }
            catch (Exception ex)
            {
                HasError = true;
                Log.WriteLine("\nEXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException, "ExportPageViewModel");
                await dataService.ShowPopupErrorAsync("EXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException, "BACKUP3 ABORTING DUE TO ERROR");
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
