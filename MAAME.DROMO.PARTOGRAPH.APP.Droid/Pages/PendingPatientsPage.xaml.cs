namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

public partial class PendingPatientsPage : ContentPage
{
	public PendingPatientsPage(PendingPatientsPageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;	
    }
}