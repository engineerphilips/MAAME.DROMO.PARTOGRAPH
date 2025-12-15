using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages
{
    public partial class HelpPage : ContentPage
    {
        public HelpPage()
        {
            InitializeComponent();

            // Set BindingContext for HelpPage
            var helpPageModel = IPlatformApplication.Current!.Services.GetService<HelpPageModel>();
            if (helpPageModel != null)
            {
                BindingContext = helpPageModel;
            }
        }
    }
}
