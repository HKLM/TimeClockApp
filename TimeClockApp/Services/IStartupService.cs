namespace TimeClockApp;

#nullable enable

/// <summary>
/// Service for managing deferred application initialization tasks.
/// Allows the app to start faster by deferring non-critical operations.
/// </summary>
public interface IStartupService
{
	/// <summary>
	/// Initializes the database and performs other deferred operations asynchronously.
	/// This should be called after the UI has been displayed to the user.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token to stop initialization.</param>
	Task InitializeAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a value indicating whether initialization is complete.
	/// </summary>
	bool IsInitialized { get; }

	/// <summary>
	/// Gets an awaitable task that completes when initialization is done.
	/// </summary>
	Task InitializationTask { get; }
}
