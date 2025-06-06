using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EngravingJobManager.Models;
using EngravingJobManager.Services;

namespace EngravingJobManager.ViewModels
{
    [QueryProperty(nameof(JobId), "jobId")] // Used for receiving navigation parameter
    public class DeletedJobDetailViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Job _selectedJob;
        private string _jobId;

        public Job SelectedJob
        {
            get => _selectedJob;
            set
            {
                SetProperty(ref _selectedJob, value);
                // Update CanExecute for commands when SelectedJob changes
                ((Command)RestoreJobCommand).ChangeCanExecute();
                ((Command)PermanentlyDeleteJobCommand).ChangeCanExecute();
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

        public ICommand RestoreJobCommand { get; }
        public ICommand PermanentlyDeleteJobCommand { get; }

        public DeletedJobDetailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            RestoreJobCommand = new Command(async () => await ExecuteRestoreJobCommand(), () => SelectedJob != null);
            PermanentlyDeleteJobCommand = new Command(async () => await ExecutePermanentlyDeleteJobCommand(), () => SelectedJob != null);
        }

        private async Task LoadJobAsync(int id)
        {
            // Fetch the job, even if it's marked as deleted
            // GetJobAsync should ideally fetch by ID regardless of IsDeleted status,
            // or we might need a specific method if it filters out deleted ones.
            // Assuming GetJobAsync fetches by ID.
            SelectedJob = await _databaseService.GetJobAsync(id);
        }

        private async Task ExecuteRestoreJobCommand()
        {
            if (SelectedJob == null) return;

            await _databaseService.RestoreJobAsync(SelectedJob);
            await Application.Current.MainPage.DisplayAlert("Job Restored", $"{SelectedJob.Title} has been restored.", "OK");
            await Shell.Current.GoToAsync(".."); // Navigate back
        }

        private async Task ExecutePermanentlyDeleteJobCommand()
        {
            if (SelectedJob == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirm Permanent Delete",
                $"Are you sure you want to permanently delete job: {SelectedJob.Title}? This action cannot be undone.",
                "Yes, Permanently Delete", "No");

            if (confirm)
            {
                await _databaseService.HardDeleteJobAsync(SelectedJob);
                await Application.Current.MainPage.DisplayAlert("Job Deleted", $"{SelectedJob.Title} has been permanently deleted.", "OK");
                await Shell.Current.GoToAsync(".."); // Navigate back
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
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