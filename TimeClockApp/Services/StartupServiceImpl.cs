using TimeClockApp.Shared.Models;

namespace TimeClockApp.Services;

#nullable enable

/// <summary>
/// Implementation of deferred startup initialization.
/// Handles database initialization and other non-critical tasks asynchronously.
/// </summary>
public class StartupServiceImpl : IStartupService
{
	private TaskCompletionSource<bool> _initializationTcs = new();
	private bool _isInitialized;
	private readonly DataBackendContext _dbContext;

	public bool IsInitialized => _isInitialized;
	public Task InitializationTask => _initializationTcs.Task;

	public StartupServiceImpl(DataBackendContext dbContext)
	{
		ArgumentNullException.ThrowIfNull(dbContext);
		_dbContext = dbContext;
	}

	/// <summary>
	/// Initializes the database asynchronously. This method should be called after the UI is displayed.
	/// It performs database migrations and ensures the database is ready for use.
	/// </summary>
	public async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		if (_isInitialized)
			return;

		try
		{
			// Perform database initialization asynchronously without blocking the UI thread
			cancellationToken.ThrowIfCancellationRequested();

			await _dbContext.InitializeDatabaseAsync(cancellationToken);

			_isInitialized = true;
			_initializationTcs.TrySetResult(true);
		}
		catch (OperationCanceledException)
		{
			_initializationTcs.TrySetCanceled();
			throw;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Startup initialization error: {ex}");
			_isInitialized = true;
			_initializationTcs.TrySetResult(false);
		}
	}
}
