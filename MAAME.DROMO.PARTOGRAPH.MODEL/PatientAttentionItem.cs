namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    public class PatientAttentionItem
    {
        public Guid PatientId { get; set; }
        public Guid PartographId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int OverdueMinutes { get; set; }
    }
}
