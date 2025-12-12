using CommunityToolkit.Maui;
using FluentIcons.Maui;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using MauiIcons.Fluent;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
using System.Runtime.Versioning;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid
{
    [SupportedOSPlatform("android21.0")] // Add this attribute to suppress CA1416
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzk1NjY2MEAzMzMwMmUzMDJlMzAzYjMzMzAzYklHdmdHUCs1WTJOR05kNGp2Y1NLTnZFa3BBZTVFMnZDMW1TTDl2cGQrTlE9");

            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureSyncfusionCore()
                .UseFluentMauiIcons()
                .ConfigureMauiHandlers(handlers =>
                {
#if IOS || MACCATALYST
                    handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                    fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
                });

#if DEBUG
            builder.Logging.AddDebug();
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            // Register Repositories                        
            builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
            builder.Services.AddSingleton<PatientRepository>();
            builder.Services.AddSingleton<PartographRepository>();
            //builder.Services.AddSingleton<VitalSignRepository>();
            //builder.Services.AddSingleton<MedicalNoteRepository>();
            builder.Services.AddSingleton<StaffRepository>();
            builder.Services.AddSingleton<CompanionRepository>();
            builder.Services.AddSingleton<PainReliefRepository>();
            builder.Services.AddSingleton<OralFluidRepository>();
            builder.Services.AddSingleton<PostureRepository>();
            builder.Services.AddSingleton<HeadDescentRepository>();
            builder.Services.AddSingleton<ContractionRepository>();
            builder.Services.AddSingleton<CervixDilatationRepository>();
            builder.Services.AddSingleton<MouldingRepository>();
            builder.Services.AddSingleton<FHRRepository>();
            builder.Services.AddSingleton<TemperatureRepository>();
            builder.Services.AddSingleton<FetalPositionRepository>();
            builder.Services.AddSingleton<BPRepository>();
            builder.Services.AddSingleton<CaputRepository>();
            builder.Services.AddSingleton<UrineRepository>();
            builder.Services.AddSingleton<AmnioticFluidRepository>();
            builder.Services.AddSingleton<IVFluidEntryRepository>();
            builder.Services.AddSingleton<MedicationEntryRepository>();
            builder.Services.AddSingleton<OxytocinRepository>();
            builder.Services.AddSingleton<AssessmentRepository>();
            builder.Services.AddSingleton<PlanRepository>();
            builder.Services.AddSingleton<PartographDiagnosisRepository>();
            builder.Services.AddSingleton<PartographRiskFactorRepository>();
            builder.Services.AddSingleton<BirthOutcomeRepository>();
            builder.Services.AddSingleton<BabyDetailsRepository>();
            builder.Services.AddSingleton<ReferralRepository>();
            // Keep existing repositories for compatibility during migration
            //builder.Services.AddSingleton<ProjectRepository>();
            //builder.Services.AddSingleton<TaskRepository>();
            //builder.Services.AddSingleton<CategoryRepository>();
            //builder.Services.AddSingleton<TagRepository>();

            // Register Services
            builder.Services.AddSingleton<SeedDataService>();
            builder.Services.AddSingleton<ModalErrorHandler>();
            //builder.Services.AddSingleton<AuthenticationService>();

            // Register Sync Services
            builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
            builder.Services.AddHttpClient<ISyncApiClient, SyncApiClient>(client =>
            {
                // Configure API base URL from preferences or use default
                // var apiUrl = Preferences.Get("SyncApiUrl", "https://api.partograph.example.com");
                client.BaseAddress = new Uri("https://localhost:7193/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            builder.Services.AddSingleton<ISyncService, SyncService>();
            builder.Services.AddSingleton<BackgroundSyncService>();

            // Register PageModels
            builder.Services.AddSingleton<AppShellModel>();
            builder.Services.AddSingleton<LoginPageModel>();
            builder.Services.AddTransient<HomePageModel>();
            builder.Services.AddTransient<PendingPatientsPageModel>();
            builder.Services.AddTransient<ActivePatientsPageModel>();
            builder.Services.AddTransient<CompletedPatientsPageModel>();
            builder.Services.AddSingleton<CompanionModalPageModel>();
            builder.Services.AddSingleton<PainReliefModalPageModel>();
            builder.Services.AddSingleton<OralFluidModalPageModel>();
            builder.Services.AddSingleton<PostureModalPageModel>();
            builder.Services.AddSingleton<SyncSettingsPageModel>();
            builder.Services.AddSingleton<PartographChartPageModel>();
            builder.Services.AddSingleton<BirthOutcomePageModel>();
            builder.Services.AddSingleton<BabyDetailsPageModel>();
            builder.Services.AddSingleton<ReferralPageModel>();

            // Register Measurement Modal PageModels
            builder.Services.AddSingleton<CervixDilatationModalPageModel>();
            builder.Services.AddSingleton<HeadDescentModalPageModel>();
            builder.Services.AddSingleton<BPPulseModalPageModel>();
            builder.Services.AddSingleton<TemperatureModalPageModel>();
            builder.Services.AddSingleton<UrineModalPageModel>();
            builder.Services.AddSingleton<AmnioticFluidModalPageModel>();
            builder.Services.AddSingleton<FetalPositionModalPageModel>();
            builder.Services.AddSingleton<MouldingModalPageModel>();
            builder.Services.AddSingleton<IVFluidModalPageModel>();
            builder.Services.AddSingleton<MedicationModalPageModel>();
            builder.Services.AddSingleton<OxytocinModalPageModel>();
            builder.Services.AddSingleton<ContractionsModalPageModel>();
            builder.Services.AddSingleton<BaselineFHRModalPageModel>();
            builder.Services.AddSingleton<CaputModalPageModel>();
            builder.Services.AddSingleton<AssessmentModalPageModel>();
            builder.Services.AddSingleton<PlanModalPageModel>();
            builder.Services.AddSingleton<FHRContractionModalPageModel>();

            // Register Pages and PageModels with routes
            builder.Services.AddTransientWithShellRoute<PatientPage, PatientPageModel>("patient");
            builder.Services.AddTransientWithShellRoute<PartographPage, PartographPageModel>("partograph");
            builder.Services.AddTransientWithShellRoute<PartographEntryPage, PartographEntryPageModel>("partographentry");
            builder.Services.AddTransientWithShellRoute<PartographChartPage, PartographChartPageModel>("partographchart");
            builder.Services.AddTransientWithShellRoute<BirthOutcomePage, BirthOutcomePageModel>("BirthOutcomePage");
            builder.Services.AddTransientWithShellRoute<BabyDetailsPage, BabyDetailsPageModel>("BabyDetailsPage");
            builder.Services.AddTransientWithShellRoute<ReferralPage, ReferralPageModel>("ReferralPage");

            //builder.Services.AddTransientWithShellRoute<VitalSignsPage, VitalSignsPageModel>("vitalsigns");
            // builder.Services.AddTransientWithShellRoute<VitalSignsPage, VitalSignsPageModel>("vitalsigns");
            builder.Services.AddTransientWithShellRoute<SyncSettingsPage, SyncSettingsPageModel>("syncsettings");
            // 1b8b53456b4bed2b1a0122e743bff645ace745d3
            //builder.Services.AddTransientWithShellRoute<MedicalNotePage, MedicalNotePageModel>("medicalnote");

            // Register Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<PendingPatientsPage>();
            builder.Services.AddTransient<ActivePatientsPage>();
            builder.Services.AddTransient<CompletedPatientsPage>();
            builder.Services.AddTransient<SyncSettingsPage>();

            //                .ConfigureMauiHandlers(handlers =>
            //                {
            //#if IOS || MACCATALYST
            //    				handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
            //#endif
            //                })
            //                .ConfigureFonts(fonts =>
            //                {
            //                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            //                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            //                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
            //                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
            //                });

            //#if DEBUG
            //    		builder.Logging.AddDebug();
            //    		builder.Services.AddLogging(configure => configure.AddDebug());
            //#endif

            //            builder.Services.AddSingleton<ProjectRepository>();
            //            builder.Services.AddSingleton<TaskRepository>();
            //            builder.Services.AddSingleton<CategoryRepository>();
            //            builder.Services.AddSingleton<TagRepository>();
            //            builder.Services.AddSingleton<SeedDataService>();
            //            builder.Services.AddSingleton<ModalErrorHandler>();
            //            builder.Services.AddSingleton<MainPageModel>();
            //            builder.Services.AddSingleton<ProjectListPageModel>();
            //            builder.Services.AddSingleton<ManageMetaPageModel>();
            //            builder.Services.AddSingleton<LoginPageModel>();

            //            builder.Services.AddTransientWithShellRoute<ProjectDetailPage, ProjectDetailPageModel>("project");
            //            builder.Services.AddTransientWithShellRoute<TaskDetailPage, TaskDetailPageModel>("task");

            //            // Pages
            //            builder.Services.AddTransient<LoginPage>();

            return builder.Build();
        }
    }
}
