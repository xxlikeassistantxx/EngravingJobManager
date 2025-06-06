using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EngravingJobManager.Models;
using EngravingJobManager.Services;
using System.Linq; // For Enum.GetValues
using System; // For DateTime

namespace EngravingJobManager.ViewModels
{
    public class SearchByDateViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private DateTime _startDate;
        private DateTime _endDate;
        private Job _selectedJob;
        private SortOrderOption _selectedSortOrder;

        public ObservableCollection<Job> SearchResults { get; } = new ObservableCollection<Job>();
        public List<SortOrderOption> SortOptions { get; }

        public ICommand SearchCommand { get; }
        public ICommand JobSelectedCommand { get; }

        public SearchByDateViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            // Initialize dates (e.g., StartDate to a week ago, EndDate to today)
            _startDate = DateTime.Today.AddDays(-7);
            _endDate = DateTime.Today;

            SortOptions = Enum.GetValues(typeof(SortOrderOption)).Cast<SortOrderOption>().ToList();
            _selectedSortOrder = SortOrderOption.NewestFirst; // Default sort order

            SearchCommand = new Command(async () => await ExecuteSearchCommand());
            JobSelectedCommand = new Command<Job>(async (job) => await ExecuteJobSelectedCommand(job));
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public SortOrderOption SelectedSortOrder
        {
            get => _selectedSortOrder;
            set => SetProperty(ref _selectedSortOrder, value);
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

        private async Task ExecuteSearchCommand()
        {
            if (StartDate > EndDate)
            {
                await Application.Current.MainPage.DisplayAlert("Invalid Range", "Start date cannot be after end date.", "OK");
                return;
            }

            SearchResults.Clear();
            var results = await _databaseService.SearchJobsByDateRangeAsync(StartDate, EndDate, SelectedSortOrder);
            foreach (var job in results)
            {
                SearchResults.Add(job);
            }
        }

        private async Task ExecuteJobSelectedCommand(Job job)
        {
            if (job == null) return;
            await Shell.Current.GoToAsync($"{nameof(JobDetailPage)}?jobId={job.Id}");
        }

        public void OnAppearing()
        {
            // Optionally clear results or set default dates again
            // SearchResults.Clear(); 
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