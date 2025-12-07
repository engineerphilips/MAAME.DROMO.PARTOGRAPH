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
    // ContractionsModalPageModel
    public partial class FHRContractionModalPageModel : ObservableObject, IQueryAttributable
    {
        private readonly ContractionRepository _contractionRepository;
        private readonly FHRRepository _fHRRepository;
        private readonly ILogger<FHRContractionModalPageModel> _logger;
        public Partograph? _patient;

        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        [ObservableProperty]
        private int _frequencyPer10Min; 
        [ObservableProperty]
        private int _durationSeconds;
        [ObservableProperty]
        private int _rate;

        [ObservableProperty]
        private string _contractionDisplay = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public Action? ClosePopup { get; set; }

        public FHRContractionModalPageModel(ContractionRepository contractionRepository, FHRRepository fHRRepository, ModalErrorHandler errorHandler)
        {
            _contractionRepository = contractionRepository;
            _fHRRepository = fHRRepository;
            _errorHandler = errorHandler;

            // Set default recorded by from preferences
            RecordedBy = Preferences.Get("StaffName", "Staff");
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                Guid? patientId = Guid.Parse(query["patientId"].ToString());
                LoadPatient(patientId).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        internal async Task LoadPatient(Guid? patientId)
        {
            try
            {
                // This would typically load from PatientRepository
                // For now, we'll use the patient ID directly
                PatientName = $"Patient ID: {patientId}";

                // Load last pain relief entry to prefill some values
                var lastContractionEntry = await _contractionRepository.GetLatestByPatientAsync(patientId);
                if (lastContractionEntry != null)
                {
                    //ContractionDisplay = lastContractionEntry.FrequencyPer10Min;
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

                var contractionEntry = new Contraction
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    FrequencyPer10Min = FrequencyPer10Min,
                    DurationSeconds = DurationSeconds,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID
                };

                var fhrEntry = new FHR
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    Rate = Rate,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID
                };

                var contraction = await _contractionRepository.SaveItemAsync(contractionEntry) != null;
                var fhr = await _fHRRepository.SaveItemAsync(fhrEntry) != null;

                if (contraction || fhr)
                {
                    await AppShell.DisplayToastAsync($"{(contraction && fhr ? "Contraction & FHR" : contraction ? "Contraction" : "FHR")} assessment saved successfully");

                    // Reset fields to default
                    ResetFields();

                    // Close the popup
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("Assessment failed to save");
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
            Rate = 0;
            FrequencyPer10Min = 0;
            DurationSeconds = 0;
            Notes = string.Empty;
        }
    }
}
