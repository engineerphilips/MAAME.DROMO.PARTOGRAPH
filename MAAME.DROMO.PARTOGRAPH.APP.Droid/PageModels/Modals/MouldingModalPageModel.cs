using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public class MouldingModalPageModel : INotifyPropertyChanged
    {
        private readonly MouldingRepository _repository;
        private readonly ILogger<MouldingModalPageModel> _logger;
        private Moulding _currentEntry;
        private Guid? _patientId;

        public MouldingModalPageModel(MouldingRepository repository, ILogger<MouldingModalPageModel> logger)
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

        public ObservableCollection<string> DegreeOptions { get; } = new ObservableCollection<string>
        {
            "None", "+", "++", "+++"
        };

        private string _selectedDegree = "None";
        public string SelectedDegree
        {
            get => _selectedDegree;
            set
            {
                _selectedDegree = value;
                OnPropertyChanged();
                UpdateMouldingStatus();
            }
        }

        private Color _mouldingColor = Colors.Green;
        public Color MouldingColor
        {
            get => _mouldingColor;
            set { _mouldingColor = value; OnPropertyChanged(); }
        }

        private string _mouldingStatus = "No moulding";
        public string MouldingStatus
        {
            get => _mouldingStatus;
            set { _mouldingStatus = value; OnPropertyChanged(); }
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
            UpdateMouldingStatus();
        }

        private void UpdateMouldingStatus()
        {
            switch (SelectedDegree)
            {
                case "None":
                    MouldingColor = Colors.Green;
                    MouldingStatus = "No moulding - Normal";
                    break;
                case "+":
                    MouldingColor = Colors.Green;
                    MouldingStatus = "Mild moulding - Normal for labor";
                    break;
                case "++":
                    MouldingColor = Colors.Orange;
                    MouldingStatus = "Moderate moulding - Monitor progress";
                    break;
                case "+++":
                    MouldingColor = Colors.Red;
                    MouldingStatus = "⚠️ Severe moulding - Consider CPD";
                    break;
                default:
                    MouldingColor = Colors.Gray;
                    MouldingStatus = "Monitor moulding";
                    break;
            }
        }

        private async Task SaveEntry()
        {
            try
            {
                var entry = new Moulding
                {
                    PartographID = _patientId,
                    Time = RecordingDate.Date + RecordingTime,
                    HandlerName = await SecureStorage.GetAsync("CurrentUser") ?? "Unknown",
                    Degree = SelectedDegree,
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
                _logger.LogError(ex, "Error saving moulding entry");
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
