namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

public partial class VitalSignsPage : ContentPage
{
	public VitalSignsPage(VitalSignsPageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
    }
}