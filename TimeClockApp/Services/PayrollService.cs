using System.Data;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace TimeClockApp.Services
{
    public class PayrollService : TimeCardService
    {
        /// <summary>
        /// Gets the WorkersCompensation rate value.
        /// </summary>
        /// <returns></returns>
        public double GetWCRate() => GetRate(5);

        protected double GetRate(int i)
        {
            Config? c = Context.Config.Find(i);
            return c != null && double.TryParse(c?.StringValue, out double wc) ? wc : 0;
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public async Task<TimeSheet> GetPayrollTimeSheetForEmployeeAsync(int employeeId, DateOnly start, DateOnly end, string employeeName, TimeSheet? sheet = null, bool onlyUnpaid = false)
        {
            if (sheet == null)
            {
                sheet = new TimeSheet(employeeId, start, end, employeeName);
            }
            else
            {
                sheet.Reset();
            }

            sheet.TimeCards = await GetTimeCardsForPayPeriodAsync(employeeId, start, end, onlyUnpaid);
            if (sheet.TimeCards == null || sheet.TimeCards?.Count == 0)
                return sheet;

            double pay = 0;
            if (sheet.TimeCards != null)
            {
                IQueryable<IGrouping<DateOnly, TimeCard>> tg = sheet.TimeCards.GroupBy(x => x.TimeCard_Date).AsQueryable();
                foreach (var g in tg)
                {
                    double x = 0;
                    foreach (TimeCard item in g)
                    {
                        if (item.TimeCard_Status == ShiftStatus.ClockedOut)
                        {
                            sheet.UnpaidTimeCards.Add(item);
                            x += item.TimeCard_WorkHours;
                        }
                        else if (item.TimeCard_Status == ShiftStatus.Paid)
                        {
                            sheet.PaidTimeCards.Add(item);
                            x += item.TimeCard_WorkHours;
                        }
                    }

                    //TODO fix
                    pay = g.Select(x => x.TimeCard_EmployeePayRate).First();
                    //dont include any cards ClockedIn. They wreak havoc on the calculations.
                    TimeCardDayTotal dayHours = new TimeCardDayTotal(g
                        .Where(x => x.TimeCard_Status == ShiftStatus.ClockedOut
                        || x.TimeCard_Status == ShiftStatus.Paid)
                        .Sum(x => x.TimeCard_WorkHours));
                    sheet.TotalWorkHours += dayHours.TotalWorkHours;
                    sheet.RegTotalHours += dayHours.RegTotalHours;
                    sheet.TotalOTHours += dayHours.TotalOTHours;
                    sheet.TotalOT2Hours += dayHours.TotalOT2Hours;

                    // Calculate unpaid hours
                    double d = g.Where(x =>
                                    x.TimeCard_Status == ShiftStatus.ClockedOut)
                                    .Sum(x => x.TimeCard_WorkHours);
                    if (d > 0)
                    {
                        TimeCardDayTotal unpaidDayHours = new(d);
                        sheet.UnPaidTotalWorkHours += unpaidDayHours.TotalWorkHours;
                        sheet.UnPaidRegTotalHours += unpaidDayHours.RegTotalHours;
                        sheet.UnPaidTotalOTHours += unpaidDayHours.TotalOTHours;
                        sheet.UnPaidTotalOT2Hours += unpaidDayHours.TotalOT2Hours;
                    }
                }
            }

            // calculate wages for this sheet
            sheet.RegTotalPay = sheet.RegTotalHours * pay;
            sheet.TotalOTPay = sheet.TotalOTHours * (pay * 1.5);
            sheet.TotalOT2Pay = sheet.TotalOT2Hours * (pay * 2);
            sheet.TotalGrossPay = sheet.RegTotalPay + sheet.TotalOTPay + sheet.TotalOT2Pay;

            // calculate wages owed for this sheet
            sheet.UnPaidRegTotalPay = sheet.UnPaidRegTotalHours * pay;
            sheet.UnPaidTotalOTPay = sheet.UnPaidTotalOTHours * (pay * 1.5);
            sheet.UnPaidTotalOT2Pay = sheet.UnPaidTotalOT2Hours * (pay * 2);
            sheet.TotalOwedGrossPay = sheet.UnPaidRegTotalPay + sheet.UnPaidTotalOTPay + sheet.UnPaidTotalOT2Pay;

            return sheet;
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public TimeSheet GetPayrollTimeSheetForEmployee(int employeeId, DateOnly start, DateOnly end, string employeeName, TimeSheet? sheet = null, bool showPaid = true)
        {
            if (sheet == null)
            {
                sheet = new(employeeId, start, end, employeeName);
            }
            else
            {
                sheet.Reset();
            }
            sheet.TimeCards = GetTimeCardsForPayPeriod(employeeId, start, end, showPaid);

            if (sheet.TimeCards == null || sheet.TimeCards?.Count == 0)
                return sheet;

            double pay = 0;
            if (sheet.TimeCards != null)
            {
                IQueryable<IGrouping<DateOnly, TimeCard>> tg = sheet.TimeCards.GroupBy(x => x.TimeCard_Date).AsQueryable();
                foreach (IGrouping<DateOnly, TimeCard> g in tg)
                {
                    double x = 0;
                    foreach (TimeCard item in g)
                    {
                        if (item != null)
                        {
                            if (item.TimeCard_Status == ShiftStatus.ClockedOut)
                            {
                                sheet.UnpaidTimeCards.Add(item);
                                x += item.TimeCard_WorkHours;
                            }
                            else if (item.TimeCard_Status == ShiftStatus.Paid)
                            {
                                sheet.PaidTimeCards.Add(item);
                                x += item.TimeCard_WorkHours;
                            }
                        }
                    }

                    //TODO fix
                    pay = g.Select(x => x.TimeCard_EmployeePayRate).First();
                    //dont include any cards ClockedIn. They wreak havoc on the calculations.
                    TimeCardDayTotal dayHours = new TimeCardDayTotal(g
                        .Where(x => x.TimeCard_Status == ShiftStatus.ClockedOut
                        || x.TimeCard_Status == ShiftStatus.Paid)
                        .Sum(x => x.TimeCard_WorkHours));
                    sheet.TotalWorkHours += dayHours.TotalWorkHours;
                    sheet.RegTotalHours += dayHours.RegTotalHours;
                    sheet.TotalOTHours += dayHours.TotalOTHours;
                    sheet.TotalOT2Hours += dayHours.TotalOT2Hours;

                    // Calculate unpaid hours
                    double d = g.Where(x =>
                                x.TimeCard_Status == ShiftStatus.ClockedOut)
                                .Sum(x => x.TimeCard_WorkHours);
                    if (d > 0)
                    {
                        TimeCardDayTotal unpaidDayHours = new(d);
                        sheet.UnPaidTotalWorkHours += unpaidDayHours.TotalWorkHours;
                        sheet.UnPaidRegTotalHours += unpaidDayHours.RegTotalHours;
                        sheet.UnPaidTotalOTHours += unpaidDayHours.TotalOTHours;
                        sheet.UnPaidTotalOT2Hours += unpaidDayHours.TotalOT2Hours;
                    }
                }
            }
            // calculate wages for this sheet
            sheet.RegTotalPay = sheet.RegTotalHours * pay;
            sheet.TotalOTPay = sheet.TotalOTHours * (pay * 1.5);
            sheet.TotalOT2Pay = sheet.TotalOT2Hours * (pay * 2);
            sheet.TotalGrossPay = sheet.RegTotalPay + sheet.TotalOTPay + sheet.TotalOT2Pay;

            // calculate wages owed for this sheet
            sheet.UnPaidRegTotalPay = sheet.UnPaidRegTotalHours * pay;
            sheet.UnPaidTotalOTPay = sheet.UnPaidTotalOTHours * (pay * 1.5);
            sheet.UnPaidTotalOT2Pay = sheet.UnPaidTotalOT2Hours * (pay * 2);
            sheet.TotalOwedGrossPay = sheet.UnPaidRegTotalPay + sheet.UnPaidTotalOTPay + sheet.UnPaidTotalOT2Pay;

            return sheet;
        }

        public double GetPayrollInfoForExpense(List<TimeCard> cards)
        {
            TimeSheet? sheet;
            if (cards != null && cards[0] != null && cards[0].EmployeeId > 0 && !string.IsNullOrEmpty(cards[0].TimeCard_EmployeeName))
            {
                sheet = new TimeSheet(cards[0].EmployeeId, DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now), cards[0].TimeCard_EmployeeName);
            }
            else
            {
                return 0;
            }

            sheet.TimeCards = cards.ToList();
            if (sheet.TimeCards == null || sheet.TimeCards?.Count == 0)
                return 0;

            double pay = 0;
            if (sheet.TimeCards != null)
            {
                IQueryable<IGrouping<DateOnly, TimeCard>> tg = sheet.TimeCards.GroupBy(x => x.TimeCard_Date).AsQueryable();
                foreach (var g in tg)
                {
                    double x = 0;
                    foreach (TimeCard item in g)
                    {
                        if (item.TimeCard_Status == ShiftStatus.ClockedOut)
                        {
                            sheet.UnpaidTimeCards.Add(item);
                            x += item.TimeCard_WorkHours;
                        }
                    }

                    pay = g.Select(x => x.TimeCard_EmployeePayRate).First();

                    // Calculate unpaid hours
                    double d = g.Where(x =>
                                    x.TimeCard_Status == ShiftStatus.ClockedOut)
                                    .Sum(x => x.TimeCard_WorkHours);
                    if (d > 0)
                    {
                        TimeCardDayTotal unpaidDayHours = new(d);
                        sheet.UnPaidTotalWorkHours += unpaidDayHours.TotalWorkHours;
                        sheet.UnPaidRegTotalHours += unpaidDayHours.RegTotalHours;
                        sheet.UnPaidTotalOTHours += unpaidDayHours.TotalOTHours;
                        sheet.UnPaidTotalOT2Hours += unpaidDayHours.TotalOT2Hours;
                    }
                }
            }

            // calculate wages owed for this sheet
            sheet.UnPaidRegTotalPay = sheet.UnPaidRegTotalHours * pay;
            sheet.UnPaidTotalOTPay = sheet.UnPaidTotalOTHours * (pay * 1.5);
            sheet.UnPaidTotalOT2Pay = sheet.UnPaidTotalOT2Hours * (pay * 2);
            sheet.TotalOwedGrossPay = sheet.UnPaidRegTotalPay + sheet.UnPaidTotalOTPay + sheet.UnPaidTotalOT2Pay;

            return sheet.TotalOwedGrossPay;
        }
    }
}
