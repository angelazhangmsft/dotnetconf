using System;
using System.Windows;
using Windows.Devices.Geolocation;

namespace GeolocationDemoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void LocationButton_Click(object sender, RoutedEventArgs e)
        {
            // Display location coordinates using Geolocator
            var locator = new Geolocator();
            var location = await locator.GetGeopositionAsync();
            var position = location.Coordinate.Point.Position;
            var latlong = string.Format("lat:{0}, long:{1}", position.Latitude, position.Longitude);
            LocationText.Text = latlong;
        }
    }
}







//private async void DisplayMonitorButton_Click(object sender, RoutedEventArgs e)
//{
//    // Collect list of displays
//    var deviceInformations = await DeviceInformation.FindAllAsync(DisplayMonitor.GetDeviceSelector());
//    foreach (DeviceInformation device in deviceInformations)
//    {
//        DisplayMonitor displayMonitor = await DisplayMonitor.FromInterfaceIdAsync(device.Id);

//        // Use light-up behavior to display Dolby vision supported property
//        string dolbyVision = "";
//        if (ApiInformation.IsPropertyPresent("Windows.Devices.Display.DisplayMonitor", "IsDolbyVisionSupportedInHdrMode"))
//        {
//            // Introduced in 19041
//            dolbyVision = displayMonitor.IsDolbyVisionSupportedInHdrMode.ToString();
//        }

//        // Display monitor info
//        MonitorText.Text += ("DisplayName: " + displayMonitor.DisplayName + "\n" +
//                            "ConnectionKind: " + displayMonitor.ConnectionKind + "\n" +
//                            "DolbyVisionSupport: " + dolbyVision + "\n");
//    }
//}