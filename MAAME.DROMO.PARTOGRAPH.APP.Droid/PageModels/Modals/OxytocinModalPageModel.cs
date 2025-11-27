using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public class OxytocinModalPageModel : INotifyPropertyChanged
    {
        private readonly OxytocinRepository _repository;
        private readonly ILogger<OxytocinModalPageModel> _logger;
        private Oxytocin _currentEntry;
        private Guid? _patientId;

        public OxytocinModalPageModel(OxytocinRepository repository, ILogger<OxytocinModalPageModel> logger)
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

        private double _doseMUnitsPerMin = 2.5;
        public double DoseMUnitsPerMin
        {
            get => _doseMUnitsPerMin;
            set
            {
                _doseMUnitsPerMin = value;
                OnPropertyChanged();
                UpdateDoseStatus();
            }
        }

        private double _totalVolumeInfused = 0;
        public double TotalVolumeInfused
        {
            get => _totalVolumeInfused;
            set
            {
                _totalVolumeInfused = value;
                OnPropertyChanged();
            }
        }

        private Color _doseColor = Colors.Green;
        public Color DoseColor
        {
            get => _doseColor;
            set { _doseColor = value; OnPropertyChanged(); }
        }

        private string _doseStatus = "Normal dose";
        public string DoseStatus
        {
            get => _doseStatus;
            set { _doseStatus = value; OnPropertyChanged(); }
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
            UpdateDoseStatus();
        }

        private void UpdateDoseStatus()
        {
            if (DoseMUnitsPerMin < 1)
            {
                DoseColor = Colors.Blue;
                DoseStatus = "Low dose - Starting dose";
            }
            else if (DoseMUnitsPerMin >= 1 && DoseMUnitsPerMin <= 8)
            {
                DoseColor = Colors.Green;
                DoseStatus = "Normal therapeutic dose";
            }
            else if (DoseMUnitsPerMin > 8 && DoseMUnitsPerMin <= 12)
            {
                DoseColor = Colors.Orange;
                DoseStatus = "High dose - Monitor closely";
            }
            else
            {
                DoseColor = Colors.Red;
                DoseStatus = "⚠️ Very high dose - Check for hyperstimulation";
            }
        }

        private async Task SaveEntry()
        {
            try
            {
                var entry = new Oxytocin
                {
                    PartographID = _patientId,
                    Time = RecordingDate.Date + RecordingTime,
                    HandlerName = await SecureStorage.GetAsync("CurrentUser") ?? "Unknown",
                    DoseMUnitsPerMin = (decimal)DoseMUnitsPerMin,
                    TotalVolumeInfused = (decimal)TotalVolumeInfused,
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
                _logger.LogError(ex, "Error saving oxytocin entry");
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
