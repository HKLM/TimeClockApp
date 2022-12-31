using System.ComponentModel;
using System.Runtime.CompilerServices;

using CommunityToolkit.Mvvm.ComponentModel;

#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class TimeStampViewModel : BaseViewModel, IDisposable
    {
        /// <summary>
        /// The ToolBar Help Icon toggles displaying the HelpInfoxBox for each page
        /// </summary>
        [ObservableProperty]
        private bool helpInfoBoxVisibile = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool isBusy;

        [ObservableProperty]
        private string? title = string.Empty;

        [ObservableProperty]
        private bool isValid = true;

        [ObservableProperty]
        private List<string?> errors = new();

        [ObservableProperty]
        private bool isAdmin = false;

        public TimeStampViewModel()
        {
            IsBusy = false;
            ErrorsChanged += OnErrorsChanged;
        }

        /// <summary>
        /// Converts Int to bool.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>true if 1</returns>
        public static bool IntToBool(int? value) => value.HasValue && value.Value is int @int && @int == 1;

        public bool IsNotBusy => !IsBusy;

        public bool Validate()
        {
            Errors.Clear();
            ValidateAllProperties();
            Errors = GetErrors().Select(e => e.ErrorMessage).ToList();
            IsValid = !HasErrors;
            return IsValid;
        }

        protected virtual void Initialize(IDictionary<string, object?>? parameters = null) { }

        protected virtual Task InitializeAsync(IDictionary<string, object?>? parameters = null)
            => Task.FromResult(0);

        protected void SetBusy(bool value) => IsBusy = value;

        protected virtual bool SetProperty<T>(ref T field,
                                                  T value,
                                                  [CallerMemberName] string? propertyName = null,
                                                  Action? onChanging = null,
                                                  Action? onChanged = null,
                                                  Func<T, T, bool>? validateValue = null)
        {
            // If value didn't change
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            // If value changed but didn't validate
            if (validateValue != null && !validateValue(field, value))
            {
                return false;
            }

            onChanging?.Invoke();
            field = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
        {
            IsValid = !HasErrors;
        }

        public void Dispose()
        {
            ErrorsChanged -= OnErrorsChanged;
        }
    }
}
