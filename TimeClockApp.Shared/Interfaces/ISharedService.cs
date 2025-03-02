namespace TimeClockApp.Shared.Interfaces
{
    public interface ISharedService
    {
        void Add<T>(string key, T value) where T : class;
        T GetValue<T>(string key) where T : class;
    }
}