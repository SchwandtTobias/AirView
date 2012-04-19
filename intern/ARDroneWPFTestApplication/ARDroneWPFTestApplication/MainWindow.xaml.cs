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



using ARDrone.Control;
using ARDrone.Control.Commands;
using ARDrone.Control.Data;
using ARDrone.Control.Events;


namespace ARDroneWPFTestApplication
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DroneControl drone;


        public MainWindow()
        {
            drone = new DroneControl();

            drone.ConnectToDroneNetworkAndDrone();
            drone.NetworkConnectionStateChanged += new DroneNetworkConnectionStateChangedEventHandler(drone_NetworkConnectionStateChanged);

            InitializeComponent();
        }

        void drone_NetworkConnectionStateChanged(object sender, DroneNetworkConnectionStateChangedEventArgs e)
        {
            if (e.State == DroneNetworkConnectionState.ConnectedToNetwork)
            {
                Console.WriteLine("CONNECTED");
            }
        }

        void recorder_CompressionComplete(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            Command comand2 = new FlightModeCommand(DroneFlightMode.TakeOff);
            drone.SendCommand(comand2);
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            Command command = new FlightModeCommand(DroneFlightMode.Land);
            drone.SendCommand(command);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Command command = new FlightMoveCommand(0.1f, 0, 0, 0);
            drone.SendCommand(command);
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            Command command = new FlightMoveCommand(0.0f, 0, 0, 0);
            drone.SendCommand(command);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Command command = new FlightModeCommand(DroneFlightMode.Reset);
            drone.SendCommand(command);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            Command command = new FlightModeCommand(DroneFlightMode.Emergency);
            drone.SendCommand(command);
        }
    }
}
