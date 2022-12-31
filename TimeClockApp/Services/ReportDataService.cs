using System.Collections.ObjectModel;
using System.Data;

using CommunityToolkit.Maui.Core.Extensions;

using Microsoft.EntityFrameworkCore;

using TimeClockApp.Helpers;
using TimeClockApp.Models;

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
                    .ToObservableCollection();

            ObservableCollection<Wages> w = new();
            ObservableCollection<Wages> o = new();
            if (t.Any())
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
                    .ToObservableCollection();

            ObservableCollection<Wages> w = new();
            ObservableCollection<Wages> o = new();
            if (t.Any())
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
                .ToListAsync();

            ObservableCollection<Wages> w = new();
            ObservableCollection<Wages> o = new();
            if (t.Any())
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
                .ToListAsync();

            ObservableCollection<Wages> w = new();
            ObservableCollection<Wages> o = new();
            if (t.Any())
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
        /// Finds all unpaid timeCards for employee
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public async Task<(ObservableCollection<TimeCard>, double)> GetAllUnpaidTimeCardsForEmployeeAsync(int employeeId)
        {
            IQueryable<TimeCard> L = Context.TimeCard
                .Where(item => item.EmployeeId == employeeId
                    && item.TimeCard_Status == ShiftStatus.ClockedOut
                    && !item.TimeCard_bReadOnly);
            double d = await L.AnyAsync() ? await L.SumAsync(o => o.TimeCard_WorkHours) : 0;

            return (new ObservableCollection<TimeCard>((IEnumerable<TimeCard>)await L.ToListAsync()), d);
        }

        public async Task<(ObservableCollection<TimeCard>, double)> GetAllUnpaidTimeCardsForEmployee(int employeeId)
        {
            ObservableCollection<TimeCard> t = new ObservableCollection<TimeCard>(await Context.TimeCard
                .Where(item => item.EmployeeId == employeeId
                    && item.TimeCard_Status == ShiftStatus.ClockedOut
                    && !item.TimeCard_bReadOnly)
                .ToListAsync());

            double d = t.Any() ? t.Sum(o => o.TimeCard_WorkHours) : 0;

            return (t, d);
        }

        public async Task<bool> MarkTimeCardAsPaidAsync(TimeCard timeCard)
        {
            if (timeCard != null && timeCard.TimeCardId != 0 && !IsTimeCardReadOnly(timeCard.TimeCardId))
            {
                if (timeCard.TimeCard_Status == ShiftStatus.ClockedIn || timeCard.TimeCard_Status == ShiftStatus.ClockedOut)
                {
                    TimeCard t = Context.TimeCard.Find(timeCard.TimeCardId);
                    if (t.TimeCard_Status == ShiftStatus.ClockedIn && await ValidateClockOutAsync(t))
                    {
                        t.TimeCard_EndTime = TimeHelper.RoundTimeOnly(new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute));
                        t.TimeCard_Status = ShiftStatus.ClockedOut;
                        _ = await CalculatePayAsync(t);
                    }
                    t.TimeCard_Status = ShiftStatus.Paid;
                    t.TimeCard_bReadOnly = true;
                    Context.Update<TimeCard>(t);
                    if (await Context.SaveChangesAsync() != 0)
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
            regHR = w.Any() ? w.Sum(o => o.RegularHours) : 0;
            otHR = w.Any() ? w.Sum(o => o.OTHours) : 0;
            ot2HR = w.Any() ? w.Sum(o => o.OT2Hours) : 0;
            totalHR = regHR + otHR + ot2HR;
            return (regHR, otHR, ot2HR, totalHR);
        }

        public (double, double, double, double) GetPay(List<Wages> w)
        {
            if (w == null || w.Count == 0) return (0, 0, 0, 0);
            double regPay, otPay, ot2Pay, totalPay = 0;
            regPay = w.Any() ? w.Sum(o => o.RegPay) : 0;
            otPay = w.Any() ? w.Sum(o => o.OT_Pay) : 0;
            ot2Pay = w.Any() ? w.Sum(o => o.OT2_Pay) : 0;
            totalPay = regPay + otPay + ot2Pay;
            return (regPay, otPay, ot2Pay, totalPay);
        }
    }
}
