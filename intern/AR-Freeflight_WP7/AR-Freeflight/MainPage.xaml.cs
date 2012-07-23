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

namespace AR_Freeflight
{
    public partial class MainPage : PhoneApplicationPage
    {


        // Konstruktor
        public MainPage()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            

           
        }

        private void navigateToControlpage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ControlPage.xaml", UriKind.Relative));
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