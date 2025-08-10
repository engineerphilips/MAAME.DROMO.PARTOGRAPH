namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Modals;

public partial class PainReliefModalPage : ContentPage
{
	public PainReliefModalPage(PainReliefModalPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }

    private void OnSideEffectsLabelTapped(object sender, TappedEventArgs e)
    {
        SideEffectsCheckBox.IsChecked = !SideEffectsCheckBox.IsChecked;
    }
}