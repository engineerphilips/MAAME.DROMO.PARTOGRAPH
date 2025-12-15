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
    }
}
