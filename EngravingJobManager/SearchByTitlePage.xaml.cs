using EngravingJobManager.ViewModels;

namespace EngravingJobManager;

public partial class SearchByTitlePage : ContentPage
{
    private readonly SearchByTitleViewModel _viewModel;

    public SearchByTitlePage(SearchByTitleViewModel viewModel)
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