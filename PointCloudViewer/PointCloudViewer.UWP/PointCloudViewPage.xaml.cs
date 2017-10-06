using PointCloudViewer.Engine;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PointCloudViewer.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PointCloudViewPage : Page
    {
        Engine.Engine _engine;
        public PointCloudViewPage()
        {
            this.InitializeComponent();
            
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var settings = e.Parameter as Domain.Settings;
            EngineSettings.Instance.ReadSettings(settings);
            _engine = MonoGame.Framework.XamlGame<Engine.Engine>.Create(string.Empty, Window.Current.CoreWindow, swapChainPanel);
        }
    }
}
