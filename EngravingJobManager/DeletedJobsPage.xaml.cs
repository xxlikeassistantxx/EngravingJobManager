using EngravingJobManager.ViewModels;

namespace EngravingJobManager;

public partial class DeletedJobsPage : ContentPage
{
    private readonly DeletedJobsViewModel _viewModel;

    public DeletedJobsPage(DeletedJobsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }
}