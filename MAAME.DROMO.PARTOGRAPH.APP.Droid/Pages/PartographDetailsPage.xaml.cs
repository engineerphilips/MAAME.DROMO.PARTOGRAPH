namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

public partial class PartographDetailsPage : ContentPage
{
    public PartographDetailsPage(PageModels.PartographDetailsPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
