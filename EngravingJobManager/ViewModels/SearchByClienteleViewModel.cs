using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EngravingJobManager.Models;
using EngravingJobManager.Services;
using System.Threading.Tasks; // For Task & CancellationTokenSource
using Microsoft.Maui.ApplicationModel; // For MainThread

namespace EngravingJobManager.ViewModels
{
    public class SearchByClienteleViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private string _searchText;
        private Job _selectedJob;
        private CancellationTokenSource _searchCancellationTokenSource;

        public ObservableCollection<Job> SearchResults { get; } = new ObservableCollection<Job>();
        public ICommand JobSelectedCommand { get; }

        public SearchByClienteleViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
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
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _searchCancellationTokenSource.Token;

            Task.Run(async () =>
            {
                await Task.Delay(300, cancellationToken); // Debounce delay
                if (cancellationToken.IsCancellationRequested) return;

                var results = await _databaseService.SearchJobsByClienteleAsync(SearchText);

                if (!cancellationToken.IsCancellationRequested)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        SearchResults.Clear();
                        if (results != null)
                        {
                            foreach (var job in results)
                            {
                                SearchResults.Add(job);
                            }
                        }
                    });
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