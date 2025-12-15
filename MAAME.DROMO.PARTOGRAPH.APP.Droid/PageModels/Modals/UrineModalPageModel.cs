using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public partial class UrineModalPageModel : ObservableObject
    {
        public Partograph? _patient;
        private readonly UrineRepository _urineRepository;
        private readonly ModalErrorHandler _errorHandler;

        public UrineModalPageModel(UrineRepository repository, ModalErrorHandler errorHandler)
        {
            _urineRepository = repository;
            _errorHandler = errorHandler;
        }

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        [ObservableProperty]
        private int _proteinIndex = -1;

        [ObservableProperty]
        private int _acetoneIndex = -1;

        [ObservableProperty]
        private string _proteinDisplay = string.Empty;

        [ObservableProperty]
        private string _acetoneDisplay = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        // WHO 2020 Enhanced Urine Assessment Fields

        // Original fields
        [ObservableProperty]
        private int _outputMl;

        [ObservableProperty]
        private string _color = "Yellow";

        [ObservableProperty]
        private string _protein = "Nil";

        [ObservableProperty]
        private string _ketones = "Nil";

        [ObservableProperty]
        private string _glucose = "Nil";

        [ObservableProperty]
        private string _specificGravity = string.Empty;

        [ObservableProperty]
        private string _voidingMethod = "Spontaneous";

        [ObservableProperty]
        private bool _bladderPalpable;

        [ObservableProperty]
        private DateTime? _lastVoided;

        [ObservableProperty]
        private string _clinicalAlert = string.Empty;

        // Volume and Pattern
        [ObservableProperty]
        private DateTime? _voidingTime;

        [ObservableProperty]
        private int? _timeSinceLastVoidMinutes;

        [ObservableProperty]
        private int? _cumulativeOutputMl;

        [ObservableProperty]
        private decimal? _hourlyOutputRate;

        [ObservableProperty]
        private bool _oliguria;

        [ObservableProperty]
        private bool _anuria;

        [ObservableProperty]
        private int? _consecutiveOliguriaHours;

        // Appearance
        [ObservableProperty]
        private string _clarity = string.Empty;

        [ObservableProperty]
        private bool _hematuria;

        [ObservableProperty]
        private bool _concentrated;

        [ObservableProperty]
        private bool _dilute;

        [ObservableProperty]
        private string _odor = string.Empty;

        // Dipstick Results
        [ObservableProperty]
        private string _bloodDipstick = "Nil";

        [ObservableProperty]
        private string _leukocytesDipstick = "Nil";

        [ObservableProperty]
        private string _nitritesDipstick = "Nil";

        [ObservableProperty]
        private float? _phLevel;

        // Bladder Management
        [ObservableProperty]
        private bool _bladderFullness;

        [ObservableProperty]
        private string _bladderFullnessLevel = string.Empty;

        [ObservableProperty]
        private bool _difficultVoiding;

        [ObservableProperty]
        private bool _urinaryRetention;

        [ObservableProperty]
        private bool _catheterizationIndicated;

        [ObservableProperty]
        private DateTime? _lastCatheterizationTime;

        [ObservableProperty]
        private string _catheterType = string.Empty;

        // Pre-eclampsia Monitoring
        [ObservableProperty]
        private bool _proteinuriaNewOnset;

        [ObservableProperty]
        private bool _proteinuriaWorsening;

        [ObservableProperty]
        private DateTime? _firstProteinDetectedTime;

        [ObservableProperty]
        private bool _laboratorySampleSent;

        [ObservableProperty]
        private string _proteinCreatinineRatio = string.Empty;

        // Dehydration/Ketosis Assessment
        [ObservableProperty]
        private bool _signsOfDehydration;

        [ObservableProperty]
        private bool _prolongedLabor;

        [ObservableProperty]
        private bool _increasedKetoneTrend;

        [ObservableProperty]
        private DateTime? _firstKetoneDetectedTime;

        // Fluid Balance
        [ObservableProperty]
        private int? _totalOralIntakeMl;

        [ObservableProperty]
        private int? _totalIVIntakeMl;

        [ObservableProperty]
        private int? _fluidBalanceMl;

        // Clinical Response
        [ObservableProperty]
        private bool _encourageOralFluids;

        [ObservableProperty]
        private bool _ivFluidsStarted;

        [ObservableProperty]
        private bool _catheterizationPerformed;

        [ObservableProperty]
        private bool _nephrologyConsultRequired;

        public Action? ClosePopup { get; set; }

        internal async Task LoadPatient(Guid? patientId)
        {
            try
            {
                // This would typically load from PatientRepository
                // For now, we'll use the patient ID directly
                PatientName = $"Patient ID: {patientId}";

                // Load last pain relief entry to prefill some values
                var lastEntry = await _urineRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    AcetoneDisplay = lastEntry.Ketones;
                    ProteinDisplay = lastEntry.Protein;
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

                var entry = new Urine
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    Protein = ProteinIndex == 0 ? "P-" : ProteinIndex == 1 ? "P" : ProteinIndex == 2 ? "P1+" : ProteinIndex == 3 ? "P2+" : ProteinIndex == 4 ? "P3+" : null,
                    Ketones = AcetoneIndex == 0 ? "P-" : AcetoneIndex == 1 ? "P" : AcetoneIndex == 2 ? "P1+" : AcetoneIndex == 3 ? "P2+" : AcetoneIndex == 4 ? "P3+" : null,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID
                };

                if (await _urineRepository.SaveItemAsync(entry) != null)
                {
                    await AppShell.DisplayToastAsync("Urine assessment saved successfully");

                    // Reset fields to default
                    ResetFields();

                    // Close the popup
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("Urine assessment failed to save");
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
            ProteinIndex = -1;
            AcetoneIndex = -1;
            Notes = string.Empty;
        }
    }
}
