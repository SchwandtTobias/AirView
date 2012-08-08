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
        private CARDrone m_ARDrone;

        private CKinect m_Kinect;

        private BackgroundWorker m_Logger;

        private BackgroundWorker m_EnergyLevel;

        private BackgroundWorker m_StartEngine;

        private DateTime m_LastUpateTime;

        private long m_UpdateInterval;

        public MainWindow()
        {
            InitializeComponent();

            m_LastUpateTime = new DateTime();

            m_UpdateInterval = 500;

            m_ARDrone = new CARDrone();

            m_Kinect = new CKinect(m_ARDrone);

            m_Logger = new BackgroundWorker();

            m_Logger.DoWork += new DoWorkEventHandler(LoggerDoWork);

            m_Logger.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LoggerRunWorkerCompleted);

            m_Logger.RunWorkerAsync();

            m_StartEngine = new BackgroundWorker();

            m_StartEngine.RunWorkerCompleted += new RunWorkerCompletedEventHandler(StartEngineRunWorkerCompleted);

            m_StartEngine.RunWorkerAsync();

            m_EnergyLevel = new BackgroundWorker();

            m_EnergyLevel.RunWorkerCompleted += new RunWorkerCompletedEventHandler(EnergyLevelRunWorkerCompleted);

            m_EnergyLevel.DoWork += new DoWorkEventHandler(EnergyLevelDoWork);

            m_EnergyLevel.RunWorkerAsync();
        }

        ~MainWindow()
        {
            m_ARDrone.Disconnect();

            m_Kinect.DisableSkeletonStream();

            m_Kinect.Disconnect();
        }

        void EnergyLevelDoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(2000);
        }

        void EnergyLevelRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            float BatteryLevel = m_ARDrone.EnergyLevel();
            
            pbARBatteryLevel.Value = BatteryLevel;

            m_EnergyLevel.RunWorkerAsync();
        }

        private void StartEngineRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Connect AR.Drone
            ConnectARDrone();

            // Connect MK
            ConnectMK();

            // Start Tracking
            StartSkeletonTrackingMK();
        }

        private void LoggerDoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(1000);
        }

        private void LoggerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TextBoxLog.Text = m_Kinect.GetLogs();

            m_Logger.RunWorkerAsync();
        }

        private void btARConnect_Click(object sender, RoutedEventArgs e)
        {
            ConnectARDrone();
        }

        private void ConnectARDrone()
        {
            SolidColorBrush YellowColor = new SolidColorBrush(Colors.Yellow);
            ARDroneStatusIndicator.Fill = YellowColor;

            if (tbOwnIPAddress.Text == "")
            {
                m_ARDrone.SetIpAddress("192.168.1.2", tbARIPAddress.Text);
            }
            else
            {
                m_ARDrone.SetIpAddress(tbOwnIPAddress.Text, tbARIPAddress.Text);
            }

            m_ARDrone.Connect();

            SolidColorBrush GreenColor = new SolidColorBrush(Colors.Green);
            ARDroneStatusIndicator.Fill = GreenColor;

            btARConnect.IsEnabled = false;
            btARDisconnet.IsEnabled = true;
            btARTakeOff.IsEnabled = true;
            btARLand.IsEnabled = true;
            btARStop.IsEnabled = true;
            btARTrim.IsEnabled = true;
        }

        private void btARDisconnet_Click(object sender, RoutedEventArgs e)
        {
            DisconnectARDrone();
        }

        private void DisconnectARDrone()
        {
            SolidColorBrush YellowColor = new SolidColorBrush(Colors.Red);
            ARDroneStatusIndicator.Fill = YellowColor;

            m_ARDrone.Fly();

            m_ARDrone.Land();

            m_ARDrone.Disconnect();

            btARConnect.IsEnabled = true;
            btARDisconnet.IsEnabled = false;
            btARTakeOff.IsEnabled = false;
            btARLand.IsEnabled = false;
            btARStop.IsEnabled = false;
            btARTrim.IsEnabled = false;
        }

        private void btARTakeOff_Click(object sender, RoutedEventArgs e)
        {
            TakeOffARDrone();
        }

        private void TakeOffARDrone()
        {
            m_ARDrone.TakeOff();

            slARNick.IsEnabled = true;
            slARRoll.IsEnabled = true;
        }

        private void btARStop_Click(object sender, RoutedEventArgs e)
        {
            StopARDrone();
        }

        private void StopARDrone()
        {
            m_ARDrone.Emergency();
        }

        private void btARLand_Click(object sender, RoutedEventArgs e)
        {
            LandARDrone();
        }

        private void LandARDrone()
        {
            m_ARDrone.Land();

            slARNick.IsEnabled = false;
            slARRoll.IsEnabled = false;
        }

        private void btARPicture_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btARTrim_Click(object sender, RoutedEventArgs e)
        {
            TrimARDrone();
        }

        private void TrimARDrone()
        {
            m_ARDrone.Trim();
        }

        private void slARRoll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_ARDrone.Fly((float)slARNick.Value, (float)slARRoll.Value, 0.0f, 0.0f);
        }

        private void slARNick_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_ARDrone.Fly((float)slARNick.Value, (float)slARRoll.Value, 0.0f, 0.0f);
        }

        private void btMKConnect_Click(object sender, RoutedEventArgs e)
        {
            ConnectMK();
        }

        private void ConnectMK()
        {
            m_Kinect.Connect();

            SolidColorBrush YellowColor = new SolidColorBrush(Colors.Yellow);
            KinectStatusIndicator.Fill = YellowColor;

            btStart.IsEnabled = true;
            btMKDisconnect.IsEnabled = true;
            btMKConnect.IsEnabled = false;
            btMKPicture.IsEnabled = true;
            slMKAngle.IsEnabled = true;
        }

        private void btStart_Click(object sender, RoutedEventArgs e)
        {
            StartSkeletonTrackingMK();
        }

        private void StartSkeletonTrackingMK()
        {
            m_Kinect.EnableSkeletonStream();

            SolidColorBrush GreenColor = new SolidColorBrush(Colors.Green);
            KinectStatusIndicator.Fill = GreenColor;

            btStart.IsEnabled = false;
            btMKStopTrack.IsEnabled = true;
        }

        private void btMKStopTrack_Click(object sender, RoutedEventArgs e)
        {
            StopSkeletonTrackingMK();
        }

        private void StopSkeletonTrackingMK()
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
            DisconnectMK();
        }

        private void DisconnectMK()
        {
            if (btMKStopTrack.IsEnabled)
            {
                m_Kinect.DisableSkeletonStream();
                btMKStopTrack.IsEnabled = false;
            }

            m_Kinect.Disconnect();

            SolidColorBrush RedColor = new SolidColorBrush(Colors.Red);
            KinectStatusIndicator.Fill = RedColor;

            btMKDisconnect.IsEnabled = false;
            btMKConnect.IsEnabled = true;
            btStart.IsEnabled = false;
            btMKPicture.IsEnabled = false;
            slMKAngle.IsEnabled = false;
        }

        private void TextBoxLog_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            svScrollView.ScrollToEnd();
        }

        private void slARNick_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            slARNick.Value = 0.0f;
        }

        private void slARRoll_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            slARRoll.Value = 0.0f;
        }
    }
}