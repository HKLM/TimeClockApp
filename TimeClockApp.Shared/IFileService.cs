namespace TimeClockApp.Shared
{
	public interface IFileService
	{
		string GetDownloadPath();
		string GetLocalFilePath(string filename);
	}
}