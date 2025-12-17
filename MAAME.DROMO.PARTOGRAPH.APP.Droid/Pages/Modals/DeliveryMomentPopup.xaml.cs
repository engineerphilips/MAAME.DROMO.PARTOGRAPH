using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;
using Syncfusion.Maui.Popup;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Modals;

public partial class DeliveryMomentPopup : SfPopup
{
    public DeliveryMomentPopup()
    {
        InitializeComponent();
    }

    public DeliveryMomentPopup(DeliveryMomentPopupPageModel viewModel) : this()
    {
        BindingContext = viewModel;
    }
}
