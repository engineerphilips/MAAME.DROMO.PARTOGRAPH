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
    public partial class EnhancedPartographPageModel : ObservableObject, IQueryAttributable
    {
        private Patient? _patient;
        private readonly PatientRepository _patientRepository;
        private readonly PartographEntryRepository _partographRepository;
        private readonly VitalSignRepository _vitalSignRepository;
        private readonly AssessmentPlanRepository _assessmentPlanRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private string _patientInfo = string.Empty;

        [ObservableProperty]
        private string _laborDuration = string.Empty;

        [ObservableProperty]
        private DateTime _currentTime = DateTime.Now;

        [ObservableProperty]
        private DateTime _currentDate = DateTime.Now;

        [ObservableProperty]
        private bool _hasAlerts = false;

        [ObservableProperty]
        private ObservableCollection<string> _alerts = new();

        [ObservableProperty]
        private bool _hasDueMeasurements = false;

        [ObservableProperty]
        private string _dueMeasurementsText = string.Empty;

        // Due status indicators
        [ObservableProperty]
        private Color _baselineFHRDueColor = Colors.Green;

        [ObservableProperty]
        private Color _fHRDecelerationDueColor = Colors.Green;

        [ObservableProperty]
        private Color _contractionsDueColor = Colors.Green;

        [ObservableProperty]
        private Color _vitalSignsDueColor = Colors.Green;

        [ObservableProperty]
        private Color _assessmentDueColor = Colors.Green;

        // Last measurement times
        [ObservableProperty]
        private DateTime _lastBaselineFHRTime;

        [ObservableProperty]
        private DateTime _lastFHRDecelerationTime;

        [ObservableProperty]
        private DateTime _lastContractionsTime;

        [ObservableProperty]
        private DateTime _lastVitalSignsTime;

        [ObservableProperty]
        private DateTime _lastAssessmentTime;

        // Latest values summary
        [ObservableProperty]
        private int _latestCervicalDilation;

        [ObservableProperty]
        private int _latestFHR;

        [ObservableProperty]
        private int _latestContractions;

        [ObservableProperty]
        private string _latestBP = string.Empty;

        [ObservableProperty]
        private int _currentPainLevel;

        [ObservableProperty]
        private bool _isBusy;

        public EnhancedPartographPageModel(
            PatientRepository patientRepository,
            PartographEntryRepository partographRepository,
            VitalSignRepository vitalSignRepository,
            AssessmentPlanRepository assessmentPlanRepository,
            ModalErrorHandler errorHandler)
        {
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _vitalSignRepository = vitalSignRepository;
            _assessmentPlanRepository = assessmentPlanRepository;
            _errorHandler = errorHandler;

            // Start timer to update time and check due measurements
            StartTimeUpdater();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("id"))
            {
                int id = Convert.ToInt32(query["id"]);
                LoadData(id).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        private void StartTimeUpdater()
        {
            Device.StartTimer(TimeSpan.FromMinutes(1), () =>
            {
                CurrentTime = DateTime.Now;
                CurrentDate = DateTime.Now;
                CheckDueMeasurements();
                CheckForAlerts();
                return true;
            });
        }

        private async Task LoadData(int patientId)
        {
            try
            {
                IsBusy = true;

                _patient = await _patientRepository.GetAsync(patientId);
                if (_patient == null)
                {
                    _errorHandler.HandleError(new Exception($"Patient with id {patientId} not found."));
                    return;
                }

                PatientName = _patient.Name;
                PatientInfo = _patient.DisplayInfo;

                // Calculate labor duration
                if (_patient.LaborStartTime.HasValue)
                {
                    var duration = DateTime.Now - _patient.LaborStartTime.Value;
                    LaborDuration = $"{(int)duration.TotalHours}h {duration.Minutes}m";
                }

                await LoadLatestMeasurements();
                CheckDueMeasurements();
                CheckForAlerts();
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

        private async Task LoadLatestMeasurements()
        {
            if (_patient == null) return;

            try
            {
                // Load latest partograph entries
                var partographEntries = await _partographRepository.ListByPatientAsync(_patient.ID);
                var latestEntry = partographEntries.OrderByDescending(e => e.RecordedTime).FirstOrDefault();

                if (latestEntry != null)
                {
                    LatestCervicalDilation = latestEntry.CervicalDilation;
                    LatestFHR = latestEntry.FetalHeartRate;
                    LatestContractions = latestEntry.ContractionsPerTenMinutes;

                    // Set last measurement times (simulated based on partograph entries)
                    LastBaselineFHRTime = latestEntry.RecordedTime;
                    LastFHRDecelerationTime = latestEntry.RecordedTime;
                    LastContractionsTime = latestEntry.RecordedTime;
                }

                // Load latest vital signs
                var vitalSigns = await _vitalSignRepository.ListByPatientAsync(_patient.ID);
                var latestVitals = vitalSigns.OrderByDescending(v => v.RecordedTime).FirstOrDefault();

                if (latestVitals != null)
                {
                    LatestBP = latestVitals.BPDisplay;
                    LastVitalSignsTime = latestVitals.RecordedTime;
                }

                // Load latest assessment
                var assessments = await _assessmentPlanRepository.ListByPatientAsync(_patient.ID);
                var latestAssessment = assessments.OrderByDescending(a => a.RecordedTime).FirstOrDefault();

                if (latestAssessment != null)
                {
                    LastAssessmentTime = latestAssessment.RecordedTime;
                }

                // Set default times if no measurements exist
                if (LastBaselineFHRTime == default) LastBaselineFHRTime = _patient.LaborStartTime ?? DateTime.Now.AddHours(-2);
                if (LastFHRDecelerationTime == default) LastFHRDecelerationTime = _patient.LaborStartTime ?? DateTime.Now.AddHours(-2);
                if (LastContractionsTime == default) LastContractionsTime = _patient.LaborStartTime ?? DateTime.Now.AddHours(-2);
                if (LastVitalSignsTime == default) LastVitalSignsTime = _patient.LaborStartTime ?? DateTime.Now.AddHours(-2);
                if (LastAssessmentTime == default) LastAssessmentTime = _patient.LaborStartTime ?? DateTime.Now.AddHours(-4);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        private void CheckDueMeasurements()
        {
            var dueMeasurements = new List<string>();
            var now = DateTime.Now;

            // Check 30-minute measurements
            if (now - LastBaselineFHRTime >= TimeSpan.FromMinutes(30))
            {
                dueMeasurements.Add("Baseline FHR");
                BaselineFHRDueColor = Colors.Red;
            }
            else if (now - LastBaselineFHRTime >= TimeSpan.FromMinutes(25))
            {
                BaselineFHRDueColor = Colors.Orange;
            }
            else
            {
                BaselineFHRDueColor = Colors.Green;
            }

            if (now - LastFHRDecelerationTime >= TimeSpan.FromMinutes(30))
            {
                dueMeasurements.Add("FHR Deceleration");
                FHRDecelerationDueColor = Colors.Red;
            }
            else if (now - LastFHRDecelerationTime >= TimeSpan.FromMinutes(25))
            {
                FHRDecelerationDueColor = Colors.Orange;
            }
            else
            {
                FHRDecelerationDueColor = Colors.Green;
            }

            if (now - LastContractionsTime >= TimeSpan.FromMinutes(30))
            {
                dueMeasurements.Add("Contractions");
                ContractionsDueColor = Colors.Red;
            }
            else if (now - LastContractionsTime >= TimeSpan.FromMinutes(25))
            {
                ContractionsDueColor = Colors.Orange;
            }
            else
            {
                ContractionsDueColor = Colors.Green;
            }

            // Check hourly measurements
            if (now - LastVitalSignsTime >= TimeSpan.FromHours(1))
            {
                dueMeasurements.Add("Vital Signs");
                VitalSignsDueColor = Colors.Red;
            }
            else if (now - LastVitalSignsTime >= TimeSpan.FromMinutes(50))
            {
                VitalSignsDueColor = Colors.Orange;
            }
            else
            {
                VitalSignsDueColor = Colors.Green;
            }

            // Check 4-hour assessments
            if (now - LastAssessmentTime >= TimeSpan.FromHours(4))
            {
                dueMeasurements.Add("Clinical Assessment");
                AssessmentDueColor = Colors.Red;
            }
            else if (now - LastAssessmentTime >= TimeSpan.FromHours(3.5))
            {
                AssessmentDueColor = Colors.Orange;
            }
            else
            {
                AssessmentDueColor = Colors.Green;
            }

            HasDueMeasurements = dueMeasurements.Any();
            DueMeasurementsText = HasDueMeasurements
                ? $"Due: {string.Join(", ", dueMeasurements)}"
                : "All measurements up to date";
        }

        private void CheckForAlerts()
        {
            var alerts = new List<string>();

            // Check for abnormal FHR
            if (LatestFHR > 0 && (LatestFHR < 110 || LatestFHR > 160))
            {
                alerts.Add($"Abnormal FHR: {LatestFHR} bpm");
            }

            // Check for prolonged labor
            if (_patient?.LaborStartTime != null)
            {
                var laborHours = (DateTime.Now - _patient.LaborStartTime.Value).TotalHours;
                if (laborHours > 12 && LatestCervicalDilation < 10)
                {
                    alerts.Add($"Prolonged labor: {laborHours:F1} hours");
                }
            }

            // Check for slow cervical progress
            if (_patient?.LaborStartTime != null && LatestCervicalDilation > 0)
            {
                var laborHours = (DateTime.Now - _patient.LaborStartTime.Value).TotalHours;
                var expectedDilation = Math.Min(4 + laborHours, 10);
                if (LatestCervicalDilation < expectedDilation - 2)
                {
                    alerts.Add("Slow cervical dilation progress");
                }
            }

            Alerts.Clear();
            foreach (var alert in alerts)
            {
                Alerts.Add(alert);
            }
            HasAlerts = alerts.Any();
        }

        // Navigation Commands
        [RelayCommand]
        private Task OpenBaselineFHR()
            => Shell.Current.GoToAsync($"baselinefhr?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenFHRDeceleration()
            => Shell.Current.GoToAsync($"fhrdeceleration?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenContractions()
            => Shell.Current.GoToAsync($"contractions?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenVitalSigns()
            => Shell.Current.GoToAsync($"vitalsigns?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenPainRelief()
            => Shell.Current.GoToAsync($"painrelief?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenAssessment()
            => Shell.Current.GoToAsync($"assessmentplan?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenCompanion()
            => Shell.Current.GoToAsync($"companion?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenOralFluid()
            => Shell.Current.GoToAsync($"oralfluid?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenPosture()
            => Shell.Current.GoToAsync($"posture?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenAmnioticFluid()
            => Shell.Current.GoToAsync($"amnioticfluid?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenFetalPosition()
            => Shell.Current.GoToAsync($"fetalposition?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenCaput()
            => Shell.Current.GoToAsync($"caput?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenMoulding()
            => Shell.Current.GoToAsync($"moulding?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenCervix()
            => Shell.Current.GoToAsync($"cervix?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenHeadDescent()
            => Shell.Current.GoToAsync($"headdescent?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenOxytocin()
            => Shell.Current.GoToAsync($"oxytocin?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenMedicine()
            => Shell.Current.GoToAsync($"medicine?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenIVFluid()
            => Shell.Current.GoToAsync($"ivfluid?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenUrine()
            => Shell.Current.GoToAsync($"urine?patientId={_patient?.ID}");

        [RelayCommand]
        private Task OpenTemperature()
            => Shell.Current.GoToAsync($"temperature?patientId={_patient?.ID}");

        [RelayCommand]
        private Task ViewChart()
            => Shell.Current.GoToAsync($"partograph?id={_patient?.ID}");

        [RelayCommand]
        private async Task Refresh()
        {
            if (_patient != null)
                await LoadData(_patient.ID);
        }
    }
}
