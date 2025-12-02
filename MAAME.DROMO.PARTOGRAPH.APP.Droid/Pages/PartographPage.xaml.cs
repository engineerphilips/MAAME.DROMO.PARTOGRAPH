using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

[QueryProperty(nameof(PatientId), "patientId")]
public partial class PartographPage : ContentPage
{
    public string PatientId { get; set; }

    public PartographPage(PartographPageModel pageModel, CompanionModalPageModel companionPageModel, PainReliefModalPageModel painReliefPageModel, FetalPositionModalPageModel fetalPositionPageModel, AmnioticFluidModalPageModel amnioticFluidPageModel, AssessmentPlanModalPageModel assessmentPlanPageModel, MouldingModalPageModel mouldingModalPageModel, UrineModalPageModel urineModalPageModel, TemperatureModalPageModel temperatureModalPageModel, OxytocinModalPageModel oxytocinModalPageModel, MedicationModalPageModel medicationModalPageModel, IVFluidModalPageModel ivFluidModalPageModel, HeadDescentModalPageModel headDescentPageModel, CervixDilatationModalPageModel cervixDilatationPageModel, BPPulseModalPageModel bPPulseModalPageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
        sfPopupPainRelief.BindingContext = painReliefPageModel;
        sfPopupCompanion.BindingContext = companionPageModel;
        sfPopupFetalPosition.BindingContext = fetalPositionPageModel;
        sfPopupAmnioticFluid.BindingContext = amnioticFluidPageModel;
        //sfPopupAssessmentPlan.BindingContext = assessmentPlanPageModel;
        sfPopupMoulding.BindingContext = mouldingModalPageModel;
        sfPopupUrine.BindingContext = urineModalPageModel;
        sfPopupTemperature.BindingContext = temperatureModalPageModel;
        sfPopupOxytocin.BindingContext = oxytocinModalPageModel;
        sfPopupMedication.BindingContext = medicationModalPageModel;
        sfPopupIVFluid.BindingContext = ivFluidModalPageModel;
        sfPopupHeadDescent.BindingContext = headDescentPageModel;
        sfPopupCervixDilatation.BindingContext = cervixDilatationPageModel;
        sfPopupBpPulse.BindingContext = bPPulseModalPageModel;

        //Loaded += (s, e) =>
        //{
        //    if (BindingContext is PartographPageModel pageModel)
        //    {
        //        var x = PatientId;
        //    }
        //};
    }

    private async void CompanionButton_Clicked(object sender, EventArgs e)
    {
        sfPopupCompanion.IsOpen = true;
        if (BindingContext is PartographPageModel pageModel)
            if (sfPopupCompanion.BindingContext is CompanionModalPageModel companionPageModel)
                if (pageModel._patient?.ID != null)
                {
                    companionPageModel._patient = pageModel._patient;
                    await companionPageModel.LoadPatient(pageModel._patient.ID);
                }
    }

    private void PainReliefButton_Clicked(object sender, EventArgs e)
    {
        sfPopupPainRelief.IsOpen = true;
    }

    private void OralFluidButton_Clicked(object sender, EventArgs e)
    {
        sfPopupOralFluid.IsOpen = true;
    }

    private void PostureButton_Clicked(object sender, EventArgs e)
    {
        sfPopupPosture.IsOpen = true;
    }

    private void AmnioticFluidButton_Clicked(object sender, EventArgs e)
    {
        sfPopupAmnioticFluid.IsOpen = true;
    }

    private void FetalPositionButton_Clicked(object sender, EventArgs e)
    {
        sfPopupFetalPosition.IsOpen = true;
    }

    private void CaputButton_Clicked(object sender, EventArgs e)
    {
        sfPopupCaput.IsOpen = true;
    }

    private void FHRContractionButton_Clicked(object sender, EventArgs e)
    {
        sfPopupFHRContraction.IsOpen = true;
    }

    private void UrineButton_Clicked(object sender, EventArgs e)
    {
        sfPopupUrine.IsOpen = true;
    }

    private void TemperatureButton_Clicked(object sender, EventArgs e)
    {
        sfPopupTemperature.IsOpen = true;
    }

    private void BpPulse_Clicked(object sender, EventArgs e)
    {
        sfPopupBpPulse.IsOpen = true;
    }

    private void Medication_Clicked(object sender, EventArgs e)
    {
        sfPopupMedication.IsOpen = true;
    }

    private void IVFluid_Clicked(object sender, EventArgs e)
    {
        sfPopupIVFluid.IsOpen = true;
    }

    private void Oxytocin_Clicked(object sender, EventArgs e)
    {
        sfPopupOxytocin.IsOpen = true;
    }

    private void HeadDescent_Clicked(object sender, EventArgs e)
    {
        sfPopupHeadDescent.IsOpen = true;
    }

    private void CervixDilatation_Clicked(object sender, EventArgs e)
    {
        sfPopupCervixDilatation.IsOpen = true;
    }
}