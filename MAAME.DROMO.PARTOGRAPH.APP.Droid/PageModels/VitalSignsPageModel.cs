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
    public partial class VitalSignsPageModel : ObservableObject, IQueryAttributable
    {
        private Patient? _patient;
        private VitalSign _vitalSign = new();
        private readonly PatientRepository _patientRepository;
        private readonly VitalSignRepository _vitalSignRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private string _patientInfo = string.Empty;

        [ObservableProperty]
        private DateTime _recordedDate = DateTime.Now;

        [ObservableProperty]
        private TimeSpan _recordedTime = DateTime.Now.TimeOfDay;

        [ObservableProperty]
        private int _systolicBP = 120;

        [ObservableProperty]
        private int _diastolicBP = 80;

        [ObservableProperty]
        private decimal _temperature = 36.5m;

        [ObservableProperty]
        private int _pulseRate = 80;

        [ObservableProperty]
        private int _respiratoryRate = 16;

        [ObservableProperty]
        private string _urineOutput = string.Empty;

        [ObservableProperty]
        private string _urineProtein = "Nil";

        [ObservableProperty]
        private string _urineAcetone = "Nil";

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        private bool _isHypertensive;

        [ObservableProperty]
        private bool _hasFever;

        [ObservableProperty]
        private bool _hasAbnormalValues;

        [ObservableProperty]
        private string _abnormalValuesSummary = string.Empty;

        [ObservableProperty]
        bool _isBusy;

        public VitalSignsPageModel(PatientRepository patientRepository,
            VitalSignRepository vitalSignRepository,
            ModalErrorHandler errorHandler)
        {
            _patientRepository = patientRepository;
            _vitalSignRepository = vitalSignRepository;
            _errorHandler = errorHandler;

            RecordedBy = Preferences.Get("StaffName", "Staff");
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                int patientId = Convert.ToInt32(query["patientId"]);
                LoadPatient(patientId).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        private async Task LoadPatient(int patientId)
        {
            try
            {
                _patient = await _patientRepository.GetAsync(patientId);
                if (_patient != null)
                {
                    PatientName = _patient.Name;
                    PatientInfo = _patient.DisplayInfo;
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
        }

        partial void OnSystolicBPChanged(int value)
        {
            CheckVitalSigns();
        }

        partial void OnDiastolicBPChanged(int value)
        {
            CheckVitalSigns();
        }

        partial void OnTemperatureChanged(decimal value)
        {
            HasFever = value > 37.5m;
            CheckVitalSigns();
        }

        partial void OnPulseRateChanged(int value)
        {
            CheckVitalSigns();
        }

        private void CheckVitalSigns()
        {
            IsHypertensive = SystolicBP >= 140 || DiastolicBP >= 90;

            var abnormalities = new List<string>();

            if (IsHypertensive)
                abnormalities.Add("High BP");

            if (HasFever)
                abnormalities.Add("Fever");

            if (PulseRate > 100)
                abnormalities.Add("Tachycardia");

            if (PulseRate < 60)
                abnormalities.Add("Bradycardia");

            if (RespiratoryRate > 20)
                abnormalities.Add("Tachypnea");

            HasAbnormalValues = abnormalities.Any();
            AbnormalValuesSummary = string.Join(", ", abnormalities);
        }

        [RelayCommand]
        private async Task Save()
        {
            if (_patient == null)
            {
                _errorHandler.HandleError(new Exception("Patient information not loaded."));
                return;
            }

            try
            {
                IsBusy = true;

                var recordedDateTime = RecordedDate.Date + RecordedTime;

                _vitalSign.PatientID = _patient.ID;
                _vitalSign.RecordedTime = recordedDateTime;
                _vitalSign.SystolicBP = SystolicBP;
                _vitalSign.DiastolicBP = DiastolicBP;
                _vitalSign.Temperature = Temperature;
                _vitalSign.PulseRate = PulseRate;
                _vitalSign.RespiratoryRate = RespiratoryRate;
                _vitalSign.UrineOutput = UrineOutput;
                _vitalSign.UrineProtein = UrineProtein;
                _vitalSign.UrineAcetone = UrineAcetone;
                _vitalSign.RecordedBy = RecordedBy;

                await _vitalSignRepository.SaveItemAsync(_vitalSign);

                // Alert for critical values
                if (HasAbnormalValues)
                {
                    await AppShell.DisplaySnackbarAsync($"⚠️ Abnormal vital signs: {AbnormalValuesSummary}");

                    // Update patient status if critical
                    if (IsHypertensive && SystolicBP >= 160)
                    {
                        _patient.Status = LaborStatus.Emergency;
                        await _patientRepository.SaveItemAsync(_patient);
                    }
                }

                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Vital signs recorded successfully");
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
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
