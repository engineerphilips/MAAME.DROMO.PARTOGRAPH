using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public class UrineModalPageModel : INotifyPropertyChanged
    {
        private readonly UrineRepository _repository;
        private readonly ILogger<UrineModalPageModel> _logger;
        private Urine _currentEntry;
        private Guid? _patientId;

        public UrineModalPageModel(UrineRepository repository, ILogger<UrineModalPageModel> logger)
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

        public ObservableCollection<string> ProteinOptions { get; } = new ObservableCollection<string>
        {
            "Nil", "Trace", "+", "++", "+++"
        };

        public ObservableCollection<string> AcetoneOptions { get; } = new ObservableCollection<string>
        {
            "Nil", "Trace", "+", "++", "+++"
        };

        private string _selectedProtein = "Nil";
        public string SelectedProtein
        {
            get => _selectedProtein;
            set
            {
                _selectedProtein = value;
                OnPropertyChanged();
                UpdateUrineStatus();
            }
        }

        private string _selectedAcetone = "Nil";
        public string SelectedAcetone
        {
            get => _selectedAcetone;
            set
            {
                _selectedAcetone = value;
                OnPropertyChanged();
                UpdateUrineStatus();
            }
        }

        private Color _urineColor = Colors.Green;
        public Color UrineColor
        {
            get => _urineColor;
            set { _urineColor = value; OnPropertyChanged(); }
        }

        private string _urineStatus = "Normal";
        public string UrineStatus
        {
            get => _urineStatus;
            set { _urineStatus = value; OnPropertyChanged(); }
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
            UpdateUrineStatus();
        }

        private void UpdateUrineStatus()
        {
            bool hasProtein = SelectedProtein != "Nil";
            bool hasAcetone = SelectedAcetone != "Nil";

            if (hasProtein && (SelectedProtein == "+++" || SelectedProtein == "++"))
            {
                UrineColor = Colors.Red;
                UrineStatus = "⚠️ Significant proteinuria - Check for pre-eclampsia";
            }
            else if (hasProtein || hasAcetone)
            {
                UrineColor = Colors.Orange;
                UrineStatus = "Abnormal findings - Monitor closely";
            }
            else
            {
                UrineColor = Colors.Green;
                UrineStatus = "Normal urine";
            }
        }

        private async Task SaveEntry()
        {
            try
            {
                var entry = new Urine
                {
                    PartographID = _patientId,
                    Time = RecordingDate.Date + RecordingTime,
                    HandlerName = await SecureStorage.GetAsync("CurrentUser") ?? "Unknown",
                    Protein = SelectedProtein,
                    Acetone = SelectedAcetone,
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
                _logger.LogError(ex, "Error saving urine entry");
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
