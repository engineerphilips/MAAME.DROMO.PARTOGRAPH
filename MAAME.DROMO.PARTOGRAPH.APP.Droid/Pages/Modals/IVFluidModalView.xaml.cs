using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Modals;

public partial class IVFluidModalView : ContentView
{
	public IVFluidModalView(IVFluidModalPageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
	}
}