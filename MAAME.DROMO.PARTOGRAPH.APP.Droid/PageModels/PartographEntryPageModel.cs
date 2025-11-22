using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class PartographEntryPageModel : ObservableObject, IQueryAttributable
    {
        private Partograph? _patient;
        private Partograph _entry = new();
        private readonly PatientRepository _patientRepository;
        private readonly PartographRepository _partographRepository;
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
        private int _cervicalDilation;

        [ObservableProperty]
        private string _descentOfHead = "0";

        [ObservableProperty]
        private string _moulding = "None";

        [ObservableProperty]
        private string _caput = "None";

        [ObservableProperty]
        private int _fetalHeartRate = 140;

        [ObservableProperty]
        private string _liquorStatus = "Clear";

        [ObservableProperty]
        private int _contractionsPerTenMinutes = 0;

        [ObservableProperty]
        private int _contractionDuration = 0;

        [ObservableProperty]
        private string _contractionStrength = "Moderate";

        [ObservableProperty]
        private string _medicationsGiven = string.Empty;

        [ObservableProperty]
        private string _oxytocinUnits = string.Empty;

        [ObservableProperty]
        private string _iVFluids = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        bool _isBusy;

        public PartographEntryPageModel(PatientRepository patientRepository,
            PartographRepository partographRepository,
            ModalErrorHandler errorHandler)
        {
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _errorHandler = errorHandler;

            // Set default recorded by from preferences/auth
            RecordedBy = Preferences.Get("StaffName", "Staff");
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                Guid? patientId = Guid.Parse(Convert.ToString(query["patientId"]));
                LoadPatient(patientId).FireAndForgetSafeAsync(_errorHandler);
            }

            if (query.ContainsKey("entryId"))
            {
                int entryId = Convert.ToInt32(query["entryId"]);
                LoadEntry(entryId).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        private async Task LoadPatient(Guid? patientId)
        {
            try
            {
                _patient = await _partographRepository.GetAsync(patientId);
                if (_patient != null)
                {
                    PatientName = _patient.Name;
                    PatientInfo = _patient.DisplayInfo;

                    // Get last entry to suggest next values
                    //var lastEntry = _patient.PartographEntries
                    //    .OrderByDescending(e => e.Time)
                    //    .FirstOrDefault();

                    //if (lastEntry != null)
                    //{
                    //    // Suggest progressive dilation
                    //    CervicalDilation = Math.Min(lastEntry.CervicalDilation + 1, 10);
                    //}
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
        }

        private async Task LoadEntry(int entryId)
        {
            // Implementation for editing existing entry
            await Task.CompletedTask;
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

                // Combine date and time
                var recordedDateTime = RecordedDate.Date + RecordedTime;

                _entry.PatientID = _patient.ID;
                _entry.Time = recordedDateTime;
                //_entry.CervicalDilation = CervicalDilation;
                //_entry.DescentOfHead = DescentOfHead;
                //_entry.Moulding = Moulding;
                //_entry.Caput = Caput;
                //_entry.FetalHeartRate = FetalHeartRate;
                _entry.LiquorStatus = LiquorStatus;
                //_entry.ContractionsPerTenMinutes = ContractionsPerTenMinutes;
                //_entry.ContractionDuration = ContractionDuration;
                //_entry.ContractionStrength = ContractionStrength;
                //_entry.MedicationsGiven = MedicationsGiven;
                //_entry.OxytocinUnits = OxytocinUnits;
                //_entry.IVFluids = IVFluids;
                //_entry.Notes = Notes;
                //_entry.RecordedBy = RecordedBy;

                await _partographRepository.SaveItemAsync(_entry);

                // Check for alerts
                await CheckForAlerts();

                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Partograph entry saved successfully");
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

        private async Task CheckForAlerts()
        {
            // Check for abnormal FHR
            if (FetalHeartRate < 110 || FetalHeartRate > 160)
            {
                await AppShell.DisplaySnackbarAsync($"⚠️ Abnormal FHR detected: {FetalHeartRate} bpm");
            }

            // Check for meconium
            if (LiquorStatus.Contains("Meconium"))
            {
                await AppShell.DisplaySnackbarAsync("⚠️ Meconium-stained liquor detected");
            }

            // Check for prolonged labor (if dilation not progressing)
            if (_patient?.LaborStartTime != null)
            {
                var hoursInLabor = (DateTime.Now - _patient.LaborStartTime.Value).TotalHours;
                if (hoursInLabor > 12 && CervicalDilation < 10)
                {
                    await AppShell.DisplaySnackbarAsync("⚠️ Prolonged labor - consider intervention");
                }
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
