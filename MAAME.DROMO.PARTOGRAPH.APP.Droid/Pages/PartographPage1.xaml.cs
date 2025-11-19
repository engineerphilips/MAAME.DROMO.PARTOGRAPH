namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;

[QueryProperty(nameof(PatientId), "patientId")]
public partial class PartographPage1 : ContentPage
{
    public string PatientId { get; set; }

    public PartographPage1(PartographPageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
        //Loaded += (s, e) =>
        //{
        //    if (BindingContext is PartographPageModel pageModel)
        //    {
        //        var x = PatientId;
        //    }
        //};
    }
}