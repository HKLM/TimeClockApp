#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class TimeStampViewModel : BaseViewModel
    {
        public bool initDone = false;

        /// <summary>
        /// The ToolBar Help Icon toggles displaying the HelpInfoBox for each page
        /// </summary>
        [ObservableProperty]
        private bool helpInfoBoxVisible = false;

        [ObservableProperty]
        private string? title = string.Empty;

        /// <summary>
        /// Used to display any errors to the user
        /// </summary>
        [ObservableProperty]
        private string? errorMsg = string.Empty;

        /// <summary>
        /// If true allows editing locked TimeCards. By default after a TimeCard has been marked paid, it is locked by being set to ReadOnly.
        /// </summary>
        [ObservableProperty]
        private bool isAdmin = false;

#region "InitAsync Properties"
        [ObservableProperty]
        private bool loading = true;
        [ObservableProperty]
        private bool hasError = false;
#endregion

        public TimeStampViewModel() { }

        /// <summary>
        /// Converts Int to bool.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>true if 1</returns>
        public static bool IntToBool(int? value) => value.HasValue && value.Value is int @int && @int == 1;
    }
}
