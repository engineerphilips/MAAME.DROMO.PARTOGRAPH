namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Modals;

public partial class AssessmentPlanModalPage : ContentPage
{
	public AssessmentPlanModalPage()
    {
        InitializeComponent();
    }

    private void OnInterventionLabelTapped(object sender, TappedEventArgs e)
    {
        InterventionCheckBox.IsChecked = !InterventionCheckBox.IsChecked;
    }
}