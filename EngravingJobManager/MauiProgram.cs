using Microsoft.Extensions.Logging;
using EngravingJobManager.Services;
using EngravingJobManager.ViewModels;

namespace EngravingJobManager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // --- Service Registration ---
            builder.Services.AddSingleton<DatabaseService>();

            // --- ViewModel Registrations ---
            // CreateJobViewModel is a Singleton to preserve state
            builder.Services.AddSingleton<CreateJobViewModel>();

            // All other ViewModels are Transient
            builder.Services.AddTransient<AllJobsViewModel>();
            builder.Services.AddTransient<JobDetailViewModel>();
            builder.Services.AddTransient<DeletedJobsViewModel>();
            builder.Services.AddTransient<DeletedJobDetailViewModel>();
            builder.Services.AddTransient<SearchByTitleViewModel>();
            builder.Services.AddTransient<SearchByDateViewModel>();
            builder.Services.AddTransient<SearchByClienteleViewModel>();
            builder.Services.AddTransient<PhotoDetailViewModel>(); // Add this line

            // --- Page Registrations ---
            // CreateJobPage is a Singleton to match its ViewModel's lifecycle
            builder.Services.AddSingleton<CreateJobPage>();

            // All other Pages are Transient
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<AllJobsPage>();
            builder.Services.AddTransient<JobDetailPage>();
            builder.Services.AddTransient<DeletedJobsPage>();
            builder.Services.AddTransient<DeletedJobDetailPage>();
            builder.Services.AddTransient<SearchByTitlePage>();
            builder.Services.AddTransient<SearchByDatePage>();
            builder.Services.AddTransient<SearchByClientelePage>();
            builder.Services.AddTransient<PhotoDetailPage>();    // Add this line

            return builder.Build();
        }
    }
}