namespace TimeClockApp.Services
{
	/// <summary>
	/// Provides methods for retrieving project and phase lists.
	/// </summary>
	public interface IProjectListService
	{
		Task<List<Phase>> GetPhaseListAsync();
		Task<List<Project>> GetProjectsListAsync();
	}
}