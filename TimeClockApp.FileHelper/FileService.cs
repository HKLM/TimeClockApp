#if (ANDROID && NET9_0)
using AndroidOS = Android.OS;
#endif
using Microsoft.Maui.Storage;
using System.IO;

namespace TimeClockApp.FileHelper
{
    public class FileService
    {
#if (ANDROID && NET9_0)
        public string GetDownloadPath() => AndroidOS.Environment.GetExternalStoragePublicDirectory(AndroidOS.Environment.DirectoryDownloads).Path;
        //public static partial string GetDownloadPath();
#elif (WINDOWS && NET9_0)
        public string GetDownloadPath() => System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory, "download");
#elif NET9_0
        public string GetDownloadPath() => System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory, "download");
#endif
        public string GetLocalFilePath(string filename) => System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.Current.CacheDirectory, filename);

    }
}
