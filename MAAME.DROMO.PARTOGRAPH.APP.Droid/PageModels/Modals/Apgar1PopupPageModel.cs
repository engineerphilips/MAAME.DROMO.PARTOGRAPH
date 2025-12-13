using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public partial class Apgar1PopupPageModel : ObservableObject
    {
        public Action? ClosePopup { get; set; }
        public Action<int, int?, int?, int?, int?, int?>? OnScoreSaved { get; set; }

        private readonly Color SelectedColor = Color.FromArgb("#2196F3"); // Blue
        private readonly Color UnselectedColor = Color.FromArgb("#F5F5F5"); // Light gray
        private readonly Color SelectedBorderColor = Color.FromArgb("#1976D2"); // Darker blue
        private readonly Color UnselectedBorderColor = Color.FromArgb("#E0E0E0"); // Gray

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

        // Heart Rate Colors
        [ObservableProperty] private Color _heartRate0Color;
        [ObservableProperty] private Color _heartRate1Color;
        [ObservableProperty] private Color _heartRate2Color;
        [ObservableProperty] private Color _heartRate0BorderColor;
        [ObservableProperty] private Color _heartRate1BorderColor;
        [ObservableProperty] private Color _heartRate2BorderColor;

        // Respiratory Colors
        [ObservableProperty] private Color _respiratory0Color;
        [ObservableProperty] private Color _respiratory1Color;
        [ObservableProperty] private Color _respiratory2Color;
        [ObservableProperty] private Color _respiratory0BorderColor;
        [ObservableProperty] private Color _respiratory1BorderColor;
        [ObservableProperty] private Color _respiratory2BorderColor;

        // Muscle Tone Colors
        [ObservableProperty] private Color _muscleTone0Color;
        [ObservableProperty] private Color _muscleTone1Color;
        [ObservableProperty] private Color _muscleTone2Color;
        [ObservableProperty] private Color _muscleTone0BorderColor;
        [ObservableProperty] private Color _muscleTone1BorderColor;
        [ObservableProperty] private Color _muscleTone2BorderColor;

        // Reflex Colors
        [ObservableProperty] private Color _reflex0Color;
        [ObservableProperty] private Color _reflex1Color;
        [ObservableProperty] private Color _reflex2Color;
        [ObservableProperty] private Color _reflex0BorderColor;
        [ObservableProperty] private Color _reflex1BorderColor;
        [ObservableProperty] private Color _reflex2BorderColor;

        // Color Colors
        [ObservableProperty] private Color _color0Color;
        [ObservableProperty] private Color _color1Color;
        [ObservableProperty] private Color _color2Color;
        [ObservableProperty] private Color _color0BorderColor;
        [ObservableProperty] private Color _color1BorderColor;
        [ObservableProperty] private Color _color2BorderColor;

        public Apgar1PopupPageModel()
        {
            // Initialize with -1 to indicate no selection
            _heartRate = -1;
            _respiratoryEffort = -1;
            _muscleTone = -1;
            _reflexIrritability = -1;
            _color = -1;

            InitializeColors();
        }

        public void LoadExistingScores(int? heartRate, int? respiratory, int? muscleTone, int? reflex, int? color)
        {
            if (heartRate.HasValue) SelectHeartRate(heartRate.Value);
            if (respiratory.HasValue) SelectRespiratory(respiratory.Value);
            if (muscleTone.HasValue) SelectMuscleTone(muscleTone.Value);
            if (reflex.HasValue) SelectReflex(reflex.Value);
            if (color.HasValue) SelectColor(color.Value);
        }

        private void InitializeColors()
        {
            // Heart Rate
            HeartRate0Color = HeartRate1Color = HeartRate2Color = UnselectedColor;
            HeartRate0BorderColor = HeartRate1BorderColor = HeartRate2BorderColor = UnselectedBorderColor;

            // Respiratory
            Respiratory0Color = Respiratory1Color = Respiratory2Color = UnselectedColor;
            Respiratory0BorderColor = Respiratory1BorderColor = Respiratory2BorderColor = UnselectedBorderColor;

            // Muscle Tone
            MuscleTone0Color = MuscleTone1Color = MuscleTone2Color = UnselectedColor;
            MuscleTone0BorderColor = MuscleTone1BorderColor = MuscleTone2BorderColor = UnselectedBorderColor;

            // Reflex
            Reflex0Color = Reflex1Color = Reflex2Color = UnselectedColor;
            Reflex0BorderColor = Reflex1BorderColor = Reflex2BorderColor = UnselectedBorderColor;

            // Color
            Color0Color = Color1Color = Color2Color = UnselectedColor;
            Color0BorderColor = Color1BorderColor = Color2BorderColor = UnselectedBorderColor;
        }

        [RelayCommand]
        private void SelectHeartRate(object parameter)
        {
            if (parameter is int value)
            {
                HeartRate = value;
                HeartRate0Color = HeartRate1Color = HeartRate2Color = UnselectedColor;
                HeartRate0BorderColor = HeartRate1BorderColor = HeartRate2BorderColor = UnselectedBorderColor;

                switch (value)
                {
                    case 0:
                        HeartRate0Color = SelectedColor;
                        HeartRate0BorderColor = SelectedBorderColor;
                        break;
                    case 1:
                        HeartRate1Color = SelectedColor;
                        HeartRate1BorderColor = SelectedBorderColor;
                        break;
                    case 2:
                        HeartRate2Color = SelectedColor;
                        HeartRate2BorderColor = SelectedBorderColor;
                        break;
                }
                UpdateTotalScore();
            }
        }

        [RelayCommand]
        private void SelectRespiratory(object parameter)
        {
            if (parameter is int value)
            {
                RespiratoryEffort = value;
                Respiratory0Color = Respiratory1Color = Respiratory2Color = UnselectedColor;
                Respiratory0BorderColor = Respiratory1BorderColor = Respiratory2BorderColor = UnselectedBorderColor;

                switch (value)
                {
                    case 0:
                        Respiratory0Color = SelectedColor;
                        Respiratory0BorderColor = SelectedBorderColor;
                        break;
                    case 1:
                        Respiratory1Color = SelectedColor;
                        Respiratory1BorderColor = SelectedBorderColor;
                        break;
                    case 2:
                        Respiratory2Color = SelectedColor;
                        Respiratory2BorderColor = SelectedBorderColor;
                        break;
                }
                UpdateTotalScore();
            }
        }

        [RelayCommand]
        private void SelectMuscleTone(object parameter)
        {
            if (parameter is int value)
            {
                MuscleTone = value;
                MuscleTone0Color = MuscleTone1Color = MuscleTone2Color = UnselectedColor;
                MuscleTone0BorderColor = MuscleTone1BorderColor = MuscleTone2BorderColor = UnselectedBorderColor;

                switch (value)
                {
                    case 0:
                        MuscleTone0Color = SelectedColor;
                        MuscleTone0BorderColor = SelectedBorderColor;
                        break;
                    case 1:
                        MuscleTone1Color = SelectedColor;
                        MuscleTone1BorderColor = SelectedBorderColor;
                        break;
                    case 2:
                        MuscleTone2Color = SelectedColor;
                        MuscleTone2BorderColor = SelectedBorderColor;
                        break;
                }
                UpdateTotalScore();
            }
        }

        [RelayCommand]
        private void SelectReflex(object parameter)
        {
            if (parameter is int value)
            {
                ReflexIrritability = value;
                Reflex0Color = Reflex1Color = Reflex2Color = UnselectedColor;
                Reflex0BorderColor = Reflex1BorderColor = Reflex2BorderColor = UnselectedBorderColor;

                switch (value)
                {
                    case 0:
                        Reflex0Color = SelectedColor;
                        Reflex0BorderColor = SelectedBorderColor;
                        break;
                    case 1:
                        Reflex1Color = SelectedColor;
                        Reflex1BorderColor = SelectedBorderColor;
                        break;
                    case 2:
                        Reflex2Color = SelectedColor;
                        Reflex2BorderColor = SelectedBorderColor;
                        break;
                }
                UpdateTotalScore();
            }
        }

        [RelayCommand]
        private void SelectColor(object parameter)
        {
            if (parameter is int value)
            {
                Color = value;
                Color0Color = Color1Color = Color2Color = UnselectedColor;
                Color0BorderColor = Color1BorderColor = Color2BorderColor = UnselectedBorderColor;

                switch (value)
                {
                    case 0:
                        Color0Color = SelectedColor;
                        Color0BorderColor = SelectedBorderColor;
                        break;
                    case 1:
                        Color1Color = SelectedColor;
                        Color1BorderColor = SelectedBorderColor;
                        break;
                    case 2:
                        Color2Color = SelectedColor;
                        Color2BorderColor = SelectedBorderColor;
                        break;
                }
                UpdateTotalScore();
            }
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
