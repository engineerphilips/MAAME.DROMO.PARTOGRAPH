using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Modals;

public partial class CervixDilatationModalPage : ContentPage
{
	public CervixDilatationModalPage(CervixDilatationModalPageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
	}

    private void OnDilatationChanged(object sender, ValueChangedEventArgs e)
    {

    }
}