using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

[QueryProperty(nameof(PatientId), "patientId")]
public partial class FourthStagePartographPage : ContentPage
{
    public string PatientId { get; set; }

    public FourthStagePartographPage(FourthStagePartographPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }
}
