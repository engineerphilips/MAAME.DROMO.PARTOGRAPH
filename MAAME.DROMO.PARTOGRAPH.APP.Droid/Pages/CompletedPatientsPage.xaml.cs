namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

public partial class CompletedPatientsPage : ContentPage
{
	public CompletedPatientsPage(CompletedPatientsPageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
    }
}