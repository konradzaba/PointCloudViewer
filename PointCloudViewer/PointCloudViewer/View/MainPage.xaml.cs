using System;
using Xamarin.Forms;

namespace PointCloudViewer.View
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnChoosePointCloudButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new StartVisualizationPage());
        }

        private async void OnOptionsButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }
    }
}
