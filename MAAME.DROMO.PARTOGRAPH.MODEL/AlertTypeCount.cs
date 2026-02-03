namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    /// <summary>
    /// Helper class for alert type counts
    /// </summary>
    public class AlertTypeCount
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }

        public string Icon => Type switch
        {
            "FHR" or "Fetal Heart Rate" => "💓",
            "Contractions" => "📊",
            "Cervical Dilatation" or "Vaginal Examination" => "📏",
            "Blood Pressure" or "BP" => "🩺",
            "Temperature" => "🌡️",
            "Urine" => "💧",
            "ClinicalAlert" => "🏥",
            _ => "📋"
        };
    }
}
