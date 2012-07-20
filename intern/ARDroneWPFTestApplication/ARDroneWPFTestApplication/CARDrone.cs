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

    public class CARDrone
    {
        public const int s_MaxSavedCommands = 5;

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

        #region FlyModeCommands
        public void TakeOff()
        {
            if (m_Commands.Count >= s_MaxSavedCommands) return;
            if (!m_DroneController.IsConnected) return;

            Command TakeOffCommand = new FlightModeCommand(DroneFlightMode.TakeOff);
            m_Commands.Add(TakeOffCommand);
        }

        public void Land()
        {
            if (m_Commands.Count >= s_MaxSavedCommands) return;
            if (!m_DroneController.IsFlying) return;

            Command LandCommand = new FlightModeCommand(DroneFlightMode.Land);
            m_Commands.Add(LandCommand);
        }

        public void Trim()
        {
            m_Commands.Clear();

            Command command = new FlightModeCommand(DroneFlightMode.Reset);
            m_DroneController.SendCommand(command);
        }

        public void Emergency()
        {
            m_Commands.Clear();

            Command command = new FlightModeCommand(DroneFlightMode.Emergency);
            m_DroneController.SendCommand(command);
        }
        #endregion

        #region FlyMoveCommands
        public void Fly(float _Roll = 0.0f, float _Pitch = 0.0f, float _Yaw = 0.0f, float _Gaz = 0.0f)
        {
            if (m_Commands.Count >= s_MaxSavedCommands) return;
            if (!m_DroneController.IsFlying) return;

            Command command = new FlightMoveCommand(_Roll, _Pitch, _Yaw, _Gaz);
            m_Commands.Add(command);
        }

        public void Pitch(float _Direction = 1.0f)
        {
            if (m_Commands.Count >= s_MaxSavedCommands) return;
            if (!m_DroneController.IsFlying) return;

            Command command = new FlightMoveCommand(0.0f, _Direction * 5.0f, 0.0f, 0.0f);
            m_Commands.Add(command);
        }

        public void Roll(float _Direction = 1.0f)
        {
            if (m_Commands.Count >= s_MaxSavedCommands) return;
            if (!m_DroneController.IsFlying) return;

            Command command = new FlightMoveCommand(_Direction * 5.0f, 0.0f, 0.0f, 0.0f);
            m_Commands.Add(command);
        }

        public void Yaw(float _Direction = 1.0f)
        {
            if (m_Commands.Count >= s_MaxSavedCommands) return;
            if (!m_DroneController.IsFlying) return;

            Command command = new FlightMoveCommand(0.0f, 0.0f, _Direction * 5.0f, 0.0f);
            m_Commands.Add(command);
        }

        public void Gaz(float _Direction = 1.0f)
        {
            if (m_Commands.Count >= s_MaxSavedCommands) return;
            if (!m_DroneController.IsFlying) return;

            Command command = new FlightMoveCommand(0.0f, 0.0f, 0.0f, _Direction * 5.0f);
            m_Commands.Add(command);
        }
        #endregion


        private DroneControl m_DroneController;

        private List<Command> m_Commands;

        private BackgroundWorker m_Sender;


        public CARDrone()
        {
            try
            {
                m_Commands = new List<Command>(s_MaxSavedCommands);

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
