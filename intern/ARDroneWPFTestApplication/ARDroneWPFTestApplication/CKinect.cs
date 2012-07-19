using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARDroneWPFTestApplication
{
    using Microsoft.Kinect;

    class CKinect
    {
        public void Connect()
        {
            try
            {
                m_KinectSensor.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public void EnableSkeletonStream()
        {
            try
            {
                m_KinectSensor.SkeletonStream.Enable();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public void DisableSkeletonStream()
        {
            try
            {
                m_KinectSensor.SkeletonStream.Disable();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        private KinectSensor m_KinectSensor;

        private Skeleton[] m_CurrentSkeletons;

        CKinect()
        {
            try
            {
                m_KinectSensor = KinectSensor.KinectSensors[0];

                m_KinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        private void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
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
    }
}
