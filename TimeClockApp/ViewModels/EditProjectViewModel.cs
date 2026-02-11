namespace TimeClockApp.ViewModels
{
    public partial class EditProjectViewModel : BaseViewModel
    {
        protected readonly EditProjectService projectService;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnableSaveButton))]
        public partial int ProjectId { get; set; } = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnableAddDelButtons))]
        public partial string Name { get; set; }
        #region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private readonly DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
#endregion

        [ObservableProperty]
        public partial DateOnly ProjectDate { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<Project> ProjectList { get; set; } = [];

        [ObservableProperty]
        public partial Project SelectedProject { get; set; }
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
        public partial ProjectStatus Project_Status { get; set; } = ProjectStatus.Active;
        public IReadOnlyList<string> AllProjectStatus { get; } = Enum.GetNames(typeof(ProjectStatus));

        /// <summary>
        /// Show all projects or the default, active projects
        /// </summary>
        [ObservableProperty]
        public partial bool ShowAll { get; set; } = false;
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
                    string projectNewName = Name.Trim();
                    if (projectService.AddNewProject(projectNewName))
                        App.AlertSvc!.ShowAlert("Notice", projectNewName + " saved.\nExisting project of same name has been archived.");
                    else
                        App.AlertSvc!.ShowAlert("Notice", projectNewName + " saved");

                    LoadProjects();
                    Name = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
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
                    LoadProjects();
                    Name = string.Empty;
                    App.AlertSvc!.ShowAlert("Notice", oldProject + " Deleted");
                }
                else if (ProjectId == 1)
                    App.AlertSvc!.ShowAlert("Notice", "This project can not be Deleted.");
                else
                    App.AlertSvc!.ShowAlert("Notice", "You must select a Project before it can be updated");
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void SaveEditProject()
        {
            try
            {
                if (!string.IsNullOrEmpty(Name) && ProjectId > 1)
                {
                    string projectNewName = Name.Trim();
                    projectService.UpdateProject(projectNewName, ProjectId, ProjectDate, Project_Status);
                    LoadProjects();
                    App.AlertSvc!.ShowAlert("Notice", projectNewName + " saved");
                    Name = string.Empty;
                }
                else if (ProjectId == 1)
                    App.AlertSvc!.ShowAlert("Notice", "Can not edit this project. It is a Read Only project.");
                else
                    App.AlertSvc!.ShowAlert("Notice", "You must select a Project before it can be updated");
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
