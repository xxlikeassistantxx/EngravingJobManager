using EngravingJobManager.ViewModels;

namespace EngravingJobManager;

public partial class SearchByClientelePage : ContentPage
{
    private readonly SearchByClienteleViewModel _viewModel;

    public SearchByClientelePage(SearchByClienteleViewModel viewModel)
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