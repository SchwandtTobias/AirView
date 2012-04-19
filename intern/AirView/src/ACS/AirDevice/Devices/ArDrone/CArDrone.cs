using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ACS
{
    using ARDrone.Control;
    using ARDrone.Control.Commands;
    using ARDrone.Control.Events;

    public class CArDrone : IAirDevice
    {
        private ARDrone.Control.DroneControl m_device;

        /// <summary>
        /// Starts and initialize device
        /// </summary>
        public void OnStart()
        {
            //init machine
            m_device = new DroneControl();
            m_device.ConnectToDroneNetworkAndDrone();
        }

        /// <summary>
        /// Method for destroying object
        /// </summary>
        public void OnExit()
        {
            //turn off device
            TurnOff();

            //delte space
            m_device = null;
        }

        /// <summary>
        /// Turns on machine
        /// </summary>
        public void TurnOn()
        {
            Command command = new FlightModeCommand(DroneFlightMode.TakeOff);
            m_device.SendCommand(command);
        }

        /// <summary>
        /// Turns off machine
        /// </summary>
        public void TurnOff()
        {
            Command command = new FlightModeCommand(DroneFlightMode.Land);
            m_device.SendCommand(command);
        }

        /// <summary>
        /// Send control to air device
        /// </summary>
        /// <param name="_port">1 = forward, 2 = backward, 3 = left, 4 = right, 5 = turn, 6 = Gas</param>
        /// <param name="_value">?5 = 0 - 360 degree, ?6 = 0 - 100</param>
        public void ControlSend(int _port, int _value)
        {
            Command command = null;
            switch (_port)
            {
                case 0:
                    command = new FlightModeCommand(DroneFlightMode.Land);
                    break;

                case 1:
                    command = new FlightMoveCommand(-0.5f, 0,    0, 0);
                    break;

                case 2:
                    command = new FlightMoveCommand( 0.5f, 0,    0, 0);
                    break;

                case 3:
                    command = new FlightMoveCommand( 0,    0.5f, 0, 0);
                    break;

                case 4:
                    command = new FlightMoveCommand(0,    -0.5f, 0, 0);
                    break;

                    //...

                case 999:
                    command = new FlightModeCommand(DroneFlightMode.Emergency);
                    break;
                    
            }
            m_device.SendCommand(command);
        }

        /// <summary>
        /// Get control of air device
        /// </summary>
        /// <param name="_port">1 = forward, 2 = backward, 3 = left, 4 = right, 5 = turn, 6 = Gas</param>
        /// <returns>actual value of this port</returns>
        public int Control(int _port)
        {
            return _port;
        }
    }
}