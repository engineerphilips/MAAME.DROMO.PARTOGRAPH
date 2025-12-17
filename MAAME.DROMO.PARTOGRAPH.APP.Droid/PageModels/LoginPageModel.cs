using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class LoginPageModel : INotifyPropertyChanged
    {
        private readonly IAuthenticationService _authService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private bool _isBusy = false;
        private bool _rememberMe = false;
        private bool _isPasswordVisible = false;
        private bool _hasError = false;

        //
        public LoginPageModel(IAuthenticationService authService)
        {
            _authService = authService;

            // Initialize commands
            LoginCommand = new Command(async () => await LoginAsync(), () => CanLogin());
            SignUpCommand = new Command(async () => await NavigateToSignUpAsync());
            ForgotPasswordCommand = new Command(async () => await NavigateToForgotPasswordAsync());
            StaffLoginCommand = new Command(async () => await NavigateToStaffLoginAsync());
            TogglePasswordVisibilityCommand = new Command(() => IsPasswordVisible = !IsPasswordVisible);
            ToggleRememberMeCommand = new Command(() => RememberMe = !RememberMe);
        }

        #region Properties

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    ((Command)LoginCommand).ChangeCanExecute();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ((Command)LoginCommand).ChangeCanExecute();
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    OnPropertyChanged(nameof(IsNotBusy));
                    ((Command)LoginCommand).ChangeCanExecute();
                }
            }
        }

        public bool HasError
        {
            get => _hasError;
            set
            {
                if (SetProperty(ref _hasError, value))
                {
                    OnPropertyChanged(nameof(HasError));
                    ((Command)LoginCommand).ChangeCanExecute();
                }
            }
        }
        
        public bool IsNotBusy => !IsBusy;

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }
        #endregion

        #region Commands

        public ICommand LoginCommand { get; }
        public ICommand SignUpCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand StaffLoginCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand ToggleRememberMeCommand { get; }

        #endregion

        #region Private Methods

        private bool CanLogin()
        {
            return !IsBusy &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password);
        }

        private async Task LoginAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                // Validate input
                if (string.IsNullOrWhiteSpace(Email))
                {
                    await ShowAlertAsync("Validation Error", "Please enter your email or username.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    await ShowAlertAsync("Validation Error", "Please enter your password.");
                    return;
                }

                // Attempt login
                var loginResult = await _authService.LoginAsync(Email.Trim(), Password);
                //true; // 

                if (loginResult)
                {
                    // Save remember me preference
                    if (RememberMe)
                    {
                        await SecureStorage.SetAsync("remembered_email", Email.Trim());
                    }
                    else
                    {
                        SecureStorage.Remove("remembered_email");
                    }

                    // Clear form
                    Email = string.Empty;
                    Password = string.Empty;

                    // Check if user is Super or Maame.Dromo.Admin
                    // If yes, redirect to FacilitySelectionPage
                    // If no, redirect to AppShell
                    if (Constants.IsSuperOrAdmin())
                    {
                        // Navigate to FacilitySelectionPage for facility selection with loading transition
                        await TransitionLoadingPage.TransitionToAsync(
                            () =>
                            {
                                var facilitySelectionPage = IPlatformApplication.Current!.Services.GetService<FacilitySelectionPage>();
                                return facilitySelectionPage ?? (Page)new AppShell();
                            },
                            "Signing In...",
                            "Loading your facilities...");
                    }
                    else
                    {
                        // Navigate directly to AppShell for non-Super/Admin users with loading transition
                        await TransitionLoadingPage.TransitionToAsync(
                            () => new AppShell(),
                            "Signing In...",
                            "Loading dashboard...");
                    }
                }
                else
                {
                    await ShowAlertAsync("Login Failed",
                        "Invalid email/username or password. Please check your credentials and try again.");
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Connection Error",
                    "Unable to connect to the server. Please check your internet connection and try again.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task NavigateToSignUpAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("SignUp");
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Navigation Error", "Unable to open sign up page.");
            }
        }

        private async Task NavigateToForgotPasswordAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("ForgotPassword");
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Navigation Error", "Unable to open forgot password page.");
            }
        }

        private async Task NavigateToStaffLoginAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("StaffLogin");
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Navigation Error", "Unable to open staff login portal.");
            }
        }

        private static async Task ShowAlertAsync(string title, string message)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, "OK");
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion

        #region Lifecycle

        public async Task InitializeAsync()
        {
            // Load remembered email if available
            try
            {
                var rememberedEmail = await SecureStorage.GetAsync("remembered_email");
                if (!string.IsNullOrEmpty(rememberedEmail))
                {
                    Email = rememberedEmail;
                    RememberMe = true;
                }
            }
            catch (Exception ex)
            {
                // Ignore errors when loading remembered email
            }
        }

        #endregion
    }
}
