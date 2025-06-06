using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EngravingJobManager.Models;
using EngravingJobManager.Services;
using System.Web; // Required for URL encoding

namespace EngravingJobManager.ViewModels
{
    [QueryProperty(nameof(JobId), "jobId")]
    public class JobDetailViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Job _selectedJob;
        private string _jobId;

        public Job SelectedJob
        {
            get => _selectedJob;
            set
            {
                if (SetProperty(ref _selectedJob, value))
                {
                    // Update the CanExecute status of commands when the job is loaded
                    ((Command)DeleteJobCommand).ChangeCanExecute();
                    ((Command)EditJobCommand).ChangeCanExecute();
                }
            }
        }

        public string JobId
        {
            get => _jobId;
            set
            {
                _jobId = value;
                if (!string.IsNullOrEmpty(_jobId) && int.TryParse(_jobId, out int jobIdValue))
                {
                    LoadJobAsync(jobIdValue);
                }
            }
        }

        public ICommand DeleteJobCommand { get; }
        public ICommand EditJobCommand { get; }
        public ICommand PhotoTappedCommand { get; } // New command for tapping photos

        public JobDetailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            DeleteJobCommand = new Command(async () => await ExecuteDeleteJobCommand(), () => SelectedJob != null);
            EditJobCommand = new Command(async () => await ExecuteEditJobCommand(), () => SelectedJob != null);
            // Initialize the new command
            PhotoTappedCommand = new Command<PhotoItem>(async (photo) => await ExecutePhotoTappedCommand(photo));
        }

        private async Task LoadJobAsync(int id)
        {
            SelectedJob = await _databaseService.GetJobAsync(id);
        }

        private async Task ExecuteDeleteJobCommand()
        {
            if (SelectedJob == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete job: {SelectedJob.Title}?",
                "Yes, Delete", "No");

            if (confirm)
            {
                await _databaseService.DeleteJobAsync(SelectedJob); // Soft delete
                await Shell.Current.GoToAsync(".."); // Navigate back
            }
        }

        private async Task ExecuteEditJobCommand()
        {
            if (SelectedJob == null) return;
            await Shell.Current.GoToAsync($"{nameof(CreateJobPage)}?jobIdToEdit={SelectedJob.Id}");
        }

        // New method to handle photo taps
        private async Task ExecutePhotoTappedCommand(PhotoItem photo)
        {
            if (photo == null || string.IsNullOrEmpty(photo.PhotoPath))
                return;

            // URL-encode the photo path to pass it safely as a navigation parameter
            string encodedPath = HttpUtility.UrlEncode(photo.PhotoPath);

            // Navigate to the PhotoDetailPage
            await Shell.Current.GoToAsync($"{nameof(PhotoDetailPage)}?PhotoPathUrl={encodedPath}");
        }

        public event PropertyChangedEventHandler? PropertyChanged; // Added '?' to address CS8612 warning
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}