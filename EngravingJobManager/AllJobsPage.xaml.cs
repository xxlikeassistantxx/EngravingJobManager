using EngravingJobManager.ViewModels;

namespace EngravingJobManager; // Or your specific Pages namespace if you used one

public partial class AllJobsPage : ContentPage
{
    private readonly AllJobsViewModel _viewModel;

    public AllJobsPage(AllJobsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Call the ViewModel's OnAppearing method
        // This is a good place to load data
        _viewModel.OnAppearing();
    }
}