using EngravingJobManager.ViewModels;

namespace EngravingJobManager;

public partial class PhotoDetailPage : ContentPage
{
    public PhotoDetailPage(PhotoDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}