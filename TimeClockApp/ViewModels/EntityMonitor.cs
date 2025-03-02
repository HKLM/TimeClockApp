namespace TimeClockApp.ViewModels
{
    public partial class EntityMonitor  : ObservableObject
    {
        [ObservableProperty]
        public partial bool NoticeProjectHasChanged { get; set; } = false;
        [ObservableProperty]
        public partial bool NoticePhaseHasChanged { get; set; } = false;
        [ObservableProperty]
        public partial bool NoticeUserHasChanged { get; set; } = false;
    }
}
