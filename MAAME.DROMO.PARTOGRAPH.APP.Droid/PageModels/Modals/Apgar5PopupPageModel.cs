using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public partial class Apgar5PopupPageModel : ObservableObject
    {
        public Action? ClosePopup { get; set; }
        public Action<int, int?, int?, int?, int?, int?>? OnScoreSaved { get; set; }

        [ObservableProperty]
        private int _heartRate;

        [ObservableProperty]
        private int _respiratoryEffort;

        [ObservableProperty]
        private int _muscleTone;

        [ObservableProperty]
        private int _reflexIrritability;

        [ObservableProperty]
        private int _color;

        [ObservableProperty]
        private int _totalScore;

        [ObservableProperty]
        private string _scoreInterpretation = string.Empty;

        [ObservableProperty]
        private Color _totalScoreColor = Colors.Gray;

        public Apgar5PopupPageModel()
        {
            // Initialize with -1 to indicate no selection
            _heartRate = -1;
            _respiratoryEffort = -1;
            _muscleTone = -1;
            _reflexIrritability = -1;
            _color = -1;

            // Subscribe to property changes to update total score
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HeartRate) ||
                    e.PropertyName == nameof(RespiratoryEffort) ||
                    e.PropertyName == nameof(MuscleTone) ||
                    e.PropertyName == nameof(ReflexIrritability) ||
                    e.PropertyName == nameof(Color))
                {
                    UpdateTotalScore();
                }
            };
        }

        public void LoadExistingScores(int? heartRate, int? respiratory, int? muscleTone, int? reflex, int? color)
        {
            if (heartRate.HasValue) HeartRate = heartRate.Value;
            if (respiratory.HasValue) RespiratoryEffort = respiratory.Value;
            if (muscleTone.HasValue) MuscleTone = muscleTone.Value;
            if (reflex.HasValue) ReflexIrritability = reflex.Value;
            if (color.HasValue) Color = color.Value;
        }

        private void UpdateTotalScore()
        {
            // Only calculate if all components have been selected
            if (HeartRate >= 0 && RespiratoryEffort >= 0 && MuscleTone >= 0 &&
                ReflexIrritability >= 0 && Color >= 0)
            {
                TotalScore = HeartRate + RespiratoryEffort + MuscleTone + ReflexIrritability + Color;

                // Update interpretation and color based on WHO 2020 guidelines
                if (TotalScore >= 7)
                {
                    ScoreInterpretation = "Normal";
                    TotalScoreColor = Colors.Green;
                }
                else if (TotalScore >= 4)
                {
                    ScoreInterpretation = "Moderately Abnormal - Monitor closely";
                    TotalScoreColor = Colors.Orange;
                }
                else
                {
                    ScoreInterpretation = "Severely Abnormal - Immediate action required";
                    TotalScoreColor = Colors.Red;
                }
            }
            else
            {
                TotalScore = 0;
                ScoreInterpretation = "Select all components to calculate";
                TotalScoreColor = Colors.Gray;
            }
        }

        [RelayCommand]
        private void Save()
        {
            // Validate that all components have been selected
            if (HeartRate < 0 || RespiratoryEffort < 0 || MuscleTone < 0 ||
                ReflexIrritability < 0 || Color < 0)
            {
                Application.Current?.MainPage?.DisplayAlert("Incomplete Score",
                    "Please score all components before saving.", "OK");
                return;
            }

            // Invoke the callback with the scores
            OnScoreSaved?.Invoke(TotalScore, HeartRate, RespiratoryEffort, MuscleTone,
                ReflexIrritability, Color);

            // Close the popup
            ClosePopup?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            ClosePopup?.Invoke();
        }
    }
}
