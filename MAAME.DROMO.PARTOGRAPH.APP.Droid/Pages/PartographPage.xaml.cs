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

        Loaded += (s, e) =>
        {
            if (BindingContext is PartographPageModel pageModel)
            {
                pageModel.CloseCompanionModalPopup += () => sfPopupCompanion.IsOpen = false;
                pageModel.OpenCompanionModalPopup += () => sfPopupCompanion.IsOpen = true;
                pageModel.ClosePainReliefModalPopup += () => sfPopupPainRelief.IsOpen = false;
                pageModel.OpenPainReliefModalPopup += () => sfPopupPainRelief.IsOpen = true;
                pageModel.CloseOralFluidModalPopup += () => sfPopupOralFluid.IsOpen = false;
                pageModel.OpenOralFluidModalPopup += () => sfPopupOralFluid.IsOpen = true;
                pageModel.ClosePostureModalPopup += () => sfPopupPosture.IsOpen = false;
                pageModel.OpenPostureModalPopup += () => sfPopupPosture.IsOpen = true;
                pageModel.CloseFetalPositionModalPopup += () => sfPopupFetalPosition.IsOpen = false;
                pageModel.OpenFetalPositionModalPopup += () => sfPopupFetalPosition.IsOpen = true;
                pageModel.CloseAmnioticFluidModalPopup += () => sfPopupAmnioticFluid.IsOpen = false;
                pageModel.OpenAmnioticFluidModalPopup += () => sfPopupAmnioticFluid.IsOpen = true;
                pageModel.CloseCaputModalPopup += () => sfPopupCaput.IsOpen = false;
                pageModel.OpenCaputModalPopup += () => sfPopupCaput.IsOpen = true;
                pageModel.CloseMouldingModalPopup += () => sfPopupMoulding.IsOpen = false;
                pageModel.OpenMouldingModalPopup += () => sfPopupMoulding.IsOpen = true;
                pageModel.CloseUrineModalPopup += () => sfPopupUrine.IsOpen = false;
                pageModel.OpenUrineModalPopup += () => sfPopupUrine.IsOpen = true;
                pageModel.CloseTemperatureModalPopup += () => sfPopupTemperature.IsOpen = false;
                pageModel.OpenTemperatureModalPopup += () => sfPopupTemperature.IsOpen = true;
                pageModel.CloseBpPulseModalPopup += () => sfPopupBpPulse.IsOpen = false;
                pageModel.OpenBpPulseModalPopup += () => sfPopupBpPulse.IsOpen = true;
                pageModel.CloseMedicationModalPopup += () => sfPopupMedication.IsOpen = false;
                pageModel.OpenMedicationModalPopup += () => sfPopupMedication.IsOpen = true;
                pageModel.CloseIVFluidModalPopup += () => sfPopupIVFluid.IsOpen = false;
                pageModel.OpenIVFluidModalPopup += () => sfPopupIVFluid.IsOpen = true;
                pageModel.CloseOxytocinModalPopup += () => sfPopupOxytocin.IsOpen = false;
                pageModel.OpenOxytocinModalPopup += () => sfPopupOxytocin.IsOpen = true;
                pageModel.CloseHeadDescentModalPopup += () => sfPopupHeadDescent.IsOpen = false;
                pageModel.OpenHeadDescentModalPopup += () => sfPopupHeadDescent.IsOpen = true;
                pageModel.CloseCervixDilatationModalPopup += () => sfPopupCervixDilatation.IsOpen = false;
                pageModel.OpenCervixDilatationModalPopup += () => sfPopupCervixDilatation.IsOpen = true;
                pageModel.CloseFHRContractionModalPopup += () => sfPopupFHRContraction.IsOpen = false;
                pageModel.OpenFHRContractionModalPopup += () => sfPopupFHRContraction.IsOpen = true;
                pageModel.CloseAssessmentModalPopup += () => sfPopupAssessment.IsOpen = false;
                pageModel.OpenAssessmentModalPopup += () => sfPopupAssessment.IsOpen = true;
                pageModel.ClosePlanModalPopup += () => sfPopupPlan.IsOpen = false;
                pageModel.OpenPlanModalPopup += () => sfPopupPlan.IsOpen = true;
            }
        };
    }
}