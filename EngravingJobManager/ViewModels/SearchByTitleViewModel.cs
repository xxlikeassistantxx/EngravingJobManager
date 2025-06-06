using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EngravingJobManager.Models;
using EngravingJobManager.Services;
using System.Linq; // For Enum.GetValues
using System.Threading.Tasks; // For Task

namespace EngravingJobManager.ViewModels
{
    public class SearchByTitleViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private string _searchText;
        private Job _selectedJob;
        private SortOrderOption _selectedSortOrder;
        private CancellationTokenSource _searchCancellationTokenSource;


        public ObservableCollection<Job> SearchResults { get; } = new ObservableCollection<Job>();
        public List<SortOrderOption> SortOptions { get; }

        public ICommand JobSelectedCommand { get; }
        // No explicit LoadCommand needed as search is triggered by text/sort changes.

        public SearchByTitleViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            SortOptions = Enum.GetValues(typeof(SortOrderOption)).Cast<SortOrderOption>().ToList();
            _selectedSortOrder = SortOrderOption.NewestFirst; // Default sort order

            JobSelectedCommand = new Command<Job>(async (job) => await ExecuteJobSelectedCommand(job));
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    PerformSearch();
                }
            }
        }

        public SortOrderOption SelectedSortOrder
        {
            get => _selectedSortOrder;
            set
            {
                if (SetProperty(ref _selectedSortOrder, value))
                {
                    PerformSearch(); // Re-run search with new sort order
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

        private void PerformSearch()
        {
            // Cancel any previous search operation
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _searchCancellationTokenSource.Token;

            // Debounce: Optional delay to prevent too many DB calls during fast typing.
            // For simplicity, we'll call directly. For a real app, debouncing (e.g., 300-500ms delay) is recommended.
            Task.Run(async () =>
            {
                await Task.Delay(300, cancellationToken); // Simple debounce delay
                if (cancellationToken.IsCancellationRequested) return;

                SearchResults.Clear();
                if (string.IsNullOrWhiteSpace(SearchText) && SearchText?.Length < 1) // Or some minimum length e.g. SearchText?.Length < 2
                {
                    // Clear results if search text is effectively empty, or too short
                    // Or you could load all items if desired, but typically for live search, empty means no specific search
                }
                else
                {
                    var results = await _databaseService.SearchJobsByTitleAsync(SearchText, SelectedSortOrder);
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            SearchResults.Clear(); // Clear again just in case of race conditions (though less likely with cancel)
                            foreach (var job in results)
                            {
                                SearchResults.Add(job);
                            }
                        });
                    }
                }
            }, cancellationToken);
        }

        private async Task ExecuteJobSelectedCommand(Job job)
        {
            if (job == null) return;
            await Shell.Current.GoToAsync($"{nameof(JobDetailPage)}?jobId={job.Id}");
        }

        public void OnAppearing()
        {
            // Clear search text and results when page appears, or load initial state if desired
            SearchText = string.Empty;
            SearchResults.Clear();
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