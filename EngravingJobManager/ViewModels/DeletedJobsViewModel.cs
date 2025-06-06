using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EngravingJobManager.Models;
using EngravingJobManager.Services;
using System.Linq;

namespace EngravingJobManager.ViewModels
{
    public class DeletedJobsViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Job _selectedJob;
        private SortOrderOption _selectedSortOrder;

        public ObservableCollection<Job> DeletedJobs { get; } = new ObservableCollection<Job>();
        public List<SortOrderOption> SortOptions { get; }

        public ICommand LoadDeletedJobsCommand { get; }
        public ICommand JobSelectedCommand { get; }

        public DeletedJobsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            SortOptions = Enum.GetValues(typeof(SortOrderOption)).Cast<SortOrderOption>().ToList();
            _selectedSortOrder = SortOrderOption.NewestFirst; // Default sort order

            LoadDeletedJobsCommand = new Command(async () => await ExecuteLoadDeletedJobsCommand());
            JobSelectedCommand = new Command<Job>(async (job) => await ExecuteJobSelectedCommand(job));
        }

        public SortOrderOption SelectedSortOrder
        {
            get => _selectedSortOrder;
            set
            {
                if (SetProperty(ref _selectedSortOrder, value))
                {
                    ExecuteLoadDeletedJobsCommand();
                }
            }
        }

        public Job SelectedJob
        {
            get => _selectedJob;
            set
            {
                SetProperty(ref _selectedJob, value);
                if (value != null)
                {
                    JobSelectedCommand.Execute(value);
                    SelectedJob = null;
                }
            }
        }

        public async Task ExecuteLoadDeletedJobsCommand()
        {
            DeletedJobs.Clear();
            // Call the method to get soft-deleted jobs
            var jobsList = await _databaseService.GetDeletedJobsAsync(SelectedSortOrder);
            foreach (var job in jobsList)
            {
                DeletedJobs.Add(job);
            }
        }

        private async Task ExecuteJobSelectedCommand(Job job)
        {
            if (job == null)
                return;
            // Navigate to DeletedJobDetailPage (we'll create this next)
            await Shell.Current.GoToAsync($"{nameof(DeletedJobDetailPage)}?jobId={job.Id}");
        }

        public void OnAppearing()
        {
            Task.Run(async () => await ExecuteLoadDeletedJobsCommand());
            SelectedJob = null;
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