using MAAME.DROMO.PARTOGRAPH.MODEL;
using Syncfusion.Maui.Picker;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

public partial class PatientPage : ContentPage
{
	public PatientPage(PatientPageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
    }

    private void RiskFactorsEntry_Completed(object sender, EventArgs e)
    {
        if (sender is InputView entry && this.BindingContext is PatientPageModel viewModel)
        {
            if (!string.IsNullOrWhiteSpace(entry.Text))
            {
                viewModel.RiskFactors.Add(new Diagnosis() { Name = entry.Text });
                entry.Text = "";
            }
        }
    }

    private void Entry_Completed(object sender, EventArgs e)
    {
        if (sender is InputView entry && this.BindingContext is PatientPageModel viewModel)
        {
            if (!string.IsNullOrWhiteSpace(entry.Text))
            {
                viewModel.Diagnoses.Add(new Diagnosis() { Name = entry.Text });
                entry.Text = "";
            }
        }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {

    }

    #region SfDatePicker and SfTimePicker Handlers

    // Date of Birth Picker
    private void DateOfBirthPicker_Tapped(object sender, TappedEventArgs e)
    {
        DateOfBirthPicker.IsOpen = true;
    }

    // EDD Picker
    private void EDDPicker_Tapped(object sender, TappedEventArgs e)
    {
        EDDPicker.IsOpen = true;
    }

    // LMP Picker
    private void LMPPicker_Tapped(object sender, TappedEventArgs e)
    {
        LMPPicker.IsOpen = true;
    }

    // Labor Start Date Picker
    private void LaborStartDatePicker_Tapped(object sender, TappedEventArgs e)
    {
        LaborStartDatePicker.IsOpen = true;
    }

    // Labor Start Time Picker
    private void LaborStartTimePicker_Tapped(object sender, TappedEventArgs e)
    {
        LaborStartTimePicker.IsOpen = true;
    }

    // Rupture Date Picker
    private void RuptureDatePicker_Tapped(object sender, TappedEventArgs e)
    {
        RuptureDatePicker.IsOpen = true;
    }

    // Rupture Time Picker
    private void RuptureTimePicker_Tapped(object sender, TappedEventArgs e)
    {
        RuptureTimePicker.IsOpen = true;
    }

    #endregion
}
