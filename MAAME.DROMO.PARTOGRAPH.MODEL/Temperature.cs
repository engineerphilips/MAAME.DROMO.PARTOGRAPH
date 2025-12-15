namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Maternal Temperature - WHO Labour Care Guide Section 4: Care of the Woman
    // WHO 2020: Monitor every 4 hours if normal
    // Alert thresholds: <35.0°C or ≥37.5°C
    public class Temperature : BasePartographMeasurement
    {
        // Temperature (renamed from "Rate" for clarity)
        public float TemperatureCelsius { get; set; } // Degrees Celsius

        // Measurement Site (affects interpretation)
        public string MeasurementSite { get; set; } = "Oral"; // Oral, Axillary, Tympanic, Rectal

        // Fever Duration (if elevated)
        public int? FeverDurationHours { get; set; }

        // Associated Symptoms
        public bool ChillsPresent { get; set; }
        public string AssociatedSymptoms { get; set; } = string.Empty;

        // Repeat Measurement
        public bool RepeatedMeasurement { get; set; }

        // Calculated Property for Fahrenheit
        public float TemperatureFahrenheit
        {
            get
            {
                return (TemperatureCelsius * 9 / 5) + 32;
            }
        }

        // WHO 2020 Alert Status
        public string ClinicalAlert { get; set; } = string.Empty;
        // Fever: ≥37.5°C (triggers infection screen protocol)
        // High Fever: ≥38.5°C (severe - requires urgent assessment)
        // Hypothermia: <35.0°C

        // WHO 2020 Enhanced Temperature Assessment
        // Fever Classification
        public string FeverCategory { get; set; } = string.Empty; // Normal, LowGrade, Moderate, High, Hyperpyrexia
        public bool IntrapartumFever { get; set; }
        public DateTime? FeverOnsetTime { get; set; }
        public float? PeakTemperature { get; set; }
        public DateTime? PeakTemperatureTime { get; set; }

        // Repeat Measurements
        public float? SecondTemperature { get; set; }
        public DateTime? SecondReadingTime { get; set; }
        public float? ThirdTemperature { get; set; }
        public DateTime? ThirdReadingTime { get; set; }

        // Infection Screening
        public bool ChoriamnionitisRisk { get; set; }
        public bool ProlongedRupture { get; set; }
        public int? HoursSinceRupture { get; set; }
        public bool MaternalTachycardia { get; set; }
        public bool FetalTachycardia { get; set; }
        public bool UterineTenderness { get; set; }
        public bool OffensiveLiquor { get; set; }

        // Associated Symptoms Details
        public bool RigorPresent { get; set; }
        public bool Sweating { get; set; }
        public bool Headache { get; set; }
        public bool MyalgiaArthralgia { get; set; }

        // Sepsis Screening
        public bool SepsisScreeningDone { get; set; }
        public DateTime? SepsisScreeningTime { get; set; }
        public string SepsisRiskLevel { get; set; } = string.Empty; // Low, Moderate, High
        public bool QSOFAPositive { get; set; }
        public int? QSOFAScore { get; set; }

        // Clinical Response
        public bool AntipyreticsGiven { get; set; }
        public string AntipyreticType { get; set; } = string.Empty;
        public DateTime? AntipyreticGivenTime { get; set; }
        public bool CulturesObtained { get; set; }
        public bool AntibioticsStarted { get; set; }
        public DateTime? AntibioticsStartTime { get; set; }
        public bool IVFluidsGiven { get; set; }
        public bool CoolingMeasures { get; set; }

        // Monitoring Frequency
        public bool IncreasedMonitoring { get; set; }
        public int? MonitoringIntervalMinutes { get; set; }

        // Display
        //public string? TemperatureDisplay => $"{TemperatureCelsius:F1}°C ({TemperatureFahrenheit:F1}°F)";
        public string? TemperatureDisplay => $"{TemperatureCelsius:F1}°C";
    }
}
