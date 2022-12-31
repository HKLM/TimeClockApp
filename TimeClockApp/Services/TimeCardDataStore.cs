using System.Collections.ObjectModel;
using System.Data;

using CommunityToolkit.Maui.Core.Extensions;

using Microsoft.EntityFrameworkCore;

using TimeClockApp.Helpers;
using TimeClockApp.Models;

namespace TimeClockApp.Services
{
    public class TimeCardDataStore : SqliteDataStore
    {
        /// <summary>
        /// returns the first previous saturday before the <paramref name="day"/>
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

        public async Task<ObservableCollection<Project>> GetProjectsListAsync()
        {
            return new ObservableCollection<Project>(await Context.Project
                .Where(item => item.Status < ProjectStatus.Completed)
                .OrderBy(e => e.Name)
                .ToListAsync());
        }

        public ObservableCollection<Project> GetAllProjectsList(bool bShowAll)
        {
            if (bShowAll)
                return Context.Project
                        .Where(item => item.Status != ProjectStatus.Deleted)
                        .OrderBy(e => e.Name)
                        .ToObservableCollection();
            else
                return Context.Project
                        .Where(item => item.Status < ProjectStatus.Completed)
                        .OrderBy(e => e.Name)
                        .ToObservableCollection();
        }

        public ObservableCollection<Phase> GetPhaseList() => Context.Phase.ToObservableCollection();

        public async Task<ObservableCollection<Phase>> GetPhaseListAsync()
        {
            return new ObservableCollection<Phase>(await Context.Phase
                .OrderBy(e => e.PhaseTitle)
                .ToListAsync());
        }

        public ObservableCollection<Employee> GetAllEmployees(bool includeNotEmployeed = false)
        {
            EmploymentStatus es = includeNotEmployeed ? EmploymentStatus.Deleted : EmploymentStatus.NotEmployeed;
            return Context.Employee
                .Where(e => e.Employee_Employed < es)
                .OrderBy(e => e.Employee_Name)
                .ToObservableCollection();
        }

        public async Task<ObservableCollection<Employee>> GetAllEmployeesAsync(bool includeNotEmployeed = false)
        {
            EmploymentStatus es = includeNotEmployeed ? EmploymentStatus.Deleted : EmploymentStatus.NotEmployeed;
            return new ObservableCollection<Employee>(await Context.Employee
                .Where(e => e.Employee_Employed < es)
                .OrderBy(e => e.Employee_Name)
                .ToListAsync());
        }

        public ObservableCollection<Employee> GetEmployeesGroupInStatus()
        {
            return Context.Employee
                .Where(e => e.Employee_Employed < EmploymentStatus.NotEmployeed)
                .OrderBy(e => e.Employee_Employed)
                .ThenBy(e => e.Employee_Name)
                .ToObservableCollection();
        }

        public Employee GetEmployeeByName(string name) => Context.Employee
            .AsNoTracking()
            .Where(e => e.Employee_Name == name)
            .First();

        public TimeCard GetTimeCard(int timeCardID) => Context.TimeCard.Find(timeCardID);

        internal bool IsTimeCardReadOnly(int timeCardId)
        {
            return false; //TODO
        }

        internal async Task<bool> ValidateClockOutAsync(TimeCard timeCard)
        {
            if (timeCard == null || timeCard.TimeCardId == 0)
                return false;

            TimeOnly clockNowRndTime = TimeHelper.RoundTimeOnly(new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute));
            DateOnly dateNow = DateOnly.FromDateTime(DateTime.Now);
            int dateCheck = timeCard.TimeCard_Date.CompareTo(dateNow);
            //timecard is in the past
            if (dateCheck < 0)
            {
                TimeOnly timeEndVar = timeCard.TimeCard_EndTime == new TimeOnly() ? TimeOnly.FromDateTime(DateTime.Now) : timeCard.TimeCard_EndTime;
                return ValidatingClockOutTimes(timeCard.TimeCard_StartTime, timeEndVar);
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
                    if (ValidatingClockOutTimes(timeCard.TimeCard_StartTime, TimeOnly.FromDateTime(DateTime.Now)))
                        return true;
                    else
                        return CheckIfTimeIsCloseToNow(clockNowRndTime, timeCard.TimeCard_StartTime);
                }
                return false;
            }

            return true;
        }

        private bool ValidatingClockOutTimes(TimeOnly startTime, TimeOnly endtime) => endtime.CompareTo(startTime) >= 0;

        //private bool ValidatingClockOutDate(ref TimeCard timeCard) => timeCard.TimeCard_Date.CompareTo(DateOnly.FromDateTime(DateTime.Now)) <= 0;

        private bool CheckIfTimeIsCloseToNow(TimeOnly clockNowRndTime, TimeOnly timestart)
        {
            TimeSpan timechk = clockNowRndTime - timestart;
            TimeSpan chkThreshold = new TimeSpan(0, 5, 0);
            return timechk < chkThreshold;
        }

        internal void CalculatePay(TimeCard t)
        {
            if (t == null || t.TimeCardId == 0 || t.TimeCard_Status != ShiftStatus.ClockedOut || t.TimeCard_bReadOnly)
                return;

            Wages w;
            bool bNew = false;
            if (t.WagesId.HasValue && t.WagesId.Value != 0)
            {
                w = Context.Wages.Find(t.WagesId);
                if (w == null)
                {
                    w = new(t.TimeCardId);
                    bNew = true;
                }
            }
            else
            {
                w = new(t.TimeCardId);
                bNew = true;
            }

            //Round to nearest 15 min
            TimeSpan ts = t.TimeCard_EndTime - t.TimeCard_StartTime;
            int min = (int)ts.Minutes;

            min += min % 15 < 15 - min % 15 ? -min % 15 : 15 - min % 15;
            ts = new TimeSpan(ts.Hours, min, 0) - new TimeSpan((int)ts.Days, 0, 0, 0);

            if (!bNew)
                w.Reset();

            w.TotalHours = ts.TotalHours;
            w.RegularHours = w.TotalHours;
            if (w.TotalHours > 8)
            {
                w.RegularHours = 8;
                w.OTHours = w.TotalHours - 8;
                if (w.OTHours > 2)
                {
                    w.OT2Hours = w.OTHours - 2;
                    w.OTHours = 2;
                }
            }
            double payRate = t.TimeCard_EmployeePayRate;
            w.RegPay = w.RegularHours * payRate;
            if (w.OTHours > 0)
            {
                w.OT_Pay = w.OTHours * (payRate * 1.5);
                if (w.OT2Hours > 0)
                    w.OT2_Pay = w.OT2Hours * (payRate * 2);
            }
            w.TotalWages = w.RegPay + w.OT_Pay + w.OT2_Pay;

            if (bNew)
            {
                Context.Add<Wages>(w);
                t.Wages = w;
                Context.Update<TimeCard>(t);
            }
            else
            {
                Context.Update<Wages>(w);
            }
            Context.SaveChanges();
        }

        internal async Task<bool> CalculatePayAsync(TimeCard t)
        {
            if (t == null || t.TimeCardId == 0 || t.TimeCard_Status != ShiftStatus.ClockedOut || t.TimeCard_bReadOnly)
                return false;

            Wages w;
            bool bNew = false;
            if (t.WagesId.HasValue && t.WagesId.Value != 0)
            {
                w = Context.Wages.Find(t.WagesId);
                if (w == null)
                {
                    w = new(t.TimeCardId);
                    bNew = true;
                }
            }
            else
            {
                w = new(t.TimeCardId);
                bNew = true;
            }

            //Round to nearest 15 min
            TimeSpan ts = t.TimeCard_EndTime - t.TimeCard_StartTime;
            int min = (int)ts.Minutes;

            min += min % 15 < 15 - min % 15 ? -min % 15 : 15 - min % 15;
            ts = new TimeSpan(ts.Hours, min, 0) - new TimeSpan((int)ts.Days, 0, 0, 0);

            if (!bNew)
                w.Reset();

            w.TotalHours = ts.TotalHours;
            w.RegularHours = w.TotalHours;
            if (w.TotalHours > 8)
            {
                w.RegularHours = 8;
                w.OTHours = w.TotalHours - 8;
                if (w.OTHours > 2)
                {
                    w.OT2Hours = w.OTHours - 2;
                    w.OTHours = 2;
                }
            }
            double payRate = t.TimeCard_EmployeePayRate;
            w.RegPay = w.RegularHours * payRate;
            if (w.OTHours > 0)
            {
                w.OT_Pay = w.OTHours * (payRate * 1.5);
                if (w.OT2Hours > 0)
                    w.OT2_Pay = w.OT2Hours * (payRate * 2);
            }
            w.TotalWages = w.RegPay + w.OT_Pay + w.OT2_Pay;

            if (bNew)
            {
                Context.Add<Wages>(w);
                t.Wages = w;
                Context.Update<TimeCard>(t);
            }
            else
            {
                Context.Update<Wages>(w);
            }
            return await Context.SaveChangesAsync() != 0;
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
    }
}
