using PointCloudViewer.Abstract;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace PointCloudViewer.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsViewModel()
        {
            var engineManager = DependencyService.Get<IEngineManager>();
            _backgroundColorsSource = new ObservableCollection<string>(engineManager.GetColors().Select(x => x.Name));

            //Set default values
            if (!_isInitialized)
            {
                LimitFPS = true;
                Resolution = 75;
                DrawDistance = 30;
                LevelOfDetail = 40;
                CameraSpeed = 15;
                ColorQuality = 25;
                ShowConsole = true;
                ShowFPS = true;
                SelectedBackgroundColor = "Black";
                _isInitialized = true;

                
                _isDeviceWithKeyboard = engineManager.IsDeviceWithKeyboard();
            }
        }
        private static bool _isDeviceWithKeyboard;
        private static bool _isInitialized = false;


        private static int _resolution;
        public int Resolution
        {
            get { return _resolution; }
            set
            {
                if (_resolution != value)
                {
                    _resolution = value;
                    OnPropertyChanged("Resolution");
                }
            }
        }

        private static int _drawDistance;
        public int DrawDistance
        {
            get { return _drawDistance; }
            set {
                if (_drawDistance != value)
                {
                    _drawDistance = value;
                    OnPropertyChanged("DrawDistance");
                }
            }
        }

        private static int _colorQuality;
        public int ColorQuality
        {
            get { return _colorQuality; }
            set
            {
                if(_colorQuality!= value)
                {
                    _colorQuality = value;
                    OnPropertyChanged("ColorQuality");
                }
            }
        }

        private static int _cameraSpeed;
        public int CameraSpeed
        {
            get { return _cameraSpeed; }
            set
            {
                if (_cameraSpeed != value)
                {
                    _cameraSpeed = value;
                    OnPropertyChanged("CameraSpeed");
                }
            }
        }

        private static int _levelOfDetail;
        public int LevelOfDetail
        {
            get { return _levelOfDetail; }
            set
            {
                if(_levelOfDetail != value)
                {
                    _levelOfDetail = value;
                    OnPropertyChanged("LevelOfDetail");
                }
            }
        }

        private static bool _limitFPS;
        public bool LimitFPS
        {
            get { return _limitFPS; }
            set
            {
                if(_limitFPS!=value)
                {
                    _limitFPS = value;
                    OnPropertyChanged("LimitFPS");
                }
            }
        }

        private static bool _showConsole;
        public bool ShowConsole
        {
            get { return _showConsole; }
            set
            {
                if (_showConsole != value)
                {
                    _showConsole = value;
                    OnPropertyChanged("ShowConsole");
                }
            }
        }

        private static bool _showFPS;
        public bool ShowFPS
        {
            get { return _showFPS; }
            set
            {
                if(_showFPS != value)
                {
                    _showFPS = value;
                    OnPropertyChanged("ShowFPS");
                }
            }
        }

        private ObservableCollection<string> _backgroundColorsSource;
        public ObservableCollection<string> BackgroundColorsSource
        {
            get
            {
                return _backgroundColorsSource;
            }
        }

        private static string _selectedBackgroundColor;
        public string SelectedBackgroundColor
        {
            get { return _selectedBackgroundColor; }
            set
            {
                if (_selectedBackgroundColor != value)
                {
                    _selectedBackgroundColor = value;
                    OnPropertyChanged("SelectedBackgroundColor");
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }

        public static PointCloudViewer.Domain.Settings GetSettings()
        {
            var engineManager = DependencyService.Get<IEngineManager>();
            if (!_isInitialized)
            {
                return new Domain.Settings { WereInitialized = _isInitialized, IsDeviceWithKeyboard = engineManager.IsDeviceWithKeyboard() };
            }
            var chosenColor = engineManager.GetColors().Single(x => x.Name == _selectedBackgroundColor);
            return new Domain.Settings
            {
                Resolution = _resolution,
                DrawDistance = _drawDistance,
                ColorQuality = _colorQuality,
                CameraSpeed = _cameraSpeed,
                LevelOfDetail = _levelOfDetail,
                LimitFPS = _limitFPS,
                ShowConsole = _showConsole,
                ShowFPS = _showFPS,
                BackgroundR = chosenColor.R,
                BackgroundG = chosenColor.G,
                BackgroundB = chosenColor.B,
                WereInitialized = true,
                IsDeviceWithKeyboard = _isDeviceWithKeyboard
            };
        }
    }
}
