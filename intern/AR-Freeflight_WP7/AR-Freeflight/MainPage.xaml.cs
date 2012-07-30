#define disableWifiCheck // This is only for testing; Undefine it before release

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using DroneController;

namespace AR_Freeflight
{
    public partial class MainPage : PhoneApplicationPage
    {
   

       // Konstruktor
        public MainPage()
        {
            InitializeComponent();
       }

        private void navigateToControlpage_Click(object sender, RoutedEventArgs e)
        {

#if disableWifiCheck
            navigateToControlpage.Content = "Loading..";
            NavigationService.Navigate(new Uri("/ControlPage.xaml", UriKind.Relative));
            return;
#endif

            if (DeviceNetworkInformation.IsWiFiEnabled && DeviceNetworkInformation.IsNetworkAvailable)
            {
                
            navigateToControlpage.Content = "Loading..";
            NavigationService.Navigate(new Uri("/ControlPage.xaml", UriKind.Relative));
            
			} else {
                testblock.Text = "Sie sind mit keinem Wifi verbunden.";
			}

        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            
            if (navigateToControlpage.Content.ToString() == "Loading..")
            {
                navigateToControlpage.Content = "Fliegen";
            }
            base.OnNavigatedFrom(e);
           
        }

		protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            

            if (e.Orientation == PageOrientation.LandscapeRight)
            {
                return;//base.OnOrientationChanged(e);
            }
            base.OnOrientationChanged(e);
        }
    }
}