using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class PartographPageModel : ObservableObject, IQueryAttributable
    {
        public Partograph? _patient;
        private readonly PatientRepository _patientRepository;
        private readonly PartographRepository _partographRepository;
        private readonly ModalErrorHandler _errorHandler;
        private readonly CompanionRepository _companionRepository;

        // Modal page models
        private readonly CompanionModalPageModel _companionModalPageModel;
        private readonly PainReliefModalPageModel _painReliefModalPageModel;
        private readonly OralFluidModalPageModel _oralFluidModalPageModel;
        private readonly PostureModalPageModel _postureModalPageModel;
        private readonly FetalPositionModalPageModel _fetalPositionModalPageModel;
        private readonly AmnioticFluidModalPageModel _amnioticFluidModalPageModel;
        private readonly CaputModalPageModel _caputModalPageModel;
        private readonly MouldingModalPageModel _mouldingModalPageModel;
        private readonly UrineModalPageModel _urineModalPageModel;
        private readonly TemperatureModalPageModel _temperatureModalPageModel;
        private readonly OxytocinModalPageModel _oxytocinModalPageModel;
        private readonly MedicationModalPageModel _medicationModalPageModel;
        private readonly IVFluidModalPageModel _ivFluidModalPageModel;
        private readonly HeadDescentModalPageModel _headDescentModalPageModel;
        private readonly CervixDilatationModalPageModel _cervixDilatationModalPageModel;
        private readonly BPPulseModalPageModel _bpPulseModalPageModel;
        private readonly FHRContractionModalPageModel _fHRContractionModalPageModel;
        public CompanionModalPageModel CompanionModalPageModel => _companionModalPageModel;
        public PainReliefModalPageModel PainReliefModalPageModel => _painReliefModalPageModel;
        public OralFluidModalPageModel OralFluidModalPageModel => _oralFluidModalPageModel;
        public PostureModalPageModel PostureModalPageModel => _postureModalPageModel;
        public FetalPositionModalPageModel FetalPositionModalPageModel => _fetalPositionModalPageModel;
        public AmnioticFluidModalPageModel AmnioticFluidModalPageModel => _amnioticFluidModalPageModel;
        public CaputModalPageModel CaputModalPageModel => _caputModalPageModel;
        public MouldingModalPageModel MouldingModalPageModel => _mouldingModalPageModel;
        public UrineModalPageModel UrineModalPageModel => _urineModalPageModel;
        public TemperatureModalPageModel TemperatureModalPageModel => _temperatureModalPageModel;
        public OxytocinModalPageModel OxytocinModalPageModel => _oxytocinModalPageModel;
        public MedicationModalPageModel MedicationModalPageModel => _medicationModalPageModel;
        public IVFluidModalPageModel IVFluidModalPageModel => _ivFluidModalPageModel;
        public HeadDescentModalPageModel HeadDescentModalPageModel => _headDescentModalPageModel;
        public CervixDilatationModalPageModel CervixDilatationModalPageModel => _cervixDilatationModalPageModel;
        public BPPulseModalPageModel BPPulseModalPageModel => _bpPulseModalPageModel;
        public FHRContractionModalPageModel FHRContractionModalPageModel => _fHRContractionModalPageModel;
        
        [ObservableProperty]
        private ObservableCollection<EnhancedTimeSlotViewModel> _timeSlots = new ();

        [ObservableProperty]
        private DateTime _startTime = DateTime.Today.AddHours(6);

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private string _patientInfo = string.Empty;

        [ObservableProperty]
        private string _laborDuration = string.Empty;

        [ObservableProperty]
        private DateTime? _lastRecordedTime;

        [ObservableProperty]
        private int _currentDilation;

        [ObservableProperty]
        private ObservableCollection<Partograph> _partographEntries = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _cervicalDilationData = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _fetalHeartRateData = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _contractionsData = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _alertLineData = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _actionLineData = new();

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        private ObservableCollection<TimeSlots> _chartinghours;

        public PartographPageModel(PatientRepository patientRepository,
            PartographRepository partographRepository,
            CompanionRepository companionRepository,
            ModalErrorHandler errorHandler,
            CompanionModalPageModel companionModalPageModel,
            PainReliefModalPageModel painReliefModalPageModel,
            OralFluidModalPageModel oralFluidModalPageModel,
            PostureModalPageModel postureModalPageModel,
            FetalPositionModalPageModel fetalPositionModalPageModel,
            AmnioticFluidModalPageModel amnioticFluidModalPageModel,
            CaputModalPageModel caputModalPageModel,
            MouldingModalPageModel mouldingModalPageModel,
            UrineModalPageModel urineModalPageModel,
            TemperatureModalPageModel temperatureModalPageModel,
            OxytocinModalPageModel oxytocinModalPageModel,
            MedicationModalPageModel medicationModalPageModel,
            IVFluidModalPageModel ivFluidModalPageModel,
            HeadDescentModalPageModel headDescentModalPageModel,
            CervixDilatationModalPageModel cervixDilatationModalPageModel,
            BPPulseModalPageModel bpPulseModalPageModel,
            FHRContractionModalPageModel fHRContractionModalPageModel)
        {
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _companionRepository = companionRepository;
            _errorHandler = errorHandler;
            _companionModalPageModel = companionModalPageModel;
            _painReliefModalPageModel = painReliefModalPageModel;
            _oralFluidModalPageModel = oralFluidModalPageModel;
            _postureModalPageModel = postureModalPageModel;
            _fetalPositionModalPageModel = fetalPositionModalPageModel;
            _amnioticFluidModalPageModel = amnioticFluidModalPageModel;
            _caputModalPageModel = caputModalPageModel;
            _mouldingModalPageModel = mouldingModalPageModel;
            _urineModalPageModel = urineModalPageModel;
            _temperatureModalPageModel = temperatureModalPageModel;
            _oxytocinModalPageModel = oxytocinModalPageModel;
            _medicationModalPageModel = medicationModalPageModel;
            _ivFluidModalPageModel = ivFluidModalPageModel;
            _headDescentModalPageModel = headDescentModalPageModel;
            _cervixDilatationModalPageModel = cervixDilatationModalPageModel;
            _bpPulseModalPageModel = bpPulseModalPageModel;
            _fHRContractionModalPageModel = fHRContractionModalPageModel;
            Chartinghours = new ObservableCollection<TimeSlots>();
            TimeSlots = new ObservableCollection<EnhancedTimeSlotViewModel>();

            GenerateInitialTimeSlots();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                Guid? id = Guid.Parse(Convert.ToString(query["patientId"]));
                LoadData(id).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        private void GenerateInitialTimeSlots()
        {
            TimeSlots.Clear();
            Chartinghours.Clear();
            DateTime date;
            date = new DateTime (StartTime.Year, StartTime.Month, StartTime.Day, StartTime.Hour, 0, 0);

            for (int i = 0; i < 12; i++)
            {
                var currentTime = StartTime.AddHours(i);
                var timeSlot = new EnhancedTimeSlotViewModel(currentTime, i + 1)
                {
                    Companion = CompanionType.None,
                    OralFluid = OralFluidType.None,
                    PainRelief = PainReliefType.None,
                    Posture = PostureType.None
                };

                //timeSlot.DataChanged += OnTimeSlotDataChanged;
                TimeSlots.Add(timeSlot);
                Chartinghours.Add(new TimeSlots() { Id = i, Slot = date.AddHours(i) });
            }

            //var x = Chartinghours?.Count ?? 0;

            if (TimeSlots.Any())
                RegenerateTimeSlots();
        }
        
        private void RegenerateTimeSlots()
        {
            foreach (var time in TimeSlots)
            {
                var x = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                var y = x.AddHours(1);
                if (time.Time >= x && time.Time < y)
                {
                    time.Companion = CompanionType.Yes;
                }
                else
                    time.Companion = CompanionType.None;
            }
        }

        //private void RegenerateTimeSlots()
        //{
        //    var existingData = TimeSlots.ToDictionary(ts => ts.Time, ts => ts.GetData());
        //    TimeSlots.Clear();

        //    for (int i = 0; i < existingData.Count; i++)
        //    {
        //        var currentTime = StartTime.AddHours(i);
        //        var timeSlot = new EnhancedTimeSlotViewModel(currentTime, i + 1);
        //        timeSlot.DataChanged += OnTimeSlotDataChanged;

        //        if (existingData.ContainsKey(currentTime))
        //        {
        //            timeSlot.LoadData(existingData[currentTime]);
        //        }

        //        TimeSlots.Add(timeSlot);
        //    }
        //}

        private async Task LoadData(Guid? patientId)
        {
            try
            {
                IsBusy = true;

                _patient = await _partographRepository.GetAsync(patientId);
                if (_patient == null)
                {
                    _errorHandler.HandleError(new Exception($"Patient with id {patientId} not found."));
                    return;
                }

                PatientName = _patient.Name;
                PatientInfo = _patient.DisplayInfo;

                // Calculate labor duration
                if (_patient.LaborStartTime.HasValue)
                {
                    var duration = DateTime.Now - _patient.LaborStartTime.Value;
                    LaborDuration = $"{(int)duration.TotalHours}h {duration.Minutes}m";
                }

                var companions = await _companionRepository.ListByPatientAsync(patientId);
                // Load partograph entries
                var entries = await _partographRepository.ListByPatientAsync(patientId);
                PartographEntries = new ObservableCollection<Partograph>(entries.OrderBy(e => e.Time));

                if (entries.Any())
                {
                    LastRecordedTime = entries.Max(e => e.Time);
                    var latestEntry = entries.OrderByDescending(e => e.Time).FirstOrDefault();
                    //CurrentDilation = latestEntry.CervicalDilation;
                }

                // Prepare chart data
                PrepareChartData();
                CalculateAlertActionLines();
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void PrepareChartData()
        {
            if (!PartographEntries.Any()) return;

            var baseTime = _patient?.LaborStartTime ?? PartographEntries.First().Time;

            //// Cervical Dilation Data
            //var dilationData = new ObservableCollection<ChartDataPoint>();
            //foreach (var entry in PartographEntries)
            //{
            //    dilationData.Add(new ChartDataPoint
            //    {
            //        Time = entry.Time,
            //        Value = entry.CervicalDilation
            //    });
            //}

            //CervicalDilationData = dilationData;

            //// Fetal Heart Rate Data
            //var fhrData = new ObservableCollection<ChartDataPoint>();
            //foreach (var entry in PartographEntries.Where(e => e.FetalHeartRate > 0))
            //{
            //    fhrData.Add(new ChartDataPoint
            //    {
            //        Time = entry.RecordedTime,
            //        Value = entry.FetalHeartRate
            //    });
            //}
            //FetalHeartRateData = fhrData;

            //// Contractions Data
            //var contractionsData = new ObservableCollection<ChartDataPoint>();
            //foreach (var entry in PartographEntries)
            //{
            //    contractionsData.Add(new ChartDataPoint
            //    {
            //        Time = entry.Time,
            //        Value = entry.ContractionsPerTenMinutes
            //    });
            //}
            //ContractionsData = contractionsData;
        }

        private void CalculateAlertActionLines()
        {
            if (_patient?.LaborStartTime == null) return;

            var startTime = _patient.LaborStartTime.Value;
            var alertLine = new ObservableCollection<ChartDataPoint>();
            var actionLine = new ObservableCollection<ChartDataPoint>();

            //// Alert line: Expected progress of 1cm/hour from 4cm
            //// Starting from when patient reached 4cm dilation
            //var fourCmEntry = PartographEntries.FirstOrDefault(e => e.CervicalDilation >= 4);
            //if (fourCmEntry != null)
            //{
            //    var fourCmTime = fourCmEntry.RecordedTime;

            //    // Alert line - normal progress
            //    for (int i = 4; i <= 10; i++)
            //    {
            //        alertLine.Add(new ChartDataPoint
            //        {
            //            Time = fourCmTime.AddHours(i - 4),
            //            Value = i
            //        });
            //    }

            //    // Action line - 2 hours behind alert line
            //    for (int i = 4; i <= 10; i++)
            //    {
            //        actionLine.Add(new ChartDataPoint
            //        {
            //            Time = fourCmTime.AddHours(i - 4 + 2),
            //            Value = i
            //        });
            //    }
            //}

            //AlertLineData = alertLine;
            //ActionLineData = actionLine;
        }

        [RelayCommand]
        private Task AddEntry()
            => Shell.Current.GoToAsync($"partographentry?patientId={_patient?.ID}");

        [RelayCommand]
        private async Task Print()
        {
            await AppShell.DisplayToastAsync("Partograph printing feature coming soon");
        }

        [RelayCommand]
        private async Task Refresh()
        {
            if (_patient != null)
                await LoadData(_patient.ID);
        }

        // Popup IsOpen properties
        //[ObservableProperty]
        //private bool _isCompanionPopupOpen;

        //public bool IsCompanionPopupOpen
        //{
        //    get => _isCompanionPopupOpen;
        //    set
        //    {
        //        SetProperty(ref _isCompanionPopupOpen, value);
        //        OnPropertyChanged(nameof(IsCompanionPopupOpen));
        //    }
        //}

        public Action? CloseCompanionModalPopup { get; set; }
        public Action? OpenCompanionModalPopup { get; set; }
        public Action? ClosePainReliefModalPopup { get; set; }
        public Action? OpenPainReliefModalPopup { get; set; }
        public Action? CloseOralFluidModalPopup { get; set; }
        public Action? OpenOralFluidModalPopup { get; set; }
        public Action? ClosePostureModalPopup { get; set; }
        public Action? OpenPostureModalPopup { get; set; }
        public Action? CloseFetalPositionModalPopup { get; set; }
        public Action? OpenFetalPositionModalPopup { get; set; }
        public Action? CloseAmnioticFluidModalPopup { get; set; }
        public Action? OpenAmnioticFluidModalPopup { get; set; }
        public Action? CloseCaputModalPopup { get; set; }
        public Action? OpenCaputModalPopup { get; set; }
        public Action? CloseMouldingModalPopup { get; set; }
        public Action? OpenMouldingModalPopup { get; set; }
        public Action? CloseUrineModalPopup { get; set; }
        public Action? OpenUrineModalPopup { get; set; }
        public Action? CloseTemperatureModalPopup { get; set; }
        public Action? OpenTemperatureModalPopup { get; set; }
        public Action? CloseBpPulseModalPopup { get; set; }
        public Action? OpenBpPulseModalPopup { get; set; }
        public Action? CloseMedicationModalPopup { get; set; }
        public Action? OpenMedicationModalPopup { get; set; }
        public Action? CloseIVFluidModalPopup { get; set; }
        public Action? OpenIVFluidModalPopup { get; set; }
        public Action? CloseOxytocinModalPopup { get; set; }
        public Action? OpenOxytocinModalPopup { get; set; }
        public Action? CloseHeadDescentModalPopup { get; set; }
        public Action? OpenHeadDescentModalPopup { get; set; }
        public Action? CloseCervixDilatationModalPopup { get; set; }
        public Action? OpenCervixDilatationModalPopup { get; set; }
        public Action? OpenFHRContractionModalPopup { get; set; }
        public Action? CloseFHRContractionModalPopup { get; set; }

        // Popup Open Commands
        [RelayCommand]
        public async Task OpenCompanionPopup()
        {
            //if (IsCompanionPopupOpen)
            //    IsCompanionPopupOpen = false;
            if (_patient?.ID != null)
            {
                CompanionModalPageModel._patient = _patient;
                CompanionModalPageModel.ClosePopup = () => CloseCompanionModalPopup?.Invoke();
                await CompanionModalPageModel.LoadPatient(_patient.ID);

                OpenCompanionModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenPainReliefPopup()
        {
            if (_patient?.ID != null)
            {
                _painReliefModalPageModel._patient = _patient;

                _painReliefModalPageModel.ClosePopup = () => ClosePainReliefModalPopup?.Invoke();
                await _painReliefModalPageModel.LoadPatient(_patient.ID);
                OpenPainReliefModalPopup?.Invoke();
                //IsPainReliefPopupOpen = true;
            }
        }

        [RelayCommand]
        private async Task OpenOralFluidPopup()
        {
            if (_patient?.ID != null)
            {
                _oralFluidModalPageModel._patient = _patient;
                _oralFluidModalPageModel.ClosePopup = () => CloseOralFluidModalPopup?.Invoke();
                await _oralFluidModalPageModel.LoadPatient(_patient.ID);
                OpenOralFluidModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenPosturePopup()
        {
            if (_patient?.ID != null)
            {
                _postureModalPageModel._patient = _patient;
                _postureModalPageModel.ClosePopup = () => ClosePostureModalPopup?.Invoke();
                await _postureModalPageModel.LoadPatient(_patient.ID);
                OpenPostureModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenAmnioticFluidPopup()
        {
            if (_patient?.ID != null)
            {
                _amnioticFluidModalPageModel._patient = _patient;
                _amnioticFluidModalPageModel.ClosePopup = () => CloseAmnioticFluidModalPopup?.Invoke();
                await _amnioticFluidModalPageModel.LoadPatient(_patient.ID);
                OpenAmnioticFluidModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenFetalPositionPopup()
        {
            if (_patient?.ID != null)
            {
                _fetalPositionModalPageModel._patient = _patient;
                _fetalPositionModalPageModel.ClosePopup = () => CloseFetalPositionModalPopup?.Invoke();
                await _fetalPositionModalPageModel.LoadPatient(_patient.ID);
                OpenFetalPositionModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenCaputPopup()
        {
            if (_patient?.ID != null)
            {
                _caputModalPageModel._patient = _patient;
                _caputModalPageModel.ClosePopup = () => CloseCaputModalPopup?.Invoke();
                await _caputModalPageModel.LoadPatient(_patient.ID);
                OpenCaputModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenFHRContractionPopup()
        {
            if (_patient?.ID != null)
            {
                _fHRContractionModalPageModel._patient = _patient;
                _fHRContractionModalPageModel.ClosePopup = () => CloseFHRContractionModalPopup?.Invoke();
                await _fHRContractionModalPageModel.LoadPatient(_patient.ID);
                OpenFHRContractionModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenUrinePopup()
        {
            if (_patient?.ID != null)
            {
                _urineModalPageModel._patient = _patient;
                _urineModalPageModel.ClosePopup = () => CloseUrineModalPopup?.Invoke();
                await _urineModalPageModel.LoadPatient(_patient.ID);
                OpenUrineModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenTemperaturePopup()
        {
            if (_patient?.ID != null)
            {
                _temperatureModalPageModel._patient = _patient;
                _temperatureModalPageModel.ClosePopup = () => CloseTemperatureModalPopup?.Invoke();
                await _temperatureModalPageModel.LoadPatient(_patient.ID);
                OpenTemperatureModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenBpPulsePopup()
        {
            if (_patient?.ID != null)
            {
                _bpPulseModalPageModel._patient = _patient;
                _bpPulseModalPageModel.ClosePopup = () => CloseBpPulseModalPopup?.Invoke();
                await _bpPulseModalPageModel.LoadPatient(_patient.ID);
                OpenBpPulseModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenMedicationPopup()
        {
            if (_patient?.ID != null)
            {
                _medicationModalPageModel._patient = _patient;
                _medicationModalPageModel.ClosePopup = () => CloseMedicationModalPopup?.Invoke();
                await _medicationModalPageModel.LoadPatient(_patient.ID);
                OpenMedicationModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenIVFluidPopup()
        {
            if (_patient?.ID != null)
            {
                _ivFluidModalPageModel._patient = _patient;
                _ivFluidModalPageModel.ClosePopup = () => CloseIVFluidModalPopup?.Invoke();
                await _ivFluidModalPageModel.LoadPatient(_patient.ID);
                OpenIVFluidModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenOxytocinPopup()
        {
            if (_patient?.ID != null)
            {
                _oxytocinModalPageModel._patient = _patient;
                _oxytocinModalPageModel.ClosePopup = () => CloseOxytocinModalPopup?.Invoke();
                await _oxytocinModalPageModel.LoadPatient(_patient.ID);
                OpenOxytocinModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenHeadDescentPopup()
        {
            if (_patient?.ID != null)
            {
                _headDescentModalPageModel._patient = _patient;
                _headDescentModalPageModel.ClosePopup = () => CloseHeadDescentModalPopup?.Invoke();
                await _headDescentModalPageModel.LoadPatient(_patient.ID);
                OpenHeadDescentModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenCervixDilatationPopup()
        {
            if (_patient?.ID != null)
            {
                _cervixDilatationModalPageModel._patient = _patient;
                _cervixDilatationModalPageModel.ClosePopup = () => CloseCervixDilatationModalPopup?.Invoke();
                await _cervixDilatationModalPageModel.LoadPatient(_patient.ID);
                OpenCervixDilatationModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenMouldingPopup()
        {
            if (_patient?.ID != null)
            {
                _mouldingModalPageModel._patient = _patient;
                _mouldingModalPageModel.ClosePopup = () => CloseMouldingModalPopup?.Invoke();
                await _mouldingModalPageModel.LoadPatient(_patient.ID);
                OpenMouldingModalPopup?.Invoke();
            }
        }
    }

    public class ChartDataPoint
    {
        public DateTime Time { get; set; }
        public double Value { get; set; }
    }
}
