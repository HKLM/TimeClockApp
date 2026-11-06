using System.ComponentModel;
using CsvHelper;
#nullable enable

namespace TimeClockApp.Shared.Models
{
	public enum ProjectStatus
	{
		[Description("Ready")]
		Ready = 0,
		[Description("Active")]
		Active = 1,
		[Description("Completed")]
		Completed = 2,
		[Description("Archived")]
		Archived = 3,
		[Description("Deleted")]
		Deleted = 4
	}

	/// <summary>
	/// Projects are a way to group together TimeCards, Wages and Expenses. From this Project totals can be calculated
	/// TotalWages, TotalExpenses, TotalHours, etc. After a Project has been completed, it can be set tto [Archived].
	/// This will remove that Project from the active ProjectLists, yet keep that Project saved. Another Project of the
	/// same name can be created. Just be sure to have only 1 of them be set to [Active] at one time. If 2+ of same name
	/// are [Active], it would be difficult for the user to determine which Project they actually want.
	/// </summary>
	[CsvRecord]
	public partial class Project
	{
		public Project()
		{
			Status = ProjectStatus.Active;
			ProjectDate = DateOnly.FromDateTime(DateTime.Now);
			ProjectDateEnd = null;
		}

		public Project(string Name)
		{
			this.Name = Name;
			Status = ProjectStatus.Active;
			ProjectDate = DateOnly.FromDateTime(DateTime.Now);
			ProjectDateEnd = null;
		}

		[CsvConstructor]
		public Project(int projectId, string name, ProjectStatus status, DateOnly projectDate, DateOnly? projectDateEnd = null)
		{
			ProjectId = projectId;
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Status = status;
			ProjectDate = projectDate;
			ProjectDateEnd = projectDateEnd;
		}

		public int ProjectId { get; set; }

		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Used to filter out older (archived, completed, deleted, etc) projects
		/// </summary>
		public ProjectStatus Status { get; set; }

		/// <summary>
		/// Date of project creation.
		/// </summary>
		/// <remarks>Used mainly for filtering out items that come after this projects date</remarks>
		public DateOnly ProjectDate { get; set; }


		/// <summary>
		/// Date of project completion.
		/// </summary>
		[CsvColumn(Optional = true)]
		public DateOnly? ProjectDateEnd { get; set; } = null;

		[CsvIgnore]
		public virtual ICollection<TimeCard> TimeCards { get; set; } = new List<TimeCard>();
		[CsvIgnore]
		public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
	}
}
