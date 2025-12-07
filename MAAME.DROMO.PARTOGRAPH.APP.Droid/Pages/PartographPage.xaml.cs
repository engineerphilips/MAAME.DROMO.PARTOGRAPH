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

        Loaded += (s, e) =>
        {
            if (BindingContext is PartographPageModel pageModel)
            {
                pageModel.CloseCompanionModalPopup += () => sfPopupCompanion.IsOpen = false;
                pageModel.OpenCompanionModalPopup += () => sfPopupCompanion.IsOpen = true;
                pageModel.ClosePainReliefModalPopup += () => sfPopupPainRelief.IsOpen = false;
                pageModel.OpenPainReliefModalPopup += () => sfPopupPainRelief.IsOpen = true;
            }
        };
    }
}