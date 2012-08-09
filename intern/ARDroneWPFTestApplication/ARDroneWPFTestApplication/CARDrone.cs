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

    using System.Configuration;

    public class CARDrone
    {
        // --------------------------------------------------------------------------------
        // Const
        // --------------------------------------------------------------------------------

        const int SEND_INTERVAL = 100;


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

        public void Disconnect()
        {
            m_DroneController.Disconnect();
        }

        public void SetIpAddress(String _OwnIPAddress, String _ARIPAddress)
        {
            m_OwnIPAddress = _OwnIPAddress;
            m_ARIPAddress = _ARIPAddress;
        }

        #region CameraCommands

        public int VideoStream(out System.Drawing.Bitmap _Stream)
        {
            _Stream = null;

            if (m_DroneController.CanSwitchCamera && !m_IsCameraOnline)
            {
                SwitchCameraCommand CameraCommand = new SwitchCameraCommand(DroneCameraMode.FrontCamera);

                m_IsCameraOnline = true;

                return 1;
            } 
            else if (m_IsCameraOnline && m_DroneController.BitmapImage != null)
            {
                _Stream = m_DroneController.BitmapImage;

                return 0;
            }
            return -1;
        }

        #endregion

        #region ConfigurationCommands

        private void SetMaxAngle(string _Angle = "0.1")
        {
            SetConfigurationCommand ConfCommand = new SetConfigurationCommand("CONTROL:euler_angle_max", _Angle);

            m_DroneController.SendCommand(ConfCommand);
        }

        private void SetVerticalSpeed(string _VerticalSpeed = "200.00000")
        {
            SetConfigurationCommand ConfCommand = new SetConfigurationCommand("CONTROL:control_vz_max", _VerticalSpeed);

            m_DroneController.SendCommand(ConfCommand);
        }

        #endregion

        #region FlyModeCommands

        public void TakeOff()
        {
            if (!m_DroneController.IsConnected) return;
            if (m_ActualState == State.Fly) return;
            if (m_ActualState == State.Error) return;

            //Trim();

            SetMaxAngle(Properties.Settings.Default.MaxAngleAR);

            SetVerticalSpeed(Properties.Settings.Default.MaxVerticalSpeed);

            Command TakeOffCommand = new FlightModeCommand(DroneFlightMode.TakeOff);

            m_DroneController.SendCommand(TakeOffCommand);

            m_ActualState = State.Hover;
        }

        public void Land()
        {
            if (!m_DroneController.IsFlying) return;
            if (m_ActualState == State.Land) return;

            Command LandCommand = new FlightModeCommand(DroneFlightMode.Land);

            m_DroneController.SendCommand(LandCommand);

            m_ActualState = State.Land;
        }

        public void Trim()
        {
            if (m_ActualState == State.Error) return;

            Command command = new FlightModeCommand(DroneFlightMode.Reset);

            m_DroneController.SendCommand(command);

            m_ActualState = State.Land;
        }

        public void Emergency()
        {
            Command command = new FlightModeCommand(DroneFlightMode.Emergency);

            m_DroneController.SendCommand(command);

        }

        public float EnergyLevel()
        {
            return (float)m_DroneController.NavigationData.BatteryLevel;
        }
        #endregion

        #region FlyMoveCommands

        public void Fly(float _Roll = 0.0f, float _Pitch = 0.0f, float _Yaw = 0.0f, float _Gaz = 0.0f)
        {
            if (!m_DroneController.IsFlying) return;
            if (m_ActualState == State.Error) return;

            m_ListLock.WaitOne(1000);
            
            //System.Console.WriteLine("Roll: " + _Roll + " Nick: "+ _Pitch + " Yaw: " + _Yaw);

            m_CurrentCommand = new FlightMoveCommand(_Pitch, _Roll, _Yaw, _Gaz);

            m_ListLock.ReleaseMutex();
        }

        #endregion

        // --------------------------------------------------------------------------------
        // Private
        // --------------------------------------------------------------------------------

        private DroneControl        m_DroneController;

        private BackgroundWorker    m_Sender;

        private static Mutex        m_ListLock;

        private State               m_ActualState;

        private string              m_OwnIPAddress;

        private string              m_ARIPAddress;

        private string              m_RouterName;

        private Command             m_CurrentCommand;

        private bool                m_IsCameraOnline;


        public CARDrone()
        {
            m_ListLock = new Mutex();

            m_ActualState = State.Disconnected;

            m_RouterName = "ardrone_";

            m_CurrentCommand = null;

            m_IsCameraOnline = false;

            try
            {
                DroneConfig RouterConfig = new DroneConfig();

                RouterConfig.StandardOwnIpAddress = m_OwnIPAddress;

                RouterConfig.DroneNetworkIdentifierStart = m_RouterName;

                m_DroneController = new DroneControl(RouterConfig);
                
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
            Land();

            Disconnect();
        }

        private void DroneError(object sender, DroneErrorEventArgs e)
        {
            m_ActualState = State.Error;

            Emergency();
        }

        private void SenderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (m_ActualState == State.Hover || m_ActualState == State.Fly)
            {
                m_ListLock.WaitOne(1000);

                if (m_CurrentCommand != null)
                {
                    m_ActualState = State.Fly;

                    if (m_DroneController.IsCommandPossible(m_CurrentCommand))
                    {
                        m_DroneController.SendCommand(m_CurrentCommand);
                    }

                    m_CurrentCommand = null;
                }
                else
                {
                    m_ActualState = State.Hover;
                }

                m_ListLock.ReleaseMutex();
            }

            // restart background worker
            m_Sender.RunWorkerAsync();
        }

        private void SenderDoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(SEND_INTERVAL);
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
    }
}
