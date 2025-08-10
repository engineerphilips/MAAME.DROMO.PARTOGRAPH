using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using System.Runtime.Versioning;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid
{
    [SupportedOSPlatform("android21.0")] // Add this attribute to suppress CA1416
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
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
                });

#if DEBUG
            builder.Logging.AddDebug();
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            // Register Repositories
            builder.Services.AddSingleton<PatientRepository>();
            builder.Services.AddSingleton<PartographEntryRepository>();
            builder.Services.AddSingleton<VitalSignRepository>();
            builder.Services.AddSingleton<MedicalNoteRepository>();
            builder.Services.AddSingleton<StaffRepository>();

            // Keep existing repositories for compatibility during migration
            builder.Services.AddSingleton<ProjectRepository>();
            builder.Services.AddSingleton<TaskRepository>();
            builder.Services.AddSingleton<CategoryRepository>();
            builder.Services.AddSingleton<TagRepository>();

            // Register Services
            builder.Services.AddSingleton<SeedDataService>();
            builder.Services.AddSingleton<ModalErrorHandler>();
            builder.Services.AddSingleton<AuthenticationService>();

            // Register PageModels
            builder.Services.AddSingleton<LoginPageModel>();
            builder.Services.AddSingleton<HomePageModel>();
            builder.Services.AddSingleton<PendingPatientsPageModel>();
            builder.Services.AddSingleton<ActivePatientsPageModel>();
            builder.Services.AddSingleton<CompletedPatientsPageModel>();

            // Register Pages and PageModels with routes
            builder.Services.AddTransientWithShellRoute<PatientDetailPage, PatientDetailPageModel>("patient");
            builder.Services.AddTransientWithShellRoute<PartographPage, PartographPageModel>("partograph");
            builder.Services.AddTransientWithShellRoute<PartographEntryPage, PartographEntryPageModel>("partographentry");
            builder.Services.AddTransientWithShellRoute<VitalSignsPage, VitalSignsPageModel>("vitalsigns");
            //builder.Services.AddTransientWithShellRoute<MedicalNotePage, MedicalNotePageModel>("medicalnote");

            // Register Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<PendingPatientsPage>();
            builder.Services.AddTransient<ActivePatientsPage>();
            builder.Services.AddTransient<CompletedPatientsPage>();

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
