using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages
{
    public partial class FacilityOnboardingPage : ContentPage
    {
        private readonly FacilityOnboardingPageModel _viewModel;

        public FacilityOnboardingPage(FacilityOnboardingPageModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.InitializeAsync();
        }
    }
}
