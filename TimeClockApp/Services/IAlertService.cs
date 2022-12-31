namespace TimeClockApp.Services
{
    public interface IAlertService
    {
        // ----- async calls (use with "await" - MUST BE ON DISPATCHER THREAD) -----
        Task ShowAlertAsync(string title, string message, string cancel = "OK");
        Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No");

        // ----- "Fire and forget" calls -----
        void ShowAlert(string title, string message, string cancel = "OK");

        /// <param name="callback">Action to perform afterwards.</param>
        void ShowConfirmation(string title, string message, Action<bool> callback, string accept = "Yes", string cancel = "No");
    }
}
