using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Modals;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

[QueryProperty(nameof(PatientId), "patientId")]
public partial class ThirdStagePartographPage : ContentPage
{
    public string PatientId { get; set; }
    private readonly ThirdStagePartographPageModel _pageModel;

    public ThirdStagePartographPage(ThirdStagePartographPageModel pageModel)
    {
        InitializeComponent();
        _pageModel = pageModel;
        BindingContext = pageModel;

        // Wire up popup actions
        _pageModel.OpenPlacentaDeliveryPopup = () => PlacentaDeliveryPopup.IsOpen = true;
        _pageModel.ClosePlacentaDeliveryPopup = () => PlacentaDeliveryPopup.IsOpen = false;

        _pageModel.OpenBpPulsePopup = () => BpPulsePopup.IsOpen = true;
        _pageModel.CloseBpPulsePopup = () => BpPulsePopup.IsOpen = false;

        _pageModel.OpenTemperaturePopup = () => TemperaturePopup.IsOpen = true;
        _pageModel.CloseTemperaturePopup = () => TemperaturePopup.IsOpen = false;

        _pageModel.OpenVitalsTrendPopup = () => VitalsTrendPopup.IsOpen = true;
        _pageModel.CloseVitalsTrendPopup = () => VitalsTrendPopup.IsOpen = false;
    }
}
