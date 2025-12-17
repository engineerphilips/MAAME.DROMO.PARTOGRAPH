using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;
using Syncfusion.Maui.Popup;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Modals;

public partial class PlacentaDeliveryPopup : SfPopup
{
    public PlacentaDeliveryPopup()
    {
        InitializeComponent();
    }

    public PlacentaDeliveryPopup(PlacentaDeliveryPopupPageModel viewModel) : this()
    {
        BindingContext = viewModel;
    }
}
