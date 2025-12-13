using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

[QueryProperty(nameof(PatientId), "patientId")]
public partial class ThirdStagePartographPage : ContentPage
{
    public string PatientId { get; set; }

    public ThirdStagePartographPage(ThirdStagePartographPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }
}
