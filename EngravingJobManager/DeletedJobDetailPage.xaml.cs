using EngravingJobManager.ViewModels;

namespace EngravingJobManager;

public partial class DeletedJobDetailPage : ContentPage
{
    public DeletedJobDetailPage(DeletedJobDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}