using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    public class LaborStage
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = "#2196F3";
        public int DisplayOrder { get; set; }

        //[JsonIgnore]
        //public Brush ColorBrush => new SolidColorBrush(Microsoft.Maui.Graphics.Color.FromArgb(Color));
    }
}
