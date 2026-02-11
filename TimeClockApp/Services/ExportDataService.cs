using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using CsvHelper;
#if SQLITE
using Microsoft.Data.Sqlite;
#endif
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TimeClockApp.Shared;

#nullable enable

namespace TimeClockApp.Services
{
    public class ExportDataService : SQLiteDataStore
    {
        // import data must be this version or newer
        private static readonly double VERSIONCHECKNUMBER = 1.0;
        public double GetVERSIONCHECKNUMBER => VERSIONCHECKNUMBER;

        // Used to return the filename of the exported zip file for user notification
        static string zip_filename = string.Empty;
        public static string ZIPFILENAME
        {
            get => zip_filename;
            private set => zip_filename = value;
        }   

        public Task<List<TimeCard>> BackupGetTimeCard() => Context.TimeCard.IgnoreAutoIncludes().ToListAsync();
        public Task<List<Employee>> BackupGetEmployee() => Context.Employee.IgnoreAutoIncludes().ToListAsync();
        public Task<List<Project>> BackupGetProject() => Context.Project.IgnoreAutoIncludes().ToListAsync();
        public Task<List<Phase>> BackupGetPhase() => Context.Phase.IgnoreAutoIncludes().ToListAsync();
        public Task<List<Config>> BackupGetConfig() => Context.Config.IgnoreAutoIncludes().ToListAsync();
        public Task<List<Expense>> BackupGetExpense() => Context.Expense.IgnoreAutoIncludes().ToListAsync();
        public Task<List<ExpenseType>> BackupGetExpenseType() => Context.ExpenseType.IgnoreAutoIncludes().ToListAsync();

#region READCSV

        private async Task<string?> ReadFileVersionAsync(string csvFile)
        {
            string? records;
            using (var reader = new StreamReader(csvFile))
            {
                records = await reader.ReadToEndAsync();
            }
            return records;
        }

        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        private async Task<List<TimeCard>> ReadCSVTimeCardsAsync(string csvFile)
        {
            List<TimeCard> records = new();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<TimeCardMap>();
                records = await csv.GetRecordsAsync<TimeCard>().ToListAsync();
            }
            return records;
        }

        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        private async Task<List<Employee>> ReadCSVEmployeeAsync(string csvFile)
        {
            List<Employee> records = new();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<EmployeeMap>();
                records = await csv.GetRecordsAsync<Employee>().ToListAsync();
            }
            return records;
        }
        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        private async Task<List<Project>> ReadCSVProjectAsync(string csvFile)
        {
            List<Project> records = new();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<ProjectMap>();
                records = await csv.GetRecordsAsync<Project>().ToListAsync();
            }
            return records;
        }

        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        private async Task<List<Phase>> ReadCSVPhaseAsync(string csvFile)
        {
            List<Phase> records = new();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<PhaseMap>();
                records = await csv.GetRecordsAsync<Phase>().ToListAsync();
            }
            return records;
        }

        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        private async Task<List<Config>> ReadCSVConfigAsync(string csvFile)
        {
            List<Config> records = new();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<ConfigMap>();
                records = await csv.GetRecordsAsync<Config>().ToListAsync();
            }
            return records;
        }

        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        private async Task<List<Expense>> ReadCSVExpenseAsync(string csvFile)
        {
            List<Expense> records = new();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<ExpenseMap>();
                records = await csv.GetRecordsAsync<Expense>().ToListAsync();
            }
            return records;
        }

        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        private async Task<List<ExpenseType>> ReadCSVExpenseTypeAsync(string csvFile)
        {
            List<ExpenseType> records = new();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<ExpenseTypeMap>();
                records = await csv.GetRecordsAsync<ExpenseType>().ToListAsync();
            }
            return records;
        }

#endregion READCSV

        public async Task BackupDatabase(string? savePath = null)
        {
#if SQLITE

            try
            {
                Log.WriteLine("BEGIN BACKUP OF SQLITE DATABASE", "Backup");

                savePath ??= Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, "TimeClockApp", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
                Log.WriteLine("Back file name: " + savePath + "\n", "Backup");
                if (File.Exists(savePath))
                {
                    Log.WriteLine("Existing backup file found. Deleting old file.\n", "Backup");
                    File.Delete(savePath);
                    Log.WriteLine("File Deleted.\n", "Backup");
                }
                Log.WriteLine("Opening SQLiteConnection and begin backing up...\n", "Backup");
                using (SqliteConnection source = new($"Data Source = {SQLiteSetting.GetSQLiteDBPath()};"))
                using (SqliteConnection target = new($"Data Source = {savePath};"))
                {
                    await source.OpenAsync();
                    await target.OpenAsync();
                    source.BackupDatabase(target);
                    Log.WriteLine("Successfully completed SQLite file backup.\n", "Backup");
                    await target.CloseAsync();
                }
            }
            catch (IOException io)
            {
                Log.WriteLine(io.Message + "\n" + io.InnerException + "\n", "Backup");
                ShowPopupError(io.Message + "\n" + io.InnerException, "ABORTING BACKUP DUE TO ERROR");
            }
            catch (AggregateException ax)
            {
                Log.WriteLine("BACKUP NOT COMPLETED!!\n", "Backup");
                TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
            }
            catch (Exception ex)
            {
                Log.WriteLine("BACKUP NOT COMPLETED!!\n", "Backup");
                Log.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Backup");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING BACKUP DUE TO ERROR");
            }
            finally
            {
                Log.WriteLine("BACKUP COMPLETE.\n", "Backup");
            }
#elif MSSQL
            Log.WriteLine("THIS FEATURE IS NOT AVAILABLE FOR MSSQL SERVER.\nDATABASE NOT BACKED UP.", "Backup");
            await ShowPopupErrorAsync("THIS FEATURE IS NOT AVAILABLE FOR MSSQL SERVER.\nDATABASE NOT BACKED UP.", "WARNING");
#endif
        }

#region IMPORT DATA

        public async Task<string> ImportData(ImportDataModel dataModel, string ExportLog, bool overWriteData)
        {
            if (dataModel.IsAnyTrue())
            {
                using (IDbContextTransaction transaction = await Context.Database.BeginTransactionAsync())
                {
                    // Version check
                    if (dataModel.bVersion)
                    {
                        ExportLog += "Checking compatibility...\n";
                        dataModel.ImportVersionString = await ReadFileVersionAsync(dataModel.FileVersion);
                        if (!string.IsNullOrEmpty(dataModel.ImportVersionString) && dataModel.ImportVersionNumber > 0)
                        {
                            dataModel.bCompatibleVersion = true;
                            ExportLog += "PASSED. Version=" + dataModel.ImportVersionString + "\n";
                        }
                    }
                    if (!dataModel.bCompatibleVersion)
                    {
                        ExportLog += "No data to import!\nABORTING DUE TO INCOMPATIBLE DATA FILES\n";
                        return ExportLog;
                    }

                    ExportLog += "Starting to import...\n";
                    try
                    {
                        await transaction.CreateSavepointAsync("optimistic-update");

                        if (dataModel.bTimeCard)
                        {
                            ExportLog += "Importing TimeCard\n";
                            dataModel.ImTimeCard = await ReadCSVTimeCardsAsync(dataModel.FileTimeCard);
                            for (int i = 0; i < dataModel.ImTimeCard.Count; i++)
                            {
                                if (await ImportTimeCard(dataModel.ImTimeCard[i], overWriteData))
                                    dataModel.ReadyToSave++;
                            }
                        }
                        if (dataModel.bEmployee)
                        {
                            ExportLog += "Importing Employee\n";
                            dataModel.ImEmployee = await ReadCSVEmployeeAsync(dataModel.FileEmployee);
                            for (int i = 0; i < dataModel.ImEmployee.Count; i++)
                            {
                                if (await ImportEmployee(dataModel.ImEmployee[i]))
                                    dataModel.ReadyToSave++;
                            }
                        }
                        if (dataModel.bProject)
                        {
                            ExportLog += "Importing Project\n";
                            dataModel.ImProject = await ReadCSVProjectAsync(dataModel.FileProject);
                            for (int i = 0; i < dataModel.ImProject.Count; i++)
                            {
                                if (await ImportProject(dataModel.ImProject[i]))
                                    dataModel.ReadyToSave++;
                            }
                        }
                        if (dataModel.bPhase)
                        {
                            ExportLog += "Importing Phase\n";
                            dataModel.ImPhase = await ReadCSVPhaseAsync(dataModel.FilePhase);
                            for (int i = 0; i < dataModel.ImPhase.Count; i++)
                            {
                                if (await ImportPhase(dataModel.ImPhase[i]))
                                    dataModel.ReadyToSave++;
                            }
                        }
                        if (dataModel.bConfig)
                        {
                            ExportLog += "Importing Config\n";
                            dataModel.ImConfig = await ReadCSVConfigAsync(dataModel.FileConfig);
                            for (int i = 0; i < dataModel.ImConfig.Count; i++)
                            {
                                if (await ImportConfig(dataModel.ImConfig[i]))
                                    dataModel.ReadyToSave++;
                            }
                        }
                        if (dataModel.bExpense)
                        {
                            ExportLog += "Importing Expense\n";
                            dataModel.ImExpense = await ReadCSVExpenseAsync(dataModel.FileExpense);
                            for (int i = 0; i < dataModel.ImExpense.Count; i++)
                            {
                                if (await ImportExpense(dataModel.ImExpense[i], overWriteData))
                                    dataModel.ReadyToSave++;
                            }
                        }
                        if (dataModel.bExpenseType)
                        {
                            ExportLog += "Importing ExpenseType\n";
                            dataModel.ImExpenseType = await ReadCSVExpenseTypeAsync(dataModel.FileExpenseType);
                            for (int i = 0; i < dataModel.ImExpenseType.Count; i++)
                            {
                                if (await ImportExpenseType(dataModel.ImExpenseType[i], overWriteData))
                                    dataModel.ReadyToSave++;
                            }
                        }

                        if (dataModel.ReadyToSave > 0)
                        {
                            //transaction.ReleaseSavepoint("optimistic-update");
                            ExportLog += dataModel.ReadyToSave + " items waiting to be saved to database\n";

                            int z = Context.SaveChanges();
                            ExportLog += "Saved " + z + " items\n";
                            //bImported = z > 0;
                        }
                        else
                            ExportLog += "No data to import!\nABORTING\n";
                    }
                    catch (Exception ex)
                    {
                        //To prevent saving in the finally block of this try-catch
                        dataModel.ReadyToSave = 0;
                        Log.WriteLine("ImportData: " + ex.Message + "\n" + ex.InnerException + "\nUNDOING CHANGES\n", "Import");

                        await transaction.RollbackToSavepointAsync("optimistic-update");

                        ShowPopupError(ex.Message + "\n" + ex.InnerException, "ImportData ABORTING DUE TO ERROR");
                    }
                    finally
                    {
                        if (dataModel.ReadyToSave > 0)
                        {
                            await transaction.CommitAsync();
                            ExportLog += "Committed Data to Database!\n";
                            ShowPopupError("Committed Data to Database.", "COMPLETED");
                        }
                    }
                }
            }
            else
            {
                ExportLog += "No valid files found for import!\nABORTING\n";
            }
            return ExportLog;
        }

        private async Task<bool> ImportTimeCard(TimeCard card, bool overWriteData)
        {
            try
            {
                TimeCard? t = Context.TimeCard.Find(card.TimeCardId);
                if (t != null && overWriteData)
                {
                    TimeCard? updateCard = await Context.TimeCard.IgnoreAutoIncludes().FirstOrDefaultAsync(x => x.TimeCardId == card.TimeCardId);
                    if (updateCard == null)
                        return false;
                    updateCard.EmployeeId = card.EmployeeId;
                    updateCard.ProjectId = card.ProjectId;
                    updateCard.PhaseId = card.PhaseId;
                    updateCard.TimeCard_EmployeeName = card.TimeCard_EmployeeName;
                    updateCard.TimeCard_Status = card.TimeCard_Status;
                    updateCard.TimeCard_DateTime = card.TimeCard_DateTime;
                    updateCard.TimeCard_Date = card.TimeCard_Date;
                    updateCard.TimeCard_StartTime = card.TimeCard_StartTime;
                    updateCard.TimeCard_EndTime = card.TimeCard_EndTime;
                    updateCard.TimeCard_WorkHours = card.TimeCard_WorkHours;
                    updateCard.TimeCard_EmployeePayRate = card.TimeCard_EmployeePayRate;
                    updateCard.TimeCard_bReadOnly = card.TimeCard_bReadOnly;
                    updateCard.ProjectName = card.ProjectName ?? string.Empty;
                    updateCard.PhaseTitle = card.PhaseTitle ?? string.Empty;
                    Context.Update(updateCard);
                    return true;
                }
                else
                {
                    Context.Add<TimeCard>(card);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Import");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ImportTimeCard ABORTING DUE TO ERROR");
            }
            return false;
        }

        private async Task<bool> ImportEmployee(Employee card)
        {
            try
            {
                Employee? t = Context.Employee.Find(card.EmployeeId);
                if (t != null)
                {
                    Employee? updateCard = await Context.Employee.IgnoreAutoIncludes().FirstOrDefaultAsync(x => x.EmployeeId == card.EmployeeId);
                    if (updateCard == null)
                        return false;
                    updateCard.Employee_Name = card.Employee_Name;
                    updateCard.Employee_PayRate = card.Employee_PayRate;
                    updateCard.Employee_Employed = card.Employee_Employed;
                    updateCard.JobTitle = card.JobTitle ?? string.Empty;
                    Context.Update<Employee>(updateCard);
                    return true;
                }
                else
                {
                    Context.Add<Employee>(card);
                    return true;
                }
            }
            catch (InvalidDataException x)
            {
                Log.WriteLine(x.Message + "\n" + x.InnerException + "\n", "Import");
                ShowPopupError(x.Message + "\n" + x.InnerException, "ABORTING DUE TO InvalidDataException");
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Import");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            return false;
        }

        private async Task<bool> ImportProject(Project card)
        {
            try
            {
                Project? t = Context.Project.Find(card.ProjectId);
                if (t != null)
                {
                    Project? updateCard = await Context.Project.IgnoreAutoIncludes().FirstOrDefaultAsync(x => x.ProjectId == card.ProjectId);
                    if (updateCard == null)
                        return false;
                    updateCard.Name = card.Name;
                    updateCard.Status = card.Status;
                    updateCard.ProjectDate = card.ProjectDate;
                    Context.Update<Project>(updateCard);
                    return true;
                }
                else
                {
                    Context.Add<Project>(card);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Import");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            return false;
        }

        private async Task<bool> ImportPhase(Phase card)
        {
            try
            {
                Phase? t = Context.Phase.Find(card.PhaseId);
                if (t != null)
                {
                    Phase? updateCard = await Context.Phase.IgnoreAutoIncludes().FirstOrDefaultAsync(x => x.PhaseId == card.PhaseId);
                    if (updateCard == null)
                        return false;
                    updateCard.PhaseTitle = card.PhaseTitle;
                    Context.Update<Phase>(updateCard);
                    return true;
                }
                else
                {
                    Context.Add<Phase>(card);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Import");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            return false;
        }

        private async Task<bool> ImportConfig(Config card)
        {
            try
            {
                Config? t = Context.Config.Find(card.ConfigId);
                if (t != null)
                {
                    Config? updateCard = await Context.Config.IgnoreAutoIncludes().FirstOrDefaultAsync(x => x.ConfigId == card.ConfigId);
                    if (updateCard == null)
                        return false;
                    updateCard.IntValue = card.IntValue ?? null;
                    updateCard.StringValue = card.StringValue ?? null;
                    updateCard.Name = card.Name;
                    updateCard.Hint = card.Hint ?? null;
                    Context.Update<Config>(updateCard);
                    return true;
                }
                else
                {
                    Context.Add<Config>(card);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Import");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            return false;
        }

        private async Task<bool> ImportExpense(Expense card, bool overWriteData)
        {
            try
            {
                Expense? t = Context.Expense.Find(card.ExpenseId);
                if (t != null && overWriteData)
                {
                    Expense? updateCard = await Context.Expense.IgnoreAutoIncludes().FirstOrDefaultAsync(x => x.ExpenseId == card.ExpenseId);
                    if (updateCard == null)
                        return false;
                    updateCard.ProjectId = card.ProjectId;
                    updateCard.PhaseId = card.PhaseId;
                    updateCard.ExpenseTypeId = card.ExpenseTypeId;
                    updateCard.ExpenseTypeCategoryName = card.ExpenseTypeCategoryName ?? string.Empty;
                    updateCard.ExpenseDate = card.ExpenseDate;
                    updateCard.Memo = card.Memo ?? string.Empty;
                    updateCard.Amount = card.Amount;
                    updateCard.IsRecent = card.IsRecent;
                    updateCard.ExpenseProject = card.ExpenseProject ?? string.Empty;
                    updateCard.ExpensePhase = card.ExpensePhase ?? string.Empty;
                    Context.Update<Expense>(updateCard);
                    return true;
                }
                else
                {
                    Context.Add<Expense>(card);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Import");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            return false;
        }

        private async Task<bool> ImportExpenseType(ExpenseType card, bool overWriteData)
        {
            try
            {
                ExpenseType? t = Context.ExpenseType.Find(card.ExpenseTypeId);
                if (t != null && overWriteData)
                {
                    ExpenseType? updateCard = await Context.ExpenseType.IgnoreAutoIncludes().FirstOrDefaultAsync(x => x.ExpenseTypeId == card.ExpenseTypeId);
                    if (updateCard == null)
                        return false    ;
                    updateCard.CategoryName = card.CategoryName;
                    Context.Update<ExpenseType>(updateCard);
                    return true;
                }
                else
                {
                    Context.Add<ExpenseType>(card);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Import");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            return false;
        }

        public async Task<ImportDataModel> UnzipArchive(string fileToUNZipPath, string unZipTempDirectory)
        {
            ImportDataModel dataModel = new();
            try
            {
                // UnZip everything 
                await ZipFile.ExtractToDirectoryAsync(fileToUNZipPath, unZipTempDirectory, true);

                string versionFile = Path.Combine(unZipTempDirectory, "FILE_ID.DIZ");
                dataModel.bVersion = File.Exists(versionFile);
                if (dataModel.bVersion)
                    dataModel.FileVersion = versionFile;

                string timecardFile = Path.Combine(unZipTempDirectory, "TimeCard.csv");
                dataModel.bTimeCard = File.Exists(timecardFile);
                dataModel.FileTimeCard = timecardFile;
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
                string expenseTypeFile = Path.Combine(unZipTempDirectory, "ExpenseType.csv");
                dataModel.bExpenseType = File.Exists(expenseTypeFile);
                dataModel.FileExpenseType = expenseTypeFile;
            }
            catch (Exception ex)
            {
                string ExportLog = "\nUnzipArchive EXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException;
                Log.WriteLine(ExportLog);
                await ShowPopupErrorAsync(ExportLog, "UnzipArchive ABORTING DUE TO ERROR");
            }
            return dataModel;
        }

#endregion

#region EXPORT DATA

        public async Task<bool> CompressAndExportFolder(string folderToZipPath)
        {
            ExportDataService.ZIPFILENAME = string.Empty;
            // Get a temporary cache directory
            string exportZipTempDirectory = Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, "Export");

            // Delete folder incase anything from previous exports, it will be recreated later anyway
            try
            {
                if (Directory.Exists(exportZipTempDirectory))
                    Directory.Delete(exportZipTempDirectory, true);
            }
            catch (AggregateException ax)
            {
                TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
            }
            catch (Exception ex)
            {
                // Log things and move on, don't want to fail just because of a left over lock or something
                Log.WriteLine("CompressAndExportFolder" + ex.Message + "\n" + ex.InnerException);
            }

            // Get a timestamped filename
            string exportZipFilename = $"TimeClockAppData_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.zip";
            Directory.CreateDirectory(exportZipTempDirectory);

            string exportZipFilePath = Path.Combine(exportZipTempDirectory, exportZipFilename);
            if (File.Exists(exportZipFilePath))
            {
                File.Delete(exportZipFilePath);
            }

            // Zip everything up
            await ZipFile.CreateFromDirectoryAsync(folderToZipPath, exportZipFilePath, CompressionLevel.Fastest, false);

            FileService fhs = new();
            // Copy zip file to public accessible download folder, outside of app sandbox
            string publicDownloadPath = fhs.GetDownloadPath();
            string filePublic = Path.Combine(publicDownloadPath, exportZipFilename);
            if (File.Exists(filePublic))
            {
                File.Delete(filePublic);
            }
            byte[] bytes = await File.ReadAllBytesAsync(exportZipFilePath);
            await File.WriteAllBytesAsync(filePublic, bytes);

            ExportDataService.ZIPFILENAME = exportZipFilename;

            // Give the user the option to share this using whatever medium they like
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "TimeClock App Export Data",
                File = new ShareFile(filePublic),
            });

            return true;
        }

#endregion

    }
}
