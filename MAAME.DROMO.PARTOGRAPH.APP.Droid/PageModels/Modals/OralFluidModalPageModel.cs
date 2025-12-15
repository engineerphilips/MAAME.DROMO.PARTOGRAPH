using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public partial class OralFluidModalPageModel : ObservableObject, IQueryAttributable
    {
        public Partograph? _patient;
        private readonly OralFluidRepository _oralFluidRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        [ObservableProperty]
        private int _oralFluidIndex = -1;

        [ObservableProperty]
        private string _oralFluidDisplay = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        // WHO 2020 Enhancements
        [ObservableProperty]
        private string _fluidType = string.Empty;

        [ObservableProperty]
        private int _amountMl;

        [ObservableProperty]
        private int _runningTotalOralIntake;

        [ObservableProperty]
        private bool _tolerated = true;

        [ObservableProperty]
        private bool _vomiting;

        [ObservableProperty]
        private bool _nausea;

        [ObservableProperty]
        private int? _vomitingEpisodes;

        [ObservableProperty]
        private string _vomitContent = string.Empty;

        [ObservableProperty]
        private bool _foodOffered;

        [ObservableProperty]
        private bool _foodConsumed;

        [ObservableProperty]
        private string _foodType = string.Empty;

        [ObservableProperty]
        private bool _nbm;

        [ObservableProperty]
        private string _nbmReason = string.Empty;

        [ObservableProperty]
        private string _restrictions = string.Empty;

        [ObservableProperty]
        private string _restrictionReason = string.Empty;

        [ObservableProperty]
        private bool _patientRequestedFluids;

        [ObservableProperty]
        private bool _patientDeclinedFluids;

        [ObservableProperty]
        private bool _aspirationRiskAssessed;

        [ObservableProperty]
        private string _aspirationRiskLevel = string.Empty;

        [ObservableProperty]
        private string _clinicalAlert = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public Action? ClosePopup { get; set; }

        public OralFluidModalPageModel(OralFluidRepository oralFluidRepository, ModalErrorHandler errorHandler)
        {
            _oralFluidRepository = oralFluidRepository;
            _errorHandler = errorHandler;
            RecordedBy = Preferences.Get("StaffName", "Staff");
            Tolerated = true;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                Guid? patientId = Guid.Parse(Convert.ToString(query["patientId"]));
                LoadPatient(patientId).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        public async Task LoadPatient(Guid? patientId)
        {
            try
            {
                PatientName = $"Patient ID: {patientId}";
                var lastEntry = await _oralFluidRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    OralFluidDisplay = lastEntry?.OralFluidDisplay;
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (_patient == null)
            {
                _errorHandler.HandleError(new Exception("Patient information not loaded."));
                return;
            }

            try
            {
                IsBusy = true;

                var entry = new OralFluidEntry
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    OralFluid = OralFluidIndex == 0 ? "N" : OralFluidIndex == 1 ? "Y" : OralFluidIndex == 2 ? "D" : null,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID,
                    // WHO 2020 Enhancements
                    FluidType = FluidType,
                    AmountMl = AmountMl,
                    RunningTotalOralIntake = RunningTotalOralIntake,
                    Tolerated = Tolerated,
                    Vomiting = Vomiting,
                    Nausea = Nausea,
                    VomitingEpisodes = VomitingEpisodes,
                    VomitContent = VomitContent,
                    FoodOffered = FoodOffered,
                    FoodConsumed = FoodConsumed,
                    FoodType = FoodType,
                    NBM = Nbm,
                    NBMReason = NbmReason,
                    Restrictions = Restrictions,
                    RestrictionReason = RestrictionReason,
                    PatientRequestedFluids = PatientRequestedFluids,
                    PatientDeclinedFluids = PatientDeclinedFluids,
                    AspirationRiskAssessed = AspirationRiskAssessed,
                    AspirationRiskLevel = AspirationRiskLevel,
                    ClinicalAlert = ClinicalAlert
                };

                if (await _oralFluidRepository.SaveItemAsync(entry) != null)
                {
                    await AppShell.DisplayToastAsync("Oral fluid assessment saved successfully");
                    ResetFields();
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("Oral fluid assessment failed to save");
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            ResetFields();
            ClosePopup?.Invoke();
        }

        private void ResetFields()
        {
            RecordingDate = DateOnly.FromDateTime(DateTime.Now);
            RecordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            OralFluidIndex = -1;
            Notes = string.Empty;
            // WHO 2020 fields
            FluidType = string.Empty;
            AmountMl = 0;
            RunningTotalOralIntake = 0;
            Tolerated = true;
            Vomiting = false;
            Nausea = false;
            VomitingEpisodes = null;
            VomitContent = string.Empty;
            FoodOffered = false;
            FoodConsumed = false;
            FoodType = string.Empty;
            Nbm = false;
            NbmReason = string.Empty;
            Restrictions = string.Empty;
            RestrictionReason = string.Empty;
            PatientRequestedFluids = false;
            PatientDeclinedFluids = false;
            AspirationRiskAssessed = false;
            AspirationRiskLevel = string.Empty;
            ClinicalAlert = string.Empty;
        }
    }
}
