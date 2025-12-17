using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;
using Syncfusion.Maui.Popup;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Modals;

public partial class CompletionChecklistPopup : SfPopup
{
    public CompletionChecklistPopup()
    {
        InitializeComponent();
    }

    public CompletionChecklistPopup(CompletionChecklistPopupPageModel viewModel) : this()
    {
        BindingContext = viewModel;
    }
}
