namespace TimeClockApp.ViewModels
{
    public partial class EntityMonitor  : ObservableObject
    {
        [ObservableProperty]
        private bool noticeProjectHasChanged = false;
        [ObservableProperty]
        private bool noticePhaseHasChanged = false;
        [ObservableProperty]
        private bool noticeUserHasChanged = false;
    }
}
