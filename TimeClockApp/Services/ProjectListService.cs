using System.Data;
using Microsoft.EntityFrameworkCore;

namespace TimeClockApp.Services
{
	/// <summary>
	/// Provides methods for retrieving project and phase lists.
	/// </summary>
	public class ProjectListService : SQLiteDataStore, IProjectListService
	{
		/// <summary>
		/// Retrieves a list of active projects.
		/// </summary>
		/// <returns>A list of active projects.</returns>
		public Task<List<Project>> GetProjectsListAsync() =>
			Context.Project
				.Where(item => item.Status < ProjectStatus.Completed)
				.OrderBy(e => e.Name)
				.ToListAsync();

		/// <summary>
		/// Retrieves a list of phases.
		/// </summary>
		/// <returns>A list of phases.</returns>
		public Task<List<Phase>> GetPhaseListAsync() =>
			Context.Phase
				.OrderBy(e => e.PhaseTitle)
				.ToListAsync();
	}
}
