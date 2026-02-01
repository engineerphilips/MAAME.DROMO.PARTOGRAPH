using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System.Collections.Generic;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public partial class BishopScorePopupPageModel : ObservableObject
    {
        public Action? ClosePopup { get; set; }
        public Action<BishopScore>? OnScoreSaved { get; set; }

        // Input properties - actual clinical values
        private int _dilationCm;
        public int DilationCm
        {
            get => _dilationCm;
            set
            {
                if (SetProperty(ref _dilationCm, value))
                {
                    UpdateTotalScore();
                }
            }
        }

        private int _effacementPercent;
        public int EffacementPercent
        {
            get => _effacementPercent;
            set
            {
                if (SetProperty(ref _effacementPercent, value))
                {
                    UpdateTotalScore();
                }
            }
        }

        private string _selectedStation = "-3";
        public string SelectedStation
        {
            get => _selectedStation;
            set
            {
                if (SetProperty(ref _selectedStation, value))
                {
                    UpdateTotalScore();
                }
            }
        }

        private string _selectedConsistency = "Firm";
        public string SelectedConsistency
        {
            get => _selectedConsistency;
            set
            {
                if (SetProperty(ref _selectedConsistency, value))
                {
                    UpdateTotalScore();
                }
            }
        }

        private string _selectedPosition = "Posterior";
        public string SelectedPosition
        {
            get => _selectedPosition;
            set
            {
                if (SetProperty(ref _selectedPosition, value))
                {
                    UpdateTotalScore();
                }
            }
        }

        // Picker options
        public List<string> StationOptions { get; } = new List<string> { "-3", "-2", "-1", "0", "+1", "+2" };
        public List<string> ConsistencyOptions { get; } = new List<string> { "Firm", "Medium", "Soft" };
        public List<string> PositionOptions { get; } = new List<string> { "Posterior", "Mid-position", "Anterior" };

        // Score display properties
        [ObservableProperty] private int _totalScore;
        [ObservableProperty] private string _scoreInterpretation = "Enter cervical exam findings to calculate";
        [ObservableProperty] private Color _totalScoreColor = Colors.Gray;
        [ObservableProperty] private string _notes = string.Empty;

        public BishopScorePopupPageModel()
        {
            UpdateTotalScore();
        }

        /// <summary>
        /// Initialize popup with existing data (for loading from measurables)
        /// </summary>
        public void InitializeWithData(int dilationCm, int effacementPercent, string station, string consistency, string position)
        {
            _dilationCm = dilationCm;
            _effacementPercent = effacementPercent;
            _selectedStation = station ?? "-3";
            _selectedConsistency = consistency ?? "Firm";
            _selectedPosition = position ?? "Posterior";

            // Notify all properties changed
            OnPropertyChanged(nameof(DilationCm));
            OnPropertyChanged(nameof(EffacementPercent));
            OnPropertyChanged(nameof(SelectedStation));
            OnPropertyChanged(nameof(SelectedConsistency));
            OnPropertyChanged(nameof(SelectedPosition));

            UpdateTotalScore();
        }

        private void UpdateTotalScore()
        {
            // Calculate Bishop score components from clinical values
            int dilationScore = BishopScoreCalculator.CalculateDilationScore(DilationCm);
            int effacementScore = BishopScoreCalculator.CalculateEffacementScore(EffacementPercent);
            int consistencyScore = BishopScoreCalculator.CalculateConsistencyScore(SelectedConsistency);
            int positionScore = BishopScoreCalculator.CalculatePositionScore(SelectedPosition);

            // Parse station value and calculate score
            int stationValue = 0;
            if (!string.IsNullOrEmpty(SelectedStation))
            {
                // Remove '+' sign if present and parse
                var stationStr = SelectedStation.Replace("+", "");
                if (int.TryParse(stationStr, out stationValue))
                {
                    // stationValue is now correctly parsed
                }
            }
            int stationScore = BishopScoreCalculator.CalculateStationScore(stationValue);

            // Calculate total and interpretation
            var result = BishopScoreCalculator.CalculateTotalScore(
                dilationScore, effacementScore, consistencyScore, positionScore, stationScore);

            TotalScore = result.totalScore;
            ScoreInterpretation = result.interpretation;
            TotalScoreColor = result.favorable ? Colors.Green : (result.totalScore >= 5 ? Colors.Orange : Colors.Red);
        }

        [RelayCommand]
        private void Save()
        {
            // Create Bishop score object with both clinical values and calculated scores
            int dilationScore = BishopScoreCalculator.CalculateDilationScore(DilationCm);
            int effacementScore = BishopScoreCalculator.CalculateEffacementScore(EffacementPercent);
            int consistencyScore = BishopScoreCalculator.CalculateConsistencyScore(SelectedConsistency);
            int positionScore = BishopScoreCalculator.CalculatePositionScore(SelectedPosition);

            int stationValue = 0;
            if (!string.IsNullOrEmpty(SelectedStation))
            {
                var stationStr = SelectedStation.Replace("+", "");
                int.TryParse(stationStr, out stationValue);
            }
            int stationScore = BishopScoreCalculator.CalculateStationScore(stationValue);

            var score = new BishopScore
            {
                // Store component scores (0-3 or 0-2)
                Dilation = dilationScore,
                Effacement = effacementScore,
                Consistency = consistencyScore,
                Position = positionScore,
                Station = stationScore,

                // Store actual clinical values for reference
                DilationCm = DilationCm,
                EffacementPercent = EffacementPercent,
                CervicalConsistency = SelectedConsistency,
                CervicalPosition = SelectedPosition,
                StationValue = stationValue,

                // Total score and interpretation
                TotalScore = TotalScore,
                Interpretation = ScoreInterpretation,
                FavorableForDelivery = TotalScore >= 8,

                // Metadata
                Time = DateTime.Now,
                Notes = Notes,
                Handler =new Guid(Constants.Staff?.StaffID ?? Preferences.Get("StaffId", Guid.NewGuid().ToString())),
                HandlerName = Constants.Staff?.Name ?? Preferences.Get("StaffName", "Staff")
            };

            OnScoreSaved?.Invoke(score);
            ClosePopup?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            ClosePopup?.Invoke();
        }
    }
}
