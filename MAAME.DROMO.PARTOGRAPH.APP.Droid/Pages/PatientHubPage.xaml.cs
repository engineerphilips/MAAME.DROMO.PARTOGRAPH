namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

public partial class PatientHubPage : ContentPage
{
    public PatientHubPage(PageModels.PatientHubPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
