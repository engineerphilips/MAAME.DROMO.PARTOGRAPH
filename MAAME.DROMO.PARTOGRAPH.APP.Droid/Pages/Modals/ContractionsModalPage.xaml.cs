using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Modals;

public partial class ContractionsModalPage : ContentPage
{
	public ContractionsModalPage(CompanionModalPageModel companion)
	{
		InitializeComponent();
		BindingContext = companion;
    }
}