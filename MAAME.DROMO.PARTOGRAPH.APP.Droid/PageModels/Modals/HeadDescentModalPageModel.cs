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
    public partial class HeadDescentModalPageModel : ObservableObject
    {
        public Partograph? _patient;
        private readonly HeadDescentRepository _headDescentRepository;
        private readonly ModalErrorHandler _errorHandler;

        public Action? ClosePopup { get; set; }

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        /// <summary>
        /// Collection of measurement history items for display
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<MeasurementHistoryItem> _measurementHistory = new();

        /// <summary>
        /// Indicates whether there is any history to display
        /// </summary>
        [ObservableProperty]
        private bool _hasHistory;

        /// <summary>
        /// The maximum allowed descent value based on previous measurements.
        /// As labor progresses, descent should drop (head descends into pelvis).
        /// Using the "fifths palpable" scale: 5/5 (high) to 0/5 (fully descended).
        /// </summary>
        [ObservableProperty]
        private int _maximumDescent = 5;

        /// <summary>
        /// Validation message shown when user tries to enter invalid value
        /// </summary>
        [ObservableProperty]
        private string _validationMessage = string.Empty;

        /// <summary>
        /// Whether the validation message should be shown
        /// </summary>
        [ObservableProperty]
        private bool _showValidationMessage;

        private int _station;

        public int Station
        {
            get => _station;
            set
            {
                // Validate that descent is dropping (labor progression rule)
                // As labor progresses, the fifths palpable should decrease (5 -> 0)
                if (value > MaximumDescent && MaximumDescent < 5)
                {
                    ValidationMessage = $"Descent cannot increase. Last recorded value was {MaximumDescent}/5. As labor progresses, descent should drop.";
                    ShowValidationMessage = true;
                    return;
                }
                ShowValidationMessage = false;
                ValidationMessage = string.Empty;
                SetProperty(ref _station, value);
            }
        }

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public HeadDescentModalPageModel(HeadDescentRepository headDescentRepository, ModalErrorHandler errorHandler)
        {
            _headDescentRepository = headDescentRepository;
            _errorHandler = errorHandler;

            // Set default recorded by from preferences
            //RecordedBy = Preferences.Get("StaffName", "Staff");
        }

        //public void ApplyQueryAttributes(IDictionary<string, object> query)
        //{
        //    if (query.ContainsKey("patientId"))
        //    {
        //        Guid? patientId = Guid.Parse(Convert.ToString(query["patientId"]));
        //        LoadPatient(patientId).FireAndForgetSafeAsync(_errorHandler);
        //    }
        //}

        public async Task LoadPatient(Guid? patientId)
        {

            try
            {
                // This would typically load from PatientRepository
                // For now, we'll use the patient ID directly
                PatientName = $"Patient ID: {patientId}";

                // Load measurement history
                await LoadMeasurementHistory(patientId);

                // Load last entry to set maximum descent (labor progression rule - descent should drop)
                var lastEntry = await _headDescentRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    // Set maximum descent - labor should progress (descent drops)
                    MaximumDescent = lastEntry.Station;
                    // Pre-fill with last known value
                    _station = lastEntry.Station;
                    OnPropertyChanged(nameof(Station));
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
        }

        /// <summary>
        /// Loads the measurement history for display in the modal.
        /// Shows values, date/time (time only if today), and who recorded it.
        /// </summary>
        private async Task LoadMeasurementHistory(Guid? patientId)
        {
            try
            {
                var historyEntries = await _headDescentRepository.ListByPatientAsync(patientId);

                MeasurementHistory.Clear();

                foreach (var entry in historyEntries.Take(10)) // Show last 10 entries
                {
                    var historyItem = new MeasurementHistoryItem(
                        $"{entry.Station}/5",
                        entry.Time,
                        entry.HandlerName ?? "Unknown"
                    );
                    MeasurementHistory.Add(historyItem);
                }

                HasHistory = MeasurementHistory.Count > 0;
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

            if (Station < 0)
            {
                _errorHandler.HandleError(new Exception("Head descent status is not selected."));
                return;
            }

            try
            {
                IsBusy = true;

                var entry = new HeadDescent
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime), 
                    Station = Station,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID
                };

                if (await _headDescentRepository.SaveItemAsync(entry) != null)
                {
                    await AppShell.DisplayToastAsync("Head descent assessment saved successfully");

                    // Reset fields to default
                    ResetFields();

                    // Close the popup
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("Head descent assessment failed to save");
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
            Station = -1;
            Notes = string.Empty;
        }
    }
}
