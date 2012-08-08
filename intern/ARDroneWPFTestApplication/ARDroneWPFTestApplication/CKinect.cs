using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Windows.Media;

namespace ARDroneWPFTestApplication
{
    using Microsoft.Kinect;
    using System.Media;
    using System.Windows;

    public class CKinect
    {
        public float DISTANCE_BARRIER = 5.0f;

        public enum KinectState
        {
            Connected = 0,
            Disconnected,
            Error,
            CountOfStates,
            Undefined = -1
        }

        public enum BodyState
        {
            PositionOnly = 0,
            Tracked,
            NotTracked,
            CountOfStates,
            Undefined = -1
        }

        public KinectState ActualKinectState
        {
            get { return m_ActualKinectState; }
        }

        public BodyState ActualBodyState
        {
            get { return m_ActualBodyState; }
        }

        public void Connect()
        {
            try
            {
                m_Logs.Add("[System] Kinrect connection will start\n");

                m_KinectSensor.Start();

                m_Logs.Add("[System] Kinrect connection started\n");
            }
            catch (Exception e)
            {
                m_Logs.Add(e.StackTrace + "\n");
            }
        }

        public void Disconnect()
        {
            try
            {
                m_Logs.Add("[System] Kinrect connection will close\n");

                m_KinectSensor.Stop();

                m_Logs.Add("[System] Kinrect connection closed\n");
            }
            catch (Exception e)
            {
                m_Logs.Add(e.StackTrace + "\n");
            }
        }

        public void EnableSkeletonStream()
        {
            try
            {
                m_KinectSensor.SkeletonStream.Enable();

                m_Logs.Add("[System] Tracking On\n");
            }
            catch (Exception e)
            {
                m_Logs.Add(e.StackTrace + "\n");
            }
        }

        public void DisableSkeletonStream()
        {
            try
            {
                m_KinectSensor.SkeletonStream.Disable();

                m_Logs.Add("[System] Tracking Off\n");
            }
            catch (Exception e)
            {
                m_Logs.Add(e.StackTrace + "\n");
            }
        }

        public void ChangeViewAngle(int _Angle)
        {
            try
            {
                m_KinectSensor.ElevationAngle = _Angle;
            }
            catch (System.Exception ex)
            {
                m_Logs.Add(ex.StackTrace + "\n");
            }
           
        }

        public BitmapSource GetSkeletonPictureContext()
        {
            return m_DepthImageSource;
        }

        public String GetLogs()
        {
            String Result = "Airview AR-Drone - Kinect FHS V0.1\n";

            foreach (String Element in m_Logs)
            {
                Result += Element;
            }

            return Result;

        }

        public void SetMinAngle(float _Angle = 5.0f)
        {
            DISTANCE_BARRIER = _Angle;
        }

        private KinectSensor m_KinectSensor;

        private CARDrone     m_ArDrone;

        private BitmapSource m_DepthImageSource;

        private KinectState  m_ActualKinectState;

        private BodyState    m_ActualBodyState;

        private float        m_NickOrientation;
        
        private float        m_RollOrientation;

        private bool         m_isTakeOff;

        private List<String> m_Logs;

        private float        m_GlobalRoll;
        private float        m_GlobalNick;


        public CKinect(CARDrone _ArDrone)
        {
            m_ArDrone = _ArDrone;

            m_DepthImageSource = null;

            m_NickOrientation = 0.0f;

            m_RollOrientation = 0.0f;

            m_ActualKinectState = KinectState.Disconnected;

            m_ActualBodyState = BodyState.NotTracked;

            m_Logs = new List<String>(200);

            try
            {
                m_KinectSensor = KinectSensor.KinectSensors[0];

                m_KinectSensor.DepthStream.Enable();

                m_KinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);

                m_KinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthFrameReady);

                SetMinAngle(Properties.Settings.Default.MinAngleMK);

                m_Logs.Add("[System] Kinect Sensor ready\n");
            }
            catch (Exception e)
            {
                m_ActualKinectState = KinectState.Error;

                m_KinectSensor = null;

                m_Logs.Add(e.StackTrace);
            }

        }

        ~CKinect()
        {
            if (m_KinectSensor != null)
            {
                m_KinectSensor.Stop();
            }
        }

        private void DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame CurrentDepthImageFrame = e.OpenDepthImageFrame())
            {
                if (CurrentDepthImageFrame != null)
                {
                    short[] DepthImageData = new short[CurrentDepthImageFrame.PixelDataLength];

                    byte[] DepthImageBytes = new byte[CurrentDepthImageFrame.PixelDataLength * 4];

                    CurrentDepthImageFrame.CopyPixelDataTo(DepthImageData);

                    const int BlueColorIndex = 0;

                    const int GreenColorIndex = 0;

                    const int RedColorIndex = 0;

                    for (int IndexOfDepthImage = 0, IndexOfDepthImageBytes = 0; IndexOfDepthImage < DepthImageData.Length && IndexOfDepthImageBytes < DepthImageData.Length * 4; ++IndexOfDepthImage, IndexOfDepthImageBytes += 4)
                    {
                        byte Intensity = (byte)((float)DepthImageData[IndexOfDepthImage] / (float)short.MaxValue * (float)byte.MaxValue);

                        DepthImageBytes[IndexOfDepthImageBytes + BlueColorIndex] = Intensity;

                        DepthImageBytes[IndexOfDepthImageBytes + GreenColorIndex] = Intensity;

                        DepthImageBytes[IndexOfDepthImageBytes + RedColorIndex] = Intensity;
                    }

                    m_DepthImageSource = BitmapSource.Create(CurrentDepthImageFrame.Width, CurrentDepthImageFrame.Height, 96, 96, PixelFormats.Bgr32, null, DepthImageBytes, CurrentDepthImageFrame.Width * 4);
                }

            }
        }

        private void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            // there skeleton availible?
            if (skeletons.Length != 0)
            {
                // is one of this skeleton do the flying gesture
                Skeleton CurrentSkeleton = null;

                foreach (Skeleton Skeleton in skeletons)
                {
                    if (Skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        if (this.isSkeletonAllowedToControl(Skeleton))
                        {
                            CurrentSkeleton = Skeleton;
                            break;
                        }
                    }
                }

                // fly !!
                if (CurrentSkeleton != null)
                {
                    Joint ShoulderCenter = CurrentSkeleton.Joints[JointType.ShoulderCenter];
                    Joint Hip = CurrentSkeleton.Joints[JointType.HipCenter];
                    Joint HandLeft = CurrentSkeleton.Joints[JointType.HandLeft];
                    Joint HandRight = CurrentSkeleton.Joints[JointType.HandRight];
                    Joint Head = CurrentSkeleton.Joints[JointType.Head];

                    float h = ShoulderCenter.Position.Y - Hip.Position.Y;
                    float n1 = ShoulderCenter.Position.Z - Hip.Position.Z;
                    float n2 = ShoulderCenter.Position.X - Hip.Position.X;

                    if (m_ArDrone != null && m_ArDrone.ActualState != CARDrone.State.Fly && m_ArDrone.ActualState != CARDrone.State.Error && !m_isTakeOff)
                    {
                        // send take of to drone
                        m_NickOrientation = (float)(Math.Atan(n1 / h) * (360 / (2 * Math.PI)));
                        m_RollOrientation = (float)(Math.Tan(n2 / h) * (360 / (2 * Math.PI)));

                        this.sendTakeOffCommandToDrone();
                    }

                    // send take of to drone
                    float AngleNick = m_NickOrientation - (float)(Math.Atan(n1 / h) * (360 / (2 * Math.PI)));
                    float AngleRoll = m_RollOrientation - (float)(Math.Tan(n2 / h) * (360 / (2 * Math.PI)));

                    //System.Console.WriteLine("Nick " + AngleNick + " Roll " + AngleRoll);
                    float Yaw = HandLeft.Position.Z - HandRight.Position.Z;

                    //very risky but we can test it
                    //m_ArDrone.Fly(angleRoll, angleNick, yaw, 0.0f);

                    //not so risky but must be tested too
                    //it can only be done one of these
                    float CurrentRollOrientation = 0.0f;
                    float CurrentNickOrientation = 0.0f;

                    if (AngleNick > DISTANCE_BARRIER)
                    {
                        CurrentNickOrientation = -AngleToARNorm(AngleNick);
                    }
                    else if (AngleNick < -DISTANCE_BARRIER)
                    {
                        CurrentNickOrientation = -AngleToARNorm(AngleNick);
                    }

                    if (AngleRoll > DISTANCE_BARRIER)
                    {
                        CurrentRollOrientation = -AngleToARNorm(AngleRoll);
                    }
                    else if (AngleRoll < -DISTANCE_BARRIER)
                    {
                        CurrentRollOrientation = -AngleToARNorm(AngleRoll);
                    }

                    if (m_ArDrone != null)
                    {
                        try
                        {
                            m_ArDrone.Fly(CurrentNickOrientation, CurrentRollOrientation, 0.0f, 0.0f);
                        }
                        catch (Exception exception)
                        {
                            // do something
                        }
                    }
                }
                else
                {
                    this.sendLandingCommandToDrone();
                }
            }
            else
            {
                this.sendLandingCommandToDrone();
            }
            
        }

        private void sendTakeOffCommandToDrone()
        {
            m_isTakeOff = true;

            // send take off to drone
            try
            {
                if (m_ArDrone != null)
                {
                    m_ArDrone.TakeOff();
                }
            }
            catch (Exception exeption)
            {
                // do something
            }
        }

        private void sendLandingCommandToDrone()
        {
            m_isTakeOff = false;

            // send landing to drone
            try
            {
                if (m_ArDrone != null)
                {
                    m_ArDrone.Fly(0.0f, 0.0f, 0.0f, 0.0f);
                    m_ArDrone.Land();
                }
            }
            catch (Exception exeption)
            {
                // do something
            }
        }

        private bool isSkeletonAllowedToControl(Skeleton CurrentSkeleton)
        {
            // get needed tacking points to calculate gestures
            Joint ShoulderCenter = CurrentSkeleton.Joints[JointType.ShoulderCenter];
            Joint Hip = CurrentSkeleton.Joints[JointType.HipCenter];
            Joint HandLeft = CurrentSkeleton.Joints[JointType.HandLeft];
            Joint HandRight = CurrentSkeleton.Joints[JointType.HandRight];

            // check pilot command
            Vector VLeftArm = new Vector(HandLeft.Position.X - ShoulderCenter.Position.X, HandLeft.Position.Y - ShoulderCenter.Position.Y);
            Vector VRightArm = new Vector(HandRight.Position.X - ShoulderCenter.Position.X, HandRight.Position.Y - ShoulderCenter.Position.Y);
            Vector VShoulderCenter = new Vector(ShoulderCenter.Position.X - Hip.Position.X, ShoulderCenter.Position.Y - Hip.Position.Y);

            VLeftArm.Normalize();
            VRightArm.Normalize();
            VShoulderCenter.Normalize();

            // do magic
            float LeftCenterScal = (float)(VLeftArm * VShoulderCenter);
            float RightCenterScal = (float)(VRightArm * VShoulderCenter);

            if (Math.Abs(LeftCenterScal) < 0.5 && Math.Abs(RightCenterScal) < 0.5)
            {
                // set person as flying and tracked
                return true;
            }
            else
            {
                // untrack person
                return false;
            }
        }

        private float AngleToARNorm(float _Angle)
        {
            return (_Angle - DISTANCE_BARRIER) / 40.0f;
        }
    }
}
