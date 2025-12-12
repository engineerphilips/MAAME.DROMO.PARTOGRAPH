using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages
{
    public partial class BirthOutcomePage : ContentPage
    {
        public BirthOutcomePage(BirthOutcomePageModel pageModel)
        {
            InitializeComponent();
            BindingContext = pageModel;
        }

        private void OnMaternalStatusChanged(object sender, EventArgs e)
        {
            if (BindingContext is BirthOutcomePageModel viewModel)
            {
                //viewModel.OnMaternalStatusChangedCommand?.Execute(null);
            }
        }
    }
}
