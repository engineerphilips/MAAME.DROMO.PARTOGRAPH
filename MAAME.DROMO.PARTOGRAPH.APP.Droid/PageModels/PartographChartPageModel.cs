using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Helpers;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class PartographChartPageModel : ObservableObject, IQueryAttributable
    {
        private readonly PartographRepository _partographRepository;
        private readonly CervixDilatationRepository _cervixDilatationRepository;
        private readonly FHRRepository _fhrRepository;
        private readonly ContractionRepository _contractionRepository;
        private readonly BPRepository _bpRepository;
        private readonly TemperatureRepository _temperatureRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _patientId = string.Empty;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private string _patientInfo = string.Empty;

        [ObservableProperty]
        private string _laborDuration = string.Empty;

        [ObservableProperty]
        private int _currentDilation;

        [ObservableProperty]
        private bool _isBusy;

        // Chart Data Collections
        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _cervicalDilationData = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _alertLineData = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _actionLineData = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _fetalHeartRateData = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _fhrUpperLimit = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _fhrLowerLimit = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _contractionsData = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _optimalContractionRange = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _systolicBPData = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _diastolicBPData = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _pulseData = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _temperatureData = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _tempUpperNormal = new();

        private Partograph? _patient;

        public PartographChartPageModel(
            PartographRepository partographRepository,
            CervixDilatationRepository cervixDilatationRepository,
            FHRRepository fhrRepository,
            ContractionRepository contractionRepository,
            BPRepository bpRepository,
            TemperatureRepository temperatureRepository,
            ModalErrorHandler errorHandler)
        {
            _partographRepository = partographRepository;
            _cervixDilatationRepository = cervixDilatationRepository;
            _fhrRepository = fhrRepository;
            _contractionRepository = contractionRepository;
            _bpRepository = bpRepository;
            _temperatureRepository = temperatureRepository;
            _errorHandler = errorHandler;
        }

        partial void OnPatientIdChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                LoadDataAsync(Guid.Parse(value)).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        private async Task LoadDataAsync(Guid patientId)
        {
            try
            {
                IsBusy = true;

                _patient = await _partographRepository.GetAsync(patientId);
                if (_patient == null)
                    return;

                PatientName = _patient.Name;
                PatientInfo = _patient.DisplayInfo;

                // Calculate labor duration
                if (_patient.LaborStartTime.HasValue)
                {
                    var duration = DateTime.Now - _patient.LaborStartTime.Value;
                    LaborDuration = $"{(int)duration.TotalHours}h {duration.Minutes}m";
                }

                // Load all measurements
                await LoadMeasurements(patientId);

                // Prepare all charts
                PrepareCervicalDilationChart();
                PrepareFetalHeartRateChart();
                PrepareContractionChart();
                PrepareMaternalVitalsChart();
                PrepareTemperatureChart();
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

        private async Task LoadMeasurements(Guid patientId)
        {
            _patient.Dilatations = await _cervixDilatationRepository.ListByPatientAsync(patientId);
            _patient.Fhrs = await _fhrRepository.ListByPatientAsync(patientId);
            _patient.Contractions = await _contractionRepository.ListByPatientAsync(patientId);
            _patient.BPs = await _bpRepository.ListByPatientAsync(patientId);
            _patient.Temperatures = await _temperatureRepository.ListByPatientAsync(patientId);

            if (_patient.Dilatations.Any())
            {
                CurrentDilation = _patient.Dilatations.OrderByDescending(d => d.Time).FirstOrDefault().DilatationCm;
            }
        }

        private void PrepareCervicalDilationChart()
        {
            CervicalDilationData.Clear();
            AlertLineData.Clear();
            ActionLineData.Clear();

            if (!_patient.Dilatations.Any() || !_patient.LaborStartTime.HasValue)
                return;

            var dilatations = _patient.Dilatations.OrderBy(d => d.Time).ToList();

            // Plot actual dilatation data
            foreach (var dil in dilatations)
            {
                CervicalDilationData.Add(new ChartDataPoint
                {
                    Time = dil.Time,
                    Value = dil.DilatationCm
                });
            }

            // WHO Labour Care Guide 2020: Alert and Action Lines
            // Alert line: 1cm/hour from 5cm (reaches 10cm in 5 hours)
            // Action line: 4 hours to the right of alert line

            var activeLaborEntry = dilatations.FirstOrDefault(d => d.DilatationCm >= 5);
            if (activeLaborEntry != null)
            {
                var startTime = activeLaborEntry.Time;

                // WHO 2020: Alert line - 1cm per hour from 5cm (reaches 10cm in 5 hours)
                for (double hour = 0; hour <= 5; hour += 0.5)
                {
                    var time = startTime.AddHours(hour);
                    var dilatation = 5 + hour; // 1cm per hour from 5cm
                    if (dilatation > 10) dilatation = 10;

                    AlertLineData.Add(new ChartDataPoint
                    {
                        Time = time,
                        Value = dilatation
                    });

                    if (dilatation >= 10) break;
                }

                // WHO 2020: Action line - 4 hours to the right of alert line
                for (double hour = 0; hour <= 9; hour += 0.5)
                {
                    var time = startTime.AddHours(hour);
                    var dilatation = 5 + Math.Max(0, hour - 4); // Starts 4 hours later, then 1cm/hour
                    if (dilatation > 10) dilatation = 10;

                    ActionLineData.Add(new ChartDataPoint
                    {
                        Time = time,
                        Value = dilatation
                    });

                    if (dilatation >= 10) break;
                }
            }
        }

        private void PrepareFetalHeartRateChart()
        {
            FetalHeartRateData.Clear();
            FhrUpperLimit.Clear();
            FhrLowerLimit.Clear();

            if (!_patient.Fhrs.Any())
                return;

            var fhrs = _patient.Fhrs.OrderBy(f => f.Time).ToList();

            // Plot FHR data
            foreach (var fhr in fhrs)
            {
                if (fhr.Rate.HasValue)
                {
                    FetalHeartRateData.Add(new ChartDataPoint
                    {
                        Time = fhr.Time,
                        Value = fhr.Rate.Value
                    });
                }
            }

            // Add normal range lines
            if (fhrs.Any())
            {
                var startTime = fhrs.First().Time;
                var endTime = fhrs.Last().Time;

                FhrUpperLimit.Add(new ChartDataPoint { Time = startTime, Value = 160 });
                FhrUpperLimit.Add(new ChartDataPoint { Time = endTime, Value = 160 });

                FhrLowerLimit.Add(new ChartDataPoint { Time = startTime, Value = 110 });
                FhrLowerLimit.Add(new ChartDataPoint { Time = endTime, Value = 110 });
            }
        }

        private void PrepareContractionChart()
        {
            ContractionsData.Clear();
            OptimalContractionRange.Clear();

            if (!_patient.Contractions.Any())
                return;

            var contractions = _patient.Contractions.OrderBy(c => c.Time).ToList();

            // Plot contraction frequency
            foreach (var contraction in contractions)
            {
                ContractionsData.Add(new ChartDataPoint
                {
                    Time = contraction.Time,
                    Value = contraction.FrequencyPer10Min
                });
            }

            // Add optimal range line (3-5 contractions per 10 minutes)
            if (contractions.Any())
            {
                var startTime = contractions.First().Time;
                var endTime = contractions.Last().Time;

                OptimalContractionRange.Add(new ChartDataPoint { Time = startTime, Value = 4 });
                OptimalContractionRange.Add(new ChartDataPoint { Time = endTime, Value = 4 });
            }
        }

        private void PrepareMaternalVitalsChart()
        {
            SystolicBPData.Clear();
            DiastolicBPData.Clear();
            PulseData.Clear();

            if (!_patient.BPs.Any())
                return;

            var bps = _patient.BPs.OrderBy(b => b.Time).ToList();

            // Plot vital signs
            foreach (var bp in bps)
            {
                SystolicBPData.Add(new ChartDataPoint
                {
                    Time = bp.Time,
                    Value = bp.Systolic
                });

                DiastolicBPData.Add(new ChartDataPoint
                {
                    Time = bp.Time,
                    Value = bp.Diastolic
                });

                if (bp.Pulse > 0)
                {
                    PulseData.Add(new ChartDataPoint
                    {
                        Time = bp.Time,
                        Value = bp.Pulse
                    });
                }
            }
        }

        private void PrepareTemperatureChart()
        {
            TemperatureData.Clear();
            TempUpperNormal.Clear();

            if (!_patient.Temperatures.Any())
                return;

            var temps = _patient.Temperatures.OrderBy(t => t.Time).ToList();

            // Plot temperature data
            foreach (var temp in temps)
            {
                TemperatureData.Add(new ChartDataPoint
                {
                    Time = temp.Time,
                    Value = temp.TemperatureCelsius
                });
            }

            // Add upper normal limit line
            if (temps.Any())
            {
                var startTime = temps.First().Time;
                var endTime = temps.Last().Time;

                TempUpperNormal.Add(new ChartDataPoint { Time = startTime, Value = 37.5 });
                TempUpperNormal.Add(new ChartDataPoint { Time = endTime, Value = 37.5 });
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            if (!string.IsNullOrEmpty(PatientId))
            {
                await LoadDataAsync(Guid.Parse(PatientId));
            }
        }

        [RelayCommand]
        private async Task ExportChart()
        {
            await AppShell.DisplayToastAsync("Chart export feature coming soon");
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                Guid id = Guid.Parse(Convert.ToString(query["patientId"]));
                LoadDataAsync(id).FireAndForgetSafeAsync(_errorHandler);
                //Refresh().FireAndForgetSafeAsync(_errorHandler);
            }
        }
    }

    public class ChartDataPoint
    {
        public DateTime Time { get; set; }
        public double Value { get; set; }
    }
}
