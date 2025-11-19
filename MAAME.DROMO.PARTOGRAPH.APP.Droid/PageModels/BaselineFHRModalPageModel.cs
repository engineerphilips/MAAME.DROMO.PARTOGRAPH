using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class BaselineFHRModalPageModel : INotifyPropertyChanged
    {
        private readonly FHRRepository _repository;
        private readonly ILogger<BaselineFHRModalPageModel> _logger;
        private FHR _currentEntry;
        private Guid? _patientId;

        public BaselineFHRModalPageModel(FHRRepository repository, ILogger<BaselineFHRModalPageModel> logger)
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

        private int _baselineRate = 140;
        public int BaselineRate
        {
            get => _baselineRate;
            set
            {
                _baselineRate = value;
                OnPropertyChanged();
                UpdateBaselineRateStatus();
            }
        }

        private string _baselineRateStatus = "Normal";
        public string BaselineRateStatus
        {
            get => _baselineRateStatus;
            set { _baselineRateStatus = value; OnPropertyChanged(); }
        }

        private Color _baselineRateColor = Colors.Green;
        public Color BaselineRateColor
        {
            get => _baselineRateColor;
            set { _baselineRateColor = value; OnPropertyChanged(); }
        }

        private string _selectedVariability = "Moderate";
        public string SelectedVariability
        {
            get => _selectedVariability;
            set { _selectedVariability = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> VariabilityOptions { get; } = new ObservableCollection<string>
        {
            "Absent", "Minimal", "Moderate", "Marked", "Sinusoidal"
        };

        private bool _hasAccelerations;
        public bool HasAccelerations
        {
            get => _hasAccelerations;
            set { _hasAccelerations = value; OnPropertyChanged(); }
        }

        private string _selectedPattern = "Normal";
        public string SelectedPattern
        {
            get => _selectedPattern;
            set { _selectedPattern = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> PatternOptions { get; } = new ObservableCollection<string>
        {
            "Normal", "Tachycardia", "Bradycardia", "Variable"
        };

        private bool _isIntermittentAuscultation;
        public bool IsIntermittentAuscultation
        {
            get => _isIntermittentAuscultation;
            set { _isIntermittentAuscultation = value; OnPropertyChanged(); }
        }

        private bool _isContinuousCTG;
        public bool IsContinuousCTG
        {
            get => _isContinuousCTG;
            set { _isContinuousCTG = value; OnPropertyChanged(); }
        }

        private bool _isDoppler;
        public bool IsDoppler
        {
            get => _isDoppler;
            set { _isDoppler = value; OnPropertyChanged(); }
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
            // Set default monitoring method
            IsIntermittentAuscultation = true;
        }

        private void UpdateBaselineRateStatus()
        {
            if (BaselineRate < 110)
            {
                BaselineRateStatus = "Bradycardia - Below normal range";
                BaselineRateColor = Colors.Red;
            }
            else if (BaselineRate >= 110 && BaselineRate <= 160)
            {
                BaselineRateStatus = "Normal range (110-160 bpm)";
                BaselineRateColor = Colors.Green;
            }
            else
            {
                BaselineRateStatus = "Tachycardia - Above normal range";
                BaselineRateColor = Colors.Orange;
            }
        }

        private string GetMonitoringMethod()
        {
            if (IsContinuousCTG) return "Continuous CTG";
            if (IsDoppler) return "Doppler";
            return "Intermittent Auscultation";
        }

        private async Task SaveEntry()
        {
            try
            {
                var entry = new FHR
                {
                    PartographID = _patientId,
                    Time = RecordingDate.Date + RecordingTime,
                    HandlerName = await SecureStorage.GetAsync("CurrentUser") ?? "Unknown",
                    //BaselineRate = BaselineRate,
                    //Variability = SelectedVariability,
                    //Accelerations = HasAccelerations,
                    //Pattern = SelectedPattern,
                    //MonitoringMethod = GetMonitoringMethod(),
                    Notes = Notes ?? string.Empty
                };

                if (_currentEntry != null)
                {
                    entry.ID = _currentEntry.ID;
                    //await _repository.UpdateAsync(entry);
                }
                else
                {
                    await _repository.SaveItemAsync(entry);
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving baseline FHR entry");
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
