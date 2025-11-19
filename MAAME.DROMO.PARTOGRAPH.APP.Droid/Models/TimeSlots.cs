using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Models
{
    public class TimeSlots
    {
        public int Id { get; set; }
        public DateTime Slot { get; set; }
        public DateTime SlotFloored => new DateTime(Slot.Year, Slot.Month, Slot.Day, Slot.Hour, 0, 0, Slot.Kind);
        public string SlotFlooredStr => $"{SlotFloored:HH:mm}";

    }
}
