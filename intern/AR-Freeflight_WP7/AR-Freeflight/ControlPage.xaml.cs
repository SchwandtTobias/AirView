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
using Matrix = Microsoft.Xna.Framework.Matrix;
using System.Windows.Media.Imaging;


namespace AR_Freeflight
{
    public partial class ControlPage : PhoneApplicationPage
    {

        internal DroneController.DroneController droneController;

        Motion motion;
        private bool firstManipulation = false;

        // values for motionsense
        private float nullPitch, nullRoll, pitch, roll;

        // values for controlstick
        private double max_x, max_y, null_section, y_axis, x_axis;

        // Max phone tilt in degrees
        const float MAX_PHONE_TILT = 30.0f;

        // Min to sense handy motion
        const float MIN_ACCEPTANCE = 3.0f;

        
        
       public ControlPage()
        {
            InitializeComponent();

           // get values from controlstick_border
            max_x = controlstick_border.Width;
            max_y = controlstick_border.Height;
            null_section = controlstick_border.Width / 9;

            motion = new Motion();
            motion.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<MotionReading>>(motion_CurrentValueChanged);

            try
            {
                motion.Start();
                // set motion update time
                motion.TimeBetweenUpdates = TimeSpan.FromMilliseconds(20);
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Unable to start the motion API");
            }
            if (!connectToDrone())
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
            else {            
                     // wait for complete connection
                     System.Threading.Thread.Sleep(1000);
                 }
        }

        void motion_CurrentValueChanged(object sender, SensorReadingEventArgs<MotionReading> e)
        {

            if(droneController != null)
            {
                if (droneController.MotionIsActive)
                {

                    //rotate handy matrix around 90° on z_axis for landscaperight orientation
                    var viewMatrix = motion.CurrentValue.Attitude.RotationMatrix * Microsoft.Xna.Framework.Matrix.CreateRotationZ(MathHelper.PiOver2);

                    //PITCH
                    //(float)Math.Atan((viewMatrix.Backward.Y) / (Math.Sqrt(Math.Pow(viewMatrix.Backward.X, 2) + Math.Pow(viewMatrix.Backward.Z, 2))));
                    // =viewMatrix.Backward.Y;
                    //ROLL
                    //(float)Math.Atan((viewMatrix.Backward.X) / (Math.Sqrt(Math.Pow(viewMatrix.Backward.Y, 2) + Math.Pow(viewMatrix.Backward.Z, 2))));
                    // =viewMatrix.Backward.x;

                    // on first movement, get nullpitch/roll
                    if (firstManipulation)
                    {
                        //create new pitch and roll from the rotated matrix
                        nullPitch = (float)Math.Atan((viewMatrix.Backward.Y) / (Math.Sqrt(Math.Pow(viewMatrix.Backward.X, 2) + Math.Pow(viewMatrix.Backward.Z, 2))));
                        nullRoll = (float)Math.Atan((viewMatrix.Backward.X) / (Math.Sqrt(Math.Pow(viewMatrix.Backward.Y, 2) + Math.Pow(viewMatrix.Backward.Z, 2))));
                        firstManipulation = false;
                    }                    
                    else
                    {
                         roll = MathHelper.ToDegrees(nullRoll - (float)Math.Atan((viewMatrix.Backward.X) / (Math.Sqrt(Math.Pow(viewMatrix.Backward.Y, 2) + Math.Pow(viewMatrix.Backward.Z, 2)))));
                         pitch = -MathHelper.ToDegrees(nullPitch - (float)Math.Atan((viewMatrix.Backward.Y) / (Math.Sqrt(Math.Pow(viewMatrix.Backward.X, 2) + Math.Pow(viewMatrix.Backward.Z, 2)))));

                        // limit the phone tilt and set minimum for sense
                        //_START
                         if (roll > MAX_PHONE_TILT)
                            {
                                droneController.roll_send = 1.0f;
                            }
                         else if (roll < -MAX_PHONE_TILT)
                            {
                                droneController.roll_send = -1.0f;
                            }
                         if (roll > MIN_ACCEPTANCE)
                            {
                                droneController.roll_send = MathHelper.Clamp(MathHelper.ToRadians(roll - MIN_ACCEPTANCE), -1.0f, 1.0f);
                            }
                         else if (roll < -MIN_ACCEPTANCE)
                            {
                                droneController.roll_send = MathHelper.Clamp(MathHelper.ToRadians(roll + MIN_ACCEPTANCE), -1.0f, 1.0f);
                            }
                         else
                            {
                                droneController.roll_send = 0.0f;
                            }

                         if (pitch > MAX_PHONE_TILT)
                            {
                                droneController.pitch_send = 1.0f;
                            }
                         else if (pitch < -MAX_PHONE_TILT)
                            {
                                droneController.pitch_send = -1.0f;
                            }
                         else if (pitch > MIN_ACCEPTANCE)
                            {
                                droneController.pitch_send = MathHelper.Clamp(MathHelper.ToRadians(pitch - MIN_ACCEPTANCE), -1.0f, 1.0f); ;
                            }
                         else if (pitch < -MIN_ACCEPTANCE)
                            {
                                droneController.pitch_send = MathHelper.Clamp(MathHelper.ToRadians(pitch + MIN_ACCEPTANCE), -1.0f, 1.0f); ;
                            }
                        else
                            {
                                droneController.pitch_send = 0.0f;
                            }
                        //_END
                         
                        }
                    }
                 }
             }

        internal bool connectToDrone()
        {
            
            // Create a new Drone Configuration
            DroneControllerConfiguration droneControllerConfig = new DroneControllerConfiguration();

            droneControllerConfig.EnableNavigationDataThread = true;
            droneControllerConfig.EnableVideoStreamThread = true;
            droneControllerConfig.EnableATCommandThread = true;
            droneControllerConfig.EnableControlInfoThread = true;
            droneControllerConfig.EnableATCommandSimulation = false;

            //drone always has this adress
            droneControllerConfig.DroneIpAddress = "192.168.1.1";

            // NULL Pointer for creation
            droneController = null;
            droneController = new DroneController.DroneController(droneControllerConfig);


            droneController.TraceNotificationLevel = TraceNotificationLevel.Verbose;
            droneController.OnNotifyTraceMessage += new EventHandler<TraceNotificationEventArgs>(droneController_OnNotifyTraceMessage);
            droneController.OnNotifyVideoMessage += new EventHandler<VideoNotificationEventArgs>(droneController_OnNotifyVideoMessage);
            droneController.OnNotifyDroneInfoMessage += new EventHandler<DroneInfoNotificationEventArgs>(droneController_OnNotifyDroneInfoMessage);
            droneController.OnConnectionStatusChanged += new EventHandler<ConnectionStatusChangedEventArgs>(droneController_OnConnectionStatusChanged);
            droneController.Connect();

            while (droneController.ConnectionStatus != ConnectionStatus.Open) Thread.Sleep(100);


            //set drone configuration (speed, euler, ..)

            //droneController.SetChildrenConfiguration();
            droneController.SetIndoorConfiguration();

            if (droneController.ConnectionStatus == ConnectionStatus.Open) { return true; }
            else { return false; }
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

        void droneController_OnNotifyVideoMessage(object sender, VideoNotificationEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action<object, VideoNotificationEventArgs>(droneController_OnNotifyVideoMessage), sender, e);
            }
            else
            {
                WriteableBitmap bitmap = new WriteableBitmap(e.Width, e.Height);

                Array.Copy(e.PixelArray, bitmap.Pixels, e.PixelArray.Length);
                bitmap.Invalidate();


                VideoImg.Source = bitmap;
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
                //say drone, that it stands on a flat subsoil
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


        private void takeoff_Click(object sender, RoutedEventArgs e)
        {
            // prevent clicking (object is visible(!) and only transparent)
            if (button3.Opacity != 0)
            {
                takeOff();
                CreateFadeInOutAnimation(1.0, 0.0, this.textBlock_takeoff).Begin();
                CreateFadeInOutAnimation(1.0, 0.0, this.button3).Begin();
                CreateFadeInOutAnimation(0.0, 1.0, this.textBlock_Land).Begin();
                CreateFadeInOutAnimation(0.0, 1.0, this.button3_land).Begin();
            }
        }
        
        private void land_Click(object sender, RoutedEventArgs e)
        {
            // prevent clicking (object is visible(!) and only transparent)
            if (button3_land.Opacity != 0)
            {
                land();
                CreateFadeInOutAnimation(0.0, 1.0, this.textBlock_takeoff).Begin();
                CreateFadeInOutAnimation(0.0, 1.0, this.button3).Begin();
                CreateFadeInOutAnimation(1.0, 0.0, this.textBlock_Land).Begin();
                CreateFadeInOutAnimation(1.0, 0.0, this.button3_land).Begin();
            }
        }

        //fadein/out animation
        private Storyboard CreateFadeInOutAnimation(double from, double to, DependencyObject target)
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation fadeInAnimation = new DoubleAnimation();
            fadeInAnimation.From = from;
            fadeInAnimation.To = to;
            fadeInAnimation.Duration = new Duration(TimeSpan.FromSeconds(1.0));

            Storyboard.SetTarget(fadeInAnimation, target);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity"));

            sb.Children.Add(fadeInAnimation);
            return sb;
        }

        private void button2_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (droneController != null)
            {
                droneController.MotionIsActive = true;
                firstManipulation = true;
            }
        }

        private void button2_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (droneController != null)
            {
                droneController.roll_send = 0;
                droneController.pitch_send = 0;
                droneController.MotionIsActive = false;
                firstManipulation = false;
            }
        }

       //force Orientation to landscape
        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.LandscapeRight)
            {
                return;//base.OnOrientationChanged(e);
            }
            base.OnOrientationChanged(e);
        }

        // if back-button is pressed during active connection, land (if necessary) and disconnect
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (droneController != null)
            {
                if (droneController.DroneIsFlying)
                {
                    land();
                    System.Threading.Thread.Sleep(300);
                }
                disconnectFromDrone();
            }
            base.OnBackKeyPress(e);
        }

        private void steuerkreuz_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            /*
            if(Debugger.IsAttached)
            Debug.WriteLine("X:{0} Y:{1}", e.CumulativeManipulation.Translation.X, e.CumulativeManipulation.Translation.Y);
            */

            x_axis = e.CumulativeManipulation.Translation.X;
            y_axis = e.CumulativeManipulation.Translation.Y;

            // yaw + gaz LEGEND
            /*
             * yaw =  0 -> do nothing
             * yaw =  1 -> turn right
             * yaw = -1 -> turn left
             * gaz =  0 -> do nothing
             * gaz =  1 -> move up
             * gaz = -1 -> move down
            */

            //
            
            if (droneController != null && droneController.DroneIsFlying)
            {

                if ((x_axis < null_section && x_axis < -null_section) && (y_axis < null_section && y_axis < -null_section))
                {
                    //magic code, do nothing
                    droneController.gaz_send = 0;
                    droneController.yaw_send = 0;
                }
                if (x_axis > null_section && x_axis < max_x/2)
                {
                    // yaw right
                    if (y_axis <= max_y/3 && y_axis >= -(max_y/3))
                    {
                        droneController.yaw_send = 1;
                        droneController.gaz_send = 0;
                    }
                    // yaw right and gaz down
                    else if ((y_axis > (max_y / 3)) && (y_axis <= (max_y / 2)))
                    {
                        droneController.gaz_send = -1;
                        droneController.yaw_send = 1;
                    }
                    // yaw right and gaz up
                    else if (y_axis < -(max_y/3) && y_axis > -(max_y/2))
                    {
                        droneController.gaz_send = 1;
                        droneController.yaw_send = 1;
                    }
                }
                else if (x_axis < -null_section && x_axis > -(max_x/2))
                {
                    // yaw left
                    if (y_axis <= max_y / 3 && y_axis >= -(max_y/3))
                    {
                        droneController.yaw_send = -1;
                        droneController.gaz_send = 0;
                    }
                    // yaw left and gaz down
                    else if (y_axis > max_y / 3 && y_axis <= max_y / 2)
                    {
                        droneController.gaz_send = -1;
                        droneController.yaw_send = -1;
                    }
                    // yaw left and gaz up
                    else if (y_axis < -(max_y/3) && y_axis > -(max_y/2))
                    {
                        droneController.gaz_send = 1;
                        droneController.yaw_send = -1;
                    }
                }
                else if (x_axis > -null_section && x_axis < null_section)
                {
                    // gaz down
                    if (y_axis > null_section && y_axis < max_y / 2)
                    {
                        droneController.gaz_send = -1;
                        droneController.yaw_send = 0;
                    }
                    // gaz up
                    else if (y_axis < -null_section && y_axis > -(max_y/2))
                    {
                        droneController.gaz_send = 1;
                        droneController.yaw_send = 0;
                    }
                }
            }
        }

        private void steuerkreuz_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (droneController != null)
            {
                droneController.gaz_send = 0;
                droneController.yaw_send = 0;
            }
        }

    }
}