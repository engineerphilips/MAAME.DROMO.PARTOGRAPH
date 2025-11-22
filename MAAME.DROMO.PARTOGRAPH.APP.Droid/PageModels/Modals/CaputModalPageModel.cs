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
    public partial class CaputModalPageModel : ObservableObject, IQueryAttributable
    {
        private Patient? _patient;
        private readonly CaputRepository _caputRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateTime _recordingTime = DateTime.Now;

        [ObservableProperty]
        private int? _degreeIndex = null;

        [ObservableProperty]
        private string _caputDisplay = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public CaputModalPageModel(CaputRepository caputRepository, ModalErrorHandler errorHandler)
        {
            _caputRepository = caputRepository;
            _errorHandler = errorHandler;

            // Set default recorded by from preferences
            RecordedBy = Preferences.Get("StaffName", "Staff");
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                Guid? patientId = Guid.Parse(query["patientId"].ToString());
                LoadPatient(patientId).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        private async Task LoadPatient(Guid? patientId)
        {
            try
            {
                // This would typically load from PatientRepository
                // For now, we'll use the patient ID directly
                PatientName = $"Patient ID: {patientId}";

                // Load last pain relief entry to prefill some values
                var lastEntry = await _caputRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    CaputDisplay = lastEntry.CaputDisplay;
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
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

                var entry = new Caput
                {
                    PartographID = _patient.ID,
                    Time = RecordingTime,
                    Degree = DegreeIndex == 0 ? "0" : DegreeIndex == 1 ? "+" : DegreeIndex == 2 ? "++" : DegreeIndex == 3 ? "+++" : null,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID
                };

                await _caputRepository.SaveItemAsync(entry);

                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Caput assessment saved successfully");
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
