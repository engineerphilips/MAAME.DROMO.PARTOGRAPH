using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

[QueryProperty(nameof(PatientId), "patientId")]
public partial class PartographChartPage : ContentPage
{
    public string PatientId { get; set; }

    public PartographChartPage(PartographChartPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is PartographChartPageModel pageModel && !string.IsNullOrEmpty(PatientId))
        {
            pageModel.PatientId = PatientId;
        }
    }
}
