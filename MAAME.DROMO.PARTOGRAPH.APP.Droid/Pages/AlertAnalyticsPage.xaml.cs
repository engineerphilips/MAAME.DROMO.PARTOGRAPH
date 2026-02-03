using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages
{
    public partial class AlertAnalyticsPage : ContentPage
    {
        private readonly AlertAnalyticsPageModel _pageModel;

        public AlertAnalyticsPage(AlertAnalyticsPageModel pageModel)
        {
            InitializeComponent();
            _pageModel = pageModel;
            BindingContext = pageModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _pageModel.OnAppearing();
        }
    }
}
