using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class PartographPageModel : ObservableObject, IQueryAttributable
    {
        private Partograph? _patient;
        private readonly PatientRepository _patientRepository;
        private readonly PartographRepository _partographRepository;
        private readonly ModalErrorHandler _errorHandler;

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
            ModalErrorHandler errorHandler)
        {
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _errorHandler = errorHandler;
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
    }

    public class ChartDataPoint
    {
        public DateTime Time { get; set; }
        public double Value { get; set; }
    }
}
