using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class AlertThresholdService : IAlertThresholdService
    {
        private readonly HttpClient _httpClient;

        public AlertThresholdService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AlertThresholdConfiguration> GetGlobalThresholdsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<AlertThresholdConfiguration>("api/monitoring/thresholds/global");
                return response ?? GetDefaultThresholds();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting global thresholds: {ex.Message}");
                return GetDefaultThresholds();
            }
        }

        public async Task<AlertThresholdConfiguration> GetFacilityThresholdsAsync(Guid facilityId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<AlertThresholdConfiguration>($"api/monitoring/thresholds/facility/{facilityId}");
                return response ?? GetDefaultThresholds(facilityId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting facility thresholds: {ex.Message}");
                return GetDefaultThresholds(facilityId);
            }
        }

        public async Task<bool> SaveFacilityThresholdsAsync(AlertThresholdConfiguration config)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/monitoring/thresholds/facility", config);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving facility thresholds: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResetToGlobalDefaultsAsync(Guid facilityId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/monitoring/thresholds/facility/{facilityId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting to global defaults: {ex.Message}");
                return false;
            }
        }

        public async Task<List<AlertThresholdConfiguration>> GetAllCustomThresholdsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<AlertThresholdConfiguration>>("api/monitoring/thresholds/custom");
                return response ?? new List<AlertThresholdConfiguration>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting custom thresholds: {ex.Message}");
                return GenerateMockCustomThresholds();
            }
        }

        private AlertThresholdConfiguration GetDefaultThresholds(Guid? facilityId = null)
        {
            return new AlertThresholdConfiguration
            {
                Id = Guid.NewGuid(),
                FacilityId = facilityId,
                FacilityName = facilityId.HasValue ? "Facility" : "Global Defaults",
                UseCustomThresholds = facilityId.HasValue,

                // FHR thresholds (WHO 2020 guidelines)
                FHRCriticalLow = 100,
                FHRWarningLow = 110,
                FHRWarningHigh = 160,
                FHRCriticalHigh = 180,

                // BP thresholds
                SystolicWarning = 140,
                SystolicCritical = 160,
                DiastolicWarning = 90,
                DiastolicCritical = 110,

                // Temperature thresholds
                TemperatureWarning = 38.0m,
                TemperatureCritical = 38.5m,

                // Contraction thresholds
                TachysystoleThreshold = 5,

                // Labor duration thresholds (hours)
                FirstStageDurationWarning = 12,
                FirstStageDurationCritical = 18,
                SecondStageDurationWarning = 2,
                SecondStageDurationCritical = 3,

                // Measurement intervals (minutes)
                FHRMeasurementInterval = 30,
                BPMeasurementInterval = 60,
                DilatationMeasurementInterval = 240,

                // Alert escalation settings
                EscalationTimeoutMinutes = 15,
                AutoEscalateUnacknowledged = true,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "System"
            };
        }

        private List<AlertThresholdConfiguration> GenerateMockCustomThresholds()
        {
            var facilities = new[]
            {
                ("Korle Bu Teaching Hospital", Guid.NewGuid()),
                ("Ridge Hospital", Guid.NewGuid()),
                ("Tema General Hospital", Guid.NewGuid())
            };

            var random = new Random();
            return facilities.Select(f =>
            {
                var config = GetDefaultThresholds(f.Item2);
                config.FacilityName = f.Item1;
                config.UseCustomThresholds = true;

                // Slightly adjust thresholds for each facility
                config.FHRWarningLow = 110 + random.Next(-5, 3);
                config.FHRWarningHigh = 160 + random.Next(-3, 5);
                config.SystolicWarning = 140 + random.Next(-5, 5);
                config.EscalationTimeoutMinutes = 15 + random.Next(-5, 10);

                return config;
            }).ToList();
        }
    }
}
