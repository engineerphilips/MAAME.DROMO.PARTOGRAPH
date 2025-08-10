namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

public partial class ActivePatientsPage : ContentPage
{
	public ActivePatientsPage(ActivePatientsPageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
    }
}