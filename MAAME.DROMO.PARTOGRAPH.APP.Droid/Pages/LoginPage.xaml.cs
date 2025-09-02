namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;

public partial class LoginPage : ContentPage
{
    private readonly LoginPageModel _viewModel;

    public LoginPage(LoginPageModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        //BindingContext = new LoginPageModel();
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