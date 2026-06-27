using System.Data;
using Microsoft.EntityFrameworkCore;
using TimeClockApp.Shared.Helpers;

#nullable enable

namespace TimeClockApp.Services
{
	public class TimeCardService : TimeCardDataStore
	{
#if WINDOWS
		[System.Diagnostics.CodeAnalysis.RequiresDynamicCode("Warning from using 'System.Linq.Queryable.AsQueryable<TElement>(IEnumerable<TElement>)' which has 'RequiresDynamicCodeAttribute'")]
		[System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Warning from using 'System.Linq.Queryable.AsQueryable<TElement>(IEnumerable<TElement>)' which has 'RequiresUnreferencedCodeAttribute'")]
#endif
		public Task<List<TimeCard>> GetLastTimeCardForAllEmployeesAsync()
		{
#if WINDOWS
#pragma warning disable IL3050
#pragma warning disable IL2026
#endif
			// Get all employed employees with their most recent unpaid timecards in a single query
			var employeesWithCards = Context.Employee
				.Where(e => e.Employee_Employed == EmploymentStatus.Employed)
				.OrderBy(e => e.Employee_Name)
				.Select(e => new
				{
					Employee = e,
					LastTimeCard = Context.TimeCard
						.Where(tc => tc.EmployeeId == e.EmployeeId
							&& tc.TimeCard_Status < ShiftStatus.Paid
							&& !tc.TimeCard_bReadOnly)
						.OrderByDescending(tc => tc.TimeCard_DateTime)
						.FirstOrDefault()
				})
				.AsQueryable();

			return employeesWithCards
				.Select(item => item.LastTimeCard ?? new TimeCard(item.Employee))
				.ToListAsync();
#if WINDOWS
#pragma warning restore IL3050
#pragma warning restore IL2026
#endif

		}

		public async Task<bool> EmployeeClockInAsync(TimeCard card, int projectID, int phaseID, string projectName, string phaseTitle)
		{
			if (card.Employee == null || card.EmployeeId == 0 || card.Employee.Employee_Employed != EmploymentStatus.Employed)
				return false;

			if (IsEmployeeNotOnTheClock(card.EmployeeId))
			{
				DateTime entry = DateTime.Now;
				TimeOnly sTime = TimeHelper.RoundTimeOnly(new TimeOnly(entry.Hour, entry.Minute));
				TimeCard c = new(card.Employee, projectID, phaseID, projectName, phaseTitle, sTime);

				Context.Add<TimeCard>(c);
				await Context.SaveChangesAsync().ConfigureAwait(false);
				return true;
			}
			return false;
		}

		public async Task<bool> EmployeeClockOutAsync(int timeCardId)
		{
			if (timeCardId > 0)
			{
				TimeCard t = GetTimeCard(timeCardId);
				if (await ValidateClockOutAsync(t))
				{
					t.TimeCard_EndTime = TimeHelper.RoundTimeOnly(new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute));
					t.TimeCard_Status = ShiftStatus.ClockedOut;
					Context.Update<TimeCard>(t);
					await Context.SaveChangesAsync().ConfigureAwait(false);
					return true;
				}
			}
			return false;
		}

		private bool IsEmployeeNotOnTheClock(int employeeID) => !Context.TimeCard
			.AsNoTracking()
			.Where(tc => tc.EmployeeId == employeeID
				&& tc.TimeCard_Status == ShiftStatus.ClockedIn
				&& !tc.TimeCard_bReadOnly)
			.Any();

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
					Context.Update<TimeCard>(t);
					await Context.SaveChangesAsync().ConfigureAwait(false);
					return true;
				}
			}
			return false;
		}
	}
}
