using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Modals;

public partial class MouldingModalView : ContentView
{
	public MouldingModalView(MouldingModalPageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
	}
}