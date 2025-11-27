using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public class TemperatureModalPageModel : INotifyPropertyChanged
    {
        private readonly TemperatureRepository _repository;
        private readonly ILogger<TemperatureModalPageModel> _logger;
        private Temperature _currentEntry;
        private Guid? _patientId;

        public TemperatureModalPageModel(TemperatureRepository repository, ILogger<TemperatureModalPageModel> logger)
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

        private double _temperature = 37.0;
        public double Temperature
        {
            get => _temperature;
            set
            {
                _temperature = value;
                OnPropertyChanged();
                UpdateTemperatureStatus();
            }
        }

        private Color _temperatureColor = Colors.Green;
        public Color TemperatureColor
        {
            get => _temperatureColor;
            set { _temperatureColor = value; OnPropertyChanged(); }
        }

        private string _temperatureStatus = "Normal";
        public string TemperatureStatus
        {
            get => _temperatureStatus;
            set { _temperatureStatus = value; OnPropertyChanged(); }
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
            UpdateTemperatureStatus();
        }

        private void UpdateTemperatureStatus()
        {
            if (Temperature < 36.0)
            {
                TemperatureColor = Colors.Blue;
                TemperatureStatus = "Hypothermia - Monitor closely";
            }
            else if (Temperature >= 36.0 && Temperature <= 37.5)
            {
                TemperatureColor = Colors.Green;
                TemperatureStatus = "Normal temperature";
            }
            else if (Temperature > 37.5 && Temperature <= 38.0)
            {
                TemperatureColor = Colors.Orange;
                TemperatureStatus = "Mild pyrexia - Monitor";
            }
            else
            {
                TemperatureColor = Colors.Red;
                TemperatureStatus = "⚠️ Fever - Requires attention";
            }
        }

        private async Task SaveEntry()
        {
            try
            {
                var entry = new Temperature
                {
                    PartographID = _patientId,
                    Time = RecordingDate.Date + RecordingTime,
                    HandlerName = await SecureStorage.GetAsync("CurrentUser") ?? "Unknown",
                    Rate = (float)Temperature,
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
                _logger.LogError(ex, "Error saving temperature entry");
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
