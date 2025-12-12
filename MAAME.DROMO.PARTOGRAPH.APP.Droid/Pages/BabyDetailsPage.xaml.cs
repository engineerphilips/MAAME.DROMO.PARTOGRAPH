using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages
{
    public partial class BabyDetailsPage : ContentPage
    {
        public BabyDetailsPage(BabyDetailsPageModel pageModel)
        {
            InitializeComponent();
            BindingContext = pageModel;
        }

        private void OnVitalStatusChanged(object sender, EventArgs e)
        {
            if (BindingContext is BabyDetailsPageModel viewModel)
            {
                //viewModel.OnVitalStatusChangedCommand?.Execute();
            }
        }

        private void OnResuscitationRequiredChanged(object sender, CheckedChangedEventArgs e)
        {
            if (BindingContext is BabyDetailsPageModel viewModel)
            {
                //viewModel.OnResuscitationRequiredChangedCommand?.Execute();
            }
        }
    }
}
