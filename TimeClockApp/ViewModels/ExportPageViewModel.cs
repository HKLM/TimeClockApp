using CsvHelper;
using TimeClockApp.Shared;
#nullable enable

namespace TimeClockApp.ViewModels
{
	public partial class ExportPageViewModel : BaseViewModel
	{
		private readonly ExportDataService dataService;
		private readonly IFileService fileService;

		internal string CSVFilePath { get; set; } = string.Empty;

		[ObservableProperty]
		public partial string ExportLog { get; set; } = string.Empty;

		//TODO FIX when adding to data fails due to existing data at same rowId.
		/// <summary>
		/// During import of Data should the TimeCards and Expenses data be overwritten or only added to (with possible duplicates)
		/// </summary>
		private bool OverwriteData { get; set; } = true;

		[ObservableProperty]
		public partial bool LogTextVisible { get; set; } = false;

		[ObservableProperty]
		public partial bool ButtonDisabled { get; set; } = false;
		private void SetPageLoading(bool loading)
		{
			Loading = loading;
			ButtonDisabled = !loading;
		}

		[ObservableProperty]
		public partial double Progress { get; private set; } = 0.0;
		private void SetProgress(double progress) => Progress = progress;

		private void IncrementProgress(double increment)
		{
			if (Progress + increment >= 1.0)
				Progress = 1.0;
			else
				Progress += increment;
		}

		private enum SQLTables
		{
			TimeCard = 0,
			Employee = 1,
			Project = 2,
			Phase = 3,
			Config = 4,
			Expense = 5,
			ExpenseType = 6,
			Version = 7
		}

		public ExportPageViewModel(IFileService _fileService)
		{
			fileService = _fileService;
			dataService = new(_fileService);
		}

		public void OnAppearing()
		{
			ExportLog = string.Empty;
			SetPageLoading(false);
		}

		static async Task<bool> ArePermissionsGranted(bool ReadPermission, bool ReadWritePermission)
		{
			if (ReadPermission)
			{
				PermissionStatus readPermissionStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
				if (readPermissionStatus is not PermissionStatus.Granted)
				{
					if (await App.AlertSvc!.ShowConfirmationAsync("Storage permission is not granted.", "Please grant the READ STORAGE permission to use this feature.", "OK", "CANCEL"))
						readPermissionStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
				}
				if (readPermissionStatus is PermissionStatus.Granted) return true;
			}
			else if (ReadWritePermission)
			{
				PermissionStatus wPermissionStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
				if (wPermissionStatus is not PermissionStatus.Granted)
				{
					if (await App.AlertSvc!.ShowConfirmationAsync("Storage permission is not granted.", "Please grant the WRITE STORAGE permission to use this feature.", "OK", "CANCEL"))
						wPermissionStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();
				}
				if (wPermissionStatus is PermissionStatus.Granted)
					return true;
			}

			return false;
		}

		private async Task<FileResult?> SelectFile()
		{
			try
			{
				FilePickerFileType customFileType = new(new Dictionary<DevicePlatform, IEnumerable<string>>
							{
								{ DevicePlatform.Android, new[] { "application/zip" } },
								{ DevicePlatform.WinUI, new[] { ".zip" } },
							});
				FileResult? openResult = await FilePicker.Default.PickAsync(new PickOptions
				{
					PickerTitle = "ZIP file to import",
					FileTypes = customFileType,
				});
				return openResult;

			}
			catch (TaskCanceledException)
			{
				return null;
			}
			catch (Exception)
			{
				return null;
			}
		}

		[RelayCommand]
		private async Task ImportData()
		{
			//if (Loading) return;

			SetPageLoading(true);
			HasError = false;
			SetProgress(0.2);

			try
			{
				if (!await ArePermissionsGranted(true, false))
				{
					SetPageLoading(false);
					await AppShell.DisplayToastAsync("Can not access storage!", true);

					return;
				}
				string outputLog = "Preparing for Import ... \n";

				FileResult? result = await SelectFile();
				string fileCopyName = String.Empty;
				if (result == null)
				{
					outputLog += "No file selected, import cancelled.\n";
					ExportLog = outputLog;
					await AppShell.DisplayToastAsync("No file selected, import cancelled.", true);

					SetPageLoading(false);
					return;
				}
				else
				{
					if (File.Exists(result.FullPath))
						fileCopyName = result.FullPath;
				}
				//fall back to default if no user selection
				if (fileCopyName == String.Empty)
					fileCopyName = Path.Combine(fileService.GetDownloadPath(), "TimeCardApp.zip");

				//  Create a location for our data
				string UnZipPath = fileService.GetLocalFilePath("TimeCardAppUNZIP");

				if (Directory.Exists(UnZipPath))
				{
					Directory.Delete(UnZipPath, true);
				}

				Directory.CreateDirectory(UnZipPath);
				outputLog += "Start Unzip file...\n";
				ImportDataModel dataModel = new();

				dataModel = await dataService.UnzipArchive(fileCopyName, UnZipPath);
				if (dataModel.IsAnyTrue())
				{
					SetProgress(0.5);
					outputLog += "Done!...\n";
					outputLog += await dataService.ImportData(dataModel, outputLog, OverwriteData);
					SetProgress(0.85);
					int icd = dataModel.DeleteCachedFile();
					outputLog += $"\nCleaned up {icd} temp files\n";
				}
				else
				{
					outputLog += "No valid files found for import!\nABORTING\n";
					ExportLog = outputLog;
					dataModel.DeleteCachedFile();
					SetProgress(1.0);
					SetPageLoading(false);
					return;
				}
				SetProgress(1.0);
				outputLog += "Done!\n";
				ExportLog = outputLog;
				await AppShell.DisplayToastAsync("Import completed successfully!", true);
			}
			catch (AggregateException ax)
			{
				HasError = true;
				string f = TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateExceptionForPopup(ax, "ExportPageViewModel");
				await dataService.ShowPopupErrorAsync(f, "ABORTING DUE TO ERROR").ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				HasError = true;
				await dataService.ShowPopupErrorAsync($"EXCEPTION ERROR\n{ex.Message}\n{ex.InnerException}", "ABORTING DUE TO ERROR").ConfigureAwait(false);
			}
			finally
			{
				SetPageLoading(false);
			}
		}

		[RelayCommand]
		private async Task ExportData()
		{
			if (Loading) return;

			SetPageLoading(true);
			HasError = false;
			string outputLog = "Preparing for export ... \n";
			try
			{
				if (!await ArePermissionsGranted(false, true))
				{
					SetPageLoading(false);
					return;
				}
				SetProgress(0.1);

				//  Create a location for our data
				CSVFilePath = Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, "TimeCardApp", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
				try
				{
					if (Directory.Exists(CSVFilePath))
					{
						Directory.Delete(CSVFilePath, true);
					}
				}
				catch (Exception ex)
				{
					// Log things and move on, don't want to fail just because of a left over lock or something
					Log.WriteLine($"\nExportData EXPECTED POSSIBLE EXCEPTION\n{ex.Message}\n{ex.InnerException}");
					outputLog += ex.Message + "\n";
				}

				Directory.CreateDirectory(CSVFilePath);
				outputLog += "Start Database export\n";
				Task<string> taskA = MakeCSVFilesAsync();
				//  Zip everything and present a share dialog to the user
				outputLog += "Zipping ... \n";
				await taskA.ContinueWith(async antecedent => await dataService.CompressAndExportFolder((await antecedent))).Unwrap();
				SetProgress(1.0);
				outputLog += $"Done!\nExported File name: {ExportDataService.ZIPFILENAME}\nLocation: Downloads folder";
				ExportLog = outputLog;
				await AppShell.DisplayToastAsync("Export completed successfully!", true);
			}
			catch (AggregateException ax)
			{
				HasError = true;
				string f = TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateExceptionForPopup(ax, "ExportPageViewModel");
				await dataService.ShowPopupErrorAsync(f, "ExportData ABORTING DUE TO ERROR").ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				HasError = true;
				await dataService.ShowPopupErrorAsync($"EXCEPTION ERROR\n{ex.Message}\n{ex.InnerException}", "ExportData ABORTING DUE TO ERROR").ConfigureAwait(false);
			}
			finally
			{
				SetPageLoading(false);
			}
		}

		private async Task<string> MakeCSVFilesAsync()
		{
			List<Task> tableTaskList = [];
			for (int i = 0; i < Enum.GetNames(typeof(SQLTables)).Length; i++)
			{
				SQLTables sQL = (SQLTables)i;
				tableTaskList.Add(CreateFileAsync(sQL, CSVFilePath, sQL.ToString() + ".csv"));
			}
			await Task.WhenAll(tableTaskList);

			return CSVFilePath;
		}

		private async Task CreateFileAsync(SQLTables table, string filePath, string fileName)
		{
			string file = Path.Combine(filePath, fileName);
			switch (table)
			{
				case SQLTables.TimeCard:
					List<TimeCard> w = await dataService.BackupGetTimeCard();
					using (CsvWriter csv = CsvWriter.ToFile(file))
					{
						await csv.WriteRecordsAsync(w);
						IncrementProgress(0.12);
						await csv.FlushAsync().ConfigureAwait(false);
					}
					return;
				case SQLTables.Employee:
					List<Employee> emp = await dataService.BackupGetEmployee();
					using (CsvWriter csv = CsvWriter.ToFile(file))
					{
						await csv.WriteRecordsAsync(emp);
						IncrementProgress(0.12);
						await csv.FlushAsync().ConfigureAwait(false);
					}
					return;
				case SQLTables.Project:
					List<Project> pr = await dataService.BackupGetProject();
					using (CsvWriter csv = CsvWriter.ToFile(file))
					{
						await csv.WriteRecordsAsync(pr);
						IncrementProgress(0.12);
						await csv.FlushAsync().ConfigureAwait(false);
					}
					return;
				case SQLTables.Phase:
					List<Phase> ph = await dataService.BackupGetPhase();
					using (CsvWriter csv = CsvWriter.ToFile(file))
					{
						await csv.WriteRecordsAsync(ph);
						IncrementProgress(0.12);
						await csv.FlushAsync().ConfigureAwait(false);
					}
					return;
				case SQLTables.Config:
					List<Config> c = await dataService.BackupGetConfig();
					using (CsvWriter csv = CsvWriter.ToFile(file))
					{
						await csv.WriteRecordsAsync(c);
						IncrementProgress(0.12);
						await csv.FlushAsync().ConfigureAwait(false);
					}
					return;
				case SQLTables.Expense:
					List<Expense> e = await dataService.BackupGetExpense();
					using (CsvWriter csv = CsvWriter.ToFile(file))
					{
						await csv.WriteRecordsAsync(e);
						IncrementProgress(0.12);
						await csv.FlushAsync().ConfigureAwait(false);
					}
					return;
				case SQLTables.ExpenseType:
					List<ExpenseType> et = await dataService.BackupGetExpenseType();
					using (CsvWriter csv = CsvWriter.ToFile(file))
					{
						await csv.WriteRecordsAsync(et);
						IncrementProgress(0.12);
						await csv.FlushAsync().ConfigureAwait(false);
					}
					return;
				case SQLTables.Version:
					using (StreamWriter outputFile = new StreamWriter(Path.Combine(filePath, "FILE_ID.DIZ")))
					{
						await outputFile.WriteAsync(dataService.GetVERSIONCHECKNUMBER.ToString());
						IncrementProgress(0.06);
					}
					return;

				default:
					break;
			}

			return;
		}

		[RelayCommand]
		private async Task OnBackupDBRequest()
		{
			SetPageLoading(true);
			HasError = false;
			try
			{
				string outputLog = "START BACKUP DATABASE: \n";
				string dbFileName = $"timeclock_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.db3";
				string file = Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, dbFileName);
				bool bFileDeleteError = false;
				try
				{
					if (File.Exists(file))
					{
						outputLog += $"Old Temp File Exists, Deleting filename: {file}\n";
						File.Delete(file);
						outputLog += "Old Temp File Deleted.\n";
					}
				}
				catch (IOException iox)
				{
					bFileDeleteError = true;
					HasError = true;
					Log.WriteLine($"\nIOException ERROR\n{iox.Message}\n{iox.InnerException}\n", "Backup");
				}
				if (!bFileDeleteError)
				{
					SetProgress(0.5);
					outputLog += "Begin backing up...\n";
					await dataService.BackupDatabase(file);
#if WINDOWS
                    await Share.Default.RequestAsync(new ShareFileRequest
                    {
                        Title = "TimeClock App SQLite3 Database",
                        File = new ShareFile(file),
                    });

#else
					// Copy file to public accessible download folder, outside of app sandbox
					string filePublic = Path.Combine(fileService.GetDownloadPath(), dbFileName);
					if (File.Exists(filePublic))
					{
						outputLog += $"Old Backup File Exists, Deleting filename: {filePublic}\n";
						File.Delete(filePublic);
						outputLog += "Old Backup File Deleted.\n";
					}
					byte[] bytes = await File.ReadAllBytesAsync(file);
					outputLog += $"Copy backup database file to: {filePublic}\n";
					await File.WriteAllBytesAsync(filePublic, bytes);
					SetProgress(1.0);
					outputLog += "Back up complete, start share file...\n";
					await AppShell.DisplayToastAsync("Database backup completed successfully!", true);

					await Share.Default.RequestAsync(new ShareFileRequest
					{
						Title = "TimeCard App SQLite3 Database",
						File = new ShareFile(filePublic),
					});
#endif
					outputLog += "FINISHED\n";
				}
				else
				{
					outputLog += "ABORT due to cant delete temp file. Database NOT backed up.\n";
				}
				ExportLog = outputLog;
			}
			catch (IOException io)
			{
				HasError = true;
				await dataService.ShowPopupErrorAsync($"IOException ERROR\n{io.Message}\n{io.InnerException}", "BACKUP1 ABORTING BACKUP DUE TO ERROR").ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				HasError = true;
				await dataService.ShowPopupErrorAsync($"EXCEPTION ERROR\n{ex.Message}\n{ex.InnerException}", "BACKUP3 ABORTING DUE TO ERROR").ConfigureAwait(false);
			}
			finally
			{
				SetPageLoading(false);
			}
		}

		[RelayCommand]
		private void OnToggleHelpInfoBox()
		{
			HelpInfoBoxVisible = !HelpInfoBoxVisible;
		}

		[RelayCommand]
		private async Task OnToggleWarningBox()
		{
			SetPageLoading(true);
			bool confirm = await App.AlertSvc!.ShowConfirmationAsync("WARNING", "This will overwrite your existing data with the imported data. \n\nThis can not be undone. Do you want to continue?", "CONTINUE", "CANCEL");
			if (confirm)
			{
				await ImportData();
			}
			else
			{
				SetPageLoading(false);
			}
		}

		[RelayCommand]
		public void OnShowLog()
		{
			LogTextVisible = !LogTextVisible;
		}
	}
}
