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

    public class CKinect
    {
        public const float DISTANCE_BARRIER = 5.0f;

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

        private KinectSensor m_KinectSensor;

        private Skeleton[]   m_CurrentSkeletons;

        private CARDrone     m_ArDrone;

        private BitmapSource m_DepthImageSource;

        private KinectState  m_ActualKinectState;

        private BodyState    m_ActualBodyState;

        private float        m_NickOrientation;
        
        private float        m_RollOrientation;
        private bool m_isTakeOff;

        private List<String> m_Logs;


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
            using (SkeletonFrame CurrentSkeletonFrame = e.OpenSkeletonFrame())
            {
                if (CurrentSkeletonFrame != null && CurrentSkeletonFrame.SkeletonArrayLength > 0)
                {
                    m_CurrentSkeletons = new Skeleton[CurrentSkeletonFrame.SkeletonArrayLength];

                    CurrentSkeletonFrame.CopySkeletonDataTo(m_CurrentSkeletons);

                    Skeleton CurrentSkeleton = m_CurrentSkeletons[0];


                    if (CurrentSkeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        if (m_ActualBodyState != BodyState.Tracked)
                        {
                            SystemSounds.Beep.Play();
                            m_Logs.Add("[User] user tracked\n");
                        }
                        m_ActualBodyState = BodyState.Tracked;

                        Joint ShoulderCenter;
                        Joint Hip;
                        Joint HandLeft;
                        Joint HandRight;
                        Joint Head;

                        ShoulderCenter      = CurrentSkeleton.Joints[JointType.ShoulderCenter     ];
                        Hip       = CurrentSkeleton.Joints[JointType.HipCenter];
                        HandLeft  = CurrentSkeleton.Joints[JointType.HandLeft ];
                        HandRight = CurrentSkeleton.Joints[JointType.HandRight];
                        Head  = CurrentSkeleton.Joints[JointType.Head];

                        if (Math.Abs(HandLeft.Position.X - HandRight.Position.X) > 2 * Math.Abs(ShoulderCenter.Position.Y - Hip.Position.Y))
                        {
                            float h = ShoulderCenter.Position.Y - Hip.Position.Y;
                            float n1 = ShoulderCenter.Position.Z - Hip.Position.Z;
                            float n2 = ShoulderCenter.Position.X - Hip.Position.X;
                            float n3 = ShoulderCenter.Position.Y - HandRight.Position.Y;
                            float n4 = ShoulderCenter.Position.X - HandRight.Position.X;

                            
                            //fly position detected
                            if (m_ArDrone.ActualState != CARDrone.State.Fly && m_ArDrone.ActualState != CARDrone.State.Error && !m_isTakeOff)
                            {
                                m_isTakeOff = true;
                                m_NickOrientation = (float)(Math.Atan(n1 / h) * (360 / (2 * Math.PI)));
                                m_RollOrientation = (float)(Math.Tan(n3 / n4) * (360 / (2 * Math.PI)));
                                m_ArDrone.TakeOff();
                            }

                            //float AngleNick = (float)(Math.Atan2(n1, h) * (360 / (2 * Math.PI)));
                            //float AngleRoll = (float)(Math.Atan2(n2, h) * (360 / (2 * Math.PI)));

                            float AngleNick = m_NickOrientation - (float)(Math.Atan(n1 / h) * (360 / (2 * Math.PI)));
                            float AngleRoll = (float)(Math.Tan(n3 / n4) * (360 / (2 * Math.PI)));

                            System.Console.WriteLine("Nick " + AngleNick + " Roll " + AngleRoll);
                            float Yaw = HandLeft.Position.Z - HandRight.Position.Z;

                            //very risky but we can test it
                            //m_ArDrone.Fly(angleRoll, angleNick, yaw, 0.0f);

                            //not so risky but must be tested too
                            //it can only be done one of these
                            float CurrentRollOrientation = 0.0f;
                            float CurrentNickOrientation = 0.0f;

                            
                            if (AngleNick > -DISTANCE_BARRIER)
                            {
                                //CurrentNickOrientation = -1.0f;
                                CurrentNickOrientation = -AngleToARNorm(AngleNick);
                            }
                            else if (AngleNick < DISTANCE_BARRIER)
                            {
                                //CurrentNickOrientation = 1.0f;
                                CurrentNickOrientation = -AngleToARNorm(AngleNick);
                            }
                            
                            if (AngleRoll > DISTANCE_BARRIER)
                            {
                                //CurrentRollOrientation = -1.0f;
                                CurrentRollOrientation = -AngleToARNorm(AngleRoll);
                            }
                            else if (AngleRoll < -DISTANCE_BARRIER)
                            {
                                //CurrentRollOrientation = 1.0f;
                                CurrentRollOrientation = -AngleToARNorm(AngleRoll);
                            }

                            //m_Logs.Add("Nick: " + CurrentNickOrientation + "\n");

                            m_ArDrone.Fly(CurrentNickOrientation, CurrentRollOrientation, 0.0f, 0.0f);

                            //if (m_NickOrientation != CurrentNickOrientation)
                            //{
                            //    m_NickOrientation = CurrentNickOrientation;

                            //    m_ArDrone.Pitch(m_NickOrientation);
                            //}

                            //if (m_RollOrientation != CurrentRollOrientation)
                            //{
                            //    m_RollOrientation = CurrentRollOrientation;

                            //    m_ArDrone.Roll(m_RollOrientation);
                            //}
                        }
                        else
                        {

                            m_isTakeOff = false;
                            m_ArDrone.Fly(0.0f, 0.0f, 0.0f, 0.0f);
                            //land position detected
                            m_ArDrone.Land();
                        }
                    }
                    else
                    {
                        if (m_ActualBodyState != BodyState.PositionOnly)
                        {
                            m_ArDrone.Land();
                        }

                        m_ActualBodyState = BodyState.PositionOnly;
                    }
                }
                else
                {
                    if (m_ActualBodyState != BodyState.NotTracked)
                    {
                        m_ArDrone.Land();
                    }

                    m_ActualBodyState = BodyState.NotTracked;
                }
            }
        }

        private float AngleToARNorm(float _Angle)
        {
            return (_Angle - DISTANCE_BARRIER) / 40.0f;
        }
    }
}
