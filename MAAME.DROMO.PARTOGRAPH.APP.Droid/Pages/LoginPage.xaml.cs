namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        BindingContext = new LoginPageModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Focus on email entry when page appears
        MainThread.BeginInvokeOnMainThread(() =>
        {
            EmailEntry.Focus();
        });
    }
}