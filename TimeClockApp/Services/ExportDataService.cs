using System.Diagnostics.CodeAnalysis;
using CsvHelper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TimeClockApp.Shared;

#nullable enable

namespace TimeClockApp.Services
{
    //TODO Make class content async
    public class ExportDataService : SQLiteDataStore
    {
        // import data must be this version or newer
        private static readonly double VERSIONCHECKNUMBER = 1.0;
        public double GetVERSIONCHECKNUMBER => VERSIONCHECKNUMBER;

        public Task<List<TimeCard>> BackupGetTimeCard() => Context.TimeCard
             .IgnoreAutoIncludes().ToListAsync();
        public Task<List<Employee>> BackupGetEmployee() => Context.Employee
            .IgnoreAutoIncludes().ToListAsync();
        public Task<List<Project>> BackupGetProject() => Context.Project
            .IgnoreAutoIncludes().ToListAsync();
        public Task<List<Phase>> BackupGetPhase() => Context.Phase
            .IgnoreAutoIncludes().ToListAsync();
        public Task<List<Config>> BackupGetConfig() => Context.Config
            .IgnoreAutoIncludes().ToListAsync();
        public Task<List<Expense>> BackupGetExpense() => Context.Expense
            .IgnoreAutoIncludes().ToListAsync();
        public Task<List<ExpenseType>> BackupGetExpenseType() => Context.ExpenseType
            .IgnoreAutoIncludes().ToListAsync();

#region READCSV
        private string? ReadFileVersion(string csvFile)
        {
            string? records;
            using (var reader = new StreamReader(csvFile))
            {
                records = reader.ReadToEnd();
            }
            return records;
        }

        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        private List<TimeCard> ReadCSVTimeCards(string csvFile)
        {
            List<TimeCard> records = new();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<TimeCardMap>();
                records = csv.GetRecords<TimeCard>().ToList();
            }
            return records;
        }

        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        private List<Employee> ReadCSVEmployee(string csvFile)
        {
            List<Employee> records = new();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<EmployeeMap>();
                records = csv.GetRecords<Employee>().ToList();
            }
            return records;
        }
        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        private List<Project> ReadCSVProject(string csvFile)
        {
            List<Project> records = new();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<ProjectMap>();
                records = csv.GetRecords<Project>().ToList();
            }
            return records;
        }

        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        private List<Phase> ReadCSVPhase(string csvFile)
        {
            List<Phase> records = new();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<PhaseMap>();
                records = csv.GetRecords<Phase>().ToList();
            }
            return records;
        }

        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        private List<Config> ReadCSVConfig(string csvFile)
        {
            List<Config> records = new();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<ConfigMap>();
                records = csv.GetRecords<Config>().ToList();
            }
            return records;
        }

        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        private List<Expense> ReadCSVExpense(string csvFile)
        {
            List<Expense> records = new();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<ExpenseMap>();
                records = csv.GetRecords<Expense>().ToList();
            }
            return records;
        }

        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        private List<ExpenseType> ReadCSVExpenseType(string csvFile)
        {
            List<ExpenseType> records = new();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<ExpenseTypeMap>();
                records = csv.GetRecords<ExpenseType>().ToList();
            }
            return records;
        }

#endregion READCSV

        public async Task BackupDatabase(string? savePath = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("BEGIN BACKUP OF SQLITE DATABASE", "Backup");
                savePath ??= $"{SQLiteSetting.SQLiteDBPath}.bk";
                System.Diagnostics.Debug.WriteLine("Back file name: " + savePath + "\n", "Backup");
                if (File.Exists(savePath))
                {
                    System.Diagnostics.Debug.WriteLine("Existing backup file found. Deleting old file.\n", "Backup");
                    File.Delete(savePath);
                }
                System.Diagnostics.Debug.WriteLine("Opening SQLiteConnection and begin backing up...\n", "Backup");
                using (SqliteConnection source = new($"Data Source = {SQLiteSetting.SQLiteDBPath};"))
                using (SqliteConnection target = new($"Data Source = {savePath};"))
                {
                    await source.OpenAsync();
                    await target.OpenAsync();
                    source.BackupDatabase(target);
                    System.Diagnostics.Debug.WriteLine("Successfully completed SQLite file backup.\n", "Backup");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("BACKUP NOT COMPLETED!!\n", "Backup");
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Backup");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("BACKUP COMPLETE.\n", "Backup");
            }
        }

        public string ImportData(ImportDataModel dataModel, string ExportLog, bool overWriteData)
        {
            if (dataModel.IsAnyTrue())
            {
                using (IDbContextTransaction transaction = Context.Database.BeginTransaction())
                {
                    // Version check
                    if (dataModel.bVersion)
                    {
                        ExportLog += "Checking compatibility...\n";
                        dataModel.ImportVersionString = ReadFileVersion(dataModel.FileVersion);
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
#nullable restore
                    ExportLog += "Starting to import...\n";
                    try
                    {
                        transaction.CreateSavepoint("optimistic-update");

                        if (dataModel.bTimeCard)
                        {
                            ExportLog += "Importing TimeCard\n";
                            dataModel.ImTimeCard = ReadCSVTimeCards(dataModel.FileTimeCard);
                            for (int i = 0; i < dataModel.ImTimeCard.Count; i++)
                            {
                                if (ImportTimeCard(dataModel.ImTimeCard[i], overWriteData))
                                    dataModel.ReadyToSave++;
                            }
                        }
                        if (dataModel.bEmployee)
                        {
                            ExportLog += "Importing Employee\n";
                            dataModel.ImEmployee = ReadCSVEmployee(dataModel.FileEmployee);
                            for (int i = 0; i < dataModel.ImEmployee.Count; i++)
                            {
                                if (ImportEmployee(dataModel.ImEmployee[i]))
                                    dataModel.ReadyToSave++;
                            }
                        }
                        if (dataModel.bProject)
                        {
                            ExportLog += "Importing Project\n";
                            dataModel.ImProject = ReadCSVProject(dataModel.FileProject);
                            for (int i = 0; i < dataModel.ImProject.Count; i++)
                            {
                                if (ImportProject(dataModel.ImProject[i]))
                                    dataModel.ReadyToSave++;
                            }
                        }
                        if (dataModel.bPhase)
                        {
                            ExportLog += "Importing Phase\n";
                            dataModel.ImPhase = ReadCSVPhase(dataModel.FilePhase);
                            for (int i = 0; i < dataModel.ImPhase.Count; i++)
                            {
                                if (ImportPhase(dataModel.ImPhase[i]))
                                    dataModel.ReadyToSave++;
                            }
                        }
                        if (dataModel.bConfig)
                        {
                            ExportLog += "Importing Config\n";
                            dataModel.ImConfig = ReadCSVConfig(dataModel.FileConfig);
                            for (int i = 0; i < dataModel.ImConfig.Count; i++)
                            {
                                if (ImportConfig(dataModel.ImConfig[i]))
                                    dataModel.ReadyToSave++;
                            }
                        }
                        if (dataModel.bExpense)
                        {
                            ExportLog += "Importing Expense\n";
                            dataModel.ImExpense = ReadCSVExpense(dataModel.FileExpense);
                            for (int i = 0; i < dataModel.ImExpense.Count; i++)
                            {
                                if (ImportExpense(dataModel.ImExpense[i], overWriteData))
                                    dataModel.ReadyToSave++;
                            }
                        }
                        if (dataModel.bExpenseType)
                        {
                            ExportLog += "Importing ExpenseType\n";
                            dataModel.ImExpenseType = ReadCSVExpenseType(dataModel.FileExpense);
                            for (int i = 0; i < dataModel.ImExpenseType.Count; i++)
                            {
                                if (ImportExpenseType(dataModel.ImExpenseType[i], overWriteData))
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
                        System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException + "\nUNDOING CHANGES\n", "Import");

                        transaction.RollbackToSavepoint("optimistic-update");

                        ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
                    }
                    finally
                    {
                        if (dataModel.ReadyToSave > 0)
                        {
                            transaction.Commit();
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

#region INSERT DATA TO DB
        private bool ImportTimeCard(TimeCard card, bool overWriteData)
        {
            try
            {
                TimeCard t = Context.TimeCard.Find(card.TimeCardId);
                if (t != null && overWriteData)
                {
                    TimeCard updateCard = Context.TimeCard.IgnoreAutoIncludes().FirstOrDefault(x => x.TimeCardId == card.TimeCardId);

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
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Import");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            return false;
        }

        private bool ImportEmployee(Employee card)
        {
            try
            {
                Employee t = Context.Employee.Find(card.EmployeeId);
                if (t != null)
                {
                    Employee updateCard = Context.Employee.IgnoreAutoIncludes().FirstOrDefault(x => x.EmployeeId == card.EmployeeId);
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
                System.Diagnostics.Debug.WriteLine(x.Message + "\n" + x.InnerException + "\n", "Import");
                ShowPopupError(x.Message + "\n" + x.InnerException, "ABORTING DUE TO InvalidDataException");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Import");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            return false;
        }

        private bool ImportProject(Project card)
        {
            try
            {
                Project t = Context.Project.Find(card.ProjectId);
                if (t != null)
                {
                    Project updateCard = Context.Project.IgnoreAutoIncludes().FirstOrDefault(x => x.ProjectId == card.ProjectId);
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
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Import");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            return false;
        }

        private bool ImportPhase(Phase card)
        {
            try
            {
                Phase t = Context.Phase.Find(card.PhaseId);
                if (t != null)
                {
                    Phase updateCard = Context.Phase.IgnoreAutoIncludes().FirstOrDefault(x => x.PhaseId == card.PhaseId);
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
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Import");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            return false;
        }

        private bool ImportConfig(Config card)
        {
            try
            {
                Config t = Context.Config.Find(card.ConfigId);
                if (t != null)
                {
                    Config updateCard = Context.Config.IgnoreAutoIncludes().FirstOrDefault(x => x.ConfigId == card.ConfigId);
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
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Import");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            return false;
        }

        private bool ImportExpense(Expense card, bool overWriteData)
        {
            try
            {
                Expense t = Context.Expense.Find(card.ExpenseId);
                if (t != null && overWriteData)
                {
                    Expense updateCard = Context.Expense.IgnoreAutoIncludes().FirstOrDefault(x => x.ExpenseId == card.ExpenseId);
                    updateCard.ProjectId = card.ProjectId;
                    updateCard.PhaseId = card.PhaseId;
                    updateCard.ExpenseTypeId = card.ExpenseTypeId;
                    updateCard.ExpenseType_CategoryName = card.ExpenseType_CategoryName ?? string.Empty;
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
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Import");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            return false;
        }

        private bool ImportExpenseType(ExpenseType card, bool overWriteData)
        {
            try
            {
                ExpenseType t = Context.ExpenseType.Find(card.ExpenseTypeId);
                if (t != null && overWriteData)
                {
                    ExpenseType updateCard = Context.ExpenseType.IgnoreAutoIncludes().FirstOrDefault(x => x.ExpenseTypeId == card.ExpenseTypeId);
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
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException + "\n", "Import");
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "ABORTING DUE TO ERROR");
            }
            return false;
        }

#endregion
    }
}
