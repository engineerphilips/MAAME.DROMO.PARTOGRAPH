namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

public partial class PatientsPage : ContentPage
{
    public PatientsPage(PageModels.PatientsPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
