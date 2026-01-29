using System;
using System.Collections.Generic;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Models
{
    public class ClinicalAlert
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PartographId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public ClinicalAlertSeverity Severity { get; set; }
        public ClinicalAlertCategory Category { get; set; }
        
        /// <summary>
        /// Short title for the alert (e.g., "Severe Fetal Distress")
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Detailed description of the finding (e.g., "FHR < 110 bpm for > 10 minutes")
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Actionable recommendation properly referenced (e.g., "Start intrauterine resuscitation")
        /// </summary>
        public string Recommendation { get; set; }
        
        /// <summary>
        /// Reference to the specific guideline (e.g., "WHO LCG 2020: Table 2.1")
        /// </summary>
        public string GuidelineReference { get; set; }
        
        public bool IsDismissed { get; set; }
    }

    public enum ClinicalAlertSeverity
    {
        Info,
        Warning,
        Critical,
        Emergency
    }

    public enum ClinicalAlertCategory
    {
        Fetal,
        Maternal,
        LaborProgress,
        Therapeutics,
        DataQuality
    }

    public class ClinicalGuideline
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Source { get; set; } // e.g., "WHO 2020"
        public string Url { get; set; }
    }
}
