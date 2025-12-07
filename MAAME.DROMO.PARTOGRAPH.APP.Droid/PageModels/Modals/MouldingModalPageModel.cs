using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public partial class MouldingModalPageModel : ObservableObject
    {
        public Partograph? _patient;
        private readonly MouldingRepository _mouldingRepository;
        private readonly ModalErrorHandler _errorHandler;

        public MouldingModalPageModel(MouldingRepository repository, ModalErrorHandler errorHandler)
        {
            _mouldingRepository = repository;
            _errorHandler = errorHandler;
        }

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateTime _recordingTime = DateTime.Now;

        [ObservableProperty]
        private int _degreeIndex = -1;

        [ObservableProperty]
        private string _degreeDisplay = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public Action? ClosePopup { get; set; }

        internal async Task LoadPatient(Guid? patientId)
        {
            try
            {
                // This would typically load from PatientRepository
                // For now, we'll use the patient ID directly
                PatientName = $"Patient ID: {patientId}";

                // Load last moulding entry to prefill some values
                var lastEntry = await _mouldingRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    DegreeIndex = lastEntry.Degree;
                    DegreeDisplay = lastEntry.DegreeDisplay;
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

                var entry = new Moulding
                {
                    PartographID = _patient.ID,
                    Time = RecordingTime,
                    Degree = DegreeIndex, 
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID
                };

                await _mouldingRepository.SaveItemAsync(entry);

                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Moulding assessment saved successfully");
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
