using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public class MedicationModalPageModel : INotifyPropertyChanged
    {
        private readonly MedicationEntryRepository _repository;
        private readonly ILogger<MedicationModalPageModel> _logger;
        private MedicationEntry _currentEntry;
        private Guid? _patientId;

        public MedicationModalPageModel(MedicationEntryRepository repository, ILogger<MedicationModalPageModel> logger)
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

        private string _medicationName = string.Empty;
        public string MedicationName
        {
            get => _medicationName;
            set { _medicationName = value; OnPropertyChanged(); }
        }

        private string _dose = string.Empty;
        public string Dose
        {
            get => _dose;
            set { _dose = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> RouteOptions { get; } = new ObservableCollection<string>
        {
            "IV", "IM", "PO", "Sublingual", "SC", "PR", "Topical", "Inhaled"
        };

        private string _selectedRoute = "IV";
        public string SelectedRoute
        {
            get => _selectedRoute;
            set { _selectedRoute = value; OnPropertyChanged(); }
        }

        private DateTime _administrationTime = DateTime.Now;
        public DateTime AdministrationTime
        {
            get => _administrationTime;
            set { _administrationTime = value; OnPropertyChanged(); }
        }

        private string _indication = string.Empty;
        public string Indication
        {
            get => _indication;
            set { _indication = value; OnPropertyChanged(); }
        }

        private string _prescribedBy = string.Empty;
        public string PrescribedBy
        {
            get => _prescribedBy;
            set { _prescribedBy = value; OnPropertyChanged(); }
        }

        private string _response = string.Empty;
        public string Response
        {
            get => _response;
            set { _response = value; OnPropertyChanged(); }
        }

        private bool _adverseReaction = false;
        public bool AdverseReaction
        {
            get => _adverseReaction;
            set
            {
                _adverseReaction = value;
                OnPropertyChanged();
            }
        }

        private string _adverseReactionDetails = string.Empty;
        public string AdverseReactionDetails
        {
            get => _adverseReactionDetails;
            set { _adverseReactionDetails = value; OnPropertyChanged(); }
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
        }

        private async Task SaveEntry()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(MedicationName))
                {
                    await Application.Current.MainPage.DisplayAlert("Validation", "Please enter medication name", "OK");
                    return;
                }

                var entry = new MedicationEntry
                {
                    PartographID = _patientId,
                    Time = RecordingDate.Date + RecordingTime,
                    HandlerName = await SecureStorage.GetAsync("CurrentUser") ?? "Unknown",
                    MedicationName = MedicationName,
                    Dose = Dose ?? string.Empty,
                    Route = SelectedRoute,
                    AdministrationTime = AdministrationTime,
                    Indication = Indication ?? string.Empty,
                    PrescribedBy = PrescribedBy ?? string.Empty,
                    Response = Response ?? string.Empty,
                    AdverseReaction = AdverseReaction,
                    AdverseReactionDetails = AdverseReactionDetails ?? string.Empty,
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
                _logger.LogError(ex, "Error saving medication entry");
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
