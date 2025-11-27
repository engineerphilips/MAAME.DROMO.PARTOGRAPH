using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Modals;

public partial class AmnioticFluidModalView : ContentView
{
	public AmnioticFluidModalView(AmnioticFluidModalPageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
	}
}