using System.Data;
using CommunityToolkit.Maui.Core.Extensions;
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
            if (day.DayOfWeek == DayOfWeek.Saturday)
                return DateTime.Now.AddDays(-7);

            while (day.DayOfWeek != DayOfWeek.Saturday)
            {
                day = day.AddDays(-1);
            }
            return day;
        }

        public DateTime GetAppFirstRunDate()
        {
            int i = 2;
            Config c = Context.Config.Find(i);
            DateOnly d = DateOnly.Parse(c.StringValue);
            d = d.AddDays(-6);
            return d.ToDateTime(new TimeOnly(0, 0));
        }

        public Project GetProject(int projectID) => Context.Project.Find(projectID);

        public Phase GetPhase(int phaseID) => Context.Phase.Find(phaseID);

        public ObservableCollection<Project> GetProjectsList()
        {
            return Context.Project
                .Where(item => item.Status < ProjectStatus.Completed)
                .OrderBy(e => e.Name)
                .ToObservableCollection();
        }

        public ObservableCollection<Project> GetAllProjectsList(bool bShowAll) => 
            (bShowAll) ? Context.Project
                                    .Where(item => item.Status != ProjectStatus.Deleted)
                                    .OrderBy(e => e.Name)
                                    .ToObservableCollection()
                                : Context.Project
                                    .Where(item => item.Status < ProjectStatus.Completed)
                                    .OrderBy(e => e.Name)
                                    .ToObservableCollection();

        public ObservableCollection<Phase> GetPhaseList() => Context.Phase.ToObservableCollection();

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
                .ToListAsync());
        }

        public async Task<ObservableCollection<Employee>> GetEmployeesAsync() => new ObservableCollection<Employee>(await Context.Employee
                .Where(e => e.Employee_Employed == EmploymentStatus.Employed)
                .OrderBy(e => e.Employee_Name)
                .ToListAsync());

        public async Task<List<Employee>> GetEmployeesGroupInStatusAsync() => await Context.Employee
                .Where(e => e.Employee_Employed < EmploymentStatus.NotEmployed)
                .OrderBy(e => e.Employee_Employed)
                .ThenBy(e => e.Employee_Name)
                .ToListAsync();

        public TimeCard GetTimeCard(int timeCardID) => Context.TimeCard.Find(timeCardID);

        /// <summary>
        /// Checks if the time card has the read only flag set
        /// </summary>
        /// <param name="timeCardId">Id of time card to check</param>
        /// <param name="isAdmin">if IsAdmin is set true, override the ReadOnly flag and allow edits. Defaults to false</param>
        /// <returns></returns>
        internal bool IsTimeCardReadOnly(int timeCardId, bool isAdmin = false) => !isAdmin && GetTimeCard(timeCardId).TimeCard_bReadOnly;

        internal async Task<bool> ValidateClockOutAsync(TimeCard timeCard)
        {
            if (timeCard == null || timeCard.TimeCardId == 0)
                return false;

            TimeOnly clockNowRndTime = TimeHelper.RoundTimeOnly(new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute));
            DateOnly dateNow = DateOnly.FromDateTime(DateTime.Now);
            int dateCheck = timeCard.TimeCard_Date.CompareTo(dateNow);
            if (dateCheck < 0)
            {
                System.Diagnostics.Debug.WriteLine("ERROR! Card Time needs to be checked before saving. Most likely this card was ClockedIn on a prior date and not clockedOut on said date.");
                await Shell.Current.GoToAsync($"EditTimeCard?id={timeCard.TimeCardId}&e=1");
                return false;
            }
            else if (timeCard.TimeCard_StartTime > TimeOnly.FromDateTime(DateTime.Now))
            {
                if (CheckIfTimeIsCloseToNow(clockNowRndTime, timeCard.TimeCard_StartTime))
                    return true;

                System.Diagnostics.Debug.WriteLine("ERROR! Card Time start time is greater than the current time.");
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
                System.Diagnostics.Debug.WriteLine("ERROR! TimeCard Date is in the future: " + timeCard.TimeCard_Date.ToShortDateString() + "\nTIMECARD REDIRECTING TO EDIT TIMECARD PAGE");
                //ERROR CODE V2
                await App.AlertSvc.ShowAlertAsync("ERROR!", "ERROR! TimeCard Date is in the future: " + timeCard.TimeCard_Date.ToShortDateString() + "\nCan not save TimeCard until this is corrected.");
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

        private bool CheckIfTimeIsCloseToNow(TimeOnly clockNowRndTime, TimeOnly timeStart)
        {
            TimeSpan timeChk = clockNowRndTime - timeStart;
            TimeSpan chkThreshold = new(0, 15, 0);
            return timeChk < chkThreshold;
        }

        public int GetCurrentProject()
        {
            if (App.CurrentProjectId == 0)
                App.CurrentProjectId = GetConfigInt(3, 1);

            return App.CurrentProjectId;
        }

        public int GetCurrentPhase()
        {
            if (App.CurrentPhaseId == 0)
                App.CurrentPhaseId = GetConfigInt(4, 1);

            return App.CurrentPhaseId;
        }

        public Project GetCurrentProjectEntity() => Context.Project.Find(GetCurrentProject());

        public Phase GetCurrentPhaseEntity() => Context.Phase.Find(GetCurrentPhase());

        public void SaveCurrentProject(int projectId)
        {
            int i = 3;
            Config C = Context.Config.Find(i);
            if (C.IntValue.HasValue && C.IntValue.Value != projectId)
            {
                C.IntValue = projectId;
                Context.Update<Config>(C);
                Context.SaveChanges();
                App.CurrentProjectId = projectId;
            }
        }

        public void SaveCurrentPhase(int phaseId)
        {
            int i = 4;
            Config C = Context.Config.Find(i);
            if (C.IntValue.HasValue && C.IntValue.Value != phaseId)
            {
                C.IntValue = phaseId;
                Context.Update<Config>(C);
                Context.SaveChanges();
                App.CurrentPhaseId = phaseId;
            }
        }

        public Employee GetEmployee(int employeeID) => Context.Employee.Find(employeeID);

        public List<TimeCard> GetTimeCardsForPayPeriod(int employeeId, DateOnly start, DateOnly end, bool showPaid = true) => showPaid
                ? GetListPaidTimeCardsForPayPeriod(employeeId, start, end)
                : GetListUnpaidTimeCardsForPayPeriod(employeeId);

        private List<TimeCard> GetListUnpaidTimeCardsForPayPeriod(int employeeId)
        {
            return Context.TimeCard
                    .Where(item => item.EmployeeId == employeeId
                        && (item.TimeCard_Status == ShiftStatus.ClockedOut
                        || item.TimeCard_Status == ShiftStatus.ClockedIn))
                    .OrderBy(item => item.TimeCard_Date)
                    .ToList();
        }
        private List<TimeCard> GetListPaidTimeCardsForPayPeriod(int employeeId, DateOnly start, DateOnly end)
        {
            return Context.TimeCard
                    .Where(item => item.EmployeeId == employeeId
                        && item.TimeCard_Date >= start
                        && item.TimeCard_Date <= end
                        && item.TimeCard_Status < ShiftStatus.Deleted)
                    .OrderBy(item => item.TimeCard_Date)
                    .ToList();
        }

        public async Task<List<TimeCard>> GetTimeCardsForPayPeriodAsync(int employeeId, DateOnly start, DateOnly end, bool showPaid = true) => showPaid
                ? await Task.Run(() => GetListPaidTimeCardsForPayPeriodAsync(employeeId, start, end))
                : await Task.Run(() => GetListUnpaidTimeCardsForPayPeriodAsync(employeeId));

        private async Task<List<TimeCard>> GetListUnpaidTimeCardsForPayPeriodAsync(int employeeId) =>
                await Context.TimeCard
                    .Where(item => item.EmployeeId == employeeId
                        && (item.TimeCard_Status == ShiftStatus.ClockedOut
                        || item.TimeCard_Status == ShiftStatus.ClockedIn))
                    .OrderBy(item => item.TimeCard_Date)
                    .ToListAsync();
        private async Task<List<TimeCard>> GetListPaidTimeCardsForPayPeriodAsync(int employeeId, DateOnly start, DateOnly end) =>
                await Context.TimeCard
                    .Where(item => item.EmployeeId == employeeId
                        && item.TimeCard_Date >= start
                        && item.TimeCard_Date <= end
                        && item.TimeCard_Status < ShiftStatus.Deleted)
                    .OrderBy(item => item.TimeCard_Date)
                    .ToListAsync();
    }
}
