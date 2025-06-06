using EngravingJobManager.ViewModels;

namespace EngravingJobManager; // Or your specific Pages namespace

public partial class JobDetailPage : ContentPage
{
    public JobDetailPage(JobDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}