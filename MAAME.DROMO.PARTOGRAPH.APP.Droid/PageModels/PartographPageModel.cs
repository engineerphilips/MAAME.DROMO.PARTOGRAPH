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
        private Patient? _patient;
        private readonly PatientRepository _patientRepository;
        private readonly PartographEntryRepository _partographRepository;
        private readonly ModalErrorHandler _errorHandler;

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
        private ObservableCollection<PartographEntry> _partographEntries = new();

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

        public PartographPageModel(PatientRepository patientRepository,
            PartographEntryRepository partographRepository,
            ModalErrorHandler errorHandler)
        {
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _errorHandler = errorHandler;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("id"))
            {
                int id = Convert.ToInt32(query["id"]);
                LoadData(id).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        private async Task LoadData(int patientId)
        {
            try
            {
                IsBusy = true;

                _patient = await _patientRepository.GetAsync(patientId);
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
                PartographEntries = new ObservableCollection<PartographEntry>(entries.OrderBy(e => e.RecordedTime));

                if (entries.Any())
                {
                    LastRecordedTime = entries.Max(e => e.RecordedTime);
                    var latestEntry = entries.OrderByDescending(e => e.RecordedTime).First();
                    CurrentDilation = latestEntry.CervicalDilation;
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

            var baseTime = _patient?.LaborStartTime ?? PartographEntries.First().RecordedTime;

            // Cervical Dilation Data
            var dilationData = new ObservableCollection<ChartDataPoint>();
            foreach (var entry in PartographEntries)
            {
                dilationData.Add(new ChartDataPoint
                {
                    Time = entry.RecordedTime,
                    Value = entry.CervicalDilation
                });
            }
            CervicalDilationData = dilationData;

            // Fetal Heart Rate Data
            var fhrData = new ObservableCollection<ChartDataPoint>();
            foreach (var entry in PartographEntries.Where(e => e.FetalHeartRate > 0))
            {
                fhrData.Add(new ChartDataPoint
                {
                    Time = entry.RecordedTime,
                    Value = entry.FetalHeartRate
                });
            }
            FetalHeartRateData = fhrData;

            // Contractions Data
            var contractionsData = new ObservableCollection<ChartDataPoint>();
            foreach (var entry in PartographEntries)
            {
                contractionsData.Add(new ChartDataPoint
                {
                    Time = entry.RecordedTime,
                    Value = entry.ContractionsPerTenMinutes
                });
            }
            ContractionsData = contractionsData;
        }

        private void CalculateAlertActionLines()
        {
            if (_patient?.LaborStartTime == null) return;

            var startTime = _patient.LaborStartTime.Value;
            var alertLine = new ObservableCollection<ChartDataPoint>();
            var actionLine = new ObservableCollection<ChartDataPoint>();

            // Alert line: Expected progress of 1cm/hour from 4cm
            // Starting from when patient reached 4cm dilation
            var fourCmEntry = PartographEntries.FirstOrDefault(e => e.CervicalDilation >= 4);
            if (fourCmEntry != null)
            {
                var fourCmTime = fourCmEntry.RecordedTime;

                // Alert line - normal progress
                for (int i = 4; i <= 10; i++)
                {
                    alertLine.Add(new ChartDataPoint
                    {
                        Time = fourCmTime.AddHours(i - 4),
                        Value = i
                    });
                }

                // Action line - 2 hours behind alert line
                for (int i = 4; i <= 10; i++)
                {
                    actionLine.Add(new ChartDataPoint
                    {
                        Time = fourCmTime.AddHours(i - 4 + 2),
                        Value = i
                    });
                }
            }

            AlertLineData = alertLine;
            ActionLineData = actionLine;
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
