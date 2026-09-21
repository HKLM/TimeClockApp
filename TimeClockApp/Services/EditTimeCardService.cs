using System.Data;
using Microsoft.EntityFrameworkCore;
using TimeClockApp.Shared.Helpers;

#nullable enable
namespace TimeClockApp.Services
{
	/// <summary>
	/// Provides services for editing time cards.
	/// </summary>
	public class EditTimeCardService : TimeCardDataStore
	{
		/// <summary>
		/// Gets a list of time cards asynchronously.
		/// </summary>
		/// <param name="NumResults">The number of results to return.</param>
		/// <returns>The list of time cards</returns>
		public Task<List<TimeCard>> GetTimeCards(int NumResults) =>
			Context.TimeCard
				.AsNoTracking()
				.OrderByDescending(item => item.TimeCard_DateTime)
				.Take(NumResults)
				.ToListAsync();

		/// <summary>
		/// Gets a list of time cards asynchronously within a date range.
		/// </summary>
		/// <param name="NumResults">The number of results to return.</param>
		/// <param name="start">The start date.</param>
		/// <param name="end">The end date.</param>
		/// <returns>The list of time cards</returns>
		public Task<List<TimeCard>> GetTimeCards(int NumResults, DateOnly? start, DateOnly? end) =>
			Context.TimeCard
				.AsNoTracking()
				.Where(item =>
						item.TimeCard_Date >= start!.Value
					 && item.TimeCard_Date <= end!.Value)
				.OrderByDescending(item => item.TimeCard_DateTime)
				.Take(NumResults)
				.ToListAsync();

		// Copy from TimeCardService for use in EditMultiTimeCardsViewModel
		/// <summary>
		/// Gets the last time card for all employed employees asynchronously.
		/// </summary>
		/// <returns>The list of last time cards</returns>
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
		/// Updates a time card asynchronously.
		/// </summary>
		/// <param name="newTimeCard">The updated time card.</param>
		/// <param name="isAdmin">Indicates whether the user is an administrator.</param>
		/// <param name="bChangedDate">Indicates whether the date was changed.</param>
		/// <returns>true if the time card was updated successfully; otherwise, false</returns>
		public async Task<bool> UpdateTimeCardAsync(TimeCard newTimeCard, bool isAdmin = false, bool bChangedDate = false)
		{
			if (newTimeCard == null)
				return false;

			if (IsTimeCardReadOnly(newTimeCard.TimeCardId, isAdmin))
				return false;

			TimeCard? origTimeCard = Context.TimeCard.Find(newTimeCard.TimeCardId);
			if (origTimeCard != null)
			{
				origTimeCard.PhaseId = newTimeCard.PhaseId;
				origTimeCard.PhaseTitle = newTimeCard.PhaseTitle;
				origTimeCard.ProjectId = newTimeCard.ProjectId;
				origTimeCard.ProjectName = newTimeCard.ProjectName;
				origTimeCard.TimeCard_Status = newTimeCard.TimeCard_Status;
				origTimeCard.TimeCard_bReadOnly = newTimeCard.TimeCard_bReadOnly;
				origTimeCard.TimeCard_StartTime = TimeHelper.RoundTimeOnly(new TimeOnly(newTimeCard.TimeCard_StartTime.Hour, newTimeCard.TimeCard_StartTime.Minute));
				origTimeCard.TimeCard_EndTime = TimeHelper.RoundTimeOnly(new TimeOnly(newTimeCard.TimeCard_EndTime.Hour, newTimeCard.TimeCard_EndTime.Minute));

				DateOnly newDate = newTimeCard.TimeCard_Date;
				if (bChangedDate)
				{
					origTimeCard.TimeCard_Date = newDate;
					if (DateOnly.FromDateTime(origTimeCard.TimeCard_DateTime) != newDate)
						origTimeCard.TimeCard_DateTime = new DateTime(newTimeCard.TimeCard_Date.Year, newTimeCard.TimeCard_Date.Month, newTimeCard.TimeCard_Date.Day, newTimeCard.TimeCard_DateTime.Hour, newTimeCard.TimeCard_DateTime.Minute, newTimeCard.TimeCard_DateTime.Second);
				}

				Context.Update<TimeCard>(origTimeCard);
				if (await Context.SaveChangesAsync().ConfigureAwait(false) > 0)
				{
					return true;
				}
			}
			return false;
		}

		// Used in EditMultiTimeCardsViewModel to save multiple timecards at once
		/// <summary>
		/// Updates a list of time cards asynchronously.
		/// </summary>
		/// <param name="newTimeCards">The list of updated time cards.</param>
		/// <returns>true if the time cards were updated successfully; otherwise, false</returns>
		public async Task<bool> UpdateTimeCardListAsync(List<TimeCard> newTimeCards)
		{
			if (newTimeCards == null || newTimeCards.Count == 0)
				return false;

			foreach (TimeCard newTimeCard in newTimeCards)
			{
				Context.Update(newTimeCard);
			}
			return await Context.SaveChangesAsync() > 0;
		}
	}
}
