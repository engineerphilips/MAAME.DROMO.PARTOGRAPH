namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    /// <summary>
    /// Navigation service that provides smooth page transitions with loading indicators.
    /// Wraps Shell navigation to show loading overlay during navigation.
    /// </summary>
    public interface INavigationService
    {
        Task GoToAsync(string route, string loadingMessage = "Loading...");
        Task GoToAsync(string route, IDictionary<string, object> parameters, string loadingMessage = "Loading...");
        Task GoBackAsync(string loadingMessage = "Going back...");
        Task GoToRootAsync(string loadingMessage = "Loading...");
    }

    public class NavigationService : INavigationService
    {
        private readonly ILoadingOverlayService _loadingOverlay;

        public NavigationService(ILoadingOverlayService loadingOverlay)
        {
            _loadingOverlay = loadingOverlay;
        }

        public async Task GoToAsync(string route, string loadingMessage = "Loading...")
        {
            try
            {
                _loadingOverlay.Show(loadingMessage);

                // Small delay to ensure the loading overlay is visible
                await Task.Delay(50);

                await Shell.Current.GoToAsync(route);
            }
            finally
            {
                // Delay hiding to allow page to render
                await Task.Delay(100);
                _loadingOverlay.Hide();
            }
        }

        public async Task GoToAsync(string route, IDictionary<string, object> parameters, string loadingMessage = "Loading...")
        {
            try
            {
                _loadingOverlay.Show(loadingMessage);

                // Small delay to ensure the loading overlay is visible
                await Task.Delay(50);

                await Shell.Current.GoToAsync(route, parameters);
            }
            finally
            {
                // Delay hiding to allow page to render
                await Task.Delay(100);
                _loadingOverlay.Hide();
            }
        }

        public async Task GoBackAsync(string loadingMessage = "Going back...")
        {
            try
            {
                _loadingOverlay.Show(loadingMessage);
                await Task.Delay(50);

                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                await Task.Delay(100);
                _loadingOverlay.Hide();
            }
        }

        public async Task GoToRootAsync(string loadingMessage = "Loading...")
        {
            try
            {
                _loadingOverlay.Show(loadingMessage);
                await Task.Delay(50);

                await Shell.Current.GoToAsync("///");
            }
            finally
            {
                await Task.Delay(100);
                _loadingOverlay.Hide();
            }
        }
    }
}
