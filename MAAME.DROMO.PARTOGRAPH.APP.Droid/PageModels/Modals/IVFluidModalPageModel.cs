using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public class IVFluidModalPageModel : INotifyPropertyChanged
    {
        private readonly IVFluidEntryRepository _repository;
        private readonly ILogger<IVFluidModalPageModel> _logger;
        private IVFluidEntry _currentEntry;
        private Guid? _patientId;

        public IVFluidModalPageModel(IVFluidEntryRepository repository, ILogger<IVFluidModalPageModel> logger)
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

        public ObservableCollection<string> FluidTypeOptions { get; } = new ObservableCollection<string>
        {
            "Normal Saline 0.9%", "Hartmann's Solution", "Dextrose 5%", "Dextrose 10%",
            "Ringer's Lactate", "Dextrose Saline"
        };

        private string _selectedFluidType = "Normal Saline 0.9%";
        public string SelectedFluidType
        {
            get => _selectedFluidType;
            set { _selectedFluidType = value; OnPropertyChanged(); }
        }

        private double _volumeInfused = 0;
        public double VolumeInfused
        {
            get => _volumeInfused;
            set
            {
                _volumeInfused = value;
                OnPropertyChanged();
            }
        }

        private string _rate = "125";
        public string Rate
        {
            get => _rate;
            set { _rate = value; OnPropertyChanged(); }
        }

        private DateTime _startTime = DateTime.Now;
        public DateTime StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(); }
        }

        private string _additives = string.Empty;
        public string Additives
        {
            get => _additives;
            set { _additives = value; OnPropertyChanged(); }
        }

        private string _ivSite = "Left hand";
        public string IVSite
        {
            get => _ivSite;
            set { _ivSite = value; OnPropertyChanged(); }
        }

        private bool _siteHealthy = true;
        public bool SiteHealthy
        {
            get => _siteHealthy;
            set
            {
                _siteHealthy = value;
                OnPropertyChanged();
            }
        }

        private string _siteCondition = "Clean";
        public string SiteCondition
        {
            get => _siteCondition;
            set { _siteCondition = value; OnPropertyChanged(); }
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
                var entry = new IVFluidEntry
                {
                    PartographID = _patientId,
                    Time = RecordingDate.Date + RecordingTime,
                    HandlerName = await SecureStorage.GetAsync("CurrentUser") ?? "Unknown",
                    FluidType = SelectedFluidType,
                    VolumeInfused = (int)Math.Round(VolumeInfused),
                    Rate = Rate,
                    StartTime = StartTime,
                    Additives = Additives ?? string.Empty,
                    IVSite = IVSite ?? string.Empty,
                    SiteHealthy = SiteHealthy,
                    SiteCondition = SiteCondition ?? string.Empty,
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
                _logger.LogError(ex, "Error saving IV fluid entry");
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
