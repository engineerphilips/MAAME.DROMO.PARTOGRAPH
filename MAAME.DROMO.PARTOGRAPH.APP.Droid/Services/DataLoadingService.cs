using System;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    /// <summary>
    /// Service to facilitate UI-first loading pattern with progress tracking.
    /// Allows the UI to render first, then loads data from the database with progress updates.
    /// </summary>
    public interface IDataLoadingService
    {
        /// <summary>
        /// Executes multiple data loading operations with progress tracking.
        /// Shows a loading progress bar while fetching data from the database.
        /// </summary>
        /// <param name="loadingMessage">The message to display during loading</param>
        /// <param name="operations">Array of named operations to execute</param>
        Task LoadDataWithProgressAsync(string loadingMessage, params (string name, Func<Task> operation)[] operations);

        /// <summary>
        /// Executes multiple data loading operations with progress tracking and returns a result.
        /// </summary>
        Task<T> LoadDataWithProgressAsync<T>(string loadingMessage, Func<IProgress<(double progress, string? message)>, Task<T>> loadOperation);
    }

    public class DataLoadingService : IDataLoadingService
    {
        private readonly ILoadingOverlayService _loadingOverlayService;

        public DataLoadingService(ILoadingOverlayService loadingOverlayService)
        {
            _loadingOverlayService = loadingOverlayService;
        }

        public async Task LoadDataWithProgressAsync(string loadingMessage, params (string name, Func<Task> operation)[] operations)
        {
            if (operations == null || operations.Length == 0)
                return;

            try
            {
                _loadingOverlayService.ShowWithProgress(loadingMessage, "Initializing...");

                // Small delay to allow UI to render first
                await Task.Delay(50);

                var totalOperations = operations.Length;
                var completedOperations = 0;

                foreach (var (name, operation) in operations)
                {
                    // Update progress message with current operation name
                    var progress = (double)completedOperations / totalOperations;
                    _loadingOverlayService.UpdateProgress(progress, loadingMessage, $"Loading {name}...");

                    // Execute the operation
                    await operation();

                    completedOperations++;
                }

                // Show completion state briefly
                _loadingOverlayService.UpdateProgress(1.0, loadingMessage, "Complete!");
                await Task.Delay(100);
            }
            finally
            {
                _loadingOverlayService.Hide();
            }
        }

        public async Task<T> LoadDataWithProgressAsync<T>(string loadingMessage, Func<IProgress<(double progress, string? message)>, Task<T>> loadOperation)
        {
            try
            {
                _loadingOverlayService.ShowWithProgress(loadingMessage, "Initializing...");

                // Small delay to allow UI to render first
                await Task.Delay(50);

                var progress = new Progress<(double progress, string? message)>(report =>
                {
                    _loadingOverlayService.UpdateProgress(report.progress, loadingMessage, report.message);
                });

                var result = await loadOperation(progress);

                // Show completion state briefly
                _loadingOverlayService.UpdateProgress(1.0, loadingMessage, "Complete!");
                await Task.Delay(100);

                return result;
            }
            finally
            {
                _loadingOverlayService.Hide();
            }
        }
    }

    /// <summary>
    /// Extension methods to help with progress reporting in data loading operations.
    /// </summary>
    public static class DataLoadingExtensions
    {
        /// <summary>
        /// Reports progress with a step-based calculation.
        /// </summary>
        public static void ReportStep(this IProgress<(double progress, string? message)> progress, int currentStep, int totalSteps, string? message = null)
        {
            var progressValue = (double)currentStep / totalSteps;
            progress.Report((progressValue, message));
        }
    }
}
