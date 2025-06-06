using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Web; // Needed for HttpUtility

namespace EngravingJobManager.ViewModels
{
    // This attribute receives the 'PhotoPath' parameter from navigation
    [QueryProperty(nameof(PhotoPathUrl), "PhotoPathUrl")]
    public class PhotoDetailViewModel : INotifyPropertyChanged
    {
        private string _photoSource;

        // The UI will bind the Image source to this property
        public string PhotoSource
        {
            get => _photoSource;
            set => SetProperty(ref _photoSource, value);
        }

        // This property receives the URL-encoded path from the navigation query
        public string PhotoPathUrl
        {
            set
            {
                // We must decode the path because file paths can contain characters
                // that are not allowed in a URI and were encoded for navigation.
                string decodedPath = HttpUtility.UrlDecode(value);
                PhotoSource = decodedPath;
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