using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class EnhancedTimeSlotViewModel : INotifyPropertyChanged
    {
        private CompanionType _companion = CompanionType.No;
        private PainReliefType _painRelief = PainReliefType.No;
        private OralFluidType _oralFluid = OralFluidType.No;
        private PostureType _posture = PostureType.None;
        private FHRDecelerationType _fHRDeceleration = FHRDecelerationType.None;
        private float? _baselineFHR = null;
        private bool _isHighlighted = false;

        // Additional measurables
        private float? _temperature = null;
        private string _urineProtein = string.Empty;
        private string _urineAcetone = string.Empty;
        private decimal? _oxytocinDose = null;
        private decimal? _oxytocinVolume = null;
        private string _ivFluidType = string.Empty;
        private int? _ivFluidVolume = null;
        private string _ivFluidRate = string.Empty;
        private int? _cervixDilatation = null;
        private int? _contractionFrequency = null;
        private int? _contractionDuration = null;
        private int? _headDescentStation = null;
        private string _fetalPosition = string.Empty;
        private string _amnioticFluidColor = string.Empty;
        private string _caputDegree = string.Empty;
        private string? _mouldingDegree = null;
        private int? _bpSystolic = null;
        private int? _bpDiastolic = null;
        private int? _pulse = null;

        public DateTime Time { get; }
        public int HourNumber { get; }
        public string TimeDisplay => Time.ToString("H:mm");

        public event EventHandler DataChanged;

        // Properties with change notification
        public CompanionType Companion
        {
            get => _companion;
            set
            {
                _companion = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CompanionDisplay));
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public PainReliefType PainRelief
        {
            get => _painRelief;
            set
            {
                _painRelief = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PainReliefDisplay));
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public OralFluidType OralFluid
        {
            get => _oralFluid;
            set
            {
                _oralFluid = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(OralFluidDisplay));
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public PostureType Posture
        {
            get => _posture;
            set
            {
                _posture = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PostureDisplay));
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public float? BaselineFHR
        {
            get => _baselineFHR;
            set
            {
                _baselineFHR = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BaselineFHRDisplay));
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public FHRDecelerationType FHRDeceleration
        {
            get => _fHRDeceleration;
            set
            {
                _fHRDeceleration = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FHRDecelerationDisplay));
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                _isHighlighted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BackgroundColor));
                OnPropertyChanged(nameof(TextColor));
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // Additional measurable properties
        public float? Temperature
        {
            get => _temperature;
            set
            {
                _temperature = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string UrineProtein
        {
            get => _urineProtein;
            set
            {
                _urineProtein = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string UrineAcetone
        {
            get => _urineAcetone;
            set
            {
                _urineAcetone = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string UrineDisplay => $"{UrineProtein}/{UrineAcetone}";

        public decimal? OxytocinDose
        {
            get => _oxytocinDose;
            set
            {
                _oxytocinDose = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public decimal? OxytocinVolume
        {
            get => _oxytocinVolume;
            set
            {
                _oxytocinVolume = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string IVFluidType
        {
            get => _ivFluidType;
            set
            {
                _ivFluidType = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int? IVFluidVolume
        {
            get => _ivFluidVolume;
            set
            {
                _ivFluidVolume = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string IVFluidRate
        {
            get => _ivFluidRate;
            set
            {
                _ivFluidRate = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int? CervixDilatation
        {
            get => _cervixDilatation;
            set
            {
                _cervixDilatation = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int? ContractionFrequency
        {
            get => _contractionFrequency;
            set
            {
                _contractionFrequency = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int? ContractionDuration
        {
            get => _contractionDuration;
            set
            {
                _contractionDuration = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int? HeadDescentStation
        {
            get => _headDescentStation;
            set
            {
                _headDescentStation = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string FetalPosition
        {
            get => _fetalPosition;
            set
            {
                _fetalPosition = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string AmnioticFluidColor
        {
            get => _amnioticFluidColor;
            set
            {
                _amnioticFluidColor = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string CaputDegree
        {
            get => _caputDegree;
            set
            {
                _caputDegree = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string? MouldingDegree
        {
            get => _mouldingDegree;
            set
            {
                _mouldingDegree = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int? BPSystolic
        {
            get => _bpSystolic;
            set
            {
                _bpSystolic = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int? BPDiastolic
        {
            get => _bpDiastolic;
            set
            {
                _bpDiastolic = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int? Pulse
        {
            get => _pulse;
            set
            {
                _pulse = value;
                OnPropertyChanged();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // Display properties
        public string CompanionDisplay => GetDisplayValue(Companion);
        public string PainReliefDisplay => GetDisplayValue(PainRelief);
        public string OralFluidDisplay => GetDisplayValue(OralFluid);
        public string PostureDisplay => GetDisplayValue(Posture);
        public string FHRDecelerationDisplay => GetDisplayValue(FHRDeceleration);
        public string BaselineFHRDisplay => BaselineFHR != null && BaselineFHR > 0 ? $"{BaselineFHR}" : string.Empty;

        public Color BackgroundColor => IsHighlighted ? Colors.Red : Colors.Transparent;
        public Color TextColor => IsHighlighted ? Colors.White : Colors.Black;

        // Commands
        public Command ToggleCompanionCommand { get; }
        public Command TogglePainReliefCommand { get; }
        public Command ToggleOralFluidCommand { get; }
        public Command TogglePostureCommand { get; }
        public Command ToggleHighlightCommand { get; }

        public EnhancedTimeSlotViewModel(DateTime time, int hourNumber)
        {
            Time = time;
            HourNumber = hourNumber;

            ToggleCompanionCommand = new Command(() => Companion = GetNext(Companion));
            TogglePainReliefCommand = new Command(() => PainRelief = GetNext(PainRelief));
            ToggleOralFluidCommand = new Command(() => OralFluid = GetNext(OralFluid));
            TogglePostureCommand = new Command(() => Posture = GetNext(Posture));
            ToggleHighlightCommand = new Command(() => IsHighlighted = !IsHighlighted);
        }

        private string GetDisplayValue(Enum enumValue)
        {
            return enumValue switch
            {
                CompanionType.None => "",
                CompanionType.No => "N",
                CompanionType.Yes => "Y",
                PainReliefType.None => "",
                PainReliefType.No => "N",
                PainReliefType.Yes => "Y",
                OralFluidType.None => "",
                OralFluidType.No => "N",
                OralFluidType.Difficulty => "D",
                OralFluidType.Yes => "Y",
                PostureType.None => "",
                PostureType.Supine => "SP",
                PostureType.Mobile => "M0",
                PostureType.Upright => "UP",
                PostureType.SideLeft => "SL",
                PostureType.SideRight => "SR",
                FHRDecelerationType.No => "N",
                FHRDecelerationType.Late => "L",
                FHRDecelerationType.Early => "E",
                FHRDecelerationType.Variable => "V",
                _ => ""
            };
        }

        private T GetNext<T>(T current) where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            var currentIndex = Array.IndexOf(values, current);
            var nextIndex = (currentIndex + 1) % values.Length;
            return values[nextIndex];
        }

        //public PartographEntry GetData()
        //{
        //    return new PartographEntry
        //    {
        //        TimeStamp = Time,
        //        Companion = Companion,
        //        PainRelief = PainRelief,
        //        OralFluid = OralFluid,
        //        Posture = Posture,
        //        IsHighlighted = IsHighlighted
        //    };
        //}

        //public void LoadData(PartographEntry data)
        //{
        //    Companion = data.Companion;
        //    PainRelief = data.PainRelief;
        //    OralFluid = data.OralFluid;
        //    Posture = data.Posture;
        //    IsHighlighted = data.IsHighlighted;
        //}

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
