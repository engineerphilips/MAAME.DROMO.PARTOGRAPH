using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

/// <summary>
/// Fourth Stage Partograph Page - Immediate postpartum monitoring
/// WHO 2020: 2-hour monitoring with vitals every 15 minutes
/// Enhanced with comprehensive maternal vitals and fourth stage specific assessments
/// </summary>
[QueryProperty(nameof(PatientId), "patientId")]
public partial class FourthStagePartographPage : ContentPage
{
    public string PatientId { get; set; }
    private readonly FourthStagePartographPageModel _pageModel;

    public FourthStagePartographPage(FourthStagePartographPageModel pageModel)
    {
        InitializeComponent();
        _pageModel = pageModel;
        BindingContext = pageModel;

        // Wire up popup actions for BP/Pulse modal
        _pageModel.OpenBpPulsePopup = () =>
        {
            BpPulsePopup.IsOpen = true;
        };
        _pageModel.CloseBpPulsePopup = () =>
        {
            BpPulsePopup.IsOpen = false;
        };

        // Wire up popup actions for Temperature modal
        _pageModel.OpenTemperaturePopup = () =>
        {
            TemperaturePopup.IsOpen = true;
        };
        _pageModel.CloseTemperaturePopup = () =>
        {
            TemperaturePopup.IsOpen = false;
        };

        // Wire up popup actions for Vitals Trend (BP/Pulse)
        _pageModel.OpenVitalsTrendPopup = () =>
        {
            VitalsTrendPopup.IsOpen = true;
        };
        _pageModel.CloseVitalsTrendPopup = () =>
        {
            VitalsTrendPopup.IsOpen = false;
        };

        // Wire up popup actions for Temperature Trend
        _pageModel.OpenTemperatureTrendPopup = () =>
        {
            TemperatureTrendPopup.IsOpen = true;
        };
        _pageModel.CloseTemperatureTrendPopup = () =>
        {
            TemperatureTrendPopup.IsOpen = false;
        };

        // Wire up popup actions for Completion Checklist
        _pageModel.OpenCompletionChecklistPopup = () =>
        {
            CompletionChecklistPopup.IsOpen = true;
        };
        _pageModel.CloseCompletionChecklistPopup = () =>
        {
            CompletionChecklistPopup.IsOpen = false;
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Refresh data when page appears
        if (_pageModel.RefreshCommand.CanExecute(null))
        {
            _pageModel.RefreshCommand.Execute(null);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Close any open popups when leaving the page
        BpPulsePopup.IsOpen = false;
        TemperaturePopup.IsOpen = false;
        VitalsTrendPopup.IsOpen = false;
        TemperatureTrendPopup.IsOpen = false;
        CompletionChecklistPopup.IsOpen = false;
    }
}
