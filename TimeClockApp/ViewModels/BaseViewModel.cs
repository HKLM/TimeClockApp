#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        public bool initDone = false;

        /// <summary>
        /// The ToolBar Help Icon toggles displaying the HelpInfoBox for each page
        /// </summary>
        [ObservableProperty]
        public partial bool HelpInfoBoxVisible { get; set; } = false;

        /// <summary>
        /// The ToolBar Icon toggles displaying the OptionsBox for the page
        /// </summary>
        [ObservableProperty]
        public partial bool OptionsBoxVisible { get; set; } = false;

        /// <summary>
        /// Page title bindable property
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PageTitle))]
        public partial string? Title { get; set; } = string.Empty;
        /// <summary>
        /// Displays the page Title value
        /// </summary>
        public virtual string? PageTitle => Title;

        /// <summary>
        /// Used to display any errors to the user
        /// </summary>
        [ObservableProperty]
        public partial string? ErrorMsg { get; set; } = string.Empty;

        /// <summary>
        /// If true allows editing locked TimeCards. By default after a TimeCard has been marked paid, it is locked by being set to ReadOnly.
        /// </summary>
        [ObservableProperty]
        public partial bool IsAdmin { get; set; } = false;

        [ObservableProperty]
        public partial bool Loading { get; set; } = false;

        [ObservableProperty]
        public partial bool HasError { get; set; } = false;

        public BaseViewModel() { }

        /// <summary>
        /// Converts Int to bool.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>true if 1</returns>
        public static bool IntToBool(int? value) => value.HasValue && value.Value is int @int && @int == 1;
    }
}
