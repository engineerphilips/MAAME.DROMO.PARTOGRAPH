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
    public partial class AmnioticFluidModalPageModel : ObservableObject
    {
        public Partograph? _patient;
        private readonly AmnioticFluidRepository _amnioticFluidRepository;
        private readonly ModalErrorHandler _errorHandler;

        public AmnioticFluidModalPageModel(AmnioticFluidRepository repository, ModalErrorHandler errorHandler)
        {
            _amnioticFluidRepository = repository;
            _errorHandler = errorHandler;
        }

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateTime _recordingTime = DateTime.Now;

        [ObservableProperty]
        private int? _amnioticFluidIndex = null;

        [ObservableProperty]
        private string _amnioticFluidDisplay = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        internal async Task LoadPatient(Guid? patientId)
        {
            try
            {
                // This would typically load from PatientRepository
                // For now, we'll use the patient ID directly
                PatientName = $"Patient ID: {patientId}";

                // Load last pain relief entry to prefill some values
                var lastEntry = await _amnioticFluidRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    AmnioticFluidDisplay = lastEntry.AmnioticFluidDisplay;
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

                var entry = new AmnioticFluid
                {
                    PartographID = _patient.ID,
                    Time = RecordingTime,
                    Color = AmnioticFluidIndex == 0 ? "N" : AmnioticFluidIndex == 1 ? "Y" : AmnioticFluidIndex == 2 ? "D" : string.Empty,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID
                };

                await _amnioticFluidRepository.SaveItemAsync(entry);

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
