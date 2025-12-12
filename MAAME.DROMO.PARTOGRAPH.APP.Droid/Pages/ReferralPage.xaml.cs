using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages
{
    public partial class ReferralPage : ContentPage
    {
        public ReferralPage(ReferralPageModel pageModel)
        {
            InitializeComponent();
            BindingContext = pageModel;
        }
    }
}
