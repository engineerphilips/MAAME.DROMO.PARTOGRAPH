﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class CompletedPatientsPageModel : ObservableObject
    {
        private readonly PatientRepository _patientRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private List<Patient> _patients = [];

        [ObservableProperty]
        private List<Patient> _todaysDeliveries = [];

        [ObservableProperty]
        private List<Patient> _recentDeliveries = [];

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        private List<Patient> _allPatients = [];

        public CompletedPatientsPageModel(PatientRepository patientRepository, ModalErrorHandler errorHandler)
        {
            _patientRepository = patientRepository;
            _errorHandler = errorHandler;
        }

        [RelayCommand]
        private async Task Appearing()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                IsBusy = true;
                _allPatients = await _patientRepository.ListAsync(LaborStatus.Completed);

                // Separate today's deliveries
                TodaysDeliveries = _allPatients
                    .Where(p => p.DeliveryTime?.Date == DateTime.Today)
                    .OrderByDescending(p => p.DeliveryTime)
                    .ToList();

                // Recent deliveries (last 7 days)
                RecentDeliveries = _allPatients
                    .Where(p => p.DeliveryTime?.Date > DateTime.Today.AddDays(-7) &&
                               p.DeliveryTime?.Date < DateTime.Today)
                    .OrderByDescending(p => p.DeliveryTime)
                    .ToList();

                FilterPatients();
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

        private void FilterPatients()
        {
            var filtered = _allPatients.AsEnumerable();

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(p =>
                    p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.HospitalNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by selected date
            if (SelectedDate != DateTime.MinValue)
            {
                filtered = filtered.Where(p => p.DeliveryTime?.Date == SelectedDate.Date);
            }

            Patients = filtered.OrderByDescending(p => p.DeliveryTime).ToList();
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterPatients();
        }

        partial void OnSelectedDateChanged(DateTime value)
        {
            FilterPatients();
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
        private Task NavigateToPatient(Patient patient)
            => Shell.Current.GoToAsync($"patient?id={patient.ID}");

        [RelayCommand]
        private Task ViewPartograph(Patient patient)
            => Shell.Current.GoToAsync($"partograph?id={patient.ID}");

        [RelayCommand]
        private Task GenerateReport(Patient patient)
        {
            // Generate delivery report
            return AppShell.DisplayToastAsync("Delivery report generation coming soon");
        }

        [RelayCommand]
        private async Task ExportData()
        {
            // Export completed cases data
            await AppShell.DisplayToastAsync("Data export feature coming soon");
        }
    }
}
