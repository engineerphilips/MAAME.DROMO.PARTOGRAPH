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
    public partial class AssessmentPlanModalPageModel : ObservableObject, IQueryAttributable
    {
        private Partograph? _patient;
        private readonly AssessmentPlanRepository _assessmentPlanRepository;
        private readonly PartographRepository _partographRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private string _laborDuration = string.Empty;

        [ObservableProperty]
        private DateTime _currentTime = DateTime.Now;

        [ObservableProperty]
        private DateTime _currentDate = DateTime.Now;

        // Quick Assessment Status
        [ObservableProperty]
        private string _laborProgressStatus = "Normal";

        [ObservableProperty]
        private string _fetalWellbeingStatus = "Satisfactory";

        [ObservableProperty]
        private string _maternalConditionStatus = "Stable";

        [ObservableProperty]
        private Color _laborProgressColor = Colors.Green;

        [ObservableProperty]
        private Color _fetalWellbeingColor = Colors.Green;

        [ObservableProperty]
        private Color _maternalConditionColor = Colors.Green;

        // Detailed Assessment
        [ObservableProperty]
        private bool _laborProgressNormal = true;

        [ObservableProperty]
        private bool _laborProgressDelayed;

        [ObservableProperty]
        private bool _laborProgressRapid;

        [ObservableProperty]
        private bool _fetalWellbeingSatisfactory = true;

        [ObservableProperty]
        private bool _fetalWellbeingConcerning;

        [ObservableProperty]
        private bool _fetalWellbeingCompromised;

        [ObservableProperty]
        private bool _maternalConditionStable = true;

        [ObservableProperty]
        private bool _maternalConditionConcerned;

        [ObservableProperty]
        private bool _maternalConditionCritical;

        [ObservableProperty]
        private string _riskFactors = string.Empty;

        [ObservableProperty]
        private string _complications = string.Empty;

        [ObservableProperty]
        private string _plan = string.Empty;

        [ObservableProperty]
        private string _expectedDelivery = "Normal vaginal delivery";

        [ObservableProperty]
        private bool _requiresIntervention;

        [ObservableProperty]
        private string _interventionRequired = string.Empty;

        [ObservableProperty]
        private DateTime _nextAssessmentDate = DateTime.Now.AddHours(4);

        [ObservableProperty]
        private TimeSpan _nextAssessmentTime = DateTime.Now.AddHours(4).TimeOfDay;

        [ObservableProperty]
        private string _assessedBy = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public AssessmentPlanModalPageModel(AssessmentPlanRepository assessmentPlanRepository,
            PartographRepository partographRepository, ModalErrorHandler errorHandler)
        {
            _assessmentPlanRepository = assessmentPlanRepository;
            _partographRepository = partographRepository;
            _errorHandler = errorHandler;

            // Set default assessed by from preferences
            AssessedBy = Preferences.Get("StaffName", "Staff");

            // Start timer to update current time
            StartTimeUpdater();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                Guid? patientId = Guid.Parse(Convert.ToString(query["patientId"]));
                LoadPatient(patientId).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        private void StartTimeUpdater()
        {
            Device.StartTimer(TimeSpan.FromMinutes(1), () =>
            {
                CurrentTime = DateTime.Now;
                CurrentDate = DateTime.Now;
                return true;
            });
        }

        private async Task LoadPatient(Guid? patientId)
        {
            try
            {
                _patient = await _partographRepository.GetAsync(patientId);
                if (_patient != null)
                {
                    PatientName = _patient.Patient?.Name;

                    // Calculate labor duration
                    if (_patient.LaborStartTime.HasValue)
                    {
                        var duration = DateTime.Now - _patient.LaborStartTime.Value;
                        LaborDuration = $"{(int)duration.TotalHours}h {duration.Minutes}m";
                    }

                    // Load existing risk factors and complications
                    RiskFactors = _patient.RiskFactors;
                    Complications = _patient.Complications;

                    // Auto-assess based on current data
                    //await PerformAutoAssessment();
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
        }

        //private async Task PerformAutoAssessment()
        //{
        //    if (_patient == null) return;

        //    try
        //    {
        //        // Assess labor progress based on cervical dilation and time
        //        if (_patient.LaborStartTime.HasValue)
        //        {
        //            var laborHours = (DateTime.Now - _patient.LaborStartTime.Value).TotalHours;
        //            var latestEntry = _patient.PartographEntries.OrderByDescending(e => e.Time).FirstOrDefault();

        //            if (latestEntry != null)
        //            {
        //                var expectedDilation = Math.Min(4 + laborHours, 10); // Expecting 1cm/hour after 4cm

        //                if (latestEntry.CervicalDilation >= expectedDilation - 1)
        //                {
        //                    LaborProgressNormal = true;
        //                    LaborProgressStatus = "Normal";
        //                    LaborProgressColor = Colors.Green;
        //                }
        //                else if (latestEntry.CervicalDilation < expectedDilation - 2)
        //                {
        //                    LaborProgressDelayed = true;
        //                    LaborProgressNormal = false;
        //                    LaborProgressStatus = "Delayed";
        //                    LaborProgressColor = Colors.Orange;
        //                }
        //            }
        //        }

        //        // Assess fetal wellbeing based on latest FHR
        //        var latestFHR = _patient.PartographEntries
        //            .Where(e => e.FetalHeartRate > 0)
        //            .OrderByDescending(e => e.RecordedTime)
        //            .FirstOrDefault();

        //        if (latestFHR != null)
        //        {
        //            if (latestFHR.FetalHeartRate >= 110 && latestFHR.FetalHeartRate <= 160)
        //            {
        //                FetalWellbeingSatisfactory = true;
        //                FetalWellbeingStatus = "Satisfactory";
        //                FetalWellbeingColor = Colors.Green;
        //            }
        //            else if (latestFHR.FetalHeartRate < 110 || latestFHR.FetalHeartRate > 180)
        //            {
        //                FetalWellbeingCompromised = true;
        //                FetalWellbeingSatisfactory = false;
        //                FetalWellbeingStatus = "Compromised";
        //                FetalWellbeingColor = Colors.Red;
        //                RequiresIntervention = true;
        //                InterventionRequired = "Immediate review for abnormal FHR";
        //            }
        //            else
        //            {
        //                FetalWellbeingConcerning = true;
        //                FetalWellbeingSatisfactory = false;
        //                FetalWellbeingStatus = "Concerning";
        //                FetalWellbeingColor = Colors.Orange;
        //            }
        //        }

        //        // Assess maternal condition based on vital signs
        //        var latestVitals = _patient.VitalSigns.OrderByDescending(v => v.RecordedTime).FirstOrDefault();
        //        if (latestVitals != null)
        //        {
        //            if (latestVitals.SystolicBP > 160 || latestVitals.DiastolicBP > 100 ||
        //                latestVitals.Temperature > 38.0m || latestVitals.PulseRate > 120)
        //            {
        //                MaternalConditionCritical = true;
        //                MaternalConditionStable = false;
        //                MaternalConditionStatus = "Critical";
        //                MaternalConditionColor = Colors.Red;
        //                RequiresIntervention = true;
        //                if (string.IsNullOrEmpty(InterventionRequired))
        //                    InterventionRequired = "Immediate review for abnormal maternal vital signs";
        //            }
        //            else if (latestVitals.SystolicBP > 140 || latestVitals.DiastolicBP > 90 ||
        //                     latestVitals.Temperature > 37.5m)
        //            {
        //                MaternalConditionConcerned = true;
        //                MaternalConditionStable = false;
        //                MaternalConditionStatus = "Concerned";
        //                MaternalConditionColor = Colors.Orange;
        //            }
        //        }

        //        // Set default plan based on assessment
        //        if (string.IsNullOrEmpty(Plan))
        //        {
        //            Plan = GenerateDefaultPlan();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _errorHandler.HandleError(ex);
        //    }
        //}

        private string GenerateDefaultPlan()
        {
            var planItems = new List<string>();

            if (LaborProgressNormal)
                planItems.Add("Continue current management");
            else if (LaborProgressDelayed)
                planItems.Add("Consider augmentation of labor");

            if (FetalWellbeingSatisfactory)
                planItems.Add("Continue fetal monitoring every 30 minutes");
            else
                planItems.Add("Increase fetal monitoring frequency");

            if (MaternalConditionStable)
                planItems.Add("Continue routine maternal observations");
            else
                planItems.Add("Increase maternal monitoring frequency");

            planItems.Add("Reassess in 4 hours or sooner if concerns arise");

            return string.Join("\n• ", planItems.Prepend("• "));
        }

        // Radio button change handlers
        partial void OnLaborProgressNormalChanged(bool value)
        {
            if (value)
            {
                LaborProgressDelayed = false;
                LaborProgressRapid = false;
                LaborProgressStatus = "Normal";
                LaborProgressColor = Colors.Green;
            }
        }

        partial void OnLaborProgressDelayedChanged(bool value)
        {
            if (value)
            {
                LaborProgressNormal = false;
                LaborProgressRapid = false;
                LaborProgressStatus = "Delayed";
                LaborProgressColor = Colors.Orange;
            }
        }

        partial void OnLaborProgressRapidChanged(bool value)
        {
            if (value)
            {
                LaborProgressNormal = false;
                LaborProgressDelayed = false;
                LaborProgressStatus = "Rapid";
                LaborProgressColor = Colors.Red;
            }
        }

        partial void OnFetalWellbeingSatisfactoryChanged(bool value)
        {
            if (value)
            {
                FetalWellbeingConcerning = false;
                FetalWellbeingCompromised = false;
                FetalWellbeingStatus = "Satisfactory";
                FetalWellbeingColor = Colors.Green;
            }
        }

        partial void OnFetalWellbeingConcerningChanged(bool value)
        {
            if (value)
            {
                FetalWellbeingSatisfactory = false;
                FetalWellbeingCompromised = false;
                FetalWellbeingStatus = "Concerning";
                FetalWellbeingColor = Colors.Orange;
            }
        }

        partial void OnFetalWellbeingCompromisedChanged(bool value)
        {
            if (value)
            {
                FetalWellbeingSatisfactory = false;
                FetalWellbeingConcerning = false;
                FetalWellbeingStatus = "Compromised";
                FetalWellbeingColor = Colors.Red;
                RequiresIntervention = true;
            }
        }

        partial void OnMaternalConditionStableChanged(bool value)
        {
            if (value)
            {
                MaternalConditionConcerned = false;
                MaternalConditionCritical = false;
                MaternalConditionStatus = "Stable";
                MaternalConditionColor = Colors.Green;
            }
        }

        partial void OnMaternalConditionConcernedChanged(bool value)
        {
            if (value)
            {
                MaternalConditionStable = false;
                MaternalConditionCritical = false;
                MaternalConditionStatus = "Concerned";
                MaternalConditionColor = Colors.Orange;
            }
        }

        partial void OnMaternalConditionCriticalChanged(bool value)
        {
            if (value)
            {
                MaternalConditionStable = false;
                MaternalConditionConcerned = false;
                MaternalConditionStatus = "Critical";
                MaternalConditionColor = Colors.Red;
                RequiresIntervention = true;
            }
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

                var entry = new AssessmentPlanEntry
                {
                    PartographID = _patient.ID,
                    Time = DateTime.Now,
                    LaborProgress = LaborProgressStatus,
                    FetalWellbeing = FetalWellbeingStatus,
                    MaternalCondition = MaternalConditionStatus,
                    RiskFactors = RiskFactors,
                    Complications = Complications,
                    Plan = Plan,
                    ExpectedDelivery = ExpectedDelivery,
                    RequiresIntervention = RequiresIntervention,
                    InterventionRequired = InterventionRequired,
                    NextAssessment = NextAssessmentDate.Date.Add(NextAssessmentTime),
                    AssessedBy = AssessedBy,
                    HandlerName = AssessedBy
                };

                await _assessmentPlanRepository.SaveItemAsync(entry);

                // Update patient risk factors and complications
                if (_patient != null)
                {
                    _patient.RiskFactors = RiskFactors;
                    _patient.Complications = Complications;
                    await _partographRepository.SaveItemAsync(_patient);
                }

                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Assessment and plan saved successfully");
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
