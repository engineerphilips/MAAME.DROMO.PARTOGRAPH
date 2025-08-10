namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

public partial class HomePage : ContentPage
{
	public HomePage(HomePageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
    }
}