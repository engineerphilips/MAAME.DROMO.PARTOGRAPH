using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    public class AuthenticationService
    {
        private readonly StaffRepository _staffRepository;

        public AuthenticationService(StaffRepository staffRepository)
        {
            _staffRepository = staffRepository;
        }

        public async Task<bool> LoginAsync(string emailOrStaffId, string password)
        {
            try
            {
                // For demo purposes, accept any login
                // In production, validate against staff database
                if (string.IsNullOrEmpty(emailOrStaffId) || string.IsNullOrEmpty(password))
                    return false;

                // Simulate authentication
                await Task.Delay(500);

                // Store authentication state
                Preferences.Set("IsAuthenticated", true);
                Preferences.Set("StaffName", "Dr. Sarah Johnson");
                Preferences.Set("StaffRole", "Labor Ward Supervisor");
                Preferences.Set("LastLogin", DateTime.Now.ToString("O"));

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            await Task.CompletedTask;

            // Clear authentication state
            Preferences.Remove("IsAuthenticated");
            Preferences.Remove("StaffName");
            Preferences.Remove("StaffRole");
            Preferences.Remove("LastLogin");

            // Navigate to login
            Application.Current.MainPage = new LoginPage();
        }

        public bool IsAuthenticated()
        {
            return Preferences.Get("IsAuthenticated", false);
        }

        public string GetCurrentStaffName()
        {
            return Preferences.Get("StaffName", "Staff");
        }
    }
}
