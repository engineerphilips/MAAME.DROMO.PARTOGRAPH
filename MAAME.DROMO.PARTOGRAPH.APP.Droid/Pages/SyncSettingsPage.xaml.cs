using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

public partial class SyncSettingsPage : ContentPage
{
    public SyncSettingsPage(SyncSettingsPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }
}
