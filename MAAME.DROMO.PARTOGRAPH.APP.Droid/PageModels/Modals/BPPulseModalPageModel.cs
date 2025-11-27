using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public class BPPulseModalPageModel : INotifyPropertyChanged
    {
        private readonly BPRepository _repository;
        private readonly ILogger<BPPulseModalPageModel> _logger;
        private BP _currentEntry;
        private Guid? _patientId;

        public BPPulseModalPageModel(BPRepository repository, ILogger<BPPulseModalPageModel> logger)
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

        private double _pulse = 80;
        public double Pulse
        {
            get => _pulse;
            set
            {
                _pulse = value;
                OnPropertyChanged();
                UpdatePulseStatus();
            }
        }

        private double _systolic = 120;
        public double Systolic
        {
            get => _systolic;
            set
            {
                _systolic = value;
                OnPropertyChanged();
                UpdateBPStatus();
            }
        }

        private double _diastolic = 80;
        public double Diastolic
        {
            get => _diastolic;
            set
            {
                _diastolic = value;
                OnPropertyChanged();
                UpdateBPStatus();
            }
        }

        private Color _pulseColor = Colors.Green;
        public Color PulseColor
        {
            get => _pulseColor;
            set { _pulseColor = value; OnPropertyChanged(); }
        }

        private string _pulseStatus = "Normal";
        public string PulseStatus
        {
            get => _pulseStatus;
            set { _pulseStatus = value; OnPropertyChanged(); }
        }

        private Color _bpColor = Colors.Green;
        public Color BPColor
        {
            get => _bpColor;
            set { _bpColor = value; OnPropertyChanged(); }
        }

        private string _bpStatus = "Normal";
        public string BPStatus
        {
            get => _bpStatus;
            set { _bpStatus = value; OnPropertyChanged(); }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
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
            UpdatePulseStatus();
            UpdateBPStatus();
        }

        private void UpdatePulseStatus()
        {
            if (Pulse < 60)
            {
                PulseColor = Colors.Orange;
                PulseStatus = "Bradycardia - Low pulse";
            }
            else if (Pulse >= 60 && Pulse <= 100)
            {
                PulseColor = Colors.Green;
                PulseStatus = "Normal pulse (60-100 bpm)";
            }
            else if (Pulse > 100 && Pulse <= 120)
            {
                PulseColor = Colors.Orange;
                PulseStatus = "Tachycardia - Monitor closely";
            }
            else
            {
                PulseColor = Colors.Red;
                PulseStatus = "⚠️ Severe tachycardia - Requires attention";
            }
        }

        private void UpdateBPStatus()
        {
            // Check for hypertension (pre-eclampsia threshold)
            if (Systolic >= 140 || Diastolic >= 90)
            {
                if (Systolic >= 160 || Diastolic >= 110)
                {
                    BPColor = Colors.Red;
                    BPStatus = "⚠️ Severe hypertension - Immediate action required";
                }
                else
                {
                    BPColor = Colors.Orange;
                    BPStatus = "Mild hypertension - Monitor closely";
                }
            }
            else if (Systolic < 90 || Diastolic < 60)
            {
                BPColor = Colors.Orange;
                BPStatus = "Hypotension - Monitor closely";
            }
            else
            {
                BPColor = Colors.Green;
                BPStatus = "Normal blood pressure";
            }
        }

        private async Task SaveEntry()
        {
            try
            {
                var entry = new BP
                {
                    PartographID = _patientId,
                    Time = RecordingDate.Date + RecordingTime,
                    HandlerName = await SecureStorage.GetAsync("CurrentUser") ?? "Unknown",
                    Pulse = (int)Math.Round(Pulse),
                    Systolic = (int)Math.Round(Systolic),
                    Diastolic = (int)Math.Round(Diastolic),
                    Notes = Notes ?? string.Empty
                };

                if (_currentEntry != null)
                {
                    entry.ID = _currentEntry.ID;
                }

                await _repository.SaveItemAsync(entry);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving BP/Pulse entry");
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
