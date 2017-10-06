using PointCloudViewer.Abstract;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace PointCloudViewer.ViewModel
{
    class ChoosePointCloudViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand StartVisualizationCommand { get; private set; }
        public ICommand SetSelectedPointCloudCommand { get; private set; }

        private string _selectedPointCloud;

        public ChoosePointCloudViewModel()
        {
            StartVisualizationCommand = new Command(() =>
            {
                if (string.IsNullOrEmpty(_selectedPointCloud))
                {
                    App.Current.MainPage.DisplayAlert("Alert", "First choose the point cloud to visualize.", "OK");
                }
                else
                {
                    var engineManager = DependencyService.Get<IEngineManager>();
                    engineManager.StartVisualization(_selectedPointCloud);
                }
            });
            SetSelectedPointCloudCommand = new Command<SelectedItemChangedEventArgs>((SelectedItemChangedEventArgs obj) =>
            {
                _selectedPointCloud = obj.SelectedItem.ToString();
            });
        }

        public ObservableCollection<string> LocalPointClouds
        {
            get
            {
                //this should be made dynamic in real application
                return new ObservableCollection<string>() { "Agricultural", "City"};
            }
        }
    }
}
