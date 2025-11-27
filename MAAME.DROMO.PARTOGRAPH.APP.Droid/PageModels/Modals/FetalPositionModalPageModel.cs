using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public class FetalPositionModalPageModel : INotifyPropertyChanged
    {
        private readonly FetalPositionRepository _repository;
        private readonly ILogger<FetalPositionModalPageModel> _logger;
        private FetalPosition _currentEntry;
        private Guid? _patientId;

        public FetalPositionModalPageModel(FetalPositionRepository repository, ILogger<FetalPositionModalPageModel> logger)
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

        public ObservableCollection<string> PositionOptions { get; } = new ObservableCollection<string>
        {
            "LOA", "ROA", "LOP", "ROP", "LOT", "ROT", "OA", "OP"
        };

        private string _selectedPosition = "LOA";
        public string SelectedPosition
        {
            get => _selectedPosition;
            set
            {
                _selectedPosition = value;
                OnPropertyChanged();
                UpdatePositionStatus();
            }
        }

        private Color _positionColor = Colors.Green;
        public Color PositionColor
        {
            get => _positionColor;
            set { _positionColor = value; OnPropertyChanged(); }
        }

        private string _positionStatus = "Favorable position";
        public string PositionStatus
        {
            get => _positionStatus;
            set { _positionStatus = value; OnPropertyChanged(); }
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
            UpdatePositionStatus();
        }

        private void UpdatePositionStatus()
        {
            if (SelectedPosition == "LOA" || SelectedPosition == "ROA" || SelectedPosition == "OA")
            {
                PositionColor = Colors.Green;
                PositionStatus = "Favorable anterior position";
            }
            else if (SelectedPosition == "LOP" || SelectedPosition == "ROP" || SelectedPosition == "OP")
            {
                PositionColor = Colors.Orange;
                PositionStatus = "Posterior position - May prolong labor";
            }
            else if (SelectedPosition == "LOT" || SelectedPosition == "ROT")
            {
                PositionColor = Colors.Blue;
                PositionStatus = "Transverse position - Monitor rotation";
            }
            else
            {
                PositionColor = Colors.Gray;
                PositionStatus = "Monitor fetal position";
            }
        }

        private async Task SaveEntry()
        {
            try
            {
                var entry = new FetalPosition
                {
                    PartographID = _patientId,
                    Time = RecordingDate.Date + RecordingTime,
                    HandlerName = await SecureStorage.GetAsync("CurrentUser") ?? "Unknown",
                    Position = SelectedPosition,
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
                _logger.LogError(ex, "Error saving fetal position entry");
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
