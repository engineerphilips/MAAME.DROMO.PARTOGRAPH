namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

public partial class PartographEntryPage : ContentPage
{
	public PartographEntryPage(PartographEntryPageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
    }
}