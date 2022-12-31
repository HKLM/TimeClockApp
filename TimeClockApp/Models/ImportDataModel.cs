namespace TimeClockApp.Models
{
    public class ImportDataModel
    {
        public bool bTimeCard;
        public string FileTimeCard;
        public bool bWages;
        public string FileWages;
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
        public List<Wages> ImWages;
        public List<Project> ImProject;
        public List<Phase> ImPhase;
        public List<Config> ImConfig;
        public List<Expense> ImExpense;

        public int ReadyToSave = 0;

        public bool IsAnyTrue()
        {
            List<bool> list = new List<bool>();
            list.Add(this.bTimeCard);
            list.Add(this.bWages);
            list.Add(this.bEmployee);
            list.Add(this.bPhase);
            list.Add(this.bProject);
            list.Add(this.bConfig);
            list.Add(this.bExpense);
            return list.Find(element => element.Equals(true));
        }

        /// <summary>
        /// Cleans up it's own cached files
        /// </summary>
        /// <returns>number of files deleted</returns>
        public int DeleteCachedFile()
        {
            int i = 0;
            if (File.Exists(FileTimeCard))
            {
                File.Delete(FileTimeCard);
                i++;
            }
            if (File.Exists(FileWages))
            {
                File.Delete(FileWages);
                i++;
            }
            if (File.Exists(FileEmployee))
            {
                File.Delete(FileEmployee);
                i++;
            }
            if (File.Exists(FileProject))
            {
                File.Delete(FileProject);
                i++;
            }
            if (File.Exists(FilePhase))
            {
                File.Delete(FilePhase);
                i++;
            }
            if (File.Exists(FileConfig))
            {
                File.Delete(FileConfig);
                i++;
            }
            if (File.Exists(FileExpense))
            {
                File.Delete(FileExpense);
                i++;
            }
            return i;
        }
    }
}
