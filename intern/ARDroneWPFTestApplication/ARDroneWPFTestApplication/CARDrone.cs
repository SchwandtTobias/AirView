using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARDroneWPFTestApplication
{
    using ARDrone.Control;
    using ARDrone.Control.Commands;
    using ARDrone.Control.Data;
    using ARDrone.Control.Events;

    using System.ComponentModel;

    class CARDrone
    {
        public void Connect()
        {
            try
            {
                m_DroneController.ConnectToDroneNetworkAndDrone();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public void TakeOff()
        {
            Command TakeOffCommand = new FlightModeCommand(DroneFlightMode.TakeOff);
            m_Commands.Add(TakeOffCommand);
        }

        public void Land()
        {
            Command LandCommand = new FlightModeCommand(DroneFlightMode.Land);
            m_Commands.Add(LandCommand);
        }

        public void Trim()
        {
            Command command = new FlightModeCommand(DroneFlightMode.Reset);
            m_DroneController.SendCommand(command);
        }

        public void Emergency()
        {
            Command command = new FlightModeCommand(DroneFlightMode.Emergency);
            m_DroneController.SendCommand(command);
        }


        private DroneControl m_DroneController;

        private List<Command> m_Commands;

        private BackgroundWorker m_Sender;


        CARDrone()
        {
            try
            {
                m_Commands = new List<Command>();

                m_DroneController = new DroneControl();

                m_DroneController.NetworkConnectionStateChanged += new DroneNetworkConnectionStateChangedEventHandler(NetworkConnectionStateChanged);


                m_Sender = new BackgroundWorker();

                m_Sender.DoWork += new DoWorkEventHandler(SenderDoWork);

                m_Sender.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SenderRunWorkerCompleted);

                m_Sender.RunWorkerAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        private void SenderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            m_Sender.RunWorkerAsync();
        }

        private void SenderDoWork(object sender, DoWorkEventArgs e)
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

        private void NetworkConnectionStateChanged(object sender, DroneNetworkConnectionStateChangedEventArgs e)
        {
            if (e.State == DroneNetworkConnectionState.ConnectedToNetwork)
            {

            }
        }
    }
}
