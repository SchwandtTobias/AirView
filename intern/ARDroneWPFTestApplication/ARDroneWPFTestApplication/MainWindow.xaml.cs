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
using System.Drawing;
using System.IO;
using System.ComponentModel;


namespace ARDroneWPFTestApplication
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CARDrone         m_ARDrone;

        private CKinect          m_Kinect;

        private BackgroundWorker m_Logger;

        public MainWindow()
        {
            InitializeComponent();

            m_ARDrone = new CARDrone();
            
            m_Kinect = new CKinect( m_ARDrone );

            m_Logger = new BackgroundWorker();

            m_Logger.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LoggerRunWorkerCompleted);

            m_Logger.RunWorkerAsync();
        }

        void LoggerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TextBoxLog.Text = m_Kinect.GetLogs();

            m_Logger.RunWorkerAsync();
        }

        private void btARConnect_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush YellowColor = new SolidColorBrush(Colors.Yellow);
            ARDroneStatusIndicator.Fill = YellowColor;

            m_ARDrone.SetIpAddress(tbOwnIPAddress.Text, tbARIPAddress.Text);

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
            KinectStatusIndicator.Fill = YellowColor;

            btStart.IsEnabled = true;
            btMKDisconnect.IsEnabled = true;
            btMKConnect.IsEnabled = false;
            btMKPicture.IsEnabled = true;
        }

        private void btStart_Click(object sender, RoutedEventArgs e)
        {
            m_Kinect.EnableSkeletonStream();

            SolidColorBrush GreenColor = new SolidColorBrush(Colors.Green);
            KinectStatusIndicator.Fill = GreenColor;

            btStart.IsEnabled = false;
            btMKStopTrack.IsEnabled = true;
        }

        private void btMKStopTrack_Click(object sender, RoutedEventArgs e)
        {
            m_Kinect.DisableSkeletonStream();

            SolidColorBrush YellowColor = new SolidColorBrush(Colors.Yellow);
            KinectStatusIndicator.Fill = YellowColor;

            btStart.IsEnabled = true;
            btMKStopTrack.IsEnabled = false;
        }

        private void btMKPicture_Click(object sender, RoutedEventArgs e)
        {

            imgMKSkeletonBox.Source = m_Kinect.GetSkeletonPictureContext();

        }

        private void slMKAngle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_Kinect.ChangeViewAngle((int)(e.NewValue));
        }

        private void btMKDisconnect_Click(object sender, RoutedEventArgs e)
        {
            m_Kinect.Disconnect();

            SolidColorBrush RedColor = new SolidColorBrush(Colors.Red);
            KinectStatusIndicator.Fill = RedColor;

            btMKDisconnect.IsEnabled = false;
            btMKConnect.IsEnabled = true;
            btStart.IsEnabled = false;
            btMKPicture.IsEnabled = false;
        }

        private void tbOwnIPAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbOwnIPAddress.Text == "")
                tbOwnIPAddress.Text = "Own IP-Address";
        }

        private void tbOwnIPAddress_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tbOwnIPAddress.Text == "Own IP-Address")
                tbOwnIPAddress.Text = "192.168.1.2";
        }

        private void tbARIPAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbARIPAddress.Text == "")
                tbARIPAddress.Text = "AR IP-Address";
        }

        private void tbARIPAddress_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tbARIPAddress.Text == "AR IP-Address")
                tbARIPAddress.Text = "192.168.1.1";
        }
    }
}
