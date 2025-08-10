namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

public partial class PartographPage : ContentPage
{
	public PartographPage(PartographPageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
    }
}