namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    public class OverdueMeasurement
    {
        public Guid PartographId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MeasurementType { get; set; } = string.Empty;
        public DateTime LastMeasurementTime { get; set; }
        public int MinutesOverdue { get; set; }
    }
}
