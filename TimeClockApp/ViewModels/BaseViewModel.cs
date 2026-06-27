#nullable enable

namespace TimeClockApp.ViewModels
{
	public partial class BaseViewModel : ObservableObject
	{
		/// <summary>
		/// The ToolBar Help Icon toggles displaying the HelpInfoBox for each page
		/// </summary>
		[ObservableProperty]
		public partial bool HelpInfoBoxVisible { get; set; } = false;

		/// <summary>
		/// If true allows editing locked TimeCards. By default after a TimeCard has been marked paid, it is locked by being set to ReadOnly.
		/// </summary>
		[ObservableProperty]
		public partial bool IsAdmin { get; set; } = false;

		[ObservableProperty]
		public partial bool Loading { get; set; } = false;

		[ObservableProperty]
		public partial bool HasError { get; set; } = false;

		/// <summary>
		/// Gets or sets a value indicating whether scheduled notifications are enabled. Lunch Break, End of Shift, etc.
		/// </summary>
		/// <remarks>Is not supported on WinUI</remarks>
		[ObservableProperty]
		public partial bool UseNotifications { get; set; } = false;

		public BaseViewModel() { }

		/// <summary>
		/// Converts Int to bool.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>true if 1</returns>
		public static bool IntToBool(int? value) => value.HasValue && value.Value is int @int && @int == 1;
	}
}
