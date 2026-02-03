namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    public class HourlyAlertCount
    {
        public int Hour { get; set; }
        public int Count { get; set; }
        public int Acknowledged { get; set; }
        public int Missed { get; set; }
    }
}
