using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace ARDroneWPFTestApplication
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CARDrone m_ARDrone;

        private CKinect m_Kinect;

        public MainWindow()
        {
            InitializeComponent();

            m_ARDrone = new CARDrone();
            
            m_Kinect = new CKinect();
        }

        private void btARConnect_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush YellowColor = new SolidColorBrush(Colors.Yellow);
            ARDroneStatusIndicator.Fill = YellowColor;

            m_ARDrone.Connect();

            SolidColorBrush GreenColor = new SolidColorBrush(Colors.Green);
            ARDroneStatusIndicator.Fill = GreenColor;
        }

        private void btARTakeOff_Click(object sender, RoutedEventArgs e)
        {
            m_ARDrone.TakeOff();
        }

        private void btARStop_Click(object sender, RoutedEventArgs e)
        {
            m_ARDrone.Emergency();
        }

        private void btARLand_Click(object sender, RoutedEventArgs e)
        {
            m_ARDrone.Land();
        }

        private void btARPicture_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btARTrim_Click(object sender, RoutedEventArgs e)
        {
            m_ARDrone.Trim();
        }

        private void slARRoll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void slARNick_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void btMKConnect_Click(object sender, RoutedEventArgs e)
        {
            m_Kinect.Connect();

            SolidColorBrush YellowColor = new SolidColorBrush(Colors.Yellow);
            ARDroneStatusIndicator.Fill = YellowColor;
        }

        private void btStart_Click(object sender, RoutedEventArgs e)
        {
            m_Kinect.EnableSkeletonStream();

            SolidColorBrush GreenColor = new SolidColorBrush(Colors.Green);
            ARDroneStatusIndicator.Fill = GreenColor;
        }

        private void btMKStopTrack_Click(object sender, RoutedEventArgs e)
        {
            m_Kinect.DisableSkeletonStream();

            SolidColorBrush YellowColor = new SolidColorBrush(Colors.Yellow);
            ARDroneStatusIndicator.Fill = YellowColor;
        }

        private void btMKPicture_Click(object sender, RoutedEventArgs e)
        {

        }

        private void slMKAngle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
