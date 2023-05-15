using System.Data;

using CommunityToolkit.Maui.Core.Extensions;

using Microsoft.EntityFrameworkCore;

using TimeClockApp.Helpers;

namespace TimeClockApp.Services
{
    public partial class ReportDataService : TimeCardService
    {
        public (ObservableCollection<TimeCard> cards, ObservableCollection<Wages> allwages, ObservableCollection<Wages> owedwages) GetReportTimeCardsForEmployee(int employeeId, DateOnly start, DateOnly end, bool showPaid = true)
        {
            ObservableCollection<TimeCard> t = new();
            ShiftStatus shift = showPaid ? ShiftStatus.Deleted : ShiftStatus.Paid;
            t = Context.TimeCard
                    .AsNoTracking()
                    .Include(item => item.Wages)
                    .Where(item => item.EmployeeId == employeeId
                        && (item.TimeCard_Date >= start
                        && item.TimeCard_Date <= end
                        && item.TimeCard_Status < shift))
                    .OrderBy(item => item.TimeCard_Date)
                    .ToObservableCollection();

            ObservableCollection<Wages> w = new();
            ObservableCollection<Wages> o = new();
            if (t.Count != 0)
                for (int i = 0; i < t.Count; i++)
                {
                    if (t[i].WagesId.HasValue && t[i].WagesId.Value > 0)
                    {
                        w.Add(t[i].Wages);
                        if (t[i].TimeCard_Status == ShiftStatus.ClockedOut)
                            o.Add(t[i].Wages);
                    }
                }

            return (t, w, o);
        }

        public (ObservableCollection<TimeCard> cards, ObservableCollection<Wages> allwages, ObservableCollection<Wages> owedwages) GetReportTimeCardsForEmployee(int employeeId, int projectId, DateOnly start, DateOnly end, bool showPaid = true)
        {
            ObservableCollection<TimeCard> t = new();
            ShiftStatus shift = showPaid ? ShiftStatus.Deleted : ShiftStatus.Paid;
            t = Context.TimeCard
                    .AsNoTracking()
                    .Include(item => item.Wages)
                    .Where(item => item.EmployeeId == employeeId
                        && item.ProjectId == projectId
                        && (item.TimeCard_Date >= start
                        && item.TimeCard_Date <= end
                        && item.TimeCard_Status < shift))
                    .OrderBy(item => item.TimeCard_Date)
                    .ToObservableCollection();

            ObservableCollection<Wages> w = new();
            ObservableCollection<Wages> o = new();
            if (t.Count != 0)
                for (int i = 0; i < t.Count; i++)
                {
                    if (t[i].WagesId.HasValue && t[i].WagesId.Value > 0)
                    {
                        w.Add(t[i].Wages);
                        if (t[i].TimeCard_Status == ShiftStatus.ClockedOut)
                            o.Add(t[i].Wages);
                    }
                }

            return (t, w, o);
        }

        public async Task<(ObservableCollection<TimeCard> cards, ObservableCollection<Wages> allwages, ObservableCollection<Wages> owedwages)> GetReportTimeCardsForEmployeeAsync(int employeeId, DateOnly start, DateOnly end, bool showPaid = true)
        {
            ShiftStatus shift = showPaid ? ShiftStatus.Deleted : ShiftStatus.Paid;
            List<TimeCard> t = await Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.EmployeeId == employeeId
                    && (item.TimeCard_Date >= start
                    && item.TimeCard_Date <= end
                    && item.TimeCard_Status < shift))
                .OrderBy(item => item.TimeCard_Date)
                .ToListAsync();

            ObservableCollection<Wages> w = new();
            ObservableCollection<Wages> o = new();
            if (t.Count != 0)
                for (int i = 0; i < t.Count; i++)
                {
                    if (t[i].WagesId.HasValue && t[i].WagesId.Value > 0)
                    {
                        w.Add(t[i].Wages);
                        if (t[i].TimeCard_Status == ShiftStatus.ClockedOut)
                            o.Add(t[i].Wages);
                    }
                }

            return (new ObservableCollection<TimeCard>(t), w, o);
        }

        public async Task<(ObservableCollection<TimeCard> cards, ObservableCollection<Wages> allwages, ObservableCollection<Wages> owedwages)> GetReportTimeCardsForEmployeeAsync(int employeeId, int projectId, DateOnly start, DateOnly end, bool showPaid = true)
        {
            ShiftStatus shift = showPaid ? ShiftStatus.Deleted : ShiftStatus.Paid;
            List<TimeCard> t = await Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.EmployeeId == employeeId
                    && item.ProjectId == projectId
                    && (item.TimeCard_Date >= start
                    && item.TimeCard_Date <= end
                    && item.TimeCard_Status < shift))
                .OrderBy(item => item.TimeCard_Date)
                .ToListAsync();

            ObservableCollection<Wages> w = new();
            ObservableCollection<Wages> o = new();
            if (t.Count != 0)
                for (int i = 0; i < t.Count; i++)
                {
                    if (t[i].WagesId.HasValue && t[i].WagesId.Value > 0)
                    {
                        w.Add(t[i].Wages);
                        if (t[i].TimeCard_Status == ShiftStatus.ClockedOut)
                            o.Add(t[i].Wages);
                    }
                }

            return (new ObservableCollection<TimeCard>(t), w, o);
        }

        /// <summary>
        /// Gets All of the timeCards for the employee that has not yet been marked as paid.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public async Task<(ObservableCollection<TimeCard> cards, ObservableCollection<Wages> allwages, ObservableCollection<Wages> owedwages)> GetAllUnpaidTimeCardsForEmployeeAsync(int employeeId)
        {
            List<TimeCard> t = await Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.EmployeeId == employeeId
                    && item.TimeCard_Status == ShiftStatus.ClockedOut
                    && !item.TimeCard_bReadOnly)
                .ToListAsync();

            ObservableCollection<Wages> w = new();
            ObservableCollection<Wages> o = new();
            if (t.Count != 0)
                for (int i = 0; i < t.Count; i++)
                {
                    if (t[i].WagesId.HasValue && t[i].WagesId.Value > 0)
                    {
                        w.Add(t[i].Wages);
                        if (t[i].TimeCard_Status == ShiftStatus.ClockedOut)
                            o.Add(t[i].Wages);
                    }
                }

            return (new ObservableCollection<TimeCard>(t), w, o);
        }


        public async Task<bool> MarkTimeCardAsPaidAsync(TimeCard timeCard)
        {
            if (timeCard != null && timeCard.TimeCardId != 0 && !IsTimeCardReadOnly(timeCard.TimeCardId))
            {
                if (timeCard.TimeCard_Status == ShiftStatus.ClockedIn || timeCard.TimeCard_Status == ShiftStatus.ClockedOut)
                {
                    TimeCard t = Context.TimeCard.Find(timeCard.TimeCardId);
                    Task<bool> b = ValidateClockOutAsync(t);
                    if (t.TimeCard_Status == ShiftStatus.ClockedIn && await b)
                    {
                        t.TimeCard_EndTime = TimeHelper.RoundTimeOnly(new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute));
                        t.TimeCard_Status = ShiftStatus.ClockedOut;
                        Task<bool> c = CalculatePayAsync(t);
                        _ = await c;
                    }
                    t.TimeCard_Status = ShiftStatus.Paid;
                    t.TimeCard_bReadOnly = true;
                    Context.Update<TimeCard>(t);
                    Task<int> i = Context.SaveChangesAsync();
                    if (await i != 0)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="w"></param>
        /// <returns>regPay, otPay, ot2Pay, totalPay</returns>
        public (double, double, double, double) GetHours(List<Wages> w)
        {
            if (w == null || w.Count == 0) return (0, 0, 0, 0);
            double regHR, otHR, ot2HR, totalHR = 0;
            regHR = w.Sum(o => (double?)o.RegularHours) ?? 0;
            otHR = w.Sum(o => (double?)o.OTHours) ?? 0;
            ot2HR = w.Sum(o => (double?)o.OT2Hours) ?? 0;

            totalHR = regHR + otHR + ot2HR;
            return (regHR, otHR, ot2HR, totalHR);
        }

        public (double, double, double, double) GetPay(List<Wages> w)
        {
            if (w == null || w.Count == 0) return (0, 0, 0, 0);
            double regPay, otPay, ot2Pay, totalPay = 0;
            regPay = w.Sum(o => (double?)o.RegPay) ?? 0;
            otPay = w.Sum(o => (double?)o.OT_Pay) ?? 0;
            ot2Pay = w.Sum(o => (double?)o.OT2_Pay) ?? 0;

            totalPay = regPay + otPay + ot2Pay;
            return (regPay, otPay, ot2Pay, totalPay);
        }
    }
}
