#if (ANDROID)
using AndroidOS = Android.OS;
#endif
using Microsoft.Maui.Storage;
using System.IO;

#nullable enable
namespace TimeClockApp.Shared
{
    public class FileService
    {
#if (ANDROID)
        /// <summary>
        /// Returns the path to the downloads folder
        /// </summary>
        /// <returns>string folder path</returns>
#elif (WINDOWS)
        /// <summary>
        /// Returns the path to the users Documents folder
        /// </summary>
        /// <returns>string folder path</returns>
#endif
        public string GetDownloadPath() =>
#if (ANDROID)
            AndroidOS.Environment.GetExternalStoragePublicDirectory(AndroidOS.Environment.DirectoryDownloads)!.Path;
#elif (WINDOWS)
            Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#elif NET
            System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory, "download");
#endif

        /// <summary>
        /// Returns the filePath to the applications private cached folder + filename
        /// </summary>
        /// <param name="filename">file name</param>
        /// <returns>string full path with filename</returns>
        public string GetLocalFilePath(string filename) => Path.Combine(FileSystem.Current.CacheDirectory, filename);
    }
}
