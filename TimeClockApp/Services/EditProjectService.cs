namespace TimeClockApp.Services
{
	public class EditProjectService : EditParentService
	{
		/// <summary>
		/// Add new project
		/// </summary>
		/// <param name="projectName"></param>
		/// <returns>Returns true if replacing existing Project of same name.</returns>
		public bool AddNewProject(string projectName)
		{
			if (projectName == null)
				return false;
			bool bReplaceExisting = false;
			try
			{
				//Archive old projects
				if (Context.Project.Any(x => x.Name.Contains(projectName)))
				{
					var x = Context.Project
						.Where(x => x.Name
						.Contains(projectName));
					foreach (var item in x)
					{
						if (item.Status < ProjectStatus.Archived)
						{
							item.Status = ProjectStatus.Archived;
							item.ProjectDateEnd = DateOnly.FromDateTime(DateTime.Now);
							bReplaceExisting = true;
						}
					}
				}

				Project p = new(projectName);
				Context.Add<Project>(p);
				Context.SaveChanges();
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.Message + "\n" + ex.InnerException);
				App.AlertSvc!.ShowAlert("Exception", ex.Message + "\n" + ex.InnerException, "ERROR");
			}
			return bReplaceExisting;
		}

		public bool UpdateProject(string name, int id, DateOnly date, ProjectStatus status, DateOnly? endDate = null)
		{
			if (id < 2 || name == null || name?.Length == 0 || Context.Project.Any(x => x.ProjectId != id && x.Name.Contains(name)))
				return false;
			try
			{
				Project item = GetProject(id);
				if (item != null)
				{
					// Don't allow editing of archived projects
					if (item.Status == ProjectStatus.Archived)
						return false;

					item.Name = name;
					item.ProjectDate = date;

					if (item.Status != status && status == ProjectStatus.Completed)
					{
						item.Status = ProjectStatus.Completed;
						item.ProjectDateEnd = DateOnly.FromDateTime(DateTime.Now);
					}
					else if (item.Status != status && status == ProjectStatus.Archived)
					{
						item.Status = ProjectStatus.Archived;
						if (item.ProjectDateEnd.HasValue == false)
							item.ProjectDateEnd = DateOnly.FromDateTime(DateTime.Now);
					}
					else if (status < ProjectStatus.Completed)
					{
						item.Status = status;
						item.ProjectDateEnd = null;
					}
					else if (status == ProjectStatus.Completed)
					{
						item.Status = status;
						item.ProjectDateEnd = endDate;
					}

					//item.ProjectDateEnd = endDate;
					Context.Update<Project>(item);
					return (Context.SaveChanges() > 0);
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.Message + "\n" + ex.InnerException);
			}
			return false;
		}

		public bool DeleteProject(Project item)
		{
			if (item == null)
				return false;
			try
			{
				Context.Remove<Project>(item);
				return (Context.SaveChanges() > 0);
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.Message + "\n" + ex.InnerException);
			}
			return false;
		}
	}
}
