#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class TimeStampViewModel : BaseViewModel, IDisposable
    {
        /// <summary>
        /// The ToolBar Help Icon toggles displaying the HelpInfoxBox for each page
        /// </summary>
        [ObservableProperty]
        private bool helpInfoBoxVisible = false;

        [ObservableProperty]
        private string? title = string.Empty;

        [ObservableProperty]
        private bool isAdmin = false;
        private bool disposedValue;

        public TimeStampViewModel()
        {
        }

        /// <summary>
        /// Converts Int to bool.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>true if 1</returns>
        public static bool IntToBool(int? value) => value.HasValue && value.Value is int @int && @int == 1;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TimeStampViewModel()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
