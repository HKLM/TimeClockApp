using System.Data;
using TimeClockApp.Shared;
using Microsoft.EntityFrameworkCore;

#nullable enable

namespace TimeClockApp.Services
{
	public class PayrollService : TimeCardService
	{
		/// <summary>
		/// Gets the time cards for a specific pay period.
		/// </summary>
		/// <param name="employeeId"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="onlyUnpaid">false=all cards, true=only unpaid cards</param>
		/// <returns></returns>
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
						&& item.TimeCard_Status != ShiftStatus.Deleted
						&& item.TimeCard_Date >= start
						&& item.TimeCard_Date <= end)
					.OrderBy(item => item.TimeCard_Date)
					.ToListAsync();

		/// <summary>
		/// Gets the list of employed employees.
		/// </summary>
		/// <returns></returns>
		public virtual Task<List<Employee>> GetEmployeeListAsync() =>
				Context.Employee
					.Where(e => e.Employee_Employed == EmploymentStatus.Employed)
					.OrderBy(e => e.Employee_Name)
					.ToListAsync();

		public Employee GetEmployee(int employeeID) => Context.Employee.Find(employeeID)!;

		/// <summary>
		/// Gets the WorkersCompensation rate value.
		/// </summary>
		/// <returns></returns>
		public double GetWCRate() => GetRate(5);

		/// <summary>
		/// Gets the rate for a specific configuration ID.
		/// </summary>
		/// <param name="i">5=Workers Compensation, 7=Profit, 8=Overhead</param>
		/// <returns></returns>
		public double GetRate(int i)
		{
			if (i == 5 && Preferences.ContainsKey("WorkCompRate", PrefSetting.PrefKey))
			{
				return Preferences.Get("WorkCompRate", 0.171118, PrefSetting.PrefKey);
			}
			else if (i == 7 && Preferences.ContainsKey("ProfitRate", PrefSetting.PrefKey))
			{
				return Preferences.Get("ProfitRate", 0.1, PrefSetting.PrefKey);
			}
			else if (i == 8 && Preferences.ContainsKey("OverHeadRate", PrefSetting.PrefKey))
			{
				return Preferences.Get("OverHeadRate", 0.08, PrefSetting.PrefKey);
			}
			else
			{
				Config? c = Context.Config.Find(i);
				return double.TryParse(c?.StringValue, out double wc) ? (double)wc : 0;
			}
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
			//TODO use pay rate for that rate on the timecard, not the first one in the group.  This is a temporary fix until we can implement a better solution for pay rates.
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

		/// <summary>
		/// Gets the payroll time sheet for a specific employee.
		/// </summary>
		/// <param name="employeeId"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="employeeName"></param>
		/// <param name="sheet"></param>
		/// <param name="onlyUnpaid"></param>
		/// <returns></returns>
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
