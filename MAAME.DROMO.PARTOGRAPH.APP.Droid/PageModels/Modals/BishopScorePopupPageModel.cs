using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public partial class BishopScorePopupPageModel : ObservableObject
    {
        public Action? ClosePopup { get; set; }
        public Action<BishopScore>? OnScoreSaved { get; set; }

        private readonly Color SelectedColor = Color.FromArgb("#2196F3");
        private readonly Color UnselectedColor = Color.FromArgb("#F5F5F5");
        private readonly Color SelectedBorderColor = Color.FromArgb("#1976D2");
        private readonly Color UnselectedBorderColor = Color.FromArgb("#E0E0E0");

        [ObservableProperty] private int _dilation;
        [ObservableProperty] private int _effacement;
        [ObservableProperty] private int _consistency;
        [ObservableProperty] private int _position;
        [ObservableProperty] private int _station;
        [ObservableProperty] private int _totalScore;
        [ObservableProperty] private string _scoreInterpretation = string.Empty;
        [ObservableProperty] private Color _totalScoreColor = Colors.Gray;

        // Dilation Colors (0-3)
        [ObservableProperty] private Color _dilation0Color;
        [ObservableProperty] private Color _dilation1Color;
        [ObservableProperty] private Color _dilation2Color;
        [ObservableProperty] private Color _dilation3Color;
        [ObservableProperty] private Color _dilation0BorderColor;
        [ObservableProperty] private Color _dilation1BorderColor;
        [ObservableProperty] private Color _dilation2BorderColor;
        [ObservableProperty] private Color _dilation3BorderColor;

        // Effacement Colors (0-3)
        [ObservableProperty] private Color _effacement0Color;
        [ObservableProperty] private Color _effacement1Color;
        [ObservableProperty] private Color _effacement2Color;
        [ObservableProperty] private Color _effacement3Color;
        [ObservableProperty] private Color _effacement0BorderColor;
        [ObservableProperty] private Color _effacement1BorderColor;
        [ObservableProperty] private Color _effacement2BorderColor;
        [ObservableProperty] private Color _effacement3BorderColor;

        // Consistency Colors (0-2)
        [ObservableProperty] private Color _consistency0Color;
        [ObservableProperty] private Color _consistency1Color;
        [ObservableProperty] private Color _consistency2Color;
        [ObservableProperty] private Color _consistency0BorderColor;
        [ObservableProperty] private Color _consistency1BorderColor;
        [ObservableProperty] private Color _consistency2BorderColor;

        // Position Colors (0-2)
        [ObservableProperty] private Color _position0Color;
        [ObservableProperty] private Color _position1Color;
        [ObservableProperty] private Color _position2Color;
        [ObservableProperty] private Color _position0BorderColor;
        [ObservableProperty] private Color _position1BorderColor;
        [ObservableProperty] private Color _position2BorderColor;

        // Station Colors (0-3)
        [ObservableProperty] private Color _station0Color;
        [ObservableProperty] private Color _station1Color;
        [ObservableProperty] private Color _station2Color;
        [ObservableProperty] private Color _station3Color;
        [ObservableProperty] private Color _station0BorderColor;
        [ObservableProperty] private Color _station1BorderColor;
        [ObservableProperty] private Color _station2BorderColor;
        [ObservableProperty] private Color _station3BorderColor;

        public BishopScorePopupPageModel()
        {
            _dilation = _effacement = _consistency = _position = _station = -1;
            InitializeColors();
        }

        private void InitializeColors()
        {
            // Dilation
            Dilation0Color = Dilation1Color = Dilation2Color = Dilation3Color = UnselectedColor;
            Dilation0BorderColor = Dilation1BorderColor = Dilation2BorderColor = Dilation3BorderColor = UnselectedBorderColor;

            // Effacement
            Effacement0Color = Effacement1Color = Effacement2Color = Effacement3Color = UnselectedColor;
            Effacement0BorderColor = Effacement1BorderColor = Effacement2BorderColor = Effacement3BorderColor = UnselectedBorderColor;

            // Consistency
            Consistency0Color = Consistency1Color = Consistency2Color = UnselectedColor;
            Consistency0BorderColor = Consistency1BorderColor = Consistency2BorderColor = UnselectedBorderColor;

            // Position
            Position0Color = Position1Color = Position2Color = UnselectedColor;
            Position0BorderColor = Position1BorderColor = Position2BorderColor = UnselectedBorderColor;

            // Station
            Station0Color = Station1Color = Station2Color = Station3Color = UnselectedColor;
            Station0BorderColor = Station1BorderColor = Station2BorderColor = Station3BorderColor = UnselectedBorderColor;
        }

        [RelayCommand]
        private void SelectDilation(object parameter)
        {
            if (parameter is int value)
            {
                Dilation = value;
                Dilation0Color = Dilation1Color = Dilation2Color = Dilation3Color = UnselectedColor;
                Dilation0BorderColor = Dilation1BorderColor = Dilation2BorderColor = Dilation3BorderColor = UnselectedBorderColor;

                switch (value)
                {
                    case 0: Dilation0Color = SelectedColor; Dilation0BorderColor = SelectedBorderColor; break;
                    case 1: Dilation1Color = SelectedColor; Dilation1BorderColor = SelectedBorderColor; break;
                    case 2: Dilation2Color = SelectedColor; Dilation2BorderColor = SelectedBorderColor; break;
                    case 3: Dilation3Color = SelectedColor; Dilation3BorderColor = SelectedBorderColor; break;
                }
                UpdateTotalScore();
            }
        }

        [RelayCommand]
        private void SelectEffacement(object parameter)
        {
            if (parameter is int value)
            {
                Effacement = value;
                Effacement0Color = Effacement1Color = Effacement2Color = Effacement3Color = UnselectedColor;
                Effacement0BorderColor = Effacement1BorderColor = Effacement2BorderColor = Effacement3BorderColor = UnselectedBorderColor;

                switch (value)
                {
                    case 0: Effacement0Color = SelectedColor; Effacement0BorderColor = SelectedBorderColor; break;
                    case 1: Effacement1Color = SelectedColor; Effacement1BorderColor = SelectedBorderColor; break;
                    case 2: Effacement2Color = SelectedColor; Effacement2BorderColor = SelectedBorderColor; break;
                    case 3: Effacement3Color = SelectedColor; Effacement3BorderColor = SelectedBorderColor; break;
                }
                UpdateTotalScore();
            }
        }

        [RelayCommand]
        private void SelectConsistency(object parameter)
        {
            if (parameter is int value)
            {
                Consistency = value;
                Consistency0Color = Consistency1Color = Consistency2Color = UnselectedColor;
                Consistency0BorderColor = Consistency1BorderColor = Consistency2BorderColor = UnselectedBorderColor;

                switch (value)
                {
                    case 0: Consistency0Color = SelectedColor; Consistency0BorderColor = SelectedBorderColor; break;
                    case 1: Consistency1Color = SelectedColor; Consistency1BorderColor = SelectedBorderColor; break;
                    case 2: Consistency2Color = SelectedColor; Consistency2BorderColor = SelectedBorderColor; break;
                }
                UpdateTotalScore();
            }
        }

        [RelayCommand]
        private void SelectPosition(object parameter)
        {
            if (parameter is int value)
            {
                Position = value;
                Position0Color = Position1Color = Position2Color = UnselectedColor;
                Position0BorderColor = Position1BorderColor = Position2BorderColor = UnselectedBorderColor;

                switch (value)
                {
                    case 0: Position0Color = SelectedColor; Position0BorderColor = SelectedBorderColor; break;
                    case 1: Position1Color = SelectedColor; Position1BorderColor = SelectedBorderColor; break;
                    case 2: Position2Color = SelectedColor; Position2BorderColor = SelectedBorderColor; break;
                }
                UpdateTotalScore();
            }
        }

        [RelayCommand]
        private void SelectStation(object parameter)
        {
            if (parameter is int value)
            {
                Station = value;
                Station0Color = Station1Color = Station2Color = Station3Color = UnselectedColor;
                Station0BorderColor = Station1BorderColor = Station2BorderColor = Station3BorderColor = UnselectedBorderColor;

                switch (value)
                {
                    case 0: Station0Color = SelectedColor; Station0BorderColor = SelectedBorderColor; break;
                    case 1: Station1Color = SelectedColor; Station1BorderColor = SelectedBorderColor; break;
                    case 2: Station2Color = SelectedColor; Station2BorderColor = SelectedBorderColor; break;
                    case 3: Station3Color = SelectedColor; Station3BorderColor = SelectedBorderColor; break;
                }
                UpdateTotalScore();
            }
        }

        private void UpdateTotalScore()
        {
            if (Dilation >= 0 && Effacement >= 0 && Consistency >= 0 && Position >= 0 && Station >= 0)
            {
                var result = BishopScoreCalculator.CalculateTotalScore(Dilation, Effacement, Consistency, Position, Station);
                TotalScore = result.totalScore;
                ScoreInterpretation = result.interpretation;
                TotalScoreColor = result.favorable ? Colors.Green : (result.totalScore >= 5 ? Colors.Orange : Colors.Red);
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
            if (Dilation < 0 || Effacement < 0 || Consistency < 0 || Position < 0 || Station < 0)
            {
                Application.Current?.MainPage?.DisplayAlert("Incomplete Score",
                    "Please score all components before saving.", "OK");
                return;
            }

            var score = new BishopScore
            {
                Dilation = Dilation,
                Effacement = Effacement,
                Consistency = Consistency,
                Position = Position,
                Station = Station,
                TotalScore = TotalScore,
                Interpretation = ScoreInterpretation,
                FavorableForDelivery = TotalScore >= 8,
                Time = DateTime.Now
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
