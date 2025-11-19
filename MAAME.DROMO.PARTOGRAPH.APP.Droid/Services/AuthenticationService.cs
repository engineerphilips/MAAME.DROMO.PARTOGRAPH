using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    public class AuthenticationService : IAuthenticationService
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
                //await Task.Delay(500);

                //await _staffRepository.AuthenticateAsync();
                var staff = await _staffRepository.AuthenticateAsync(emailOrStaffId, password);
                if (staff != null)
                {
                    // Store authentication state
                    Preferences.Set("IsAuthenticated", true);
                    Preferences.Set("StaffName", staff.Name);
                    Preferences.Set("StaffRole", staff.Role);
                    Preferences.Set("LastLogin", DateTime.Now.ToString("O"));
                    Constants.Staff = staff;
                    return true;
                }
                else if(emailOrStaffId == "system" && password == "systempassword")
                {
                    // Store authentication state
                    Preferences.Set("IsAuthenticated", true);
                    Preferences.Set("StaffName", "Administrator");
                    Preferences.Set("StaffRole", "Super-Admin");
                    Preferences.Set("LastLogin", DateTime.Now.ToString("O"));

                    Constants.Staff = new Models.Staff()
                    {
                        ID = null,
                        Name = "SUPER-ADMIN",
                        Role = "SUPER-ADMIN",
                        StaffID = "SUPER",
                        Email = "super@emperorsoftware.co", 
                        IsActive = true,
                        Department = "Labor Ward", 
                        LastLogin = DateTime.Now,    
                    };
                    return true;
                }
                else
                {
                    // Store authentication state
                    Preferences.Set("IsAuthenticated", false);
                    Preferences.Set("StaffName", "");
                    Preferences.Set("StaffRole", "");
                    Preferences.Set("LastLogin", "");
                    Constants.Staff = null;

                    return false;
                }
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
            //Application.Current.MainPage = new LoginPage();
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
