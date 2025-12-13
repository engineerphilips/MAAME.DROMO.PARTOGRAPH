using System;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    /// <summary>
    /// Bishop Score - Predicts likelihood of successful vaginal delivery or labor induction
    /// Based on WHO Labour Care Guide 2020
    /// </summary>
    public class BishopScore : BasePartographMeasurement
    {
        // Component Scores (stored for transparency and verification)
        public int Dilation { get; set; } // 0-3 points
        public int Effacement { get; set; } // 0-3 points
        public int Consistency { get; set; } // 0-2 points
        public int Position { get; set; } // 0-2 points
        public int Station { get; set; } // 0-3 points

        // Total Score (0-13)
        public int TotalScore { get; set; }

        // Clinical Parameters (actual values for reference)
        public int? DilationCm { get; set; } // Actual dilation in cm
        public int? EffacementPercent { get; set; } // Actual effacement percentage
        public string CervicalConsistency { get; set; } = string.Empty; // Firm, Medium, Soft
        public string CervicalPosition { get; set; } = string.Empty; // Posterior, Mid, Anterior
        public int? StationValue { get; set; } // Actual station (-3 to +2)

        // Interpretation
        public string Interpretation { get; set; } = string.Empty;
        public bool FavorableForDelivery { get; set; } // True if score ≥ 8

        // Clinical Notes
        public string Notes { get; set; } = string.Empty; // Additional clinical observations
        public string RecordedBy { get; set; } = string.Empty; // Staff member who recorded the score
    }

    /// <summary>
    /// Helper class for calculating Bishop Score
    /// </summary>
    public static class BishopScoreCalculator
    {
        /// <summary>
        /// Calculate Bishop score for cervical dilation component (0-3 points)
        /// </summary>
        public static int CalculateDilationScore(int dilationCm)
        {
            if (dilationCm >= 5) return 3;
            if (dilationCm >= 3) return 2;
            if (dilationCm >= 1) return 1;
            return 0; // Closed
        }

        /// <summary>
        /// Calculate Bishop score for effacement component (0-3 points)
        /// </summary>
        public static int CalculateEffacementScore(int effacementPercent)
        {
            if (effacementPercent >= 80) return 3;
            if (effacementPercent >= 60) return 2;
            if (effacementPercent >= 40) return 1;
            return 0; // 0-30%
        }

        /// <summary>
        /// Calculate Bishop score for consistency component (0-2 points)
        /// </summary>
        public static int CalculateConsistencyScore(string consistency)
        {
            return consistency?.ToLower() switch
            {
                "soft" => 2,
                "medium" => 1,
                "firm" => 0,
                _ => 0
            };
        }

        /// <summary>
        /// Calculate Bishop score for position component (0-2 points)
        /// </summary>
        public static int CalculatePositionScore(string position)
        {
            return position?.ToLower() switch
            {
                "anterior" => 2,
                "mid" or "midposition" or "mid-position" => 1,
                "posterior" => 0,
                _ => 0
            };
        }

        /// <summary>
        /// Calculate Bishop score for station component (0-3 points)
        /// </summary>
        public static int CalculateStationScore(int station)
        {
            if (station >= 1) return 3; // +1, +2
            if (station == 0 || station == -1) return 2;
            if (station == -2) return 1;
            return 0; // -3 or lower
        }

        /// <summary>
        /// Calculate total Bishop score and interpretation
        /// </summary>
        public static (int totalScore, string interpretation, bool favorable) CalculateTotalScore(
            int dilation, int effacement, int consistency, int position, int station)
        {
            int totalScore = dilation + effacement + consistency + position + station;
            string interpretation;
            bool favorable;

            if (totalScore >= 8)
            {
                interpretation = "Favorable - High likelihood of successful vaginal delivery";
                favorable = true;
            }
            else if (totalScore >= 5)
            {
                interpretation = "Intermediate - Moderate likelihood of vaginal delivery";
                favorable = false;
            }
            else
            {
                interpretation = "Unfavorable - Low likelihood, induction may fail";
                favorable = false;
            }

            return (totalScore, interpretation, favorable);
        }
    }
}
