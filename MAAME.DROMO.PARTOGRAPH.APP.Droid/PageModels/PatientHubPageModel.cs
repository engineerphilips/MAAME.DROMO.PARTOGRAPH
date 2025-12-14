using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class PatientHubPageModel : ObservableObject, IQueryAttributable
    {
        private readonly PatientRepository _patientRepository;
        private readonly PartographRepository _partographRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private Patient? _patient;

        [ObservableProperty]
        private List<Partograph> _partographs = [];

        [ObservableProperty]
        private string _patientId = string.Empty;

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        public PatientHubPageModel(
            PatientRepository patientRepository,
            PartographRepository partographRepository,
            ModalErrorHandler errorHandler)
        {
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _errorHandler = errorHandler;
        }

        [RelayCommand]
        private async Task Appearing()
        {
            try
            {
                await LoadData();
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
        }

        partial void OnPatientIdChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Task.Run(async () => await LoadData());
            }
        }

        private async Task LoadData()
        {
            try
            {
                IsBusy = true;

                if (!Guid.TryParse(PatientId, out var patientGuid))
                    return;

                // Load patient details
                Patient = await _patientRepository.GetAsync(patientGuid);

                if (Patient == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Patient not found", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                // Load all partographs for this patient (including completed ones)
                var allPartographs = new List<Partograph>();

                // Load partographs by status
                foreach (LaborStatus status in Enum.GetValues(typeof(LaborStatus)))
                {
                    var statusPartographs = await _partographRepository.ListAsync(status);
                    if (statusPartographs != null)
                    {
                        var patientPartographs = statusPartographs.Where(p => p.PatientID == patientGuid).ToList();
                        allPartographs.AddRange(patientPartographs);
                    }
                }

                // Sort by time (most recent first)
                Partographs = allPartographs.OrderByDescending(p => p.Time).ToList();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            try
            {
                IsRefreshing = true;
                await LoadData();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private Task NavigateToPartograph(Partograph partograph)
        {
            // Navigate based on labor stage
            var route = partograph.Status switch
            {
                LaborStatus.SecondStage => "secondpartograph",
                LaborStatus.ThirdStage => "thirdpartograph",
                LaborStatus.FourthStage => "fourthpartograph",
                _ => "partograph" // FirstStage, Pending, or any other status defaults to first stage partograph
            };

            return Shell.Current.GoToAsync($"{route}?patientId={partograph.ID}");
        }

        [RelayCommand]
        private Task EditPatient()
            => Shell.Current.GoToAsync($"patient?id={PatientId}");

        [RelayCommand]
        private async Task CallPatient()
        {
            if (Patient == null || string.IsNullOrEmpty(Patient.PhoneNumber))
            {
                await Shell.Current.DisplayAlert("No Phone Number", "This patient does not have a phone number on record.", "OK");
                return;
            }

            try
            {
                if (PhoneDialer.Default.IsSupported)
                    PhoneDialer.Default.Open(Patient.PhoneNumber);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        [RelayCommand]
        private async Task CallEmergencyContact()
        {
            if (Patient == null || string.IsNullOrEmpty(Patient.EmergencyContactPhone))
            {
                await Shell.Current.DisplayAlert("No Emergency Contact", "This patient does not have an emergency contact phone number on record.", "OK");
                return;
            }

            try
            {
                if (PhoneDialer.Default.IsSupported)
                    PhoneDialer.Default.Open(Patient.EmergencyContactPhone);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                Guid? id = Guid.Parse(Convert.ToString(query["patientId"]));
                if (id != null)
                {
                    PatientId = id.ToString();
                    LoadData().FireAndForgetSafeAsync(_errorHandler);
                }
            }
        }
    }
}
