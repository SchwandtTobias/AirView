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
    using System.Threading;

    public class CARDrone
    {
        // --------------------------------------------------------------------------------
        // Public
        // --------------------------------------------------------------------------------

        public enum State
        {
            Connected = 0,
            Disconnected,
            Hover,
            Fly,
            Land,
            Error,
            CountOfStates,
            Undifined = -1
        };

        public State ActualState
        {
            get { return m_ActualState; }
        }

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

        public void SetIpAddress(String _OwnIPAddress, String _ARIPAddress)
        {
            m_OwnIPAddress = _OwnIPAddress;
            m_ARIPAddress = _ARIPAddress;
        }

        #region FlyModeCommands

        public void TakeOff()
        {
            if (!m_DroneController.IsConnected) return;
            if (m_ActualState == State.Fly) return;
            if (m_ActualState == State.Error) return;

            m_Commands.Clear();

            Command TakeOffCommand = new FlightModeCommand(DroneFlightMode.TakeOff);

            m_DroneController.SendCommand(TakeOffCommand);

            m_ActualState = State.Hover;

        }

        public void Land()
        {
            if (!m_DroneController.IsFlying) return;
            if (m_ActualState == State.Land) return;
            if (m_ActualState == State.Error) return;

            m_Commands.Clear();

            Command LandCommand = new FlightModeCommand(DroneFlightMode.Land);

            m_DroneController.SendCommand(LandCommand);

            m_ActualState = State.Land;
        }

        public void Trim()
        {
            if (m_ActualState == State.Error) return;

            m_Commands.Clear();

            Command command = new FlightModeCommand(DroneFlightMode.Reset);

            m_DroneController.SendCommand(command);

            m_ActualState = State.Land;
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
            if (!m_DroneController.IsFlying) return;
            if (m_ActualState == State.Error) return;

            m_ListLock.WaitOne(1000);
            
            System.Console.WriteLine("Roll: " + _Roll + " Nick: "+ _Pitch + " Yaw: " + _Yaw);

            m_CurrentCommand = new FlightMoveCommand(_Roll, _Pitch, _Yaw, _Gaz);

            m_ListLock.ReleaseMutex();
        }

        public void Pitch(float _Direction = 1.0f)
        {
            if (!m_DroneController.IsFlying) return;
            if (m_ActualState == State.Error) return;

            Command command = new FlightMoveCommand(0.0f, _Direction * 0.5f, 0.0f, 0.0f);
            AddCommand(command);
        }

        public void Roll(float _Direction = 1.0f)
        {
            if (!m_DroneController.IsFlying) return;
            if (m_ActualState == State.Error) return;

            Command command = new FlightMoveCommand(_Direction * 0.5f, 0.0f, 0.0f, 0.0f);
            AddCommand(command);
        }

        public void Yaw(float _Direction = 1.0f)
        {
            if (!m_DroneController.IsFlying) return;
            if (m_ActualState == State.Error) return;

            Command command = new FlightMoveCommand(0.0f, 0.0f, _Direction * 0.5f, 0.0f);
            AddCommand(command);
        }

        public void Gaz(float _Direction = 1.0f)
        {
            if (!m_DroneController.IsFlying) return;
            if (m_ActualState == State.Error) return;

            Command command = new FlightMoveCommand(0.0f, 0.0f, 0.0f, _Direction * 0.1f);
            AddCommand(command);
        }
        #endregion

        // --------------------------------------------------------------------------------
        // Private
        // --------------------------------------------------------------------------------

        private DroneControl        m_DroneController;

        private List<Command>       m_Commands;

        private BackgroundWorker    m_Sender;

        private static Mutex        m_ListLock;

        private State               m_ActualState;

        private DateTime            m_LastUpateTime;

        private long                m_UpdateInterval;

        private string              m_OwnIPAddress;

        private string              m_ARIPAddress;

        private string              m_RouterName;

        private int                 m_NumberOfCommands;

        private const int           c_MaxNumberOfCommands = 2;

        private Command             m_CurrentCommand;


        public CARDrone()
        {
            m_ListLock = new Mutex();

            m_ActualState = State.Disconnected;

            m_LastUpateTime = new DateTime();

            m_UpdateInterval = 100;

            m_NumberOfCommands = 0;

            m_RouterName = "ardrone_";

            m_CurrentCommand = null;

            try
            {
                m_Commands = new List<Command>(c_MaxNumberOfCommands);

                DroneConfig RouterConfig = new DroneConfig();

                RouterConfig.DroneNetworkIdentifierStart = m_RouterName;

                m_DroneController = new DroneControl();
                
                m_DroneController.NetworkConnectionStateChanged += new DroneNetworkConnectionStateChangedEventHandler(NetworkConnectionStateChanged);

                m_DroneController.Error += new DroneErrorEventHandler(DroneError);


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

        ~CARDrone()
        {
            if (m_ActualState != State.Error && m_ActualState != State.Land)
            {
                m_ActualState = State.Disconnected;
                Land();
            }
            m_DroneController.Disconnect();
        }

        private void DroneError(object sender, DroneErrorEventArgs e)
        {
            m_ActualState = State.Error;
            Emergency();
        }

        private void SenderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            m_Sender.RunWorkerAsync();
        }

        private void SenderDoWork(object sender, DoWorkEventArgs e)
        {
            long TimeBetweenUpdates = (DateTime.Now.Ticks - m_LastUpateTime.Ticks) / TimeSpan.TicksPerMillisecond;


            if (TimeBetweenUpdates > m_UpdateInterval && m_ActualState != State.Land || m_ActualState != State.Disconnected)
            {
                m_LastUpateTime = DateTime.Now;

                m_ListLock.WaitOne(1000);

                if (m_CurrentCommand != null)
                {
                    m_ActualState = State.Fly;

                    //foreach (Command CurrentCommand in m_Commands)
                    //{
                    //    m_DroneController.SendCommand(CurrentCommand);
                    //}

                    //m_Commands.Clear();

                    m_DroneController.SendCommand(m_CurrentCommand);

                    m_CurrentCommand = null;
                }
                else
                {
                    m_ActualState = State.Hover;
                }

                m_NumberOfCommands = 0;

                m_ListLock.ReleaseMutex();
            }
        }

        private void NetworkConnectionStateChanged(object sender, DroneNetworkConnectionStateChangedEventArgs e)
        {
            if (e.State == DroneNetworkConnectionState.ConnectedToNetwork)
            {
                m_ActualState = State.Connected;
            }
            else if (e.State == DroneNetworkConnectionState.NotConnected)
            {
                m_ActualState = State.Disconnected;
            }
        }

        private void AddCommand(Command _Command)
        {
            m_ListLock.WaitOne(1000);

            int IndeOfCommand = m_NumberOfCommands % c_MaxNumberOfCommands;

            m_Commands.Insert(IndeOfCommand, _Command);

            ++ m_NumberOfCommands;

            m_ListLock.ReleaseMutex();
        }

    }
}
