using PointCloudViewer.Domain;

namespace PointCloudViewer.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            LoadApplication(new PointCloudViewer.App());
        }

        public void NavigateToVisualization(Settings settings)
        {
            this.Frame.Navigate(typeof(PointCloudViewPage), settings);
        }
    }
}
