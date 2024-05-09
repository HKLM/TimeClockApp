namespace TimeClockApp.ViewModels
{
    public partial class EditProjectViewModel : TimeStampViewModel
    {
        protected EditProjectService projectService;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnableSaveButton))]
        private int projectId = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnableAddDelButtons))]
        private string name;
#region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private readonly DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
#endregion

        [ObservableProperty]
        private DateOnly projectDate;

        [ObservableProperty]
        private ObservableCollection<Project> projectList = [];

        [ObservableProperty]
        private Project selectedProject;
        partial void OnSelectedProjectChanged(global::TimeClockApp.Shared.Models.Project value)
        {
            if (value != null)
            {
                ProjectId = value.ProjectId;
                Name = value.Name;
                ProjectDate = value.ProjectDate;
                Project_Status = value.Status;
            }
        }

        [ObservableProperty]
        private ProjectStatus project_Status = ProjectStatus.Active;
        public IReadOnlyList<string> AllProjectStatus { get; } = Enum.GetNames(typeof(ProjectStatus));

        /// <summary>
        /// Show all projects or the default, active projects
        /// </summary>
        [ObservableProperty]
        private bool showAll = false;
        partial void OnShowAllChanged(bool value)
        {
            LoadProjects();
        }

        public bool EnableAddDelButtons => !string.IsNullOrEmpty(Name);
        public bool EnableSaveButton => ProjectId > 0;

        public EditProjectViewModel()
        {
            projectService = new();
        }

        public void OnAppearing()
        {
            PickerMinDate = projectService.GetAppFirstRunDate();
            ProjectDate = DateOnly.FromDateTime(DateTime.Now);
            LoadProjects();
        }

        private void LoadProjects()
        {
            if (ProjectList.Any())
                ProjectList.Clear();

            ProjectList = projectService.GetAllProjectsList(ShowAll);
        }

        [RelayCommand]
        private void SaveNewProject()
        {
            try
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    if (projectService.AddNewProject(Name))
                        App.AlertSvc.ShowAlert("Notice", Name + " saved.\nExisting project of same name has been archived.");
                    else
                        App.AlertSvc.ShowAlert("Notice", Name + " saved");

                    App.NoticeProjectHasChanged = true;
                    LoadProjects();
                    Name = string.Empty;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void DeleteProject()
        {
            try
            {
                if (SelectedProject?.ProjectId > 1)
                {
                    string oldProject = SelectedProject.Name;
                    projectService.DeleteProject(SelectedProject);
                    App.NoticeProjectHasChanged = true;
                    LoadProjects();
                    Name = string.Empty;
                    App.AlertSvc.ShowAlert("Notice", oldProject + " Deleted");
                }
                else if (ProjectId == 1)
                    App.AlertSvc.ShowAlert("Notice", "This project can not be Deleted.");
                else
                    App.AlertSvc.ShowAlert("Notice", "You must select a Project before it can be updated");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void SaveEditProject()
        {
            try
            {
                if (!string.IsNullOrEmpty(Name) && ProjectId > 1)
                {
                    projectService.UpdateProject(Name, ProjectId, ProjectDate, Project_Status);
                    App.NoticeProjectHasChanged = true;
                    LoadProjects();
                    Name = string.Empty;
                    App.AlertSvc.ShowAlert("Notice", Name + " saved");
                }
                else if (ProjectId == 1)
                    App.AlertSvc.ShowAlert("Notice", "Can not edit this project. It is a Read Only project.");
                else
                    App.AlertSvc.ShowAlert("Notice", "You must select a Project before it can be updated");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
