using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;
using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages
{
    [QueryProperty(nameof(PartographId), "PartographId")]
    [QueryProperty(nameof(BirthOutcomeId), "BirthOutcomeId")]
    [QueryProperty(nameof(NumberOfBabies), "NumberOfBabies")]
    public partial class BabyDetailsPage : ContentPage
    {
        public string PartographId { get; set; }
        public string BirthOutcomeId { get; set; }
        public string NumberOfBabies { get; set; }

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
