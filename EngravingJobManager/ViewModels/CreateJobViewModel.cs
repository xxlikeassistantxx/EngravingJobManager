using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EngravingJobManager.Models;
using EngravingJobManager.Services;
using Microsoft.Maui.Storage;
using System.Linq;

namespace EngravingJobManager.ViewModels
{
    // Add QueryProperty to receive the Job ID for editing
    [QueryProperty(nameof(JobIdToEdit), "jobIdToEdit")]
    public class CreateJobViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private int _editingJobId; // To store the ID of the job being edited

        // Properties for UI binding (JobTitle, CustomerName, etc. remain the same)
        private string _jobTitle;
        public string JobTitle { get => _jobTitle; set => SetProperty(ref _jobTitle, value); }

        private string _customerName;
        public string CustomerName { get => _customerName; set => SetProperty(ref _customerName, value); }

        private string _customerOrganization;
        public string CustomerOrganization { get => _customerOrganization; set => SetProperty(ref _customerOrganization, value); }

        private string _phoneNumber;
        public string PhoneNumber { get => _phoneNumber; set => SetProperty(ref _phoneNumber, value); }

        private string _details;
        public string Details { get => _details; set => SetProperty(ref _details, value); }

        // New property for the page title
        private string _pageTitle;
        public string PageTitle { get => _pageTitle; set => SetProperty(ref _pageTitle, value); }

        public ObservableCollection<string> PhotoPaths { get; } = new ObservableCollection<string>();
        private List<PhotoItem> _tempPhotoItems = new List<PhotoItem>();

        // This property receives the Job ID from navigation
        public string JobIdToEdit
        {
            set
            {
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int jobId))
                {
                    _editingJobId = jobId;
                    LoadJobForEditing(jobId);
                }
                else
                {
                    // If no ID is passed, it's a new job
                    PageTitle = "Create New Job";
                    _editingJobId = 0; // Ensure ID is 0 for new jobs
                    ClearForm();
                }
            }
        }

        public ICommand SaveJobCommand { get; }
        public ICommand AddPhotoFromGalleryCommand { get; }
        public ICommand AddPhotoFromCameraCommand { get; }

        public CreateJobViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            SaveJobCommand = new Command(async () => await OnSaveJob(), () => !string.IsNullOrWhiteSpace(JobTitle));
            AddPhotoFromGalleryCommand = new Command(async () => await OnAddPhoto(false));
            AddPhotoFromCameraCommand = new Command(async () => await OnAddPhoto(true));

            PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(JobTitle))
                {
                    ((Command)SaveJobCommand).ChangeCanExecute();
                }
            };

            PageTitle = "Create New Job"; // Default title
        }

        private async void LoadJobForEditing(int jobId)
        {
            PageTitle = "Edit Job";
            var job = await _databaseService.GetJobAsync(jobId);
            if (job != null)
            {
                JobTitle = job.Title;
                CustomerName = job.CustomerName;
                CustomerOrganization = job.CustomerOrganization;
                PhoneNumber = job.PhoneNumber;
                Details = job.Details;

                PhotoPaths.Clear();
                _tempPhotoItems.Clear();
                // Load existing photos for display and to keep track of them
                foreach (var photo in job.Photos)
                {
                    PhotoPaths.Add(photo.PhotoPath);
                }
                // We'll handle adding/deleting photos in a later step if needed.
                // For now, this just displays existing photos and allows adding new ones.
                _tempPhotoItems.AddRange(job.Photos);
            }
        }

        private async Task OnSaveJob()
        {
            if (string.IsNullOrWhiteSpace(JobTitle))
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", "Job Title is required.", "OK");
                return;
            }

            var jobToSave = new Job
            {
                Id = _editingJobId, // If new job, Id is 0. If editing, it's the correct ID.
                Title = JobTitle,
                CustomerName = CustomerName,
                CustomerOrganization = CustomerOrganization,
                PhoneNumber = PhoneNumber,
                Details = Details,
                Photos = new List<PhotoItem>(_tempPhotoItems)
            };

            await _databaseService.SaveJobAsync(jobToSave);

            await Application.Current.MainPage.DisplayAlert("Success", "Job saved successfully!", "OK");

            // After the user sees the success message, reset the ViewModel's state.
            // This ensures that the singleton is clean for the next time it's used.
            ClearForm();
            PageTitle = "Create New Job"; // Reset title back to default
            _editingJobId = 0;            // Reset the editing ID

            // Now, navigate back to the previous page.
            await Shell.Current.GoToAsync("..");
        }

        private void ClearForm()
        {
            JobTitle = string.Empty;
            CustomerName = string.Empty;
            CustomerOrganization = string.Empty;
            PhoneNumber = string.Empty;
            Details = string.Empty;
            PhotoPaths.Clear();
            _tempPhotoItems.Clear();
        }

        // OnAddPhoto method remains the same as before
        private async Task OnAddPhoto(bool fromCamera)
        {
            try
            {
                FileResult photoResult = null;
                if (fromCamera)
                {
                    if (MediaPicker.Default.IsCaptureSupported)
                    {
                        photoResult = await MediaPicker.Default.CapturePhotoAsync();
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("No Camera", "No camera available.", "OK");
                        return;
                    }
                }
                else
                {
                    photoResult = await MediaPicker.Default.PickPhotoAsync();
                }

                if (photoResult != null)
                {
                    var newFilePath = Path.Combine(FileSystem.AppDataDirectory, photoResult.FileName);
                    using (var stream = await photoResult.OpenReadAsync())
                    using (var newStream = File.OpenWrite(newFilePath))
                    {
                        await stream.CopyToAsync(newStream);
                    }

                    PhotoPaths.Add(newFilePath);
                    // Add a NEW PhotoItem. When editing, we only add new ones this way.
                    // A more complex system could handle removing photos from the UI and a "deleted photos" list.
                    _tempPhotoItems.Add(new PhotoItem { PhotoPath = newFilePath });
                }
            }
            catch (PermissionException pEx)
            {
                await Application.Current.MainPage.DisplayAlert("Permission Error", "Photo permissions not granted. Please check app settings.", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
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