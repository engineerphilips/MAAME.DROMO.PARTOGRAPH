using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public partial class IVFluidModalPageModel : ObservableObject
    {
        public Partograph? _patient;
        private readonly IVFluidEntryRepository _ivFluidRepository;
        private readonly ModalErrorHandler _errorHandler;

        public IVFluidModalPageModel(IVFluidEntryRepository repository, ModalErrorHandler errorHandler)
        {
            _ivFluidRepository = repository;
            _errorHandler = errorHandler;
            SiteHealthy = true;
        }

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        [ObservableProperty]
        private string _fluidType = string.Empty;

        [ObservableProperty]
        private int _volumeInfused;

        [ObservableProperty]
        private string _rate = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        // WHO 2020 Enhancements
        [ObservableProperty]
        private decimal _rateMlPerHour;

        [ObservableProperty]
        private DateTime? _startTime;

        [ObservableProperty]
        private DateTime? _endTime;

        [ObservableProperty]
        private int? _durationMinutes;

        [ObservableProperty]
        private string _additives = string.Empty;

        [ObservableProperty]
        private string _additiveConcentration = string.Empty;

        [ObservableProperty]
        private string _additiveDose = string.Empty;

        [ObservableProperty]
        private string _ivSite = string.Empty;

        [ObservableProperty]
        private bool _siteHealthy = true;

        [ObservableProperty]
        private string _siteCondition = string.Empty;

        [ObservableProperty]
        private int _phlebitisScore;

        [ObservableProperty]
        private DateTime? _lastSiteAssessment;

        [ObservableProperty]
        private DateTime? _lastDressingChange;

        [ObservableProperty]
        private DateTime? _cannelaInsertionDate;

        [ObservableProperty]
        private string _indication = string.Empty;

        [ObservableProperty]
        private string _batchNumber = string.Empty;

        [ObservableProperty]
        private int _runningTotalInput;

        [ObservableProperty]
        private string _clinicalAlert = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public Action? ClosePopup { get; set; }

        internal async Task LoadPatient(Guid? patientId)
        {
            try
            {
                PatientName = $"Patient ID: {patientId}";
                var lastEntry = await _ivFluidRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    Rate = lastEntry.Rate;
                    FluidType = lastEntry.FluidType;
                    VolumeInfused = lastEntry.VolumeInfused;
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

                var entry = new IVFluidEntry
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    FluidType = FluidType,
                    VolumeInfused = VolumeInfused,
                    Rate = Rate,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID,
                    // WHO 2020 Enhancements
                    RateMlPerHour = RateMlPerHour,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    DurationMinutes = DurationMinutes,
                    Additives = Additives,
                    AdditiveConcentration = AdditiveConcentration,
                    AdditiveDose = AdditiveDose,
                    IVSite = IvSite,
                    SiteHealthy = SiteHealthy,
                    SiteCondition = SiteCondition,
                    PhlebitisScore = PhlebitisScore,
                    LastSiteAssessment = LastSiteAssessment,
                    LastDressingChange = LastDressingChange,
                    CannelaInsertionDate = CannelaInsertionDate,
                    Indication = Indication,
                    BatchNumber = BatchNumber,
                    RunningTotalInput = RunningTotalInput,
                    ClinicalAlert = ClinicalAlert
                };

                if (await _ivFluidRepository.SaveItemAsync(entry) != null)
                {
                    await AppShell.DisplayToastAsync("IV Fluid assessment saved successfully");
                    ResetFields();
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("IV Fluid assessment failed to save");
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
            FluidType = string.Empty;
            VolumeInfused = 0;
            Rate = string.Empty;
            Notes = string.Empty;
            // WHO 2020 fields
            RateMlPerHour = 0;
            StartTime = null;
            EndTime = null;
            DurationMinutes = null;
            Additives = string.Empty;
            AdditiveConcentration = string.Empty;
            AdditiveDose = string.Empty;
            IvSite = string.Empty;
            SiteHealthy = true;
            SiteCondition = string.Empty;
            PhlebitisScore = 0;
            LastSiteAssessment = null;
            LastDressingChange = null;
            CannelaInsertionDate = null;
            Indication = string.Empty;
            BatchNumber = string.Empty;
            RunningTotalInput = 0;
            ClinicalAlert = string.Empty;
        }
    }
}
