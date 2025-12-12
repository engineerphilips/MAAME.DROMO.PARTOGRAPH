using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;
using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages
{
    [QueryProperty(nameof(PartographId), "PartographId")]
    public partial class BirthOutcomePage : ContentPage
    {
        public string PartographId { get; set; }

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
