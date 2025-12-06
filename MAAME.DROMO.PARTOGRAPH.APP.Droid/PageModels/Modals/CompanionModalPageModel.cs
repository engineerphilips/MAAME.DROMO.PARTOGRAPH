using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public partial class CompanionModalPageModel : ObservableObject
    {
        public Partograph? _patient;
        private readonly CompanionRepository _companionRepository;
        private readonly ModalErrorHandler _errorHandler;

        public Action? ClosePopup { get; set; }

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        [ObservableProperty]
        private int _companionIndex = -1;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        private string _companionDisplay = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public CompanionModalPageModel(CompanionRepository companionRepository, ModalErrorHandler errorHandler)
        {
            _companionRepository = companionRepository;
            _errorHandler = errorHandler;

            // Set default recorded by from preferences
            RecordedBy = Preferences.Get("StaffName", "Staff");
        }

        //public void ApplyQueryAttributes(IDictionary<string, object> query)
        //{
        //    if (query.ContainsKey("patientId"))
        //    {
        //        Guid? patientId = Guid.Parse(Convert.ToString(query["patientId"]));
        //        LoadPatient(patientId).FireAndForgetSafeAsync(_errorHandler);
        //    }
        //}

        public async Task LoadPatient(Guid? patientId)
        {

            try
            {
                // This would typically load from PatientRepository
                // For now, we'll use the patient ID directly
                PatientName = $"Patient ID: {patientId}";

                // Load last pain relief entry to prefill some values
                var lastEntry = await _companionRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    CompanionDisplay = lastEntry?.CompanionDisplay;
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
        }

        //partial void OnPainReliefMethodChanged(string value)
        //{
        //    // Show medication details for pharmacological interventions
        //    ShowMedicationDetails = value is "Paracetamol" or "Codeine" or "Pethidine injection"
        //                                   or "Gas and air (Entonox)" or "Epidural" or "Spinal block"
        //                                   or "Combined spinal-epidural";
        //}

        //partial void OnEffectivenessPoorChanged(bool value)
        //{
        //    if (value)
        //    {
        //        EffectivenessFair = false;
        //        EffectivenessGood = false;
        //        EffectivenessExcellent = false;
        //    }
        //}

        //partial void OnEffectivenessFairChanged(bool value)
        //{
        //    if (value)
        //    {
        //        EffectivenessPoor = false;
        //        EffectivenessGood = false;
        //        EffectivenessExcellent = false;
        //    }
        //}

        //partial void OnEffectivenessGoodChanged(bool value)
        //{
        //    if (value)
        //    {
        //        EffectivenessPoor = false;
        //        EffectivenessFair = false;
        //        EffectivenessExcellent = false;
        //    }
        //}

        //partial void OnEffectivenessExcellentChanged(bool value)
        //{
        //    if (value)
        //    {
        //        EffectivenessPoor = false;
        //        EffectivenessFair = false;
        //        EffectivenessGood = false;
        //    }
        //}

        [RelayCommand]
        private async Task Save()
        {
            if (_patient == null)
            {
                _errorHandler.HandleError(new Exception("Patient information not loaded."));
                return;
            }

            if (CompanionIndex < 0)
            {
                _errorHandler.HandleError(new Exception("Companion status is not selected."));
                return;
            }

            try
            {
                IsBusy = true;

                var entry = new CompanionEntry
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    Companion = CompanionIndex == 0 ? "N" : CompanionIndex == 1 ? "Y" : CompanionIndex == 2 ? "D" : null,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID
                };

                if (await _companionRepository.SaveItemAsync(entry) != null)
                {
                    await AppShell.DisplayToastAsync("Companion assessment saved successfully");

                    // Reset fields to default
                    ResetFields();

                    // Close the popup
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("Companion assessment failed to save");
                }
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
        private void Cancel()
        {
            ResetFields();
            ClosePopup?.Invoke();
        }

        private void ResetFields()
        {
            RecordingDate = DateOnly.FromDateTime(DateTime.Now);
            RecordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            CompanionIndex = -1;
            Notes = string.Empty;
        }

        //private string GetSelectedEffectiveness()
        //{
        //    if (EffectivenessPoor) return "Poor";
        //    if (EffectivenessFair) return "Fair";
        //    if (EffectivenessGood) return "Good";
        //    if (EffectivenessExcellent) return "Excellent";
        //    return "";
        //}
    }
}
