namespace TimeClockApp.Services
{
	/// <summary>
	/// Inheritable methods for working with projects.
	/// </summary>
	public class EditParentService : TimeCardDataStore
	{
		/// <summary>
		/// Gets a project by its ID.
		/// </summary>
		/// <param name="projectID">The ID of the project to retrieve.</param>
		/// <returns>The project with the specified ID, or null if not found.</returns>
		public Project GetProject(int projectID) => Context.Project.Find(projectID);

		/// <summary>
		/// Gets a phase by its ID.
		/// </summary>
		/// <param name="phaseID">The ID of the phase to retrieve.</param>
		/// <returns>The phase with the specified ID, or null if not found.</returns>
		public Phase GetPhase(int phaseID) => Context.Phase.Find(phaseID);

		/// <summary>
		/// Gets a list of all projects based on the show all flag.
		/// </summary>
		/// <param name="bShowAll">Indicates whether to show all projects.</param>
		/// <returns>The list of projects.</returns>
		public ObservableCollection<Project> GetAllProjectsList(bool bShowAll) =>
			(bShowAll) ? Get_AllProjectsList() : Get_NotAllProjectsList();

		private ObservableCollection<Project> Get_AllProjectsList() =>
			Context.Project
				.Where(item => item.Status != ProjectStatus.Deleted)
				.OrderBy(e => e.Name)
				.ToObservableCollection();
		private ObservableCollection<Project> Get_NotAllProjectsList() =>
			Context.Project
				.Where(item => item.Status < ProjectStatus.Completed)
				.OrderBy(e => e.Name)
				.ToObservableCollection();
	}
}
