using TimeClockApp.Shared.Interfaces;

namespace TimeClockApp.Services
{
    public class SharedService : ISharedService
    {
        private Dictionary<string, object> DTODict { get; set; } = new Dictionary<string, object>();
        public void Add<T>(string key, T value) where T : class
        {
            if (DTODict.ContainsKey(key))
            {
                DTODict[key] = value;
            }
            else
            {
                DTODict.Add(key, value);
            }
        }

        public T GetValue<T>(string key) where T : class
        {
            if (DTODict.ContainsKey(key))
            {
                return DTODict[key] as T;
            }
            return null;
        }
    }
}
