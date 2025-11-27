using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public class HeadDescentModalPageModel : INotifyPropertyChanged
    {
        private readonly HeadDescentRepository _repository;
        private readonly ILogger<HeadDescentModalPageModel> _logger;
        private HeadDescent _currentEntry;
        private Guid? _patientId;

        public HeadDescentModalPageModel(HeadDescentRepository repository, ILogger<HeadDescentModalPageModel> logger)
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

        private double _station = 0;
        public double Station
        {
            get => _station;
            set
            {
                _station = value;
                OnPropertyChanged();
                UpdateStationDisplay();
                UpdateProgressMessage();
            }
        }

        private string _stationDisplay = "0";
        public string StationDisplay
        {
            get => _stationDisplay;
            set { _stationDisplay = value; OnPropertyChanged(); }
        }

        private Color _stationColor = Colors.Blue;
        public Color StationColor
        {
            get => _stationColor;
            set { _stationColor = value; OnPropertyChanged(); }
        }

        private Color _progressColor = Colors.Blue;
        public Color ProgressColor
        {
            get => _progressColor;
            set { _progressColor = value; OnPropertyChanged(); }
        }

        private string _progressMessage = "Monitor descent";
        public string ProgressMessage
        {
            get => _progressMessage;
            set { _progressMessage = value; OnPropertyChanged(); }
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
            UpdateStationDisplay();
            UpdateProgressMessage();
        }

        private void UpdateStationDisplay()
        {
            int stationInt = (int)Math.Round(Station);
            if (stationInt > 0)
                StationDisplay = $"+{stationInt}";
            else
                StationDisplay = stationInt.ToString();

            // Color coding based on station
            if (Station >= 2)
            {
                StationColor = Colors.Green;
            }
            else if (Station >= 0)
            {
                StationColor = Colors.Orange;
            }
            else if (Station >= -2)
            {
                StationColor = Colors.Blue;
            }
            else
            {
                StationColor = Colors.Gray;
            }
        }

        private void UpdateProgressMessage()
        {
            if (Station >= 3)
            {
                ProgressColor = Colors.Green;
                ProgressMessage = "✓ Head delivered or crowning";
            }
            else if (Station >= 2)
            {
                ProgressColor = Colors.Green;
                ProgressMessage = "Good descent - Visible at introitus";
            }
            else if (Station >= 0)
            {
                ProgressColor = Colors.Orange;
                ProgressMessage = "Head at spines - Progressing well";
            }
            else if (Station >= -2)
            {
                ProgressColor = Colors.Blue;
                ProgressMessage = "Head engaged - Monitor progress";
            }
            else
            {
                ProgressColor = Colors.Gray;
                ProgressMessage = "Head not engaged";
            }
        }

        private async Task SaveEntry()
        {
            try
            {
                var entry = new HeadDescent
                {
                    PartographID = _patientId,
                    Time = RecordingDate.Date + RecordingTime,
                    HandlerName = await SecureStorage.GetAsync("CurrentUser") ?? "Unknown",
                    Station = (int)Math.Round(Station),
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
                _logger.LogError(ex, "Error saving head descent entry");
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
