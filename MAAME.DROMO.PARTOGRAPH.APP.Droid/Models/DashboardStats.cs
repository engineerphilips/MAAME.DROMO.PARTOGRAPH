using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Models
{
    public class DashboardStats
    {
        public int TotalPatients { get; set; }
        public int PendingLabor { get; set; }
        public int ActiveLabor { get; set; }
        public int CompletedToday { get; set; }
        public int EmergencyCases { get; set; }
        public List<LaborProgressData> RecentProgress { get; set; } = [];
    }

    public class LaborProgressData
    {
        public string PatientName { get; set; } = string.Empty;
        public int HoursInLabor { get; set; }
        public int CervicalDilation { get; set; }
        public LaborStatus Status { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
