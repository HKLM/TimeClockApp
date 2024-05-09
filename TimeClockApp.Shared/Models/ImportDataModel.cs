namespace TimeClockApp.Shared.Models
{
    public class ImportDataModel
    {
        public bool bTimeCard;
        public string FileTimeCard;
        public bool bEmployee;
        public string FileEmployee;
        public bool bProject;
        public string FileProject;
        public bool bPhase;
        public string FilePhase;
        public bool bConfig;
        public string FileConfig;
        public bool bExpense;
        public string FileExpense;

        public List<TimeCard> ImTimeCard;
        public List<Employee> ImEmployee;
        public List<Project> ImProject;
        public List<Phase> ImPhase;
        public List<Config> ImConfig;
        public List<Expense> ImExpense;

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
                this.bExpense
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
