using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public class CervixDilatationModalPageModel : INotifyPropertyChanged
    {
        private readonly CervixDilatationRepository _repository;
        private readonly ILogger<CervixDilatationModalPageModel> _logger;
        private CervixDilatation _currentEntry;
        private Guid? _patientId;

        public CervixDilatationModalPageModel(CervixDilatationRepository repository, ILogger<CervixDilatationModalPageModel> logger)
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

        private double _dilatation = 0;
        public double Dilatation
        {
            get => _dilatation;
            set
            {
                _dilatation = value;
                OnPropertyChanged();
                UpdateDilatationDisplay();
                UpdateProgressMessage();
            }
        }

        private string _dilatationDisplay = "0 cm";
        public string DilatationDisplay
        {
            get => _dilatationDisplay;
            set { _dilatationDisplay = value; OnPropertyChanged(); }
        }

        private string _stageDescription = "Labor not started";
        public string StageDescription
        {
            get => _stageDescription;
            set { _stageDescription = value; OnPropertyChanged(); }
        }

        private Color _dilatationColor = Colors.Gray;
        public Color DilatationColor
        {
            get => _dilatationColor;
            set { _dilatationColor = value; OnPropertyChanged(); }
        }

        private bool _fullyDilated;
        public bool FullyDilated
        {
            get => _fullyDilated;
            set
            {
                _fullyDilated = value;
                if (value) Dilatation = 10;
                OnPropertyChanged();
            }
        }

        private double _effacement = 0;
        public double Effacement
        {
            get => _effacement;
            set
            {
                _effacement = value;
                OnPropertyChanged();
                UpdateEffacementDisplay();
            }
        }

        private string _effacementDisplay = "0%";
        public string EffacementDisplay
        {
            get => _effacementDisplay;
            set { _effacementDisplay = value; OnPropertyChanged(); }
        }

        private bool _isFirm;
        public bool IsFirm
        {
            get => _isFirm;
            set { _isFirm = value; OnPropertyChanged(); }
        }

        private bool _isMedium = true;
        public bool IsMedium
        {
            get => _isMedium;
            set { _isMedium = value; OnPropertyChanged(); }
        }

        private bool _isSoft;
        public bool IsSoft
        {
            get => _isSoft;
            set { _isSoft = value; OnPropertyChanged(); }
        }

        private bool _isPosterior;
        public bool IsPosterior
        {
            get => _isPosterior;
            set { _isPosterior = value; OnPropertyChanged(); }
        }

        private bool _isMid = true;
        public bool IsMid
        {
            get => _isMid;
            set { _isMid = value; OnPropertyChanged(); }
        }

        private bool _isAnterior;
        public bool IsAnterior
        {
            get => _isAnterior;
            set { _isAnterior = value; OnPropertyChanged(); }
        }

        private bool _isWellApplied;
        public bool IsWellApplied
        {
            get => _isWellApplied;
            set { _isWellApplied = value; OnPropertyChanged(); }
        }

        private bool _isLooselyApplied;
        public bool IsLooselyApplied
        {
            get => _isLooselyApplied;
            set { _isLooselyApplied = value; OnPropertyChanged(); }
        }

        private bool _isNotApplied;
        public bool IsNotApplied
        {
            get => _isNotApplied;
            set { _isNotApplied = value; OnPropertyChanged(); }
        }

        private Color _progressColor = Colors.Blue;
        public Color ProgressColor
        {
            get => _progressColor;
            set { _progressColor = value; OnPropertyChanged(); }
        }

        private string _progressMessage = "Monitor progress";
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
            UpdateDilatationDisplay();
            UpdateEffacementDisplay();
            UpdateProgressMessage();
        }

        private void UpdateDilatationDisplay()
        {
            DilatationDisplay = $"{Dilatation:F1} cm";

            if (Dilatation == 10)
            {
                StageDescription = "Fully dilated - Second stage";
                DilatationColor = Colors.Green;
                FullyDilated = true;
            }
            else if (Dilatation >= 4)
            {
                StageDescription = "Active labor";
                DilatationColor = Colors.Orange;
            }
            else if (Dilatation > 0)
            {
                StageDescription = "Latent phase";
                DilatationColor = Colors.Blue;
            }
            else
            {
                StageDescription = "Labor not started";
                DilatationColor = Colors.Gray;
            }
        }

        private void UpdateEffacementDisplay()
        {
            EffacementDisplay = $"{Effacement:F0}%";
        }

        private void UpdateProgressMessage()
        {
            // Expected dilatation rate is 1cm/hour in active labor
            if (Dilatation >= 10)
            {
                ProgressColor = Colors.Green;
                ProgressMessage = "✓ Fully dilated - Ready for second stage";
            }
            else if (Dilatation >= 4)
            {
                ProgressColor = Colors.Green;
                ProgressMessage = "Good progress - Active labor";
            }
            else if (Dilatation > 0)
            {
                ProgressColor = Colors.Blue;
                ProgressMessage = "Latent phase - Monitor progress";
            }
            else
            {
                ProgressColor = Colors.Gray;
                ProgressMessage = "Cervix not dilated";
            }
        }

        private async Task SaveEntry()
        {
            try
            {
                var entry = new CervixDilatation
                {
                    PartographID = _patientId,
                    Time = RecordingDate.Date + RecordingTime,
                    HandlerName = await SecureStorage.GetAsync("CurrentUser") ?? "Unknown",
                    DilatationCm = (int)Math.Round(Dilatation),
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
                _logger.LogError(ex, "Error saving cervix dilatation entry");
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
