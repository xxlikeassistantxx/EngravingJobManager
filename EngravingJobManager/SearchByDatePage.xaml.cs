using EngravingJobManager.ViewModels;

namespace EngravingJobManager;

public partial class SearchByDatePage : ContentPage
{
    private readonly SearchByDateViewModel _viewModel;

    public SearchByDatePage(SearchByDateViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // You can call _viewModel.OnAppearing(); if you add specific logic there,
        // like clearing previous search results or resetting dates.
        // For now, the ViewModel constructor sets default dates.
    }
}