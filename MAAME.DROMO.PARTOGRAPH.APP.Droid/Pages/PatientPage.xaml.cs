using MAAME.DROMO.PARTOGRAPH.MODEL;

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
}