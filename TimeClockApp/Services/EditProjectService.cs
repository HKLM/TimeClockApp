using Microsoft.EntityFrameworkCore;

namespace TimeClockApp.Services
{
    public partial class EditProjectService : TimeCardDataStore
    {
        public static string GetProjectNameFromId(int projectId)
        {
            if (projectId == 1) return ".None";

            return Context.Project
            .AsNoTracking()
            .Where(p => p.ProjectId == projectId)
            .Select(p => p.Name)
            .First();
        }

        /// <summary>
        /// 
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
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                App.AlertSvc.ShowAlert("Exception", ex.Message + "\n" + ex.InnerException, "ERROR");
            }
            return bReplaceExisting;
        }

        public bool UpdateProject(string name, int id, DateOnly date, ProjectStatus status)
        {
            if (id < 2 || name == null || name == "" || Context.Project.Any(x => x.ProjectId != id && x.Name.Contains(name)))
                return false;
            try
            {
                Project item = GetProject(id);
                if (item != null)
                {
                    item.Name = name;
                    item.Status = status;
                    item.ProjectDate = date;
                    Context.Update<Project>(item);
                    return (Context.SaveChanges() > 0);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
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
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
            return false;
        }
    }
}
