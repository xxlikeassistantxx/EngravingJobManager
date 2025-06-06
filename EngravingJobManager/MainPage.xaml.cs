namespace EngravingJobManager; // Make sure this namespace matches your project

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnCreateNewJobClicked(object sender, EventArgs e)
    {
        // Placeholder for navigation or action
        //await DisplayAlert("Navigation", "Create New Job page will open here.", "OK");
        // Later, this will be: await Shell.Current.GoToAsync("//CreateJobPage");
        await Shell.Current.GoToAsync(nameof(CreateJobPage)); // Use nameof for type safety
    }

    private async void OnSearchJobByTitleClicked(object sender, EventArgs e)
    {
        // await DisplayAlert("Navigation", "Search by Title page will open here.", "OK");
        await Shell.Current.GoToAsync(nameof(SearchByTitlePage)); // Modified
    }

    private async void OnSearchJobByDateClicked(object sender, EventArgs e)
    {
        // await DisplayAlert("Navigation", "Search by Date page will open here.", "OK");
        await Shell.Current.GoToAsync(nameof(SearchByDatePage)); // Modified
    }

    private async void OnSearchAllJobsClicked(object sender, EventArgs e)
    {
        // await DisplayAlert("Navigation", "Browse All Jobs page will open here.", "OK");
        await Shell.Current.GoToAsync(nameof(AllJobsPage)); // Modified
    }

    private async void OnSearchJobsByClienteleClicked(object sender, EventArgs e)
    {
        // await DisplayAlert("Navigation", "Search by Clientele page will open here.", "OK");
        await Shell.Current.GoToAsync(nameof(SearchByClientelePage)); // Modified
    }

    private async void OnDeletedJobsFolderClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(DeletedJobsPage)); // Modified
    }
   
}