using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    // FHRDecelerationModalPageModel
    public class FHRDecelerationModalPageModel : INotifyPropertyChanged
    {
        private readonly FHRDecelerationRepository _repository;
        private readonly ILogger<FHRDecelerationModalPageModel> _logger;
        private FHRDecelerationEntry _currentEntry;
        private int _patientId;

        public FHRDecelerationModalPageModel(FHRDecelerationRepository repository, ILogger<FHRDecelerationModalPageModel> logger)
        {
            _repository = repository;
            _logger = logger;
            InitializeCommands();
            InitializeData();
        }

        // Properties
        private DateTime _recordingDate = DateTime.Now;
        public DateTime RecordingDate
        {
            get => _recordingDate;
            set { _recordingDate = value; OnPropertyChanged(); }
        }

        private TimeSpan _recordingTime = DateTime.Now.TimeOfDay;
        public TimeSpan RecordingTime
        {
            get => _recordingTime;
            set { _recordingTime = value; OnPropertyChanged(); }
        }

        private bool _decelerationsPresent;
        public bool DecelerationsPresent
        {
            get => _decelerationsPresent;
            set
            {
                _decelerationsPresent = value;
                OnPropertyChanged();
                UpdateWarning();
            }
        }

        public ObservableCollection<string> DecelerationTypes { get; } = new ObservableCollection<string>
        {
            "Early", "Variable", "Late", "Prolonged"
        };

        private string _selectedDecelerationType = "Variable";
        public string SelectedDecelerationType
        {
            get => _selectedDecelerationType;
            set
            {
                _selectedDecelerationType = value;
                OnPropertyChanged();
                UpdateWarning();
            }
        }

        private bool _isMild = true;
        public bool IsMild
        {
            get => _isMild;
            set { _isMild = value; OnPropertyChanged(); }
        }

        private bool _isModerate;
        public bool IsModerate
        {
            get => _isModerate;
            set { _isModerate = value; OnPropertyChanged(); }
        }

        private bool _isSevere;
        public bool IsSevere
        {
            get => _isSevere;
            set
            {
                _isSevere = value;
                OnPropertyChanged();
                UpdateWarning();
            }
        }

        private int _duration;
        public int Duration
        {
            get => _duration;
            set { _duration = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> RecoveryOptions { get; } = new ObservableCollection<string>
        {
            "Quick", "Slow", "Incomplete", "Absent"
        };

        private string _selectedRecovery = "Quick";
        public string SelectedRecovery
        {
            get => _selectedRecovery;
            set
            {
                _selectedRecovery = value;
                OnPropertyChanged();
                UpdateWarning();
            }
        }

        private bool _requiresAction;
        public bool RequiresAction
        {
            get => _requiresAction;
            set { _requiresAction = value; OnPropertyChanged(); }
        }

        private string _actionTaken;
        public string ActionTaken
        {
            get => _actionTaken;
            set { _actionTaken = value; OnPropertyChanged(); }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        private bool _showWarning;
        public bool ShowWarning
        {
            get => _showWarning;
            set { _showWarning = value; OnPropertyChanged(); }
        }

        private string _warningMessage;
        public string WarningMessage
        {
            get => _warningMessage;
            set { _warningMessage = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private void InitializeCommands()
        {
            SaveCommand = new Command(async () => await SaveEntry());
            CancelCommand = new Command(async () => await Cancel());
        }

        private void InitializeData()
        {
            // Initialize default values
        }

        private void UpdateWarning()
        {
            ShowWarning = false;
            WarningMessage = string.Empty;

            if (!DecelerationsPresent) return;

            // Check for concerning patterns
            if (SelectedDecelerationType == "Late" || SelectedDecelerationType == "Prolonged")
            {
                ShowWarning = true;
                WarningMessage = "⚠️ Late or prolonged decelerations require immediate evaluation";
                RequiresAction = true;
            }
            else if (IsSevere)
            {
                ShowWarning = true;
                WarningMessage = "⚠️ Severe decelerations require immediate evaluation";
                RequiresAction = true;
            }
            else if (SelectedRecovery == "Incomplete" || SelectedRecovery == "Absent")
            {
                ShowWarning = true;
                WarningMessage = "⚠️ Poor recovery pattern requires immediate evaluation";
                RequiresAction = true;
            }
        }

        private string GetSeverity()
        {
            if (IsSevere) return "Severe";
            if (IsModerate) return "Moderate";
            return "Mild";
        }

        private async Task SaveEntry()
        {
            try
            {
                var entry = new FHRDecelerationEntry
                {
                    PatientID = _patientId,
                    RecordedTime = RecordingDate.Date + RecordingTime,
                    RecordedBy = await SecureStorage.GetAsync("CurrentUser") ?? "Unknown",
                    DecelerationsPresent = DecelerationsPresent,
                    DecelerationType = DecelerationsPresent ? SelectedDecelerationType : null,
                    Severity = DecelerationsPresent ? GetSeverity() : null,
                    Duration = Duration,
                    Recovery = DecelerationsPresent ? SelectedRecovery : null,
                    RequiresAction = RequiresAction,
                    ActionTaken = ActionTaken ?? string.Empty,
                    Notes = Notes ?? string.Empty
                };

                if (_currentEntry != null)
                {
                    entry.ID = _currentEntry.ID;
                    await _repository.SaveItemAsync(entry);
                }
                else
                {
                    await _repository.SaveItemAsync(entry);
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving FHR deceleration entry");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to save entry", "OK");
            }
        }

        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
