using MAAME.DROMO.PARTOGRAPH.MODEL;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class ShiftHandoverReportPageModel : INotifyPropertyChanged
    {
        private readonly PartographMonitoringService _monitoringService;
        private readonly AlertHistoryRepository _alertHistoryRepository;
        private bool _isLoading;
        private ShiftHandoverReport? _report;

        public ShiftHandoverReportPageModel(
            PartographMonitoringService monitoringService,
            AlertHistoryRepository alertHistoryRepository)
        {
            _monitoringService = monitoringService;
            _alertHistoryRepository = alertHistoryRepository;

            RefreshCommand = new Command(async () => await LoadReportAsync());
            StartNewShiftCommand = new Command(async () => await StartNewShiftAsync());
            NavigateToPatientCommand = new Command<PatientAttentionItem>(async (p) => await NavigateToPatientAsync(p));
            ShareReportCommand = new Command(async () => await ShareReportAsync());
        }

        #region Properties

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ShiftHandoverReport? Report
        {
            get => _report;
            set
            {
                if (SetProperty(ref _report, value))
                {
                    OnPropertyChanged(nameof(HasReport));
                    OnPropertyChanged(nameof(ShiftDuration));
                    OnPropertyChanged(nameof(ComplianceColor));
                    OnPropertyChanged(nameof(PendingAlerts));
                    OnPropertyChanged(nameof(PatientsRequiringAttention));
                    OnPropertyChanged(nameof(OverdueMeasurements));
                    OnPropertyChanged(nameof(HasPendingAlerts));
                    OnPropertyChanged(nameof(HasPatientsRequiringAttention));
                    OnPropertyChanged(nameof(HasOverdueMeasurements));
                }
            }
        }

        public bool HasReport => Report != null;

        public string ShiftDuration
        {
            get
            {
                if (Report == null) return "";
                var duration = Report.ShiftEnd - Report.ShiftStart;
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            }
        }

        public string ComplianceColor
        {
            get
            {
                if (Report == null) return "#4CAF50";
                return Report.CompliancePercentage >= 90 ? "#4CAF50" :
                       Report.CompliancePercentage >= 70 ? "#FF9800" : "#EF5350";
            }
        }

        public ObservableCollection<AlertHistoryRecord> PendingAlerts =>
            new ObservableCollection<AlertHistoryRecord>(Report?.PendingAlerts ?? new List<AlertHistoryRecord>());

        public ObservableCollection<PatientAttentionItem> PatientsRequiringAttention =>
            new ObservableCollection<PatientAttentionItem>(Report?.PatientsRequiringAttention ?? new List<PatientAttentionItem>());

        public ObservableCollection<OverdueMeasurement> OverdueMeasurements =>
            new ObservableCollection<OverdueMeasurement>(Report?.OverdueMeasurements ?? new List<OverdueMeasurement>());

        public bool HasPendingAlerts => PendingAlerts.Any();
        public bool HasPatientsRequiringAttention => PatientsRequiringAttention.Any();
        public bool HasOverdueMeasurements => OverdueMeasurements.Any();

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand StartNewShiftCommand { get; }
        public ICommand NavigateToPatientCommand { get; }
        public ICommand ShareReportCommand { get; }

        #endregion

        #region Methods

        public async Task LoadReportAsync()
        {
            try
            {
                IsLoading = true;
                Report = await _monitoringService.GenerateShiftHandoverReportAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading report: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task StartNewShiftAsync()
        {
            var confirmed = await Shell.Current.DisplayAlert(
                "Start New Shift",
                "This will end your current shift and start a new one. All pending alerts will be carried over. Continue?",
                "Yes, Start New Shift",
                "Cancel");

            if (confirmed)
            {
                _monitoringService.StartNewShift();
                await LoadReportAsync();
                await AppShell.DisplayToastAsync("New shift started");
            }
        }

        private async Task NavigateToPatientAsync(PatientAttentionItem? patient)
        {
            if (patient == null) return;

            try
            {
                await Shell.Current.GoToAsync($"partograph?id={patient.PartographId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        private async Task ShareReportAsync()
        {
            if (Report == null) return;

            try
            {
                var reportText = GenerateReportText();
                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Text = reportText,
                    Title = $"Shift Handover Report - {Report.ShiftStart:yyyy-MM-dd}"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Share error: {ex.Message}");
            }
        }

        private string GenerateReportText()
        {
            if (Report == null) return "";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine("       SHIFT HANDOVER REPORT");
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine($"Facility: {Report.FacilityName}");
            sb.AppendLine($"Staff: {Report.StaffName}");
            sb.AppendLine($"Shift: {Report.ShiftStart:HH:mm} - {Report.ShiftEnd:HH:mm}");
            sb.AppendLine($"Duration: {ShiftDuration}");
            sb.AppendLine();
            sb.AppendLine("───────────────────────────────────────");
            sb.AppendLine("           SUMMARY");
            sb.AppendLine("───────────────────────────────────────");
            sb.AppendLine($"Active Patients: {Report.TotalActivePatients}");
            sb.AppendLine($"Alerts Generated: {Report.TotalAlertsGenerated}");
            sb.AppendLine($"Alerts Acknowledged: {Report.AlertsAcknowledged}");
            sb.AppendLine($"Alerts Missed: {Report.AlertsMissed}");
            sb.AppendLine($"Compliance: {Report.CompliancePercentage:F1}%");
            sb.AppendLine();

            if (Report.PatientsRequiringAttention.Any())
            {
                sb.AppendLine("───────────────────────────────────────");
                sb.AppendLine("    PATIENTS REQUIRING ATTENTION");
                sb.AppendLine("───────────────────────────────────────");
                foreach (var patient in Report.PatientsRequiringAttention)
                {
                    sb.AppendLine($"• {patient.PatientName}");
                    sb.AppendLine($"  Reason: {patient.Reason}");
                    sb.AppendLine($"  Overdue: {patient.OverdueMinutes} minutes");
                    sb.AppendLine();
                }
            }

            if (Report.OverdueMeasurements.Any())
            {
                sb.AppendLine("───────────────────────────────────────");
                sb.AppendLine("      OVERDUE MEASUREMENTS");
                sb.AppendLine("───────────────────────────────────────");
                foreach (var measurement in Report.OverdueMeasurements)
                {
                    sb.AppendLine($"• {measurement.PatientName}: {measurement.MeasurementType}");
                    sb.AppendLine($"  Last recorded: {measurement.LastMeasurementTime:HH:mm}");
                    sb.AppendLine($"  Overdue by: {measurement.MinutesOverdue} minutes");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine("MAAME DROMO Partograph System");

            return sb.ToString();
        }

        public void OnAppearing()
        {
            _ = LoadReportAsync();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
