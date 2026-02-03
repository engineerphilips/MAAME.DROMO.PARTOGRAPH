using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class PatientPageModel : ObservableObject, IQueryAttributable
    {
        private Patient? _patient;
        private readonly PatientRepository _patientRepository;
        private readonly PartographRepository _partographRepository;
        private readonly CervixDilatationRepository _cervixDilatationRepository;
        private readonly HeadDescentRepository _headDescentRepository;
        private readonly BishopScoreRepository _bishopScoreRepository;
        private readonly PartographDiagnosisRepository _partographDiagnosisRepository;
        private readonly PartographRiskFactorRepository _partographRiskFactorRepository;
        //private readonly VitalSignRepository _vitalSignRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _hospitalNumber = string.Empty;

        [ObservableProperty]
        private int? _age;
        
        private DateTime? _dateOfBirth;
        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                SetProperty(ref _dateOfBirth, value);
                OnPropertyChanged(nameof(AgeIsReadOnly));
                LoadAgeFromDateOfBirth();
                CalculateRiskAssessment();
            }
        }

        public bool AgeIsReadOnly => DateOfBirth != null;

        public void LoadAgeFromDateOfBirth()
        {
            if (DateOfBirth != null)
                Age = new EMPEROR.COMMON.Age(DateOfBirth.Value, DateTime.Today).Years;
        }

        private int _gravidity;
        public int Gravidity
        {
            get => _gravidity;
            set
            {
                SetProperty(ref _gravidity, value);
                OnPropertyChanged(nameof(AbortionVisibility));
                LoadAbortion();
                CalculateRiskAssessment();
            }
        }

        private int _parity;

        public int Parity
        {
            get => _parity;
            set
            {
                SetProperty(ref _parity, value);
                OnPropertyChanged(nameof(AbortionVisibility));
                LoadAbortion();
                CalculateRiskAssessment();
            }
        }

        [ObservableProperty]
        private int _abortion;
        public bool AbortionVisibility => Gravidity - Parity > 1;
        public void LoadAbortion()
        {
            if (AbortionVisibility)
                Abortion = (Gravidity - Parity) - 1;
        }

        [ObservableProperty]
        private DateTime _admissionDate = DateTime.Now;

        private DateTime? _expectedDeliveryDate = null;
        public DateTime? ExpectedDeliveryDate
        {
            get => _expectedDeliveryDate;
            set
            {
                SetProperty(ref _expectedDeliveryDate, value);
                OnPropertyChanged(nameof(FormattedGestationalAge));
                OnPropertyChanged(nameof(GestationalAgeVisibility));
                OnPropertyChanged(nameof(GestationalStatus));
                OnPropertyChanged(nameof(IsPostDate));
                OnPropertyChanged(nameof(IsSVDAllowed));
                OnPropertyChanged(nameof(RecommendedDeliveryMode));
                CalculateRiskAssessment();
            }
        }

        private DateTime? _lastMenstrualDate = null;
        public DateTime? LastMenstrualDate
        {
            get => _lastMenstrualDate;
            set
            {
                SetProperty(ref _lastMenstrualDate, value);
                OnPropertyChanged(nameof(FormattedGestationalAge));
                OnPropertyChanged(nameof(GestationalAgeVisibility));
                OnPropertyChanged(nameof(GestationalStatus));
                OnPropertyChanged(nameof(IsPostDate));
                OnPropertyChanged(nameof(IsSVDAllowed));
                OnPropertyChanged(nameof(RecommendedDeliveryMode));
                CalculateRiskAssessment();
            }
        }

        public string FormattedGestationalAge => ExpectedDeliveryDate != null ? new EMPEROR.COMMON.GestationalAge().Age(ExpectedDeliveryDate.Value, DateTime.Now, true) : LastMenstrualDate != null ? new EMPEROR.COMMON.GestationalAge().Age(LastMenstrualDate.Value, DateTime.Now, false) : string.Empty;

        public bool GestationalAgeVisibility => !string.IsNullOrWhiteSpace(FormattedGestationalAge);

        //[ObservableProperty]
        private DateTime? _laborStartDate = DateTime.Now;
        public DateTime? LaborStartDate
        {
            get => _laborStartDate; 
            set => SetProperty(ref _laborStartDate, value);
        }
        //[ObservableProperty]
        private TimeSpan? _laborStartTime = DateTime.Now.TimeOfDay;
        public TimeSpan? LaborStartTime
        {
            get => _laborStartTime;
            set => SetProperty(ref _laborStartTime, value);
        }
        [ObservableProperty]
        private DateOnly? _rupturedMembraneDate = null;
        [ObservableProperty]
        private TimeSpan? _rupturedMembraneTime = null;

        [ObservableProperty]
        private string _address = string.Empty;

        [ObservableProperty]
        private string _bloodGroup = string.Empty;

        public List<string> BloodGroupOptions => new()
        {
            "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-", "Unknown"
        };

        public List<string> RelationshipOptions => new()
        {
            "Spouse", "Partner", "Mother", "Father", "Sister", "Brother",
            "Daughter", "Son", "Aunt", "Uncle", "Grandmother", "Grandfather",
            "Mother-in-law", "Father-in-law", "Friend", "Neighbor", "Other"
        };

        [ObservableProperty]
        private string _phoneNumber = string.Empty;

        [ObservableProperty]
        private string _emergencyContactPhone = string.Empty;

        [ObservableProperty]
        private string _emergencyContactRelationship = string.Empty;

        [ObservableProperty]
        private string _emergencyContactName = string.Empty;

        // Anthropometric Data for BMI Calculation
        private double? _weight;
        public double? Weight
        {
            get => _weight;
            set
            {
                SetProperty(ref _weight, value);
                OnPropertyChanged(nameof(HasBmiData));
                OnPropertyChanged(nameof(Bmi));
                OnPropertyChanged(nameof(FormattedBmi));
                OnPropertyChanged(nameof(BmiCategory));
                OnPropertyChanged(nameof(BmiColor));
                CalculateRiskAssessment();
            }
        }

        private double? _height;
        public double? Height
        {
            get => _height;
            set
            {
                SetProperty(ref _height, value);
                OnPropertyChanged(nameof(HasBmiData));
                OnPropertyChanged(nameof(Bmi));
                OnPropertyChanged(nameof(FormattedBmi));
                OnPropertyChanged(nameof(BmiCategory));
                OnPropertyChanged(nameof(BmiColor));
                CalculateRiskAssessment();
            }
        }

        public bool HasBmiData => Weight != null && Height != null && Height > 0;

        public double? Bmi
        {
            get
            {
                if (!HasBmiData) return null;
                double heightM = Height.Value / 100.0;
                return Weight.Value / (heightM * heightM);
            }
        }

        public string FormattedBmi => Bmi != null ? $"BMI: {Bmi.Value:F1} kg/m²" : "";

        public string BmiCategory => Bmi switch
        {
            null => "",
            < 18.5 => "Underweight",
            >= 18.5 and < 25 => "Normal",
            >= 25 and < 30 => "Overweight",
            >= 30 and < 35 => "Obese",
            >= 35 and < 40 => "Average Obesity",
            >= 40 => "Severe obesity",
            _ => ""
        };

        public string BmiColor => Bmi switch
        {
            null => "#808080",
            < 18.5 => "#FF9800",
            >= 18.5 and < 25 => "#4CAF50",
            >= 25 and < 30 => "#FFC107",
            >= 30 and < 35 => "#FF5722",
            >= 35 => "#F44336",
            _ => "#808080"
        };

        // Previous Pregnancy Outcomes
        [ObservableProperty]
        private bool _hasPreviousCSection = false;

        [ObservableProperty]
        private int? _numberOfPreviousCsections;

        [ObservableProperty]
        private int? _liveBirths;

        [ObservableProperty]
        private int? _stillbirths;

        [ObservableProperty]
        private int? _neonatalDeaths;

        public bool HasPreviousPregnancies => Parity > 0;

        [ObservableProperty]
        private bool _hasRupturedMembrane = false;

        [ObservableProperty]
        private LaborStatus _status = LaborStatus.Pending;

        [ObservableProperty]
        private string _membraneStatus = "Intact";

        [ObservableProperty]
        private string _liquorStatus = "Clear";

        private int? _cervicalDilationOnAdmission;
        public int? CervicalDilationOnAdmission
        {
            get => _cervicalDilationOnAdmission;
            set
            {
                SetProperty(ref _cervicalDilationOnAdmission, value);
                OnPropertyChanged(nameof(HasDilatationValue));
                OnPropertyChanged(nameof(LaborStatusIndicator));
                OnPropertyChanged(nameof(LaborStatusColor));

                // Sync with Bishop Score dilatation when induction is planned
                if (IsInductionPlanned && value.HasValue)
                {
                    BishopDilatation = value.Value;
                }
            }
        }

        public bool HasDilatationValue => CervicalDilationOnAdmission != null;

        public string LaborStatusIndicator => CervicalDilationOnAdmission switch
        {
            null => "",
            <= 4 => "Latent Phase / Not in Active Labour",
            > 4 and <= 7 => "Active First Stage - Early",
            > 7 and < 10 => "Active First Stage - Advanced",
            10 => "Fully Dilated - Second Stage",
            _ => ""
        };

        public string LaborStatusColor => CervicalDilationOnAdmission switch
        {
            null => "#808080",
            <= 4 => "#FF9800",
            > 4 and <= 7 => "#FFC107",
            > 7 and < 10 => "#FF9800",
            10 => "#F44336",
            _ => "#808080"
        };

        //[ObservableProperty]
        //private string _riskFactors = string.Empty;

        private ObservableCollection<Diagnosis> _riskFactors = new ObservableCollection<Diagnosis>();

        public ObservableCollection<Diagnosis> RiskFactors
        {
            get { return _riskFactors; }
            set
            {
                SetProperty(ref _riskFactors, value);
                OnPropertyChanged(nameof(RiskFactors));
            }
        }

        [ObservableProperty]
        private string _complications = string.Empty;

        // Bishop Score Calculator
        [ObservableProperty]
        private bool _isInductionPlanned = false;

        private int _bishopDilatation = 0;
        public int BishopDilatation
        {
            get => _bishopDilatation;
            set
            {
                SetProperty(ref _bishopDilatation, value);
                OnPropertyChanged(nameof(CalculatedBishopScore));
                OnPropertyChanged(nameof(BishopScoreInterpretation));
                CalculateRiskAssessment();

                // Sync with CervicalDilationOnAdmission if it's not already set or different
                if (CervicalDilationOnAdmission == null || CervicalDilationOnAdmission.Value != value)
                {
                    _cervicalDilationOnAdmission = value;
                    OnPropertyChanged(nameof(CervicalDilationOnAdmission));
                    OnPropertyChanged(nameof(HasDilatationValue));
                    OnPropertyChanged(nameof(LaborStatusIndicator));
                    OnPropertyChanged(nameof(LaborStatusColor));
                }
            }
        }

        private int _bishopEffacement = 0;
        public int BishopEffacement
        {
            get => _bishopEffacement;
            set
            {
                SetProperty(ref _bishopEffacement, value);
                OnPropertyChanged(nameof(CalculatedBishopScore));
                OnPropertyChanged(nameof(BishopScoreInterpretation));
                CalculateRiskAssessment();
            }
        }

        private string _bishopStation = "-3";
        public string BishopStation
        {
            get => _bishopStation;
            set
            {
                SetProperty(ref _bishopStation, value);
                OnPropertyChanged(nameof(CalculatedBishopScore));
                OnPropertyChanged(nameof(BishopScoreInterpretation));
                CalculateRiskAssessment();
            }
        }

        private string _bishopConsistency = "Firm";
        public string BishopConsistency
        {
            get => _bishopConsistency;
            set
            {
                SetProperty(ref _bishopConsistency, value);
                OnPropertyChanged(nameof(CalculatedBishopScore));
                OnPropertyChanged(nameof(BishopScoreInterpretation));
                CalculateRiskAssessment();
            }
        }

        private string _bishopPosition = "Posterior";
        public string BishopPosition
        {
            get => _bishopPosition;
            set
            {
                SetProperty(ref _bishopPosition, value);
                OnPropertyChanged(nameof(CalculatedBishopScore));
                OnPropertyChanged(nameof(BishopScoreInterpretation));
            }
        }

        public List<string> StationOptions => new() { "-3", "-2", "-1", "0", "+1", "+2", "+3" };
        public List<string> ConsistencyOptions => new() { "Firm", "Medium", "Soft" };
        public List<string> PositionOptions => new() { "Posterior", "Mid", "Anterior" };

        public int CalculatedBishopScore
        {
            get
            {
                int score = 0;

                // Dilatation (0-3 points)
                score += BishopDilatation switch
                {
                    0 => 0,
                    >= 1 and <= 2 => 1,
                    >= 3 and <= 4 => 2,
                    >= 5 => 3,
                    _ => 0
                };

                // Effacement (0-3 points)
                score += BishopEffacement switch
                {
                    >= 0 and < 30 => 0,
                    >= 30 and < 50 => 1,
                    >= 50 and < 80 => 2,
                    >= 80 => 3,
                    _ => 0
                };

                // Station (0-3 points)
                score += BishopStation switch
                {
                    "-3" => 0,
                    "-2" => 0,
                    "-1" => 1,
                    "0" => 1,
                    "+1" => 2,
                    "+2" => 2,
                    "+3" => 3,
                    _ => 0
                };

                // Consistency (0-2 points)
                score += BishopConsistency switch
                {
                    "Firm" => 0,
                    "Medium" => 1,
                    "Soft" => 2,
                    _ => 0
                };

                // Position (0-2 points)
                score += BishopPosition switch
                {
                    "Posterior" => 0,
                    "Mid" => 1,
                    "Anterior" => 2,
                    _ => 0
                };

                return score;
            }
        }

        public string BishopScoreInterpretation => CalculatedBishopScore switch
        {
            <= 5 => "Unfavorable (Induction may be difficult)",
            >= 6 and <= 8 => "Moderately Favorable",
            >= 9 => "Favorable (Good for induction)",
            //_ => ""
        };

        private ObservableCollection<Diagnosis> _diagnoses = new ObservableCollection<Diagnosis>();

        public ObservableCollection<Diagnosis> Diagnoses
        {
            get { return _diagnoses; }
            set
            {
                SetProperty(ref _diagnoses, value);
                OnPropertyChanged(nameof(Diagnoses));
            }
        }

        [ObservableProperty]
        private List<Partograph> _partographEntries = [];

        //[ObservableProperty]
        //private List<VitalSign> _vitalSigns = [];

        [ObservableProperty]
        private List<MedicalNote> _medicalNotes = [];

        // Validation Properties
        private Dictionary<string, string> _errors = new();

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError = false;

        // Risk Assessment Properties
        [ObservableProperty]
        private int _riskScore = 0;

        [ObservableProperty]
        private string _riskLevel = "Low";

        [ObservableProperty]
        private string _riskColor = "#4CAF50";

        [ObservableProperty]
        private ObservableCollection<string> _riskAssessmentFactors = new();

        [ObservableProperty]
        private ObservableCollection<string> _recommendedActions = new();

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        private bool _isEditMode = false;

        public bool IsNewPatient => _patient?.ID == null;

        //VitalSignRepository vitalSignRepository,
        public PatientPageModel(PatientRepository patientRepository,
            PartographRepository partographRepository,
            CervixDilatationRepository cervixDilatationRepository,
            HeadDescentRepository headDescentRepository,
            BishopScoreRepository bishopScoreRepository,
            PartographDiagnosisRepository partographDiagnosisRepository,
            PartographRiskFactorRepository partographRiskFactorRepository,
            ModalErrorHandler errorHandler)
        {
            //_vitalSignRepository = vitalSignRepository;
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _cervixDilatationRepository = cervixDilatationRepository;
            _headDescentRepository = headDescentRepository;
            _bishopScoreRepository = bishopScoreRepository;
            _partographDiagnosisRepository = partographDiagnosisRepository;
            _partographRiskFactorRepository = partographRiskFactorRepository;
            _errorHandler = errorHandler;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("id"))
            {
                Guid? id = Guid.Parse(Convert.ToString(query["id"]));
                LoadData(id).FireAndForgetSafeAsync(_errorHandler);
            }
            else
            {
                // New patient - reset all fields for fresh entry
                ResetForm();
            }
        }

        /// <summary>
        /// Resets all form fields to prepare for a new patient entry.
        /// This ensures clean state when onboarding a new patient.
        /// </summary>
        public void ResetForm()
        {
            // Reset patient object
            _patient = new Patient();
            IsEditMode = true;

            // Reset patient demographics
            FirstName = string.Empty;
            LastName = string.Empty;
            HospitalNumber = string.Empty;
            Age = null;
            _dateOfBirth = null;
            OnPropertyChanged(nameof(DateOfBirth));
            OnPropertyChanged(nameof(AgeIsReadOnly));
            BloodGroup = string.Empty;
            PhoneNumber = string.Empty;
            Address = string.Empty;

            // Reset emergency contact
            EmergencyContactName = string.Empty;
            EmergencyContactPhone = string.Empty;
            EmergencyContactRelationship = string.Empty;

            // Reset anthropometric data
            Weight = null;
            Height = null;

            // Reset obstetric history
            _gravidity = 0;
            OnPropertyChanged(nameof(Gravidity));
            _parity = 0;
            OnPropertyChanged(nameof(Parity));
            Abortion = 0;
            OnPropertyChanged(nameof(AbortionVisibility));
            HasPreviousCSection = false;
            NumberOfPreviousCsections = null;
            LiveBirths = null;
            Stillbirths = null;
            NeonatalDeaths = null;

            // Reset dates
            _expectedDeliveryDate = null;
            OnPropertyChanged(nameof(ExpectedDeliveryDate));
            _lastMenstrualDate = null;
            OnPropertyChanged(nameof(LastMenstrualDate));
            OnPropertyChanged(nameof(FormattedGestationalAge));
            OnPropertyChanged(nameof(GestationalAgeVisibility));
            OnPropertyChanged(nameof(GestationalStatus));
            OnPropertyChanged(nameof(IsPostDate));
            OnPropertyChanged(nameof(IsSVDAllowed));
            OnPropertyChanged(nameof(RecommendedDeliveryMode));

            // Reset labor information
            _laborStartDate = DateTime.Now;
            OnPropertyChanged(nameof(LaborStartDate));
            _laborStartTime = DateTime.Now.TimeOfDay;
            OnPropertyChanged(nameof(LaborStartTime));
            HasRupturedMembrane = false;
            RupturedMembraneDate = null;
            RupturedMembraneTime = null;
            _cervicalDilationOnAdmission = null;
            OnPropertyChanged(nameof(CervicalDilationOnAdmission));
            OnPropertyChanged(nameof(HasDilatationValue));
            OnPropertyChanged(nameof(LaborStatusIndicator));
            OnPropertyChanged(nameof(LaborStatusColor));
            Status = LaborStatus.Pending;
            MembraneStatus = "Intact";
            LiquorStatus = "Clear";

            // Reset Bishop Score
            IsInductionPlanned = false;
            _bishopDilatation = 0;
            OnPropertyChanged(nameof(BishopDilatation));
            _bishopEffacement = 0;
            OnPropertyChanged(nameof(BishopEffacement));
            _bishopStation = "-3";
            OnPropertyChanged(nameof(BishopStation));
            _bishopConsistency = "Firm";
            OnPropertyChanged(nameof(BishopConsistency));
            _bishopPosition = "Posterior";
            OnPropertyChanged(nameof(BishopPosition));
            OnPropertyChanged(nameof(CalculatedBishopScore));
            OnPropertyChanged(nameof(BishopScoreInterpretation));

            // Reset collections
            RiskFactors.Clear();
            Diagnoses.Clear();
            PartographEntries = new List<Partograph>();
            MedicalNotes = new List<MedicalNote>();

            // Reset risk assessment
            RiskScore = 0;
            RiskLevel = "Low";
            RiskColor = "#4CAF50";
            RiskAssessmentFactors.Clear();
            RecommendedActions.Clear();

            // Reset validation state
            _errors.Clear();
            HasError = false;
            ErrorMessage = string.Empty;

            // Recalculate risk assessment with clean state
            CalculateRiskAssessment();

            // Notify new patient state
            OnPropertyChanged(nameof(IsNewPatient));
            OnPropertyChanged(nameof(HasPreviousPregnancies));
        }

        private async Task LoadData(Guid? id)
        {
            try
            {
                IsBusy = true;

                _patient = await _patientRepository.GetAsync(id);

                if (_patient == null)
                {
                    _errorHandler.HandleError(new Exception($"Patient with id {id} could not be found."));
                    return;
                }

                FirstName = _patient.FirstName;
                LastName = _patient.LastName;
                HospitalNumber = _patient.HospitalNumber;
                Age = _patient.Age ?? 0;
                DateOfBirth = _patient.DateOfBirth != null ? new DateTime(_patient.DateOfBirth.Value.Year, _patient.DateOfBirth.Value.Month, _patient.DateOfBirth.Value.Day) : null;
                BloodGroup = _patient.BloodGroup;
                PhoneNumber = _patient.PhoneNumber;
                Address = _patient.Address;
                EmergencyContactName = _patient.EmergencyContactName;
                EmergencyContactPhone = _patient.EmergencyContactPhone;
                EmergencyContactRelationship = _patient.EmergencyContactRelationship;

                // Load anthropometric data
                Weight = _patient.Weight;
                Height = _patient.Height;

                // Load previous pregnancy outcomes
                HasPreviousCSection = _patient.HasPreviousCSection;
                NumberOfPreviousCsections = _patient.NumberOfPreviousCsections;
                LiveBirths = _patient.LiveBirths;
                Stillbirths = _patient.Stillbirths;
                NeonatalDeaths = _patient.NeonatalDeaths;

                PartographEntries = _patient.PartographEntries;
                MedicalNotes = _patient.MedicalNotes;
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool ValidateForm()
        {
            _errors.Clear();

            // Required fields
            if (string.IsNullOrWhiteSpace(FirstName))
                _errors.Add(nameof(FirstName), "First name is required");

            if (string.IsNullOrWhiteSpace(LastName))
                _errors.Add(nameof(LastName), "Last name is required");

            if (string.IsNullOrWhiteSpace(HospitalNumber))
                _errors.Add(nameof(HospitalNumber), "Hospital number/MRN is required");

            if (Age == null && DateOfBirth == null)
                _errors.Add(nameof(Age), "Age or Date of Birth is required");

            // Obstetric validation
            if (Gravidity < Parity)
                _errors.Add(nameof(Gravidity), "Gravidity cannot be less than Parity");

            if (ExpectedDeliveryDate == null && LastMenstrualDate == null)
                _errors.Add(nameof(ExpectedDeliveryDate), "Either EDD or LMP is required");

            if (LaborStartDate == null && LaborStartTime == null)
                _errors.Add(nameof(LaborStartDate), "Labour onset date and time are required");

            // Date logic validation
            if (DateOfBirth != null && DateOfBirth.Value > DateTime.Today)
                _errors.Add(nameof(DateOfBirth), "Date of birth cannot be in the future");

            // EDD can be in the past - we validate based on gestational age instead
            // EGA > 41W5D should not allow SVD

            // Phone validation (basic)
            if (!string.IsNullOrWhiteSpace(PhoneNumber) && PhoneNumber.Length < 10)
                _errors.Add(nameof(PhoneNumber), "Phone number should be at least 10 digits");

            // Previous C-section validation
            if (HasPreviousCSection && (NumberOfPreviousCsections == null || NumberOfPreviousCsections <= 0))
                _errors.Add(nameof(NumberOfPreviousCsections), "Number of previous C-sections is required");

            HasError = _errors.Any();
            ErrorMessage = string.Join("\n• ", _errors.Values);
            if (HasError)
                ErrorMessage = "Please correct the following:\n• " + ErrorMessage;

            return !HasError;
        }

        private void CalculateRiskAssessment()
        {
            RiskAssessmentFactors.Clear();
            RecommendedActions.Clear();
            int score = 0;

            // Age-based risk
            if (Age < 18)
            {
                score += 2;
                RiskAssessmentFactors.Add("Young maternal age (<18 years)");
            }
            else if (Age > 35)
            {
                score += 2;
                RiskAssessmentFactors.Add("Advanced maternal age (>35 years)");
            }

            // Parity-based risk
            if (Parity == 0)
            {
                score += 1;
                RiskAssessmentFactors.Add("Nulliparous (First pregnancy)");
            }
            else if (Parity >= 5)
            {
                score += 3;
                RiskAssessmentFactors.Add("Grand multiparity (≥5 births)");
            }

            // Gestational age risk - WHO guidelines for post-term pregnancy
            if (ExpectedDeliveryDate != null || LastMenstrualDate != null)
            {
                var (weeks, days) = CalculateGestationalWeeksAndDays();
                int totalDays = weeks * 7 + days;

                if (weeks < 37)
                {
                    score += 3;
                    RiskAssessmentFactors.Add($"Preterm labour ({weeks}W{days}D)");
                }
                else if (totalDays > 292) // > 41W5D
                {
                    score += 5; // Critical risk - CS required
                    RiskAssessmentFactors.Add($"Post Date - Critical ({weeks}W{days}D) - SVD NOT ALLOWED");
                    RecommendedActions.Add("CESAREAN SECTION REQUIRED - EGA exceeds 41W5D");
                    RecommendedActions.Add("SVD is contraindicated at this gestational age");
                }
                else if (totalDays >= 287) // >= 41W
                {
                    score += 3;
                    RiskAssessmentFactors.Add($"Post Date ({weeks}W{days}D)");
                    RecommendedActions.Add("Consider induction of labour or CS");
                    RecommendedActions.Add("Increased fetal monitoring required");
                }
                else if (totalDays > 280) // > 40W (Prolonged pregnancy)
                {
                    score += 2;
                    RiskAssessmentFactors.Add($"Prolonged Pregnancy ({weeks}W{days}D)");
                    RecommendedActions.Add("Monitor closely for signs of post-term complications");
                    RecommendedActions.Add("Consider membrane sweep or induction planning");
                }
            }

            // BMI-based risk
            if (Bmi < 18.5)
            {
                score += 1;
                RiskAssessmentFactors.Add("Underweight (BMI < 18.5)");
            }
            else if (Bmi >= 30 && Bmi < 35)
            {
                score += 2;
                RiskAssessmentFactors.Add($"Obesity (BMI {Bmi:F1})");
            }
            else if (Bmi >= 35)
            {
                score += 3;
                RiskAssessmentFactors.Add($"Severe obesity (BMI {Bmi:F1})");
            }

            // Previous C-section
            if (HasPreviousCSection)
            {
                score += 2;
                RiskAssessmentFactors.Add("Previous cesarean (VBAC attempt)");
                if (NumberOfPreviousCsections > 1)
                {
                    score += 1;
                    RiskAssessmentFactors.Add($"Multiple previous C-sections ({NumberOfPreviousCsections})");
                }
            }

            // Adverse pregnancy outcomes
            if (Stillbirths > 0)
            {
                score += 2;
                RiskAssessmentFactors.Add($"Previous stillbirth(s) ({Stillbirths})");
            }

            if (NeonatalDeaths > 0)
            {
                score += 2;
                RiskAssessmentFactors.Add($"Previous neonatal death(s) ({NeonatalDeaths})");
            }

            // Membrane rupture duration
            if (RupturedMembraneDate != null && RupturedMembraneTime != null && HasRupturedMembrane)
            {
                var ruptureDateTime = new DateTime(RupturedMembraneDate.Value.Year,
                    RupturedMembraneDate.Value.Month, RupturedMembraneDate.Value.Day)
                    .Add(RupturedMembraneTime.Value);
                var rupturedHours = (DateTime.Now - ruptureDateTime).TotalHours;

                if (rupturedHours > 18)
                {
                    score += 3;
                    RiskAssessmentFactors.Add($"Prolonged rupture ({rupturedHours:F0}h) - Infection risk");
                }
            }

            // Additional risk factors from manual entry
            if (RiskFactors != null && RiskFactors.Count > 0)
            {
                score += RiskFactors.Count;
                foreach (var risk in RiskFactors)
                {
                    RiskAssessmentFactors.Add($"{risk.Name}");
                }
            }

            // Set risk level and recommendations
            RiskScore = score;

            if (score == 0)
            {
                RiskLevel = "Low Risk";
                RiskColor = "#4CAF50"; // Green
                RecommendedActions.Add("Continue routine monitoring");
                RecommendedActions.Add("Standard labour management");
            }
            else if (score >= 1 && score <= 3)
            {
                RiskLevel = "Moderate Risk";
                RiskColor = "#FFC107"; // Amber
                RecommendedActions.Add("Increase monitoring frequency");
                RecommendedActions.Add("Notify senior clinician");
                RecommendedActions.Add("Ensure emergency equipment ready");
            }
            else if (score >= 4 && score <= 6)
            {
                RiskLevel = "High Risk";
                RiskColor = "#FF5722"; // Deep Orange
                RecommendedActions.Add("Continuous monitoring required");
                RecommendedActions.Add("Senior clinician review mandatory");
                RecommendedActions.Add("Prepare for emergency interventions");
                RecommendedActions.Add("Consider referral to higher facility");
            }
            else
            {
                RiskLevel = "Critical Risk";
                RiskColor = "#F44336"; // Red
                RecommendedActions.Add("IMMEDIATE senior clinician review");
                RecommendedActions.Add("Activate emergency response team");
                RecommendedActions.Add("Prepare for emergency cesarean");
                RecommendedActions.Add("Notify neonatal team");
            }

            if (RiskAssessmentFactors.Count == 0)
            {
                RiskAssessmentFactors.Add("No significant risk factors identified");
            }
        }

        private int CalculateGestationalWeeks()
        {
            var (weeks, _) = CalculateGestationalWeeksAndDays();
            return weeks;
        }

        /// <summary>
        /// Calculates gestational age in weeks and days for more precise assessment.
        /// Uses EDD-based calculation: 280 days (40 weeks) minus days until EDD.
        /// Example: If EDD was yesterday, gestational age = 40W1D (281 days).
        /// </summary>
        private (int weeks, int days) CalculateGestationalWeeksAndDays()
        {
            int totalDays = 0;
            var today = DateTime.Today;

            if (ExpectedDeliveryDate != null)
            {
                var edd = new DateTime(ExpectedDeliveryDate.Value.Year,
                    ExpectedDeliveryDate.Value.Month, ExpectedDeliveryDate.Value.Day);
                // 280 days = 40 weeks (full term)
                // Subtract days remaining until EDD to get current gestational age
                totalDays = 280 - (edd - today).Days;
            }
            else if (LastMenstrualDate != null)
            {
                var lmp = new DateTime(LastMenstrualDate.Value.Year,
                    LastMenstrualDate.Value.Month, LastMenstrualDate.Value.Day);
                totalDays = (today - lmp).Days;
            }

            if (totalDays < 0) totalDays = 0;

            int weeks = totalDays / 7;
            int days = totalDays % 7;
            return (weeks, days);
        }

        /// <summary>
        /// Determines gestational status based on EGA
        /// - Normal: ≤40W
        /// - Prolonged Pregnancy: >40W (40W1D to 40W6D)
        /// - Post Date: ≥41W
        /// </summary>
        public string GestationalStatus
        {
            get
            {
                if (ExpectedDeliveryDate == null && LastMenstrualDate == null)
                    return string.Empty;

                var (weeks, days) = CalculateGestationalWeeksAndDays();
                int totalDays = weeks * 7 + days;

                // 41W5D = 41*7 + 5 = 292 days
                // 41W0D = 41*7 = 287 days
                // 40W0D = 40*7 = 280 days

                if (totalDays > 292) // > 41W5D
                    return "Post Date - SVD Not Recommended";
                else if (totalDays >= 287) // >= 41W
                    return "Post Date";
                else if (totalDays > 280) // > 40W
                    return "Prolonged Pregnancy";
                else
                    return "Term";
            }
        }

        /// <summary>
        /// Indicates if the pregnancy is post-date (≥41W)
        /// </summary>
        public bool IsPostDate
        {
            get
            {
                var (weeks, _) = CalculateGestationalWeeksAndDays();
                return weeks >= 41;
            }
        }

        /// <summary>
        /// Determines if SVD is allowed based on gestational age
        /// SVD is NOT allowed when EGA > 41W5D
        /// </summary>
        public bool IsSVDAllowed
        {
            get
            {
                if (ExpectedDeliveryDate == null && LastMenstrualDate == null)
                    return true; // Default to allowed if no date info

                var (weeks, days) = CalculateGestationalWeeksAndDays();
                int totalDays = weeks * 7 + days;

                // 41W5D = 292 days - SVD not allowed beyond this
                return totalDays <= 292;
            }
        }

        /// <summary>
        /// Gets the recommended delivery mode based on gestational age
        /// </summary>
        public string RecommendedDeliveryMode
        {
            get
            {
                if (ExpectedDeliveryDate == null && LastMenstrualDate == null)
                    return "SVD"; // Default

                var (weeks, days) = CalculateGestationalWeeksAndDays();
                int totalDays = weeks * 7 + days;

                // > 41W5D (292 days) - CS recommended
                if (totalDays > 292)
                    return "CS";

                return "SVD";
            }
        }

        [RelayCommand]
        private void ToggleEditMode()
        {
            IsEditMode = !IsEditMode;
        }

        [RelayCommand]
        private async Task Save()
        {
            // Validate form first
            if (!ValidateForm())
            {
                await AppShell.DisplayToastAsync("Please correct the errors before saving");
                return;
            }

            // Check for duplicate patient by hospital number (prevent duplicate onboarding)
            var existingPatient = await _patientRepository.CheckDuplicatePatientAsync(
                HospitalNumber,
                _patient?.ID);

            if (existingPatient != null)
            {
                var duplicateMessage = $"A patient with hospital number '{HospitalNumber}' already exists:\n\n" +
                    $"Name: {existingPatient.FirstName} {existingPatient.LastName}\n" +
                    $"MRN: {existingPatient.HospitalNumber}\n\n" +
                    "Please verify the patient information or use a different hospital number.";

                await Shell.Current.DisplayAlert("Duplicate Patient", duplicateMessage, "OK");

                _errors.Add(nameof(HospitalNumber), "Hospital number already exists");
                HasError = true;
                ErrorMessage = "Please correct the following:\n• Hospital number already exists for another patient";
                return;
            }

            // Calculate risk assessment
            CalculateRiskAssessment();

            if (_patient == null)
            {
                _patient = new Patient();
            }

            // Save patient data
            _patient.FirstName = FirstName;
            _patient.LastName = LastName;
            _patient.HospitalNumber = HospitalNumber;
            _patient.DateOfBirth = DateOfBirth != null ? DateOnly.FromDateTime(DateOfBirth.Value) : null;
            _patient.Age = Age;
            _patient.BloodGroup = BloodGroup;
            _patient.PhoneNumber = PhoneNumber;
            _patient.Address = Address;
            _patient.EmergencyContactName = EmergencyContactName;
            _patient.EmergencyContactPhone = EmergencyContactPhone;
            _patient.EmergencyContactRelationship = EmergencyContactRelationship;

            // Save anthropometric data
            _patient.Weight = Weight;
            _patient.Height = Height;

            // Set handler and facility from logged-in user
            _patient.Handler = Constants.Staff?.ID;
            _patient.HandlerName = Constants.Staff?.Name ?? string.Empty;
            _patient.FacilityID = Constants.GetFacilityForFiltering();

            // Save previous pregnancy outcomes
            _patient.HasPreviousCSection = HasPreviousCSection;
            _patient.NumberOfPreviousCsections = NumberOfPreviousCsections;
            _patient.LiveBirths = LiveBirths;
            _patient.Stillbirths = Stillbirths;
            _patient.NeonatalDeaths = NeonatalDeaths;

            var id = await _patientRepository.SaveItemAsync(_patient);

            if (id != null)
            {
                _patient.ID = id;
                var partographId = await _partographRepository.SaveItemAsync(new Partograph
                {
                    PatientID = id.Value,
                    Patient = _patient,
                    CreatedTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    AdmissionDate = DateTime.Now,
                    Gravida = Gravidity,
                    Parity = Parity,
                    Abortion = Abortion,
                    Time = DateTime.Now,
                    LaborStartTime = LaborStartDate != null && LaborStartTime != null ? new DateTime(LaborStartDate.Value.Year, LaborStartDate.Value.Month, LaborStartDate.Value.Day).Add(LaborStartTime.Value) : null,
                    LiquorStatus = LiquorStatus,
                    MembraneStatus = MembraneStatus,
                    ExpectedDeliveryDate = ExpectedDeliveryDate != null ? DateOnly.FromDateTime(ExpectedDeliveryDate.Value) : null,
                    LastMenstrualDate = LastMenstrualDate != null ? DateOnly.FromDateTime(LastMenstrualDate.Value) : null,
                    RupturedMembraneTime = RupturedMembraneDate != null && RupturedMembraneTime != null ? new DateTime(RupturedMembraneDate.Value.Year, RupturedMembraneDate.Value.Month, RupturedMembraneDate.Value.Day).Add(RupturedMembraneTime.Value) : null,
                    Status = CervicalDilationOnAdmission > 4 ? LaborStatus.FirstStage : LaborStatus.Pending,
                    // Set handler and facility from logged-in user
                    Handler = Constants.Staff?.ID,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    FacilityID = Constants.GetFacilityForFiltering(),
                    // Risk Assessment Summary
                    RiskScore = RiskScore,
                    RiskLevel = RiskLevel,
                    RiskColor = RiskColor
                });
                //LaborStartDate.Value.ToDateTime(LaborStartTime.Value)

                //RiskFactors = string.Empty,
                if (partographId != null)
                {
                    //if (IsNewPatient)
                    //{
                    //}

                    if (Diagnoses != null && Diagnoses.Count > 0)
                    {
                        foreach (var diagnosis in Diagnoses)
                        {
                            await _partographDiagnosisRepository.SaveItemAsync(new PartographDiagnosis
                            {
                                PartographID = partographId.Value,
                                Time = DateTime.Now,
                                Name = diagnosis.Name,
                                HandlerName = Constants.Staff?.Name ?? string.Empty,
                                Handler = Constants.Staff?.ID
                            });
                        }
                    }

                    if (RiskFactors != null && RiskFactors.Count > 0)
                    {
                        foreach (var riskFactor  in RiskFactors)
                        {
                            await _partographRiskFactorRepository.SaveItemAsync(new PartographRiskFactor
                            {
                                PartographID = partographId.Value,
                                Time = DateTime.Now,
                                Name = riskFactor.Name,
                                HandlerName = Constants.Staff?.Name ?? string.Empty,
                                Handler = Constants.Staff?.ID
                            });
                        }
                    }

                    // Record initial cervical dilatation if present (>0 cm)
                    if (CervicalDilationOnAdmission > 0)
                    {
                        await _cervixDilatationRepository.SaveItemAsync(new CervixDilatation
                        {
                            PartographID = partographId.Value,
                            Time = DateTime.Now,
                            DilatationCm = CervicalDilationOnAdmission ?? 0,
                            HandlerName = Constants.Staff?.Name ?? string.Empty,
                            Handler = Constants.Staff?.ID
                        });
                    }

                    // Save Bishop Score if induction is planned
                    if (IsInductionPlanned)
                    {
                        // Calculate component scores
                        var dilationScore = BishopScoreCalculator.CalculateDilationScore(BishopDilatation);
                        var effacementScore = BishopScoreCalculator.CalculateEffacementScore(BishopEffacement);
                        var stationScore = BishopScoreCalculator.CalculateStationScore(int.Parse(BishopStation.Replace("+", "")));
                        var consistencyScore = BishopScoreCalculator.CalculateConsistencyScore(BishopConsistency);
                        var positionScore = BishopScoreCalculator.CalculatePositionScore(BishopPosition);

                        var (totalScore, interpretation, favorable) = BishopScoreCalculator.CalculateTotalScore(
                            dilationScore, effacementScore, consistencyScore, positionScore, stationScore);

                        await _bishopScoreRepository.SaveItemAsync(new BishopScore
                        {
                            PartographID = partographId.Value,
                            Time = DateTime.Now,
                            Dilation = dilationScore,
                            Effacement = effacementScore,
                            Station = stationScore,
                            Consistency = consistencyScore,
                            Position = positionScore,
                            TotalScore = totalScore,
                            DilationCm = BishopDilatation,
                            EffacementPercent = BishopEffacement,
                            StationValue = int.Parse(BishopStation.Replace("+", "")),
                            CervicalConsistency = BishopConsistency,
                            CervicalPosition = BishopPosition,
                            Interpretation = interpretation,
                            FavorableForDelivery = favorable,
                            HandlerName = Constants.Staff?.Name ?? string.Empty,
                            Handler = Constants.Staff?.ID
                        });

                        // Record initial head descent from Bishop Station
                        //await _headDescentRepository.SaveItemAsync(new HeadDescent
                        //{
                        //    PartographID = partographId.Value,
                        //    Time = DateTime.Now,
                        //    Station = int.Parse(BishopStation.Replace("+", "")),
                        //    HandlerName = Constants.Staff?.Name ?? string.Empty,
                        //    Handler = Constants.Staff?.ID
                        //});
                    }

                    await AppShell.DisplayToastAsync("Patient registered successfully");
                    
                    if (CervicalDilationOnAdmission > 4)
                    {
                        await Shell.Current.GoToAsync($"//partograph?patientId={partographId}");
                    }
                    else
                        await Shell.Current.GoToAsync("..");
                }
                else
                {
                    IsEditMode = false;
                    await AppShell.DisplayToastAsync("Patient information updated");
                }
            }
        }

        [RelayCommand]
        private Task NavigateToPartograph()
            => Shell.Current.GoToAsync($"partograph?id={_patient?.ID}");

        [RelayCommand]
        private Task AddPartographEntry()
            => Shell.Current.GoToAsync($"partographentry?patientId={_patient?.ID}");

        //[RelayCommand]
        //private Task AddVitalSigns()
        //    => Shell.Current.GoToAsync($"vitalsigns?patientId={_patient?.ID}");

        [RelayCommand]
        private async Task StartActiveLabor()
        {
            if (_patient != null)
            {
                //_patient.Status = LaborStatus.Active;
                //_patient.LaborStartTime = DateTime.Now;
                await _patientRepository.SaveItemAsync(_patient);
                //Status = _patient.Status;
                await AppShell.DisplayToastAsync("Active labor started");
            }
        }

        [RelayCommand]
        private async Task CompleteDelivery()
        {
            if (_patient != null)
            {
                //_patient.Status = LaborStatus.Completed;
                //_patient.DeliveryTime = DateTime.Now;
                await _patientRepository.SaveItemAsync(_patient);
                //Status = _patient.Status;
                await AppShell.DisplayToastAsync("Delivery completed");
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (_patient?.ID != null)
            {
                var answer = await Shell.Current.DisplayAlert(
                    "Delete Patient",
                    $"Are you sure you want to delete {_patient.FirstName}'s record?",
                    "Yes", "No");

                if (answer)
                {
                    // Delete patient (cascade delete related records in real implementation)
                    await Shell.Current.GoToAsync("..");
                    await AppShell.DisplayToastAsync("Patient record deleted");
                }
            }
        }
    }
}
