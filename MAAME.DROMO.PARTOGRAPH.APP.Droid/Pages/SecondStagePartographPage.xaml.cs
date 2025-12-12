using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

[QueryProperty(nameof(PatientId), "patientId")]
public partial class SecondStagePartographPage : ContentPage
{
    public string PatientId { get; set; }

    public SecondStagePartographPage(SecondStagePartographPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;

        // Note: Modal popups will be added to the XAML if needed
        // For now, we're reusing the same modals from the first stage through commands
    }
}
