using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class ReportsPageModel : ObservableObject
    {
        private readonly IReportService _reportService;

        [ObservableProperty]
        private int selectedTabIndex = 0;

        //[ObservableProperty]
        private DateTime startDate = DateTime.Now.AddMonths(-1);
        public DateTime StartDate
        {
            get => startDate;
            set
            {
                SetProperty(ref startDate, value);
                var tasks = new Task[1];
                tasks[0] = RefreshCurrentReportAsync();
                Task.WhenAny(tasks);
            }
        }
        
        //[ObservableProperty]
        private DateTime endDate = DateTime.Now;
        public DateTime EndDate
        {
            get => endDate;
            set
            {
                SetProperty(ref endDate, value);
                var tasks = new Task[1];
                tasks[0] = RefreshCurrentReportAsync();
                Task.WhenAny(tasks);
            }
        }

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string loadingMessage = "Loading report...";

        // High Priority Reports
        [ObservableProperty]
        private MonthlyDeliveryDashboard monthlyDashboard;

        [ObservableProperty]
        private MaternalComplicationsReport maternalComplicationsReport;

        [ObservableProperty]
        private NeonatalOutcomesReport neonatalOutcomesReport;

        [ObservableProperty]
        private AlertResponseTimeReport alertResponseReport;

        // Medium Priority Reports
        [ObservableProperty]
        private WHOComplianceReport whoComplianceReport;

        [ObservableProperty]
        private StaffPerformanceReport staffPerformanceReport;

        [ObservableProperty]
        private OfflineSyncStatusReport syncStatusReport;

        [ObservableProperty]
        private BirthWeightApgarAnalysis birthWeightApgarReport;

        // Lower Priority Reports
        [ObservableProperty]
        private TrendAnalyticsReport trendAnalyticsReport;

        public ReportsPageModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task InitializeAsync()
        {
            // Set default date range to current month
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = DateTime.Now;

            // Load the default report (Monthly Dashboard)
            await LoadMonthlyDashboardAsync();
        }

        [RelayCommand]
        private async Task LoadMonthlyDashboardAsync()
        {
            try
            {
                SelectedTabIndex = 0;
                IsLoading = true;
                LoadingMessage = "Loading Monthly Delivery Dashboard...";
                MonthlyDashboard = await _reportService.GenerateMonthlyDeliveryDashboardAsync(StartDate, EndDate);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load dashboard: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadMaternalComplicationsAsync()
        {
            try
            {
                SelectedTabIndex = 1;
                IsLoading = true;
                LoadingMessage = "Loading Maternal Complications Report...";
                MaternalComplicationsReport = await _reportService.GenerateMaternalComplicationsReportAsync(StartDate, EndDate);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load report: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadNeonatalOutcomesAsync()
        {
            try
            {
                SelectedTabIndex = 2;
                IsLoading = true;
                LoadingMessage = "Loading Neonatal Outcomes Report...";
                NeonatalOutcomesReport = await _reportService.GenerateNeonatalOutcomesReportAsync(StartDate, EndDate);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load report: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadAlertResponseAsync()
        {
            try
            {
                SelectedTabIndex = 3;
                IsLoading = true;
                LoadingMessage = "Loading Alert Response Time Report...";
                AlertResponseReport = await _reportService.GenerateAlertResponseTimeReportAsync(StartDate, EndDate);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load report: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadWHOComplianceAsync()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "Loading WHO Compliance Report...";
                WhoComplianceReport = await _reportService.GenerateWHOComplianceReportAsync(StartDate, EndDate);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load report: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadStaffPerformanceAsync()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "Loading Staff Performance Report...";
                StaffPerformanceReport = await _reportService.GenerateStaffPerformanceReportAsync(StartDate, EndDate);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load report: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadSyncStatusAsync()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "Loading Sync Status Report...";
                SyncStatusReport = await _reportService.GenerateOfflineSyncStatusReportAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load report: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadBirthWeightApgarAsync()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "Loading Birth Weight & APGAR Analysis...";
                BirthWeightApgarReport = await _reportService.GenerateBirthWeightApgarAnalysisAsync(StartDate, EndDate);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load report: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadTrendAnalyticsAsync()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "Loading Trend Analytics...";
                // Set date range to last 12 months for trend analysis
                var trendStartDate = DateTime.Now.AddMonths(-12);
                TrendAnalyticsReport = await _reportService.GenerateTrendAnalyticsReportAsync(trendStartDate, EndDate);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load report: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshCurrentReportAsync()
        {
            switch (SelectedTabIndex)
            {
                case 0:
                    await LoadMonthlyDashboardAsync();
                    break;
                case 1:
                    await LoadMaternalComplicationsAsync();
                    break;
                case 2:
                    await LoadNeonatalOutcomesAsync();
                    break;
                case 3:
                    await LoadAlertResponseAsync();
                    break;
                case 4:
                    await LoadWHOComplianceAsync();
                    break;
                case 5:
                    await LoadStaffPerformanceAsync();
                    break;
                case 6:
                    await LoadSyncStatusAsync();
                    break;
                case 7:
                    await LoadBirthWeightApgarAsync();
                    break;
                case 8:
                    await LoadTrendAnalyticsAsync();
                    break;
            }
        }

        [RelayCommand]
        private async Task ExportReportAsync()
        {
            await Shell.Current.DisplayAlert("Export", "Export functionality will be implemented soon.", "OK");
        }

        [RelayCommand]
        private async Task PrintReportAsync()
        {
            await Shell.Current.DisplayAlert("Print", "Print functionality will be implemented soon.", "OK");
        }

        partial void OnSelectedTabIndexChanged(int value)
        {
            // Auto-load report when tab changes
            Task.Run(async () => await RefreshCurrentReportAsync());
        }
    }
}
