#if (ANDROID && NET9_0)
using AndroidOS = Android.OS;
#endif
using Microsoft.Maui.Storage;
using System.IO;

namespace TimeClockApp.Shared
{
    public class FileService
    {
        /// <summary>
        /// Returns the path to the download folder
        /// </summary>
        /// <returns>string folder path</returns>
#if (ANDROID && NET9_0)
        public string GetDownloadPath() => AndroidOS.Environment.GetExternalStoragePublicDirectory(AndroidOS.Environment.DirectoryDownloads)!.Path;
#elif (WINDOWS && NET9_0)
        public string GetDownloadPath() => System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory, "download");
#elif NET9_0
        public string GetDownloadPath() => System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory, "download");
#endif
        /// <summary>
        /// Returns the filepath to the applications private cached folder + filename
        /// </summary>
        /// <param name="filename">file name</param>
        /// <returns>string full path with filename</returns>
        public string GetLocalFilePath(string filename) => System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.Current.CacheDirectory, filename);

    }
}
