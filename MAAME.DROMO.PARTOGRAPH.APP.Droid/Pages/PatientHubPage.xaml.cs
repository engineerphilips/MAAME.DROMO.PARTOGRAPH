using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

[QueryProperty(nameof(PatientId), "patientId")]
public partial class PatientHubPage : ContentPage
{
    public string PatientId { get; set; }
    public PatientHubPage(PageModels.PatientHubPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
        Loaded += (s, e) =>
        {
            if (BindingContext is PageModels.PatientHubPageModel pageModel)
                pageModel.PatientId = PatientId;
        };
    }
}
