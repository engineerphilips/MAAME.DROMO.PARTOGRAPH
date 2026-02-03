namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    /// <summary>
    /// Helper class for hourly alert counts
    /// </summary>
    public class AlertHourCount
    {
        public int Hour { get; set; }
        public int Count { get; set; }

        public string TimeDisplay => $"{Hour:00}:00";
    }
}
