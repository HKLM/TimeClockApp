using System.Data;
using Microsoft.EntityFrameworkCore;
using TimeClockApp.Shared.Helpers;

#nullable enable

namespace TimeClockApp.Services
{
	/// <summary>
	/// Provides services for managing time cards.
	/// </summary>
	public class TimeCardService : TimeCardDataStore
	{
		/// <summary>
		/// Gets the last time card for all employed employees asynchronously.
		/// </summary>
		/// <returns>List of time cards</returns>
		public Task<List<TimeCard>> GetLastTimeCardForAllEmployeesAsync()
		{
			// Get all employed employees with their most recent unpaid timecards in a single query
			// Using GroupJoin to avoid N+1 query problem
			return Context.Employee
				.Where(e => e.Employee_Employed == EmploymentStatus.Employed)
				.OrderBy(e => e.Employee_Name)
				.GroupJoin(
					Context.TimeCard
						.Where(tc => tc.TimeCard_Status < ShiftStatus.Paid && !tc.TimeCard_bReadOnly),
					e => e.EmployeeId,
					tc => tc.EmployeeId,
					(employee, timeCards) => new
					{
						Employee = employee,
						LastTimeCard = timeCards.OrderByDescending(tc => tc.TimeCard_DateTime).FirstOrDefault()
					})
				.Select(item => item.LastTimeCard ?? new TimeCard(item.Employee))
				.ToListAsync();
		}

		/// <summary>
		/// Clocks in an employee asynchronously.
		/// </summary>
		/// <param name="card"></param>
		/// <param name="projectID"></param>
		/// <param name="phaseID"></param>
		/// <param name="projectName"></param>
		/// <param name="phaseTitle"></param>
		/// <returns>true if the employee was successfully clocked in; otherwise, false</returns>
		public async Task<bool> EmployeeClockInAsync(TimeCard card, int projectID, int phaseID, string projectName, string phaseTitle)
		{
			if (card.Employee == null || card.EmployeeId == 0 || card.Employee.Employee_Employed != EmploymentStatus.Employed)
				return false;

			if (IsEmployeeNotOnTheClock(card.EmployeeId))
			{
				DateTime now = DateTime.Now;
				TimeOnly sTime = TimeHelper.RoundTimeOnly(new TimeOnly(now.Hour, now.Minute));
				TimeCard c = new(card.Employee, projectID, phaseID, projectName, phaseTitle, sTime);

				Context.Add(c);
				await Context.SaveChangesAsync().ConfigureAwait(false);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Clocks out an employee asynchronously.
		/// </summary>
		/// <param name="timeCardId"></param>
		/// <returns>true if the employee was successfully clocked out; otherwise, false</returns>
		public async Task<bool> EmployeeClockOutAsync(int timeCardId)
		{
			if (timeCardId > 0)
			{
				TimeCard t = GetTimeCard(timeCardId);
				if (await ValidateClockOutAsync(t))
				{
					DateTime now = DateTime.Now;
					t.TimeCard_EndTime = TimeHelper.RoundTimeOnly(new TimeOnly(now.Hour, now.Minute));
					t.TimeCard_Status = ShiftStatus.ClockedOut;
					Context.Update(t);
					await Context.SaveChangesAsync().ConfigureAwait(false);
					return true;
				}
			}
			return false;
		}

		private bool IsEmployeeNotOnTheClock(int employeeID) => !Context.TimeCard
			.AsNoTracking()
			.Any(tc => tc.EmployeeId == employeeID
				&& tc.TimeCard_Status == ShiftStatus.ClockedIn
				&& !tc.TimeCard_bReadOnly);

		/// <summary>
		/// Marks a time card as paid asynchronously.
		/// </summary>
		/// <param name="timeCard"></param>
		/// <returns></returns>
		public async Task<bool> MarkTimeCardAsPaidAsync(TimeCard timeCard)
		{
			if (timeCard != null && timeCard.TimeCardId != 0
				&& !IsTimeCardReadOnly(timeCard.TimeCardId)
				&& timeCard.TimeCard_Status == ShiftStatus.ClockedOut)
			{
				TimeCard? t = Context.TimeCard.Find(timeCard.TimeCardId);
				if (t != null)
				{
					t.TimeCard_Status = ShiftStatus.Paid;
					t.TimeCard_bReadOnly = true;
					Context.Update(t);
					await Context.SaveChangesAsync().ConfigureAwait(false);
					return true;
				}
			}
			return false;
		}
	}
}
