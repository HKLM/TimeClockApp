namespace TimeClockApp.Services
{
    internal class AlertService : IAlertService
    {
        // ----- async calls (use with "await" - MUST BE ON DISPATCHER THREAD) -----

        /// <summary>
        /// Example showing that the "fire and forget" methods can be called from anywhere
        ///
        /// <code>
        /// Task.Run(async () =>
        ///{
        ///    await Task.Delay(2000);
        ///    App.AlertSvc!.ShowConfirmation("Title", "Confirmation message.", (result =>
        ///    {
        ///        App.AlertSvc!.ShowAlert("Result", $"{result}");
        ///    }));
        ///});
        /// </code>
        /// </summary>
        /// <remarks>
        /// If instead you use the "...Async" methods, but aren't on the window's Dispatcher thread (Main thread), at runtime you'll get a wrong thread exception
        /// </remarks>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public Task ShowAlertAsync(string title, string message, string cancel = "OK")
        {
            return App.Current.Windows[0].Page.DisplayAlert(title, message, cancel);
        }

        public Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No")
        {
            return App.Current.Windows[0].Page.DisplayAlert(title, message, accept, cancel);
        }

#region  "'Fire and forget' calls"

        /// <summary>
        /// "Fire and forget". Method returns BEFORE showing alert.
        ///
        /// </summary>
        public void ShowAlert(string title, string message, string cancel = "OK")
        {
            App.Current.Windows[0].Page.Dispatcher.Dispatch(async () =>
                await ShowAlertAsync(title, message, cancel)
            );
        }

        /// <summary>
        /// "Fire and forget". Method returns BEFORE showing alert.
        /// </summary>
        /// <param name="callback">Action to perform afterwards.</param>
        public void ShowConfirmation(string title, string message, Action<bool> callback, string accept = "Yes", string cancel = "No")
        {
            try
            {
                App.Current.Windows[0].Page.Dispatcher.Dispatch(async () =>
            {
                bool answer = await ShowConfirmationAsync(title, message, accept, cancel);
                callback(answer);
            });
            }
            catch (AggregateException ax)
            {
                TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
            }
            catch (Exception ex)
            {
                Log.WriteLine($"{ex.Message}\n{ex.InnerException}");
            }
        }
        #endregion
    }
}
