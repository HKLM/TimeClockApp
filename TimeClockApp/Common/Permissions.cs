#nullable enable
namespace TimeClockApp.Common
{
	public static class Permissions
	{
		public static bool? IsAdmin { get; set; }
		public static bool GetIsAdmin() => IsAdmin ?? false;
	}
}
