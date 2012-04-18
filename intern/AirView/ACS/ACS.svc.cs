using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace ACS
{ 
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CACService : IACS
    {
        private IAirDevice m_device = new CArDrone();

        public CACService()
        {
            m_device = new CArDrone();
        }

        #region public methods
        /// <summary>
        /// Login to this service to get full functionality
        /// </summary>
        /// <param name="_name">Name of the user</param>
        /// <param name="_password">Password encrypted with sha1</param>
        /// <returns>-1 = FAILURE, 0 = LOGGED, >1 = LOGGED and count of user in queue</returns>
        public int Login(string _name, string _password)
        {
            return 0;
        }

        /// <summary>
        /// Logout from service
        /// </summary>
        /// <param name="_name">Name of the user</param>
        /// <param name="_password">Password encypted with sha1</param>
        public void Logout(string _name, string _password)
        {

        }

        /// <summary>
        /// Request control to air device
        /// </summary>
        /// <returns>-1 = FAILURE, >0 = GRANTED with assumed time</returns>
        public int RequestControl()
        {
            return 0;
        }

        /// <summary>
        /// Send control to air device
        /// </summary>
        /// <param name="_port">1 = forward, 2 = backward, 3 = left, 4 = right, 5 = turn, 6 = Gas</param>
        /// <param name="_value">?5 = 0 - 360 degree, ?6 = 0 - 100</param>
        public void ControlSend(int _port, int _value)
        {
            m_device.ControlSend(_port, _value);
        }

        /// <summary>
        /// Get control of air device
        /// </summary>
        /// <param name="_port">1 = forward, 2 = backward, 3 = left, 4 = right, 5 = turn, 6 = Gas</param>
        /// <returns>actual value of this port</returns>
        public int Control(int _port)
        {
            return m_device.Control(_port);
        }
        #endregion
    }
}
