using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Modals;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

[QueryProperty(nameof(PatientId), "patientId")]
public partial class FourthStagePartographPage : ContentPage
{
    public string PatientId { get; set; }
    private readonly FourthStagePartographPageModel _pageModel;
    private CompletionChecklistPopup? _completionChecklistPopup;

    public FourthStagePartographPage(FourthStagePartographPageModel pageModel)
    {
        InitializeComponent();
        _pageModel = pageModel;
        BindingContext = pageModel;

        // Wire up popup actions
        _pageModel.OpenCompletionChecklistPopup = OpenCompletionChecklistPopup;
        _pageModel.CloseCompletionChecklistPopup = CloseCompletionChecklistPopup;
    }

    private void OpenCompletionChecklistPopup()
    {
        _completionChecklistPopup = new CompletionChecklistPopup(_pageModel.CompletionChecklistPopupPageModel)
        {
            IsOpen = true,
            ShowHeader = true,
            ShowFooter = false,
            ShowCloseButton = true,
            WidthRequest = 450,
            HeightRequest = 650
        };

        // Add popup to the page
        if (Content is Grid grid)
        {
            grid.Children.Add(_completionChecklistPopup);
        }
        else
        {
            var newGrid = new Grid();
            var oldContent = Content;
            Content = newGrid;
            newGrid.Children.Add(oldContent);
            newGrid.Children.Add(_completionChecklistPopup);
        }
    }

    private void CloseCompletionChecklistPopup()
    {
        if (_completionChecklistPopup != null)
        {
            _completionChecklistPopup.IsOpen = false;
            if (Content is Grid grid)
            {
                grid.Children.Remove(_completionChecklistPopup);
            }
            _completionChecklistPopup = null;
        }
    }
}
