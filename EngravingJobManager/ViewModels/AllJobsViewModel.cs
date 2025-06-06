using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EngravingJobManager.Models;
using EngravingJobManager.Services;
using System.Linq; // For Enum.GetValues
using System.Threading.Tasks; // For Task
using System.Collections.Generic; // For List
using System; // For Enum, Exception
using Microsoft.Maui.ApplicationModel; // For MainThread

namespace EngravingJobManager.ViewModels
{
    public class AllJobsViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Job _selectedJob;
        private SortOrderOption _selectedSortOrder;

        public ObservableCollection<Job> Jobs { get; } = new ObservableCollection<Job>();
        public List<SortOrderOption> SortOptions { get; }

        public ICommand LoadJobsCommand { get; }
        public ICommand JobSelectedCommand { get; }

        public AllJobsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            SortOptions = Enum.GetValues(typeof(SortOrderOption)).Cast<SortOrderOption>().ToList();
            _selectedSortOrder = SortOrderOption.NewestFirst; // Default sort order

            LoadJobsCommand = new Command(async () => await ExecuteLoadJobsCommand());
            JobSelectedCommand = new Command<Job>(async (job) => await ExecuteJobSelectedCommand(job));
        }

        public SortOrderOption SelectedSortOrder
        {
            get => _selectedSortOrder;
            set
            {
                if (SetProperty(ref _selectedSortOrder, value))
                {
                    ExecuteLoadJobsCommand();
                }
            }
        }

        public Job SelectedJob
        {
            get => _selectedJob;
            set
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG AllJobsVM] SelectedJob setter. Old value title: '{_selectedJob?.Title}', New value title: '{value?.Title}'");
                bool changed = SetProperty(ref _selectedJob, value);

                if (value != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG AllJobsVM] New SelectedJob is not null: '{value.Title}'. Executing command.");
                    JobSelectedCommand.Execute(value);
                    System.Diagnostics.Debug.WriteLine($"[DEBUG AllJobsVM] After JobSelectedCommand.Execute. Setting SelectedJob to null.");
                    // This line is for when using SelectedItem binding on CollectionView,
                    // to allow re-selection of the same item after navigating back.
                    // If using TapGestureRecognizer, this direct SelectedJob property might not be strictly needed
                    // for selection triggering, but clearing it in OnAppearing is still good.
                    SelectedJob = null;
                }
                else if (changed)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG AllJobsVM] SelectedJob explicitly set to null by setter.");
                }
            }
        }

        public async Task ExecuteLoadJobsCommand()
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG AllJobsVM] ExecuteLoadJobsCommand called with sort order: {SelectedSortOrder}");
            Jobs.Clear();
            try
            {
                var jobsList = await _databaseService.GetJobsAsync(SelectedSortOrder);
                System.Diagnostics.Debug.WriteLine($"[DEBUG AllJobsVM] Loaded {jobsList?.Count} jobs from database.");
                if (jobsList != null)
                {
                    foreach (var job in jobsList)
                    {
                        Jobs.Add(job);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG AllJobsVM] EXCEPTION in ExecuteLoadJobsCommand: {ex.ToString()}");
            }
        }

        private async Task ExecuteJobSelectedCommand(Job job)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG AllJobsVM] ExecuteJobSelectedCommand entered for job: '{job?.Title}' with ID: {job?.Id}");
            if (job == null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG AllJobsVM] ExecuteJobSelectedCommand: job is null, returning.");
                return;
            }

            string targetRoute = $"{nameof(JobDetailPage)}?jobId={job.Id}";
            System.Diagnostics.Debug.WriteLine($"[DEBUG AllJobsVM] Attempting navigation to: {targetRoute}");
            try
            {
                await Shell.Current.GoToAsync(targetRoute);
                System.Diagnostics.Debug.WriteLine($"[DEBUG AllJobsVM] Navigation to {targetRoute} call completed.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG AllJobsVM] EXCEPTION during GoToAsync: {ex.ToString()}");
            }
        }

        public void OnAppearing()
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG AllJobsVM] OnAppearing called.");
            Task.Run(async () => await ExecuteLoadJobsCommand());

            if (SelectedJob != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG AllJobsVM] OnAppearing: Clearing SelectedJob which was: '{SelectedJob.Title}'");
                SelectedJob = null;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG AllJobsVM] OnAppearing: SelectedJob was already null.");
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