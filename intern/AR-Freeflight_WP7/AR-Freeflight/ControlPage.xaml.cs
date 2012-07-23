using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using DroneController;
using System.Threading;
using Microsoft.Devices.Sensors;
using System.Diagnostics;

namespace AR_Freeflight
{
    public partial class ControlPage : PhoneApplicationPage
    {

        internal DroneController.DroneController droneController;

        Motion motion;
        bool firstManipulation = false;
        float refPitch;
        float refRoll;

        float pitch_in_range;
        float roll_in_range;

        int valueCounter = 0;

        const float SMOOTH = 1.5f;
        const float ACTION = 3.0f;
        // The ultimate Action-Variable for tiny curves

        public ControlPage()
        {
            InitializeComponent();
            

            motion = new Motion();
            motion.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<MotionReading>>(motion_CurrentValueChanged);

            try
            {
                motion.Start();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Unable to start the motion API");
            }

        }

        


        void motion_CurrentValueChanged(object sender, SensorReadingEventArgs<MotionReading> e)
        {

            if(droneController != null)
            {
                if (droneController.IsMoving)
                {
                    if (firstManipulation)
                    {
                        refPitch = e.SensorReading.Attitude.Pitch;
                        refRoll = e.SensorReading.Attitude.Roll;
                        firstManipulation = false;
                    }
                    else
                    {
                        // Helps to send not all Values to the drone
                        if (valueCounter <= 6)
                        {
                            valueCounter++;
                        }
                        else
                        {
                            valueCounter = 0;
                            return;
                        }


                        // Get the new Accelerometer Values and do some changes on it for smoother movement
                        var newRoll = refRoll - e.SensorReading.Attitude.Roll;
                        var newPitch = refPitch - e.SensorReading.Attitude.Pitch;

                        // if negative
                        if (newRoll * (-1) < 0)
                        {
                            if ((newRoll * (-1) - SMOOTH) < roll_in_range)
                            {
                                roll_in_range = MathHelper.Clamp(newRoll * (-1), -1.0f, 1.0f);
                            }
                        }
                        else {
                            if ((newRoll * (-1) + SMOOTH) > roll_in_range)
                            {
                                roll_in_range = MathHelper.Clamp(newRoll * (-1), -1.0f, 1.0f);
                            }
                        }
                        if (newPitch < 0)
                        {
                            if ((newPitch - SMOOTH) < pitch_in_range)
                            {
                                pitch_in_range = MathHelper.Clamp(newPitch, -1.0f, 1.0f);
                            }
                        }
                        else {
                            if ((newPitch + SMOOTH) > pitch_in_range)
                            {
                                pitch_in_range = MathHelper.Clamp(newPitch, -1.0f, 1.0f);
                            }
                        }
                        

                        if (roll_in_range > 0.5f || roll_in_range < -0.5f)
                        {
                            droneController.Pitch = roll_in_range;
                            droneController.Roll =  pitch_in_range / ACTION;
                        }
                        else if (pitch_in_range > 0.5f || pitch_in_range < -0.5f)
                        {
                            droneController.Pitch = roll_in_range / ACTION;
                            droneController.Roll = pitch_in_range;
                        }
                        else
                        {
                            droneController.Pitch = roll_in_range;
                            droneController.Roll = pitch_in_range;
                        }
                    }
                }
            }
        }

        internal void connectToDrone()
        {
            // Create a new Drone Configuration
            DroneControllerConfiguration droneControllerConfig = new DroneControllerConfiguration();

            droneControllerConfig.EnableNavigationDataThread = true;
            droneControllerConfig.EnableVideoStreamThread = true;
            droneControllerConfig.EnableGPSStreamThread = false;
            droneControllerConfig.EnableATCommandThread = true;
            droneControllerConfig.EnableControlInfoThread = true;
            droneControllerConfig.EnableATCommandSimulation = false;
            droneControllerConfig.DroneIpAddress = IP_textbox.Text;

            // NULL Pointer für Erstellung
            droneController = null;
            droneController = new DroneController.DroneController(droneControllerConfig);

            droneController.IsMoving = false;

            droneController.TraceNotificationLevel = TraceNotificationLevel.Verbose;
            droneController.OnNotifyTraceMessage += new EventHandler<TraceNotificationEventArgs>(droneController_OnNotifyTraceMessage);
            droneController.OnNotifyVideoMessage += new EventHandler<VideoNotificationEventArgs>(droneController_OnNotifyVideoMessage);
            droneController.OnNotifyGPSMessage += new EventHandler<GPSNotificationEventArgs>(droneController_OnNotifyGPSMessage);
            droneController.OnNotifyDroneInfoMessage += new EventHandler<DroneInfoNotificationEventArgs>(droneController_OnNotifyDroneInfoMessage);
            droneController.OnConnectionStatusChanged += new EventHandler<ConnectionStatusChangedEventArgs>(droneController_OnConnectionStatusChanged);
            droneController.Connect();

            while (droneController.ConnectionStatus != ConnectionStatus.Open) Thread.Sleep(100);


            // Set the indoor configuration
            droneController.SetIndoorConfiguration();
            
        }

        void droneController_OnConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action<object, ConnectionStatusChangedEventArgs>(droneController_OnConnectionStatusChanged), sender, e);
            }
            else
            {
                //labelStatus.Content = e.ConnectionStatus.ToString();
            }
        }

        void droneController_OnNotifyDroneInfoMessage(object sender, DroneInfoNotificationEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action<object, DroneInfoNotificationEventArgs>(droneController_OnNotifyDroneInfoMessage), sender, e);
            }
        }

        void droneController_OnNotifyGPSMessage(object sender, GPSNotificationEventArgs e)
        {
            throw new NotImplementedException();
        }

        void droneController_OnNotifyVideoMessage(object sender, VideoNotificationEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action<object, VideoNotificationEventArgs>(droneController_OnNotifyVideoMessage), sender, e);
            }
            else
            {
                VideoImg.Source = e.CurrentImage;
                ///VideoFeedImage.Source = e.CurrentImage;
            }
        }

        void droneController_OnNotifyTraceMessage(object sender, TraceNotificationEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action<object, TraceNotificationEventArgs>(droneController_OnNotifyTraceMessage), sender, e);
            }
            else
            {
                //Traces.Add(e.NotificationMessage);
                //TraceBox.ScrollIntoView(TraceBox.Items[TraceBox.Items.Count-1]);
            }
        }


        internal void takeOff()
        {
            if (droneController != null && droneController.ConnectionStatus == ConnectionStatus.Open)
            {
                droneController.SetFlatTrim();
                droneController.StartEngines();
                
            }
        }

        internal void land()
        {
            if (droneController != null)
            {
                droneController.StopEngines();
            }
        }

        internal void disconnectFromDrone()
        {
            droneController.Disconnect();
            droneController = null;
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)connectButton.Content == "Connect")
            {

                    connectToDrone();
                    connectButton.Content = "Disconnect";

            }
            else
            {
                    disconnectFromDrone();
                    connectButton.Content = "Connect";

            }
        }

        private void takeoffButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)takeoffButton.Content == "Take Off")
            {
                takeOff();
                takeoffButton.Content = "Land";
            }
            else
            {
                land();
                takeoffButton.Content = "Take Off";
            }
        }

        private void accelerometerButton_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (droneController != null)
            {
                droneController.IsMoving = true;
                firstManipulation = true;
            }
        }

        private void accelerometerButton_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (droneController != null)
            {
                
                droneController.Roll = 0;
                droneController.Pitch = 0;
                droneController.IsMoving = false;
                firstManipulation = false;
            }
        }

        private void yawLeftButton_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (droneController != null)
            {
                droneController.Yaw = -1; // turn left
            }
        }

        private void yawRightButton_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (droneController != null)
            {
                droneController.Yaw = 1; // turn right
            }
        }


        private void yawButtons_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (droneController != null)
            {
                droneController.Yaw = 0;
            }
        }

        private void gazUpButton_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (droneController != null)
            {

                droneController.Gaz = 1; // move up
            }
        }

        private void gazDownButton_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (droneController != null)
            {

                droneController.Gaz = -1; // nove down
            }
        }

        private void gazButtons_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (droneController != null)
            {
                droneController.Gaz = 0;
            }
        }

        private void IP_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
        
        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.LandscapeRight)
            {
                return;//base.OnOrientationChanged(e);
            }
            base.OnOrientationChanged(e);
        }

    }
}