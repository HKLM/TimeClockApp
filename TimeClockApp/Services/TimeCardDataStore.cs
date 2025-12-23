using System.Data;
using Microsoft.EntityFrameworkCore;
using TimeClockApp.Shared.Helpers;

namespace TimeClockApp.Services
{
    public class TimeCardDataStore : SQLiteDataStore
    {
        /// <summary>
        /// returns the first previous Saturday before the <paramref name="day"/>
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public DateTime GetStartOfPayPeriod(DateTime day)
        {
            DayOfWeek p = GetPayDayOfWeek();
            if (day.DayOfWeek == p)
                return DateTime.Now.AddDays(-7);

            while (day.DayOfWeek != p)
            {
                day = day.AddDays(-1);
            }
            return day;
        }

        private DayOfWeek GetPayDayOfWeek()
        {
            int i = 11;
            Config c = Context.Config.Find(i);
            int x = c.IntValue.Value + 1 == 7 ? 0 : c.IntValue.Value;
            DayOfWeek p = (DayOfWeek)x;
            return p;
        }

        public DateTime GetAppFirstRunDate()
        {
            int i = 2;
            Config c = Context.Config.Find(i);
            DateOnly d = DateOnly.Parse(c.StringValue);
            d = d.AddDays(-6);
            return d.ToDateTime(new TimeOnly(0, 0));
        }

#region PROJECTS_PHASES
        public Project GetProject(int projectID) => Context.Project.Find(projectID);

        public Phase GetPhase(int phaseID) => Context.Phase.Find(phaseID);

        public ObservableCollection<Project> GetProjectsList() =>
            Context.Project
                .Where(item => item.Status < ProjectStatus.Completed)
                .OrderBy(e => e.Name)
                .ToObservableCollection();

        public Task<List<Project>> GetProjectsListAsync() =>
            Context.Project
                .Where(item => item.Status < ProjectStatus.Completed)
                .OrderBy(e => e.Name)
                .ToListAsync();

        public ObservableCollection<Project> GetAllProjectsList(bool bShowAll) =>
            (bShowAll) ? Get_AllProjectsList() : Get_NotAllProjectsList();

        private ObservableCollection<Project> Get_AllProjectsList() =>
            Context.Project
                .Where(item => item.Status != ProjectStatus.Deleted)
                .OrderBy(e => e.Name)
                .ToObservableCollection();
        private ObservableCollection<Project> Get_NotAllProjectsList() =>
            Context.Project
                .Where(item => item.Status < ProjectStatus.Completed)
                .OrderBy(e => e.Name)
                .ToObservableCollection();

        public ObservableCollection<Phase> GetPhaseList() => Context.Phase.OrderBy(e => e.PhaseTitle).ToObservableCollection();

        public Task<List<Phase>> GetPhaseListAsync() => Context.Phase.OrderBy(e => e.PhaseTitle).ToListAsync();

        public int GetCurrentProject()
        {
            if (!App.CurrentProjectId.HasValue || App.CurrentProjectId.Value == 0)
                App.CurrentProjectId = GetConfigInt(3, 1);

            return App.CurrentProjectId.Value;
        }

        public int GetCurrentPhase()
        {
            if (!App.CurrentPhaseId.HasValue || App.CurrentPhaseId.Value == 0)
                App.CurrentPhaseId = GetConfigInt(4, 1);

            return App.CurrentPhaseId.Value;
        }

        public Project GetCurrentProjectEntity() => Context.Project.Find(GetCurrentProject());
        public async Task<Project> GetCurrentProjectEntityAsync()
        {
            if (!App.CurrentProjectId.HasValue || App.CurrentProjectId.Value == 0)
            {
                int? c = await GetConfigIntAsync(3, 1).ConfigureAwait(false);
                if (!c.HasValue || c.Value == 0)
                    c = 1;

                App.CurrentProjectId = c.Value;
            }

            int i = App.CurrentProjectId.Value;
            return await Context.Project.FindAsync(i).ConfigureAwait(false);
        }

        public Phase GetCurrentPhaseEntity() => Context.Phase.Find(GetCurrentPhase());

        public async Task<Phase> GetCurrentPhaseEntityAsync()
        {
            if (!App.CurrentPhaseId.HasValue || App.CurrentPhaseId.Value == 0)
            {
                int? c = await GetConfigIntAsync(4, 1).ConfigureAwait(false);
                if (!c.HasValue || c.Value == 0)
                    c = 1;

                App.CurrentPhaseId = c.Value;
            }

            int i = App.CurrentPhaseId.Value;
            return await Context.Phase.FindAsync(i).ConfigureAwait(false);
        }

        public async Task SaveCurrentProjectAsync(int projectId)
        {
            int i = 3;
            var C = await Context.Config.FindAsync(i).ConfigureAwait(false);
            if (C.IntValue.HasValue && C.IntValue.Value != projectId)
            {
                C.IntValue = projectId;
                Context.Update<Config>(C);
                await Context.SaveChangesAsync().ConfigureAwait(false);
                App.CurrentProjectId = projectId;
            }
        }

        public async Task SaveCurrentPhaseAsync(int phaseId)
        {
            int i = 4;
            Config C = await Context.Config.FindAsync(i).ConfigureAwait(false);
            if (C.IntValue.HasValue && C.IntValue.Value != phaseId)
            {
                C.IntValue = phaseId;
                Context.Update<Config>(C);
                await Context.SaveChangesAsync().ConfigureAwait(false);
                App.CurrentPhaseId = phaseId;
            }
        }

#endregion

#region EMPLOYEES
        public ObservableCollection<Employee> GetAllEmployees(bool includeNotEmployed = false)
        {
            EmploymentStatus es = includeNotEmployed ? EmploymentStatus.Deleted : EmploymentStatus.NotEmployed;
            return Context.Employee
                .Where(e => e.Employee_Employed < es)
                .OrderBy(e => e.Employee_Name)
                .ToObservableCollection();
        }

        public async Task<ObservableCollection<Employee>> GetAllEmployeesAsync(bool includeNotEmployed = false)
        {
            EmploymentStatus es = includeNotEmployed ? EmploymentStatus.Deleted : EmploymentStatus.NotEmployed;
            return new ObservableCollection<Employee>(await Context.Employee
                .Where(e => e.Employee_Employed < es)
                .OrderBy(e => e.Employee_Name)
                .ToListAsync().ConfigureAwait(false));
        }

        public Task<List<Employee>> GetEmployeeListAsync() =>
            Context.Employee
                .Where(e => e.Employee_Employed == EmploymentStatus.Employed)
                .OrderBy(e => e.Employee_Name)
                .ToListAsync();

        public Task<List<Employee>> GetEmployeesGroupInStatusAsync() =>
            Context.Employee
                .Where(e => e.Employee_Employed < EmploymentStatus.NotEmployed)
                .OrderBy(e => e.Employee_Employed)
                .ThenBy(e => e.Employee_Name)
                .ToListAsync();

        public Employee GetEmployee(int employeeID) => Context.Employee.Find(employeeID);

#endregion

#region TIMECARDS
        public TimeCard GetTimeCard(int timeCardID) => Context.TimeCard.Find(timeCardID);

        /// <summary>
        /// Checks if the time card has the read only flag set
        /// </summary>
        /// <param name="timeCardId">Id of time card to check</param>
        /// <param name="isAdmin">if IsAdmin is set true, override the ReadOnly flag and allow edits. Defaults to false</param>
        /// <returns></returns>
        public bool IsTimeCardReadOnly(int timeCardId, bool isAdmin = false) =>
            !isAdmin && Context.TimeCard
                .AsNoTracking()
                .Where(tc => tc.TimeCardId == timeCardId)
                .Select(tc => tc.TimeCard_bReadOnly)
                .FirstOrDefault();
    
        internal async Task<bool> ValidateClockOutAsync(TimeCard timeCard)
        {
            if (timeCard == null || timeCard.TimeCardId == 0)
                return false;

            TimeOnly clockNowRndTime = TimeHelper.RoundTimeOnly(new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute));
            DateOnly dateNow = DateOnly.FromDateTime(DateTime.Now);
            int dateCheck = timeCard.TimeCard_Date.CompareTo(dateNow);
            if (dateCheck < 0)
            {
                Log.WriteLine("ERROR! Card Time needs to be checked before saving. Most likely this card was ClockedIn on a prior date and not clockedOut on said date.");
                await Shell.Current.GoToAsync($"EditTimeCard?id={timeCard.TimeCardId}&e=1");
                return false;
            }
            else if (timeCard.TimeCard_StartTime > TimeOnly.FromDateTime(DateTime.Now))
            {
                if (CheckIfTimeIsCloseToNow(clockNowRndTime, timeCard.TimeCard_StartTime))
                    return true;

                Log.WriteLine("ERROR! Card Time start time is greater than the current time.");
                await Shell.Current.GoToAsync($"EditTimeCard?id={timeCard.TimeCardId}&e=2");
                return false;
            }
            else if (dateCheck == 0)
            {
                if (ValidatingClockOutTimes(timeCard.TimeCard_StartTime, TimeOnly.FromDateTime(DateTime.Now)))
                    return true;
                else
                    return CheckIfTimeIsCloseToNow(clockNowRndTime, timeCard.TimeCard_StartTime);
            }
            //timecard is in the future
            else if (dateCheck > 0)
            {
                Log.WriteLine($"ERROR! TimeCard Date is in the future: {timeCard.TimeCard_Date.ToShortDateString()}\nTIMECARD REDIRECTING TO EDIT TIMECARD PAGE");
                //ERROR CODE V2
                await App.AlertSvc!.ShowAlertAsync("ERROR!", $"ERROR! TimeCard Date is in the future: {timeCard.TimeCard_Date.ToShortDateString()}\nCan not save TimeCard until this is corrected.").ConfigureAwait(false);
                await Shell.Current.GoToAsync($"EditTimeCard?id={timeCard.TimeCardId}");
                // refresh the timecard, and recheck if issue is fixed
                timeCard = GetTimeCard(timeCard.TimeCardId);
                dateCheck = timeCard.TimeCard_Date.CompareTo(dateNow);
                if (dateCheck < 0)
                {
                    TimeOnly timeEndVar = timeCard.TimeCard_EndTime == new TimeOnly() ? TimeOnly.FromDateTime(DateTime.Now) : timeCard.TimeCard_EndTime;
                    return ValidatingClockOutTimes(timeCard.TimeCard_StartTime, timeEndVar);
                }
                else if (dateCheck == 0)
                {
                    bool b = CheckIfTimeIsCloseToNow(clockNowRndTime, timeCard.TimeCard_StartTime);
                    bool vb = ValidatingClockOutTimes(timeCard.TimeCard_StartTime, TimeOnly.FromDateTime(DateTime.Now));
                    return b && vb;
                }
                return false;
            }

            return true;
        }

        private bool ValidatingClockOutTimes(TimeOnly startTime, TimeOnly endTime) => endTime.CompareTo(startTime) >= 0;

        private bool CheckIfTimeIsCloseToNow(TimeOnly clockNowRndTime, TimeOnly timeStart) => clockNowRndTime - timeStart < new TimeSpan(0, 15, 0);

        public List<TimeCard> GetTimeCardsForPayPeriod(int employeeId, DateOnly start, DateOnly end, bool showPaid = true) => showPaid
                ? GetListPaidTimeCardsForPayPeriod(employeeId, start, end)
                : GetListUnpaidTimeCardsForPayPeriod(employeeId);

        private List<TimeCard> GetListUnpaidTimeCardsForPayPeriod(int employeeId) =>
                Context.TimeCard
                    .Where(item => item.EmployeeId == employeeId
                        && (item.TimeCard_Status == ShiftStatus.ClockedOut
                        || item.TimeCard_Status == ShiftStatus.ClockedIn))
                    .OrderBy(item => item.TimeCard_Date)
                    .ToList();
        private List<TimeCard> GetListPaidTimeCardsForPayPeriod(int employeeId, DateOnly start, DateOnly end) =>
                Context.TimeCard
                    .Where(item => item.EmployeeId == employeeId
                        && item.TimeCard_Status < ShiftStatus.Deleted
                        && item.TimeCard_Date >= start
                        && item.TimeCard_Date <= end)
                    .OrderBy(item => item.TimeCard_Date)
                    .ToList();

        public async Task<List<TimeCard>> GetTimeCardsForPayPeriodAsync(int employeeId, DateOnly start, DateOnly end, bool onlyUnpaid = false) => onlyUnpaid
                ? await GetListUnpaidTimeCardsForPayPeriodAsync(employeeId).ConfigureAwait(false)
                : await GetListTimeCardsForPayPeriodAsync(employeeId, start, end).ConfigureAwait(false);

        private Task<List<TimeCard>> GetListUnpaidTimeCardsForPayPeriodAsync(int employeeId) =>
                Context.TimeCard
                    .Where(item => item.EmployeeId == employeeId
                        && (item.TimeCard_Status == ShiftStatus.ClockedOut
                        || item.TimeCard_Status == ShiftStatus.ClockedIn))
                    .OrderBy(item => item.TimeCard_Date)
                    .ToListAsync();
        private Task<List<TimeCard>> GetListTimeCardsForPayPeriodAsync(int employeeId, DateOnly start, DateOnly end) =>
                Context.TimeCard
                    .Where(item => item.EmployeeId == employeeId
                        && item.TimeCard_Status < ShiftStatus.Deleted
                        && item.TimeCard_Date >= start
                        && item.TimeCard_Date <= end)
                    .OrderBy(item => item.TimeCard_Date)
                    .ToListAsync();
#endregion

#region EXPENSES
        public ExpenseType GetExpenseType(int expenseTypeId) => Context.ExpenseType.Find(expenseTypeId);
        //public async Task<ExpenseType> GetExpenseTypeAsync(int expenseTypeId) => await Context.ExpenseType.FindAsync(expenseTypeId);
        public string GetExpenseType_CategoryName(int expenseTypeId) => Context.ExpenseType.First(z => z.ExpenseTypeId == expenseTypeId).CategoryName;

        public bool AddNewExpense(int projectId, int phaseId, double amount, string memo, string projectName, string phaseTitle, int expenseTypeId = 2, string expenseTypeCategoryName = "")
        {
            string e;
            if (string.IsNullOrEmpty(expenseTypeCategoryName))
                e = GetExpenseType_CategoryName(expenseTypeId);
            else
                e = expenseTypeCategoryName;

            //make expenses (not income) a negative number
            if (expenseTypeId != 2)
                amount *= (-1);

            Expense exp = new(projectId, phaseId, amount, DateOnly.FromDateTime(DateTime.Now), projectName, phaseTitle, memo, expenseTypeId, e);

            Context.Add<Expense>(exp);
            return (Context.SaveChanges() > 0);
        }

        // used to add new entry when marking a timecard as paid
        public async Task AddNewExpenseAsync(int projectId, int phaseId, double amount, string memo, string projectName, string phaseTitle, int expenseTypeId = 3, string expenseTypeCategoryName = "")
        {
            string e;
            if (string.IsNullOrEmpty(expenseTypeCategoryName))
                e = GetExpenseType_CategoryName(expenseTypeId);
            else
                e = expenseTypeCategoryName;

            //make expenses (not income) a negative number
            if (expenseTypeId != 2)
                amount *= (-1);

            Expense exp = new(projectId, phaseId, amount, DateOnly.FromDateTime(DateTime.Now), projectName, phaseTitle, memo, expenseTypeId, e);

            Context.Add<Expense>(exp);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }
#endregion
    }
}
