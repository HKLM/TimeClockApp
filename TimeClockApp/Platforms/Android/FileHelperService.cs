using AndroidOS = Android.OS;
using ApplicationDroidApp = Android.App.Application;

namespace TimeClockApp
{
    public partial class FileHelperService
    {
        public partial string GetDownloadPath()
        {
#pragma warning disable CS0618
            return AndroidOS.Environment.GetExternalStoragePublicDirectory(AndroidOS.Environment.DirectoryDownloads).CanonicalPath;
#pragma warning restore CS0618
        }

        public partial string GetDBPath(string filename)
        {
            return Path.Combine(FileSystem.Current.AppDataDirectory, filename);
        }

        public partial string GetLocalFilePath(string filename)
        {
            return Path.Combine(FileSystem.Current.CacheDirectory, filename);
        }

        public partial string GetDownloadDirPath(string filename)
        {
            string dirpath = ApplicationDroidApp.Context.GetExternalFilesDir(AndroidOS.Environment.DirectoryDownloads).CanonicalPath;
            return Path.Combine(dirpath, filename);
        }
    }
}
