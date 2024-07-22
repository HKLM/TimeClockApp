#nullable enable

namespace TimeClockApp.Shared.Models
{
    public class ImportDataModel
    {
        public bool bTimeCard;
        public string FileTimeCard = string.Empty;
        public bool bEmployee;
        public string FileEmployee = string.Empty;
        public bool bProject;
        public string FileProject = string.Empty;
        public bool bPhase;
        public string FilePhase = string.Empty;
        public bool bConfig;
        public string FileConfig = string.Empty;
        public bool bExpense;
        public string FileExpense = string.Empty;
        public bool bExpenseType;
        public string FileExpenseType = string.Empty;
        public bool bVersion;
        public string FileVersion = string.Empty;
        public bool bCompatibleVersion;

        public List<TimeCard> ImTimeCard = [];
        public List<Employee> ImEmployee = [];
        public List<Project> ImProject = [];
        public List<Phase> ImPhase = [];
        public List<Config> ImConfig = [];
        public List<Expense> ImExpense = [];
        public List<ExpenseType> ImExpenseType = [];

        public string? ImportVersionString = null;
        private double? importVersionNumber = null;
        public double? ImportVersionNumber
        {
            get
            {
                if (!string.IsNullOrEmpty(ImportVersionString) && double.TryParse(ImportVersionString, out double x))
                    return x;
                else
                    return importVersionNumber.HasValue ? importVersionNumber.Value : 0;
            }
            set
            {
                if (!string.IsNullOrEmpty(ImportVersionString) && double.TryParse(ImportVersionString, out double x))
                    importVersionNumber = x;
            }
        }

        public int ReadyToSave = 0;

        public bool IsAnyTrue()
        {
            List<bool> list =
            [
                this.bTimeCard,
                this.bEmployee,
                this.bPhase,
                this.bProject,
                this.bConfig,
                this.bExpense,
                this.bExpenseType,
                this.bVersion
            ];
            return list.Find(element => element.Equals(true));
        }

        /// <summary>
        /// Cleans up it's own cached files
        /// </summary>
        /// <returns>number of files deleted</returns>
        public int DeleteCachedFile()
        {
            int i = 0;
            if (CachedFileCleanUp(FileTimeCard))
                i++;
            if (CachedFileCleanUp(FileEmployee))
                i++;
            if (CachedFileCleanUp(FileProject))
                i++;
            if (CachedFileCleanUp(FilePhase))
                i++;
            if (CachedFileCleanUp(FileConfig))
                i++;
            if (CachedFileCleanUp(FileExpense))
                i++;
            if (CachedFileCleanUp(FileExpenseType))
                i++;
            if (CachedFileCleanUp(FileVersion))
                i++;

            return i;
        }

        private bool CachedFileCleanUp(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
                return true;
            }
            return false;
        }
    }
}
