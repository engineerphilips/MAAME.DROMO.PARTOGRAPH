using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    // ContractionsModalPageModel
    public class ContractionsModalPageModel : INotifyPropertyChanged
    {
        private readonly ContractionRepository _repository;
        private readonly ILogger<ContractionsModalPageModel> _logger;
        private Contraction _currentEntry;
        private Guid? _patientId;

        public Action? ClosePopup { get; set; }

        public ContractionsModalPageModel(ContractionRepository repository, ILogger<ContractionsModalPageModel> logger)
        {
            _repository = repository;
            _logger = logger;
            InitializeCommands();
            InitializeData();
        }

        // Properties
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        public DateOnly RecordingDate
        {
            get => _recordingDate;
            set { _recordingDate = value; OnPropertyChanged(); }
        }

        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        public TimeSpan RecordingTime
        {
            get => _recordingTime;
            set { _recordingTime = value; OnPropertyChanged(); }
        }

        private int _frequencyPer10Min = 3;
        public int FrequencyPer10Min
        {
            get => _frequencyPer10Min;
            set
            {
                _frequencyPer10Min = value;
                OnPropertyChanged();
                UpdateFrequencyDisplay();
                UpdateProgressIndicator();
            }
        }

        private string _frequencyDisplay = "3 contractions";
        public string FrequencyDisplay
        {
            get => _frequencyDisplay;
            set { _frequencyDisplay = value; OnPropertyChanged(); }
        }

        private int _durationSeconds = 45;
        public int DurationSeconds
        {
            get => _durationSeconds;
            set
            {
                _durationSeconds = value;
                OnPropertyChanged();
                UpdateProgressIndicator();
            }
        }

        private bool _isMild;
        public bool IsMild
        {
            get => _isMild;
            set { _isMild = value; OnPropertyChanged(); }
        }

        private bool _isModerate = true;
        public bool IsModerate
        {
            get => _isModerate;
            set { _isModerate = value; OnPropertyChanged(); }
        }

        private bool _isStrong;
        public bool IsStrong
        {
            get => _isStrong;
            set { _isStrong = value; OnPropertyChanged(); }
        }

        private bool _isRegular = true;
        public bool IsRegular
        {
            get => _isRegular;
            set { _isRegular = value; OnPropertyChanged(); }
        }

        private bool _isIrregular;
        public bool IsIrregular
        {
            get => _isIrregular;
            set { _isIrregular = value; OnPropertyChanged(); }
        }

        private bool _palpableAtRest;
        public bool PalpableAtRest
        {
            get => _palpableAtRest;
            set
            {
                _palpableAtRest = value;
                OnPropertyChanged();
                UpdateProgressIndicator();
            }
        }

        public ObservableCollection<string> CervixEffectOptions { get; } = new ObservableCollection<string>
        {
            "Effective - Good Progress",
            "Moderate Effect",
            "Minimal Effect",
            "No Effect",
            "Unknown"
        };

        private string _selectedCervixEffect = "Moderate Effect";
        public string SelectedCervixEffect
        {
            get => _selectedCervixEffect;
            set { _selectedCervixEffect = value; OnPropertyChanged(); }
        }

        private bool _coordinated = true;
        public bool Coordinated
        {
            get => _coordinated;
            set { _coordinated = value; OnPropertyChanged(); }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        private Color _progressColor = Colors.Green;
        public Color ProgressColor
        {
            get => _progressColor;
            set { _progressColor = value; OnPropertyChanged(); }
        }

        private string _progressMessage = "Normal labor progress";
        public string ProgressMessage
        {
            get => _progressMessage;
            set { _progressMessage = value; OnPropertyChanged(); }
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
            UpdateFrequencyDisplay();
            UpdateProgressIndicator();
        }

        private void UpdateFrequencyDisplay()
        {
            FrequencyDisplay = $"{FrequencyPer10Min} contractions";
        }

        private void UpdateProgressIndicator()
        {
            // Assess labor progress based on contractions
            if (PalpableAtRest)
            {
                ProgressColor = Colors.Red;
                ProgressMessage = "⚠️ Hypertonic uterus - requires immediate evaluation";
            }
            else if (FrequencyPer10Min < 2)
            {
                ProgressColor = Colors.Orange;
                ProgressMessage = "Inadequate contractions - consider augmentation";
            }
            else if (FrequencyPer10Min >= 2 && FrequencyPer10Min <= 5 && DurationSeconds >= 40 && DurationSeconds <= 60)
            {
                ProgressColor = Colors.Green;
                ProgressMessage = "Good labor progress";
            }
            else if (FrequencyPer10Min > 5)
            {
                ProgressColor = Colors.Orange;
                ProgressMessage = "Tachysystole - monitor closely";
            }
            else
            {
                ProgressColor = Colors.Blue;
                ProgressMessage = "Monitor progress";
            }
        }

        private string GetStrength()
        {
            if (IsStrong) return "Strong";
            if (IsModerate) return "Moderate";
            return "Mild";
        }

        private string GetRegularity()
        {
            return IsRegular ? "Regular" : "Irregular";
        }

        private async Task SaveEntry()
        {
            try
            {
                var entry = new Contraction
                {
                    PartographID = _patientId,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    HandlerName = await SecureStorage.GetAsync("CurrentUser") ?? "Unknown",
                    FrequencyPer10Min = FrequencyPer10Min,
                    DurationSeconds = DurationSeconds,
                    //Strength = GetStrength(),
                    //Regularity = GetRegularity(),
                    //PalpableAtRest = PalpableAtRest,
                    //EffectOnCervix = SelectedCervixEffect,
                    //Coordinated = Coordinated,
                    Notes = Notes ?? string.Empty
                };

                if (_currentEntry != null)
                {
                    entry.ID = _currentEntry.ID;
                }

                if (await _repository.SaveItemAsync(entry) != null)
                {
                    await AppShell.DisplayToastAsync("Contraction assessment saved successfully");

                    // Reset fields to default
                    ResetFields();

                    // Close the popup
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("Contraction assessment failed to save");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving contraction entry");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to save entry", "OK");
            }
        }

        private async Task Cancel()
        {
            ResetFields();
            ClosePopup?.Invoke();
        }

        private void ResetFields()
        {
            RecordingDate = DateOnly.FromDateTime(DateTime.Now);
            RecordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            FrequencyPer10Min = 3;
            DurationSeconds = 45;
            IsMild = false;
            IsModerate = true;
            IsStrong = false;
            IsRegular = true;
            IsIrregular = false;
            PalpableAtRest = false;
            SelectedCervixEffect = "Moderate Effect";
            Coordinated = true;
            Notes = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
