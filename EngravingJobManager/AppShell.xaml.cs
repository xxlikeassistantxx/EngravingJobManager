namespace EngravingJobManager;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(CreateJobPage), typeof(CreateJobPage));
        Routing.RegisterRoute(nameof(AllJobsPage), typeof(AllJobsPage));
        Routing.RegisterRoute(nameof(JobDetailPage), typeof(JobDetailPage));
        Routing.RegisterRoute(nameof(DeletedJobsPage), typeof(DeletedJobsPage)); // Add this
        Routing.RegisterRoute(nameof(DeletedJobDetailPage), typeof(DeletedJobDetailPage)); // Add this (for next step)
        Routing.RegisterRoute(nameof(SearchByTitlePage), typeof(SearchByTitlePage)); // Add this
        Routing.RegisterRoute(nameof(SearchByDatePage), typeof(SearchByDatePage)); // Add this
        Routing.RegisterRoute(nameof(SearchByClientelePage), typeof(SearchByClientelePage)); // Add this
        Routing.RegisterRoute(nameof(PhotoDetailPage), typeof(PhotoDetailPage)); // Add this
    }
}