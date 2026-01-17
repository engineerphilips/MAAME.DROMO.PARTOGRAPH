using CommunityToolkit.Maui;
using FluentIcons.Maui;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services.Helper;
using MauiIcons.Fluent;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
using System.Runtime.Versioning;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid
{
    //[SupportedOSPlatform("android21.0")] // Add this attribute to suppress CA1416
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

#if RELEASE
            builder.Logging.AddDebug();
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif
            // Register Repositories
            builder.Services.AddSingleton<PatientRepository>();
            builder.Services.AddSingleton<PartographRepository>();
            //builder.Services.AddSingleton<VitalSignRepository>();
            //builder.Services.AddSingleton<MedicalNoteRepository>();
            builder.Services.AddSingleton<StaffRepository>();
            builder.Services.AddSingleton<FacilityRepository>();
            builder.Services.AddSingleton<CompanionRepository>();
            builder.Services.AddSingleton<PainReliefRepository>();
            //builder.Services.AddSingleton<PainReliefEntryRepository>();
            builder.Services.AddSingleton<OralFluidRepository>();
            builder.Services.AddSingleton<PostureRepository>();
            builder.Services.AddSingleton<HeadDescentRepository>();
            builder.Services.AddSingleton<BishopScoreRepository>();
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
            builder.Services.AddSingleton<FourthStageVitalsRepository>();
            // Keep existing repositories for compatibility during migration
            //builder.Services.AddSingleton<ProjectRepository>();
            //builder.Services.AddSingleton<TaskRepository>();
            //builder.Services.AddSingleton<CategoryRepository>();
            //builder.Services.AddSingleton<TagRepository>();

            // Register Services
            builder.Services.AddSingleton<SeedDataService>();
            builder.Services.AddSingleton<ModalErrorHandler>();
            builder.Services.AddSingleton<ILoadingOverlayService, LoadingOverlayService>();
            builder.Services.AddSingleton<IDataLoadingService, DataLoadingService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<FHRPatternAnalysisService>();
            builder.Services.AddSingleton<StageProgressionService>(); // WHO Four-Stage Labor System
            builder.Services.AddSingleton<LabourTimerService>(); // Labour stage timers and APGAR reminders
            builder.Services.AddSingleton<PartographNotesService>(); // Dynamic clinical notes generation
            builder.Services.AddSingleton<IReportService, ReportService>(); // Comprehensive reporting service
            builder.Services.AddHttpClient<IPartographPdfService, PartographPdfService>(); // PDF generation service

            // Register JWT Token Storage Service
            builder.Services.AddSingleton<ITokenStorageService, TokenStorageService>();

            // Register Auth API Client for JWT authentication
            builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
            {
                // Use same base URL as sync API
                //client.BaseAddress = new Uri("https://172.20.10.4:5218/");
                //client.BaseAddress = new Uri("http://emperor-dev:5218/");
                client.BaseAddress = new Uri("http://192.168.100.4:5218/");

                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // Allow all server certificates for development (HTTP doesn't use certs, but good for future HTTPS)
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });

            // Register Authentication Service (with JWT support)
            // Must be registered after ITokenStorageService and IAuthApiClient
            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();

            // Register Sync Services
            builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
            builder.Services.AddHttpClient<ISyncApiClient, SyncApiClient>(client =>
            {
                // Configure API base URL from preferences or use default
                // var apiUrl = Preferences.Get("SyncApiUrl", "https://api.partograph.example.com");
                //client.BaseAddress = new Uri("http://emperor-dev:5218/");
                client.BaseAddress = new Uri("http://192.168.100.4:5218/");

                client.Timeout = TimeSpan.FromSeconds(120); // Increased timeout for sync operations
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // Allow all server certificates for development (HTTP doesn't use certs, but good for future HTTPS)
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                // Increased timeout for large sync operations (patients, partographs, etc.)
                //client.Timeout = TimeSpan.FromSeconds(120);
            }); 
            builder.Services.AddSingleton<IServiceRequestProvider, ServiceRequestProvider>();
            builder.Services.AddSingleton<ISyncService, SyncService>();
            builder.Services.AddSingleton<BackgroundSyncService>();
            
            // Register PageModels
            builder.Services.AddSingleton<AppShellModel>();
            builder.Services.AddSingleton<LoginPageModel>();
            builder.Services.AddSingleton<HelpPageModel>();
            builder.Services.AddSingleton<FacilitySelectionPageModel>();
            builder.Services.AddSingleton<FacilityListPageModel>();
            builder.Services.AddSingleton<FacilityOnboardingPageModel>();
            builder.Services.AddSingleton<UsersPageModel>();
            builder.Services.AddSingleton<SignupPageModel>();
            builder.Services.AddTransient<HomePageModel>();
            builder.Services.AddTransient<PatientsPageModel>();
            builder.Services.AddTransient<PatientHubPageModel>();
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
            builder.Services.AddSingleton<BishopScorePopupPageModel>();

            // Stage Transition Popup PageModels
            builder.Services.AddSingleton<DeliveryMomentPopupPageModel>();
            builder.Services.AddSingleton<PlacentaDeliveryPopupPageModel>();
            builder.Services.AddSingleton<CompletionChecklistPopupPageModel>();

            // Register Pages and PageModels with routes
            builder.Services.AddTransientWithShellRoute<PatientPage, PatientPageModel>("patient");
            builder.Services.AddTransientWithShellRoute<PartographPage, PartographPageModel>("partograph");
            builder.Services.AddTransientWithShellRoute<SecondStagePartographPage, SecondStagePartographPageModel>("secondpartograph");
            builder.Services.AddTransientWithShellRoute<ThirdStagePartographPage, ThirdStagePartographPageModel>("thirdpartograph");
            builder.Services.AddTransientWithShellRoute<FourthStagePartographPage, FourthStagePartographPageModel>("fourthpartograph");
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
            builder.Services.AddTransient<HelpPage>();
            builder.Services.AddTransient<FacilitySelectionPage>();
            builder.Services.AddTransient<FacilityListPage>();
            builder.Services.AddTransient<FacilityOnboardingPage>();
            builder.Services.AddTransient<UsersPage>();
            builder.Services.AddTransient<SignupPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<PatientsPage>();
            builder.Services.AddTransient<PatientHubPage>();
            builder.Services.AddTransient<PendingPatientsPage>();
            builder.Services.AddTransient<ActivePatientsPage>();
            builder.Services.AddTransient<CompletedPatientsPage>();
            builder.Services.AddTransient<SyncSettingsPage>();
            builder.Services.AddTransient<ReportsPage>();
            builder.Services.AddTransient<ReportsPageModel>();

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
