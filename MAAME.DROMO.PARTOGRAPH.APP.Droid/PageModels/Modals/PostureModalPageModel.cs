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
    public partial class PostureModalPageModel : ObservableObject, IQueryAttributable
    {
        private Patient? _patient;
        private readonly PostureRepository _postureRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateTime _recordingTime = DateTime.Now;

        [ObservableProperty]
        private int? _postureIndex = null;

        [ObservableProperty]
        private string _postureDisplay = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public PostureModalPageModel(PostureRepository postureRepository, ModalErrorHandler errorHandler)
        {
            _postureRepository = postureRepository;
            _errorHandler = errorHandler;

            // Set default recorded by from preferences
            RecordedBy = Preferences.Get("StaffName", "Staff");
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                Guid? patientId = Guid.Parse(Convert.ToString(query["patientId"]));
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
                var lastEntry = await _postureRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    PostureDisplay = lastEntry.PostureDisplay;
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

                var entry = new PostureEntry
                {
                    PartographID = _patient.ID,
                    Time = RecordingTime,
                    Posture = PostureIndex == 0 ? 'N' : PostureIndex == 1 ? 'Y' : PostureIndex == 2 ? 'D' : null,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.FacilityName ?? string.Empty,
                    Handler = Constants.Staff?.ID
                };

                await _postureRepository.SaveItemAsync(entry);

                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Pain relief assessment saved successfully");
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
