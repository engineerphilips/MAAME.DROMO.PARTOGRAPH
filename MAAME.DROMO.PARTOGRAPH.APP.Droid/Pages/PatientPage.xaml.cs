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

    private void DateOfBirthEntry_Focused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            entry.Unfocus();
            DateOfBirthPicker.IsOpen = true;
        }
    }

    // EDD Picker
    private void EDDPicker_Tapped(object sender, TappedEventArgs e)
    {
        EDDPicker.IsOpen = true;
    }

    private void EDDEntry_Focused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            entry.Unfocus();
            EDDPicker.IsOpen = true;
        }
    }

    // LMP Picker
    private void LMPPicker_Tapped(object sender, TappedEventArgs e)
    {
        LMPPicker.IsOpen = true;
    }

    private void LMPEntry_Focused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            entry.Unfocus();
            LMPPicker.IsOpen = true;
        }
    }

    // Labor Start Date Picker
    private void LaborStartDatePicker_Tapped(object sender, TappedEventArgs e)
    {
        LaborStartDatePicker.IsOpen = true;
    }

    private void LaborStartDateEntry_Focused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            entry.Unfocus();
            LaborStartDatePicker.IsOpen = true;
        }
    }

    // Labor Start Time Picker
    private void LaborStartTimePicker_Tapped(object sender, TappedEventArgs e)
    {
        LaborStartTimePicker.IsOpen = true;
    }

    private void LaborStartTimeEntry_Focused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            entry.Unfocus();
            LaborStartTimePicker.IsOpen = true;
        }
    }

    // Rupture Date Picker
    private void RuptureDatePicker_Tapped(object sender, TappedEventArgs e)
    {
        RuptureDatePicker.IsOpen = true;
    }

    private void RuptureDateEntry_Focused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            entry.Unfocus();
            RuptureDatePicker.IsOpen = true;
        }
    }

    // Rupture Time Picker
    private void RuptureTimePicker_Tapped(object sender, TappedEventArgs e)
    {
        RuptureTimePicker.IsOpen = true;
    }

    private void RuptureTimeEntry_Focused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            entry.Unfocus();
            RuptureTimePicker.IsOpen = true;
        }
    }

    #endregion
}