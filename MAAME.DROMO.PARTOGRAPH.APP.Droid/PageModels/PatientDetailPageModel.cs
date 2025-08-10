using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class PatientDetailPageModel : ObservableObject, IQueryAttributable
    {
        private Patient? _patient;
        private readonly PatientRepository _patientRepository;
        private readonly PartographEntryRepository _partographRepository;
        private readonly VitalSignRepository _vitalSignRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _hospitalNumber = string.Empty;

        [ObservableProperty]
        private int _age;

        [ObservableProperty]
        private int _gravidity;

        [ObservableProperty]
        private int _parity;

        [ObservableProperty]
        private DateTime _admissionDate = DateTime.Now;

        [ObservableProperty]
        private DateTime? _expectedDeliveryDate;

        [ObservableProperty]
        private string _bloodGroup = string.Empty;

        [ObservableProperty]
        private string _phoneNumber = string.Empty;

        [ObservableProperty]
        private string _emergencyContact = string.Empty;

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

        [ObservableProperty]
        private List<PartographEntry> _partographEntries = [];

        [ObservableProperty]
        private List<VitalSign> _vitalSigns = [];

        [ObservableProperty]
        private List<MedicalNote> _medicalNotes = [];

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        private bool _isEditMode = false;

        public bool IsNewPatient => _patient?.ID == 0;

        public PatientDetailPageModel(PatientRepository patientRepository,
            PartographEntryRepository partographRepository,
            VitalSignRepository vitalSignRepository,
            ModalErrorHandler errorHandler)
        {
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _vitalSignRepository = vitalSignRepository;
            _errorHandler = errorHandler;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("id"))
            {
                int id = Convert.ToInt32(query["id"]);
                LoadData(id).FireAndForgetSafeAsync(_errorHandler);
            }
            else
            {
                // New patient
                _patient = new Patient();
                IsEditMode = true;
            }
        }

        private async Task LoadData(int id)
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

                Name = _patient.Name;
                HospitalNumber = _patient.HospitalNumber;
                Age = _patient.Age;
                Gravidity = _patient.Gravidity;
                Parity = _patient.Parity;
                AdmissionDate = _patient.AdmissionDate;
                ExpectedDeliveryDate = _patient.ExpectedDeliveryDate;
                BloodGroup = _patient.BloodGroup;
                PhoneNumber = _patient.PhoneNumber;
                EmergencyContact = _patient.EmergencyContact;
                Status = _patient.Status;
                MembraneStatus = _patient.MembraneStatus;
                LiquorStatus = _patient.LiquorStatus;
                CervicalDilationOnAdmission = _patient.CervicalDilationOnAdmission;
                RiskFactors = _patient.RiskFactors;
                Complications = _patient.Complications;

                PartographEntries = _patient.PartographEntries;
                VitalSigns = _patient.VitalSigns;
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

            _patient.Name = Name;
            _patient.HospitalNumber = HospitalNumber;
            _patient.Age = Age;
            _patient.Gravidity = Gravidity;
            _patient.Parity = Parity;
            _patient.AdmissionDate = AdmissionDate;
            _patient.ExpectedDeliveryDate = ExpectedDeliveryDate;
            _patient.BloodGroup = BloodGroup;
            _patient.PhoneNumber = PhoneNumber;
            _patient.EmergencyContact = EmergencyContact;
            _patient.Status = Status;
            _patient.MembraneStatus = MembraneStatus;
            _patient.LiquorStatus = LiquorStatus;
            _patient.CervicalDilationOnAdmission = CervicalDilationOnAdmission;
            _patient.RiskFactors = RiskFactors;
            _patient.Complications = Complications;

            await _patientRepository.SaveItemAsync(_patient);

            if (IsNewPatient)
            {
                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Patient registered successfully");
            }
            else
            {
                IsEditMode = false;
                await AppShell.DisplayToastAsync("Patient information updated");
            }
        }

        [RelayCommand]
        private Task NavigateToPartograph()
            => Shell.Current.GoToAsync($"partograph?id={_patient?.ID}");

        [RelayCommand]
        private Task AddPartographEntry()
            => Shell.Current.GoToAsync($"partographentry?patientId={_patient?.ID}");

        [RelayCommand]
        private Task AddVitalSigns()
            => Shell.Current.GoToAsync($"vitalsigns?patientId={_patient?.ID}");

        [RelayCommand]
        private async Task StartActiveLabor()
        {
            if (_patient != null)
            {
                _patient.Status = LaborStatus.Active;
                _patient.LaborStartTime = DateTime.Now;
                await _patientRepository.SaveItemAsync(_patient);
                Status = _patient.Status;
                await AppShell.DisplayToastAsync("Active labor started");
            }
        }

        [RelayCommand]
        private async Task CompleteDelivery()
        {
            if (_patient != null)
            {
                _patient.Status = LaborStatus.Completed;
                _patient.DeliveryTime = DateTime.Now;
                await _patientRepository.SaveItemAsync(_patient);
                Status = _patient.Status;
                await AppShell.DisplayToastAsync("Delivery completed");
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (_patient?.ID > 0)
            {
                var answer = await Shell.Current.DisplayAlert(
                    "Delete Patient",
                    $"Are you sure you want to delete {_patient.Name}'s record?",
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
