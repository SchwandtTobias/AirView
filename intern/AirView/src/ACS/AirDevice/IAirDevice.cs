using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACS
{
    interface IAirDevice
    {
        /// <summary>
        /// Turns on machine
        /// </summary>
        void TurnOn();

        /// <summary>
        /// Turns off machine
        /// </summary>
        void TurnOff();

        /// <summary>
        /// Send control to air device
        /// </summary>
        /// <param name="_port">1 = forward, 2 = backward, 3 = left, 4 = right, 5 = turn, 6 = Gas</param>
        /// <param name="_value">?5 = 0 - 360 degree, ?6 = 0 - 100</param>
        void ControlSend(int _port, int _value);

        /// <summary>
        /// Get control of air device
        /// </summary>
        /// <param name="_port">1 = forward, 2 = backward, 3 = left, 4 = right, 5 = turn, 6 = Gas</param>
        /// <returns>actual value of this port</returns>
        int Control(int _port);
    }
}
