namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

public partial class PatientsPage : ContentPage
{
    public PatientsPage(PageModels.PatientsPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PageModels.PatientsPageModel model)
            await model.AppearingCommand.ExecuteAsync(null);
    }
}
