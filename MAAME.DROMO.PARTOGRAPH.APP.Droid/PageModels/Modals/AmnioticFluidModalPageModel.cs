using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public class AmnioticFluidModalPageModel : INotifyPropertyChanged
    {
        private readonly AmnioticFluidRepository _repository;
        private readonly ILogger<AmnioticFluidModalPageModel> _logger;
        private AmnioticFluid _currentEntry;
        private Guid? _patientId;

        public AmnioticFluidModalPageModel(AmnioticFluidRepository repository, ILogger<AmnioticFluidModalPageModel> logger)
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

        public ObservableCollection<string> ColorOptions { get; } = new ObservableCollection<string>
        {
            "Clear", "Straw", "Green", "Brown", "Blood-stained"
        };

        private string _selectedColor = "Clear";
        public string SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                OnPropertyChanged();
                UpdateFluidStatus();
            }
        }

        private Color _fluidColor = Colors.Green;
        public Color FluidColor
        {
            get => _fluidColor;
            set { _fluidColor = value; OnPropertyChanged(); }
        }

        private string _fluidStatus = "Normal";
        public string FluidStatus
        {
            get => _fluidStatus;
            set { _fluidStatus = value; OnPropertyChanged(); }
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
            UpdateFluidStatus();
        }

        private void UpdateFluidStatus()
        {
            switch (SelectedColor)
            {
                case "Clear":
                case "Straw":
                    FluidColor = Colors.Green;
                    FluidStatus = "Normal amniotic fluid";
                    break;
                case "Green":
                    FluidColor = Colors.Orange;
                    FluidStatus = "⚠️ Meconium staining - Monitor fetal wellbeing";
                    break;
                case "Brown":
                    FluidColor = Colors.Red;
                    FluidStatus = "⚠️ Old meconium - Requires close monitoring";
                    break;
                case "Blood-stained":
                    FluidColor = Colors.Red;
                    FluidStatus = "⚠️ Blood-stained - Check for abruption";
                    break;
                default:
                    FluidColor = Colors.Blue;
                    FluidStatus = "Monitor";
                    break;
            }
        }

        private async Task SaveEntry()
        {
            try
            {
                var entry = new AmnioticFluid
                {
                    PartographID = _patientId,
                    Time = RecordingDate.Date + RecordingTime,
                    HandlerName = await SecureStorage.GetAsync("CurrentUser") ?? "Unknown",
                    Color = SelectedColor,
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
                _logger.LogError(ex, "Error saving amniotic fluid entry");
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
