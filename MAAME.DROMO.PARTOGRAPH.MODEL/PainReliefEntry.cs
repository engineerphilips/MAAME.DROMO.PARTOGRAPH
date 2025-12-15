using System;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Pain Relief - WHO Labour Care Guide Section 2: Supportive Care
    // WHO 2020: All women have the right to pain relief during labor
    // Recommend non-pharmacological methods first, then pharmacological if needed
    public class PainReliefEntry : BasePartographMeasurement
    {
        // Basic Pain Relief Status (legacy field)
        public string? PainRelief { get; set; }
        public string? PainReliefDisplay => PainRelief != null ? (PainRelief == "Y" ? "Yes" : PainRelief == "N" ? "No" : PainRelief == "D" ? "Declined" : string.Empty) : string.Empty;

        // Pain Assessment (WHO: use validated pain scales)
        public int? PainScoreBefore { get; set; } // 0-10 (VAS/NRS)
        public int? PainScoreAfter { get; set; } // 0-10 (assess 30-60 min after intervention)
        public string PainAssessmentTool { get; set; } = string.Empty; // VAS, NRS, WongBaker, Verbal

        // Pain Relief Method
        public string PainReliefMethod { get; set; } = string.Empty; // None, NonPharmacological, Epidural, Pethidine, GasAndAir, TENS, LocalBlock, SpinalBlock

        // Non-Pharmacological Methods (WHO: effective and recommended first-line)
        public string NonPharmacologicalMethods { get; set; } = string.Empty; // Positioning, Breathing, Massage, WaterImmersion, Music, Hypnobirthing

        // Pharmacological Details
        public DateTime? AdministeredTime { get; set; }
        public string AdministeredBy { get; set; } = string.Empty; // Midwife, Anesthetist, Doctor
        public string Dose { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty; // IV, IM, Epidural, Spinal, Inhalation

        // Effectiveness Assessment (WHO: document response to pain relief)
        public string Effectiveness { get; set; } = string.Empty; // Poor, Fair, Good, Excellent
        public int? TimeToEffectMinutes { get; set; }
        public int? DurationOfEffectHours { get; set; }

        // Side Effects (critical for patient safety)
        public bool SideEffects { get; set; }
        public string SideEffectsDescription { get; set; } = string.Empty; // Nausea, Vomiting, Drowsiness, Hypotension, MotorBlock, FetalDistress

        // Monitoring Requirements (especially for epidural)
        public bool ContinuousMonitoringRequired { get; set; }
        public bool BladderCareRequired { get; set; } // Epidural often requires catheterization

        // Top-up/Additional Doses (for epidural)
        public DateTime? LastTopUpTime { get; set; }
        public int TopUpCount { get; set; }

        // Contraindications Check (WHO: assess before administration)
        public bool ContraindicationsChecked { get; set; }
        public bool ContraindicationsPresent { get; set; }
        public string ContraindicationDetails { get; set; } = string.Empty;

        // Patient Consent
        public bool InformedConsentObtained { get; set; }
        public string PatientPreference { get; set; } = string.Empty; // Requested, Offered, Declined

        // WHO 2020 Alert Status
        public string ClinicalAlert { get; set; } = string.Empty;
        // Maternal hypotension post-epidural (systolic <90 or drop >30mmHg)
        // Fetal bradycardia post-epidural
        // High/total spinal block (emergency)
        // Pethidine given <4 hours before delivery (neonatal resuscitation risk)
    }
}
