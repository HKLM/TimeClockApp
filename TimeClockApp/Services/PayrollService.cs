using System.Data;

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
        public Task<double> GetWCRateAsync() => GetRateAsync(5);

        public double GetRate(int i)
        {
            Config? c = Context.Config.Find(i);
            return double.TryParse(c?.StringValue, out double wc) ? (double)wc : 0;
        }

        public async Task<double> GetRateAsync(int i)
        {
            if (i == 7 || i == 8)
                return await FixGetRateAsync(i);

            Config? c = Context.Config.Find(i);
            return double.TryParse(c?.StringValue, out double wc) ? (double)wc : 0;
        }

        //TODO: Remove this method after a few versions to clean up old int values.
        public async Task<double> FixGetRateAsync(int i)
        {
            Config? c = Context.Config.Find(i);
            if (c != null && c.IntValue.HasValue)
            {
                var ic = c.IntValue.Value;
                c.StringValue = ic.ToString();
                c.IntValue = null;
                Context.Config.Update(c);
                await Context.SaveChangesAsync().ConfigureAwait(false);
                return (double)ic;
            }
            else return double.TryParse(c?.StringValue, out double wc) ? (double)wc : 0;
        }

        private void CalculateWages(TimeSheet sheet, double payRate)
        {
            sheet.RegTotalPay = sheet.RegTotalHours * payRate;
            sheet.TotalOTPay = sheet.TotalOTHours * (payRate * 1.5);
            sheet.TotalOT2Pay = sheet.TotalOT2Hours * (payRate * 2);
            sheet.TotalGrossPay = sheet.RegTotalPay + sheet.TotalOTPay + sheet.TotalOT2Pay;
        }

        private void CalculateUnpaidWages(TimeSheet sheet, double payRate)
        {
            sheet.UnPaidRegTotalPay = sheet.UnPaidRegTotalHours * payRate;
            sheet.UnPaidTotalOTPay = sheet.UnPaidTotalOTHours * (payRate * 1.5);
            sheet.UnPaidTotalOT2Pay = sheet.UnPaidTotalOT2Hours * (payRate * 2);
            sheet.TotalOwedGrossPay = sheet.UnPaidRegTotalPay + sheet.UnPaidTotalOTPay + sheet.UnPaidTotalOT2Pay;
        }

        private void CategorizeAndSummarizeTimeCards(IGrouping<DateOnly, TimeCard> dayGroup, TimeSheet sheet, out double payRate)
        {
            payRate = dayGroup.First().TimeCard_EmployeePayRate;

            foreach (TimeCard item in dayGroup)
            {
                if (item.TimeCard_Status == ShiftStatus.ClockedOut)
                {
                    sheet.UnpaidTimeCards.Add(item);
                }
                else if (item.TimeCard_Status == ShiftStatus.Paid)
                {
                    sheet.PaidTimeCards.Add(item);
                }
            }

            List<TimeCard> paidAndClockedOut = dayGroup.Where(x => x.TimeCard_Status == ShiftStatus.ClockedOut || x.TimeCard_Status == ShiftStatus.Paid).ToList();
            TimeCardDayTotal dayHours = new(paidAndClockedOut.Sum(x => x.TimeCard_WorkHours));

            sheet.TotalWorkHours += dayHours.TotalWorkHours;
            sheet.RegTotalHours += dayHours.RegTotalHours;
            sheet.TotalOTHours += dayHours.TotalOTHours;
            sheet.TotalOT2Hours += dayHours.TotalOT2Hours;

            List<TimeCard> clockedOutCards = dayGroup.Where(x => x.TimeCard_Status == ShiftStatus.ClockedOut).ToList();
            double unpaidHours = clockedOutCards.Sum(x => x.TimeCard_WorkHours);

            if (unpaidHours > 0)
            {
                TimeCardDayTotal unpaidDayHours = new(unpaidHours);
                sheet.UnPaidTotalWorkHours += unpaidDayHours.TotalWorkHours;
                sheet.UnPaidRegTotalHours += unpaidDayHours.RegTotalHours;
                sheet.UnPaidTotalOTHours += unpaidDayHours.TotalOTHours;
                sheet.UnPaidTotalOT2Hours += unpaidDayHours.TotalOT2Hours;
            }
        }

        public async Task<TimeSheet> GetPayrollTimeSheetForEmployeeAsync(int employeeId, DateOnly start, DateOnly end, string employeeName, TimeSheet? sheet = null, bool onlyUnpaid = false)
        {
            sheet ??= new TimeSheet(employeeId, start, end, employeeName);
            sheet.Reset();

            sheet.TimeCards = await GetTimeCardsForPayPeriodAsync(employeeId, start, end, onlyUnpaid);
            ProcessTimeSheetPayroll(sheet);

            return sheet;
        }

        private void ProcessTimeSheetPayroll(TimeSheet sheet)
        {
            if (sheet.TimeCards?.Count == 0)
                return;

            double payRate = 0;
            foreach (var dayGroup in sheet.TimeCards!.GroupBy(x => x.TimeCard_Date))
            {
                CategorizeAndSummarizeTimeCards(dayGroup, sheet, out payRate);
            }

            CalculateWages(sheet, payRate);
            CalculateUnpaidWages(sheet, payRate);
        }

        public double GetPayrollInfoForExpense(List<TimeCard> cards)
        {
            if (cards == null || cards.Count == 0 || cards[0] == null || cards[0].EmployeeId <= 0 || string.IsNullOrEmpty(cards[0].TimeCard_EmployeeName))
                return 0;

            TimeSheet sheet = new TimeSheet(cards[0].EmployeeId, DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now), cards[0].TimeCard_EmployeeName);
            sheet.TimeCards = cards;

            if (sheet.TimeCards.Count == 0)
                return 0;

            double payRate = 0;
            foreach (var dayGroup in sheet.TimeCards.GroupBy(x => x.TimeCard_Date))
            {
                payRate = dayGroup.First().TimeCard_EmployeePayRate;

                List<TimeCard> clockedOutCards = dayGroup.Where(x => x.TimeCard_Status == ShiftStatus.ClockedOut).ToList();
                foreach (var item in clockedOutCards)
                {
                    sheet.UnpaidTimeCards.Add(item);
                }

                double unpaidHours = clockedOutCards.Sum(x => x.TimeCard_WorkHours);

                if (unpaidHours > 0)
                {
                    TimeCardDayTotal unpaidDayHours = new(unpaidHours);
                    sheet.UnPaidTotalWorkHours += unpaidDayHours.TotalWorkHours;
                    sheet.UnPaidRegTotalHours += unpaidDayHours.RegTotalHours;
                    sheet.UnPaidTotalOTHours += unpaidDayHours.TotalOTHours;
                    sheet.UnPaidTotalOT2Hours += unpaidDayHours.TotalOT2Hours;
                }
            }

            CalculateUnpaidWages(sheet, payRate);
            return sheet.TotalOwedGrossPay;
        }
    }
}
