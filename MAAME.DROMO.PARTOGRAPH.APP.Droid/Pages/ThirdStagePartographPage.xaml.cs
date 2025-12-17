using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Modals;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

[QueryProperty(nameof(PatientId), "patientId")]
public partial class ThirdStagePartographPage : ContentPage
{
    public string PatientId { get; set; }
    private readonly ThirdStagePartographPageModel _pageModel;
    private PlacentaDeliveryPopup? _placentaDeliveryPopup;

    public ThirdStagePartographPage(ThirdStagePartographPageModel pageModel)
    {
        InitializeComponent();
        _pageModel = pageModel;
        BindingContext = pageModel;

        // Wire up popup actions
        _pageModel.OpenPlacentaDeliveryPopup = OpenPlacentaDeliveryPopup;
        _pageModel.ClosePlacentaDeliveryPopup = ClosePlacentaDeliveryPopup;
    }

    private void OpenPlacentaDeliveryPopup()
    {
        _placentaDeliveryPopup = new PlacentaDeliveryPopup(_pageModel.PlacentaDeliveryPopupPageModel)
        {
            IsOpen = true,
            ShowHeader = true,
            ShowFooter = false,
            ShowCloseButton = true,
            WidthRequest = 400,
            HeightRequest = 600
        };

        // Add popup to the page
        if (Content is Grid grid)
        {
            grid.Children.Add(_placentaDeliveryPopup);
        }
        else
        {
            var newGrid = new Grid();
            var oldContent = Content;
            Content = newGrid;
            newGrid.Children.Add(oldContent);
            newGrid.Children.Add(_placentaDeliveryPopup);
        }
    }

    private void ClosePlacentaDeliveryPopup()
    {
        if (_placentaDeliveryPopup != null)
        {
            _placentaDeliveryPopup.IsOpen = false;
            if (Content is Grid grid)
            {
                grid.Children.Remove(_placentaDeliveryPopup);
            }
            _placentaDeliveryPopup = null;
        }
    }
}
