using EngravingJobManager.ViewModels;

namespace EngravingJobManager;

public partial class CreateJobPage : ContentPage
{
    public CreateJobPage(CreateJobViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}