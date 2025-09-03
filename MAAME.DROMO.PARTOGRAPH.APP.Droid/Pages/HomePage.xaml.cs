namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

public partial class HomePage : ContentPage
{
	public HomePage(HomePageModel pageModel, PendingPatientsPageModel pendingPageModel, ActivePatientsPageModel activePageModel, CompletedPatientsPageModel completePageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
		Loaded += (s, e) =>
		{
            PendingPatients.BindingContext = pendingPageModel;
            ActivePatients.BindingContext = activePageModel;
            CompletedPatients.BindingContext = completePageModel;
        };
	}

    private async void PendingPatients_Loaded(object sender, EventArgs e)
    {
		if (PendingPatients.BindingContext is PendingPatientsPageModel homePageModel)
		{
			 await homePageModel.AppearingCommand.ExecuteAsync(null);
        }
    }

    private async void ActivePatients_Loaded(object sender, EventArgs e)
    {
        if (ActivePatients.BindingContext is ActivePatientsPageModel homePageModel)
        {
            await homePageModel.AppearingCommand.ExecuteAsync(null);
        }
    }

    private async void CompletedPatients_Loaded(object sender, EventArgs e)
    {

        if (CompletedPatients.BindingContext is CompletedPatientsPageModel homePageModel)
        {
            await homePageModel.AppearingCommand.ExecuteAsync(null);
        }
    }
}