using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
//using static Android.Renderscripts.ScriptGroup;

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

        if (sender is Button button)
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

    private async void Partograph_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            if (BindingContext is ActivePatientsPageModel pageModel)
                //pageModel.BindToPatient((int)button.tag);
                await Shell.Current.GoToAsync($"//partograph?patientId={2}");
        }
        //?id={patient.ID}
        //await Shell.Current.GoToAsync("//main");
    }

    //private void Button_BindingContextChanged(object sender, EventArgs e)
    //{
    //    if (sender is Button btn)
    //    {
    //        if (btn.BindingContext == null)
    //        {
    //            Debug.WriteLine($"WARNING: No binding found");
    //        }
    //        else
    //        {
    //            //Debug.WriteLine($"Binding found for {btn.BindingContext}: Source={btn.BindingContext.Source}, Path={binding.Path}");

    //            Debug.WriteLine($"Binding found for {btn.BindingContext}");
    //        }
    //    }
    //}
}