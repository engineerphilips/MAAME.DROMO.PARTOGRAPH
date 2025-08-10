using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Models
{
    public class Staff
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StaffID { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Midwife"; // Doctor, Midwife, Nurse
        public string Department { get; set; } = "Labor Ward";
        public string Password { get; set; } = string.Empty; // Should be hashed
        public DateTime LastLogin { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
