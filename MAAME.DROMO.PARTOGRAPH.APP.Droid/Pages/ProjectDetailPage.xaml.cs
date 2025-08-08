using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages
{
    public partial class ProjectDetailPage : ContentPage
    {
        public ProjectDetailPage(ProjectDetailPageModel model)
        {
            InitializeComponent();

            BindingContext = model;
        }
    }
}
