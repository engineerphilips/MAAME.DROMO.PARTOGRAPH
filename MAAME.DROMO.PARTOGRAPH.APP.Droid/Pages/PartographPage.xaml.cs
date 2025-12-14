using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;
using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

[QueryProperty(nameof(PatientId), "patientId")]
public partial class PartographPage : ContentPage
{
    public string PatientId { get; set; }

    public PartographPage(PartographPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;

        // Bind modal view BindingContexts to the modal page models exposed by the main page model
        sfPopupPainRelief.BindingContext = pageModel.PainReliefModalPageModel;
        sfPopupCompanion.BindingContext = pageModel.CompanionModalPageModel;
        sfPopupOralFluid.BindingContext = pageModel.OralFluidModalPageModel;
        sfPopupPosture.BindingContext = pageModel.PostureModalPageModel;
        sfPopupFetalPosition.BindingContext = pageModel.FetalPositionModalPageModel;
        sfPopupAmnioticFluid.BindingContext = pageModel.AmnioticFluidModalPageModel;
        sfPopupCaput.BindingContext = pageModel.CaputModalPageModel;
        sfPopupMoulding.BindingContext = pageModel.MouldingModalPageModel;
        sfPopupUrine.BindingContext = pageModel.UrineModalPageModel;
        sfPopupTemperature.BindingContext = pageModel.TemperatureModalPageModel;
        sfPopupOxytocin.BindingContext = pageModel.OxytocinModalPageModel;
        sfPopupMedication.BindingContext = pageModel.MedicationModalPageModel;
        sfPopupIVFluid.BindingContext = pageModel.IVFluidModalPageModel;
        sfPopupHeadDescent.BindingContext = pageModel.HeadDescentModalPageModel;
        sfPopupCervixDilatation.BindingContext = pageModel.CervixDilatationModalPageModel;
        sfPopupBpPulse.BindingContext = pageModel.BPPulseModalPageModel;
        sfPopupFHRContraction.BindingContext = pageModel.FHRContractionModalPageModel;
        sfPopupAssessment.BindingContext = pageModel.AssessmentModalPageModel;
        sfPopupPlan.BindingContext = pageModel.PlanModalPageModel;
        sfPopupBishopScore.BindingContext = pageModel.BishopScorePopupPageModel;

        Loaded += async (s, e) =>
        {
            if (BindingContext is PartographPageModel pageModel)
            {
                pageModel.CloseCompanionModalPopup += async () => { sfPopupCompanion.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenCompanionModalPopup += () => sfPopupCompanion.IsOpen = true;
                pageModel.ClosePainReliefModalPopup += async () => { sfPopupPainRelief.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenPainReliefModalPopup += () => sfPopupPainRelief.IsOpen = true;
                pageModel.CloseOralFluidModalPopup += async () => { sfPopupOralFluid.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenOralFluidModalPopup += () => sfPopupOralFluid.IsOpen = true;
                pageModel.ClosePostureModalPopup += async () => { sfPopupPosture.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenPostureModalPopup += () => sfPopupPosture.IsOpen = true;
                pageModel.CloseFetalPositionModalPopup += async () => { sfPopupFetalPosition.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenFetalPositionModalPopup += () => sfPopupFetalPosition.IsOpen = true;
                pageModel.CloseAmnioticFluidModalPopup += async () => { sfPopupAmnioticFluid.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenAmnioticFluidModalPopup += () => sfPopupAmnioticFluid.IsOpen = true;
                pageModel.CloseCaputModalPopup += async () => { sfPopupCaput.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenCaputModalPopup += () => sfPopupCaput.IsOpen = true;
                pageModel.CloseMouldingModalPopup += async () => { sfPopupMoulding.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenMouldingModalPopup += () => sfPopupMoulding.IsOpen = true;
                pageModel.CloseUrineModalPopup += async () => { sfPopupUrine.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenUrineModalPopup += () => sfPopupUrine.IsOpen = true;
                pageModel.CloseTemperatureModalPopup += async () => { sfPopupTemperature.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenTemperatureModalPopup += () => sfPopupTemperature.IsOpen = true;
                pageModel.CloseBpPulseModalPopup += async () => { sfPopupBpPulse.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenBpPulseModalPopup += () => sfPopupBpPulse.IsOpen = true;
                pageModel.CloseMedicationModalPopup += async () => { sfPopupMedication.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenMedicationModalPopup += () => sfPopupMedication.IsOpen = true;
                pageModel.CloseIVFluidModalPopup += async () => { sfPopupIVFluid.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenIVFluidModalPopup += () => sfPopupIVFluid.IsOpen = true;
                pageModel.CloseOxytocinModalPopup += async () => { sfPopupOxytocin.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenOxytocinModalPopup += () => sfPopupOxytocin.IsOpen = true;
                pageModel.CloseHeadDescentModalPopup += async () => { sfPopupHeadDescent.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenHeadDescentModalPopup += () => sfPopupHeadDescent.IsOpen = true;
                pageModel.CloseCervixDilatationModalPopup += async () => { sfPopupCervixDilatation.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenCervixDilatationModalPopup += () => sfPopupCervixDilatation.IsOpen = true;
                pageModel.CloseFHRContractionModalPopup += async () => { sfPopupFHRContraction.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenFHRContractionModalPopup += () => sfPopupFHRContraction.IsOpen = true;
                pageModel.CloseAssessmentModalPopup += async () => { sfPopupAssessment.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenAssessmentModalPopup += () => sfPopupAssessment.IsOpen = true;
                pageModel.ClosePlanModalPopup += async () => { sfPopupPlan.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenPlanModalPopup += () => sfPopupPlan.IsOpen = true;
                pageModel.CloseBishopScorePopup += async () => { sfPopupBishopScore.IsOpen = false; await pageModel.RefreshCommand.ExecuteAsync(null); };
                pageModel.OpenBishopScorePopup += () => sfPopupBishopScore.IsOpen = true;
            }
        };
    }
}