namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Views;

public partial class ActivePatientsView : ContentView
{
	public ActivePatientsView()
	{
		InitializeComponent();
	}

    private void OnShowMenuClicked(object sender, EventArgs e)
    {
        // Show popup relative to the button that was clicked
        var button = sender as Button;
        contextMenuPopup.ShowRelativeToView(button, Syncfusion.Maui.Popup.PopupRelativePosition.AlignBottom, 0, 5);
    }

    private void OnEditClicked(object sender, EventArgs e)
    {
        contextMenuPopup.Dismiss();
        // Handle edit
    }

    private void OnDeleteClicked(object sender, EventArgs e)
    {
        contextMenuPopup.Dismiss();
        // Handle delete
    }

    private void OnShareClicked(object sender, EventArgs e)
    {
        contextMenuPopup.Dismiss();
        // Handle delete
    }
    
}