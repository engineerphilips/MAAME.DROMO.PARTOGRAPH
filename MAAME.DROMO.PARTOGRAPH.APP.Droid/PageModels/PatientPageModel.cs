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
            }
        }

        public string FormattedGestationalAge => ExpectedDeliveryDate != null ? new EMPEROR.COMMON.GestationalAge().Age(DateTime.Now, ExpectedDeliveryDate.Value, true) : LastMenstrualDate != null ? new EMPEROR.COMMON.GestationalAge().Age(LastMenstrualDate.Value, DateTime.Now, false) : string.Empty;

        public bool GestationalAgeVisibility => !string.IsNullOrWhiteSpace(FormattedGestationalAge);

        [ObservableProperty]
        private DateOnly? _laborStartDate = null;
        [ObservableProperty]
        private TimeSpan? _laborStartTime = null;
        [ObservableProperty]
        private DateOnly? _rupturedMembraneDate = null;
        [ObservableProperty]
        private TimeSpan? _rupturedMembraneTime = null;

        [ObservableProperty]
        private string _address = string.Empty;

        [ObservableProperty]
        private string _bloodGroup = string.Empty;

        [ObservableProperty]
        private string _phoneNumber = string.Empty;

        [ObservableProperty]
        private string _emergencyContactPhone = string.Empty;

        [ObservableProperty]
        private string _emergencyContactRelationship = string.Empty;

        [ObservableProperty]
        private string _emergencyContactName = string.Empty;
        
        [ObservableProperty]
        private bool _hasRupturedMembrane = false;

        [ObservableProperty]
        private LaborStatus _status = LaborStatus.Pending;

        [ObservableProperty]
        private string _membraneStatus = "Intact";

        [ObservableProperty]
        private string _liquorStatus = "Clear";

        [ObservableProperty]
        private int? _cervicalDilationOnAdmission;

        [ObservableProperty]
        private string _riskFactors = string.Empty;

        [ObservableProperty]
        private string _complications = string.Empty;

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

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        private bool _isEditMode = false;

        public bool IsNewPatient => _patient?.ID == null;

        //VitalSignRepository vitalSignRepository,
        public PatientPageModel(PatientRepository patientRepository,
            PartographRepository partographRepository,
            ModalErrorHandler errorHandler)
        {
            //_vitalSignRepository = vitalSignRepository;
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
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
                // New patient
                _patient = new Patient();
                IsEditMode = true;
            }
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
                //Gravidity = _patient.Gravida;
                //Parity = _patient.Parity;
                //AdmissionDate = _patient.AdmissionDate;
                //ExpectedDeliveryDate = _patient.ExpectedDeliveryDate;
                BloodGroup = _patient.BloodGroup;
                PhoneNumber = _patient.PhoneNumber;
                EmergencyContactName = _patient.EmergencyContactName;
                EmergencyContactPhone = _patient.EmergencyContactPhone;
                EmergencyContactRelationship = _patient.EmergencyContactRelationship;
                //Status = _patient.Status;
                //MembraneStatus = _patient.MembraneStatus;
                //LiquorStatus = _patient.LiquorStatus;
                //CervicalDilationOnAdmission = _patient.CervicalDilationOnAdmission;
                //RiskFactors = _patient.RiskFactors;
                //Complications = _patient.Complications;

                PartographEntries = _patient.PartographEntries;
                //VitalSigns = _patient.VitalSigns;
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

        [RelayCommand]
        private void ToggleEditMode()
        {
            IsEditMode = !IsEditMode;
        }

        [RelayCommand]
        private async Task Save()
        {
            if (_patient == null)
            {
                _patient = new Patient();
            }

            _patient.FirstName = FirstName;
            _patient.LastName = LastName;
            _patient.HospitalNumber = HospitalNumber;
            _patient.DateOfBirth = DateOfBirth != null ? DateOnly.FromDateTime(DateOfBirth.Value) : null;
            _patient.Age = Age;
            //_patient.Gravida = Gravidity;
            //_patient.Parity = Parity;
            //_patient.AdmissionDate = AdmissionDate;
            //_patient.ExpectedDeliveryDate = ExpectedDeliveryDate;
            //_patient.LastMenstralDate = LastMenstralDate;
            _patient.BloodGroup = BloodGroup;
            _patient.PhoneNumber = PhoneNumber;
            _patient.EmergencyContactName = EmergencyContactName;
            _patient.EmergencyContactPhone = EmergencyContactPhone;
            _patient.EmergencyContactRelationship = EmergencyContactRelationship;
            //_patient.Status = Status;
            //_patient.MembraneStatus = MembraneStatus;
            //_patient.LiquorStatus = LiquorStatus;
            //_patient.CervicalDilationOnAdmission = CervicalDilationOnAdmission;
            //_patient.RiskFactors = RiskFactors;
            //_patient.Complications = Complications;

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
                    RiskFactors = RiskFactors,
                    ExpectedDeliveryDate = ExpectedDeliveryDate != null ? DateOnly.FromDateTime(ExpectedDeliveryDate.Value) : null,
                    LastMenstrualDate = LastMenstrualDate != null ? DateOnly.FromDateTime(LastMenstrualDate.Value) : null,
                    RupturedMembraneTime = RupturedMembraneDate != null && RupturedMembraneTime != null ? new DateTime(RupturedMembraneDate.Value.Year, RupturedMembraneDate.Value.Month, RupturedMembraneDate.Value.Day).Add(RupturedMembraneTime.Value) : null
                });
                //LaborStartDate.Value.ToDateTime(LaborStartTime.Value)

                if (partographId != null)
                {
                    //if (IsNewPatient)
                    //{
                    //}

                    await AppShell.DisplayToastAsync("Patient registered successfully");
                    //await Shell.Current.GoToAsync("..");
                    await Shell.Current.GoToAsync($"//partograph?patientId={partographId}");
                }
            }
            else
            {
                IsEditMode = false;
                await AppShell.DisplayToastAsync("Patient information updated");
            }
            ;
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
