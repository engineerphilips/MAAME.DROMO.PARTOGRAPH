namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

public partial class PatientDetailPage : ContentPage
{
	public PatientDetailPage(PatientDetailPageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;

    }
}