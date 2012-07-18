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
using Microsoft.Kinect;


using ARDrone.Control;
using ARDrone.Control.Commands;
using ARDrone.Control.Data;
using ARDrone.Control.Events;
using System.ComponentModel;


namespace ARDroneWPFTestApplication
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private DroneControl    m_DroneController;
        private KinectSensor m_KinectSensor;
        private Skeleton[] m_CurrentSkeletons;

        private List<Command>    m_Commands;
        private List<String>     m_Logs;
        private BackgroundWorker m_Sender;
        private BackgroundWorker m_Logger;

        public MainWindow()
        {
            
            m_Commands = new List<Command>();
            
            m_Logs     = new List<String>();

            try
            {
                m_DroneController = new DroneControl();

                m_DroneController.ConnectToDroneNetworkAndDrone();
                
                m_DroneController.NetworkConnectionStateChanged += new DroneNetworkConnectionStateChangedEventHandler(drone_NetworkConnectionStateChanged);

    
                m_KinectSensor = KinectSensor.KinectSensors[0];

                m_KinectSensor.Start();
                
                m_KinectSensor.SkeletonStream.Enable();
                
                m_KinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(m_KinectSensor_SkeletonFrameReady);


                m_Sender = new BackgroundWorker();

                m_Sender.DoWork += new DoWorkEventHandler(SenderDoWork);

                m_Sender.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SenderRunWorkerCompleted);

                m_Sender.RunWorkerAsync();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace); 
            }

            InitializeComponent();
        }

        void SenderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            m_Sender.RunWorkerAsync();
        }

        void SenderDoWork(object sender, DoWorkEventArgs e)
        {
            if (m_Commands.Count > 0)
            {
                foreach (Command CurrentCommand in m_Commands)
                {
                    m_DroneController.SendCommand(CurrentCommand);
                }

                m_Commands.Clear();
            }
        }

        void m_KinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame CurrentSkeletonFrame = e.OpenSkeletonFrame();
            if (CurrentSkeletonFrame != null && CurrentSkeletonFrame.SkeletonArrayLength > 0)
            {    
                m_CurrentSkeletons = new Skeleton[CurrentSkeletonFrame.SkeletonArrayLength];

                CurrentSkeletonFrame.CopySkeletonDataTo(m_CurrentSkeletons);

                foreach (Skeleton CurrentSkeleton in m_CurrentSkeletons)
                {
                  
                }

            }
        }

        void drone_NetworkConnectionStateChanged(object sender, DroneNetworkConnectionStateChangedEventArgs e)
        {
            if (e.State == DroneNetworkConnectionState.ConnectedToNetwork)
            {
                
            }
        }

        void recorder_CompressionComplete(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            Command TakeOffCommand = new FlightModeCommand(DroneFlightMode.TakeOff);
            m_Commands.Add(TakeOffCommand);
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            Command LandCommand = new FlightModeCommand(DroneFlightMode.Land);
            m_Commands.Add(LandCommand);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Command command = new FlightMoveCommand(0.1f, 0, 0, 0);
            m_DroneController.SendCommand(command);
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            Command command = new FlightMoveCommand(0.0f, 0, 0, 0);
            m_DroneController.SendCommand(command);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Command command = new FlightModeCommand(DroneFlightMode.Reset);
            m_DroneController.SendCommand(command);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            Command command = new FlightModeCommand(DroneFlightMode.Emergency);
            m_DroneController.SendCommand(command);
        }
    }
}
