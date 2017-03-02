using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectHandTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;
        float x = 0, y = 0, z = 0, deltax = 0, deltay = 0, deltaz = 0;
        float tmpx = 0, tmpy = 0, tmpz = 0;
        int flag = 0, flag1 = 0;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    camera.Source = frame.ToBitmap();
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    var csv = new StringBuilder();
                    string filePath = "C:\\Users\\Arif\\Documents\\k.csv";

                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Find the joints
                                Joint handRight = body.Joints[JointType.HandRight];
                                Joint thumbRight = body.Joints[JointType.ThumbRight];

                                Joint handLeft = body.Joints[JointType.HandLeft];
                                Joint thumbLeft = body.Joints[JointType.ThumbLeft];

                                // Draw hands and thumbs
                                canvas.DrawSkeleton(body, _sensor.CoordinateMapper);
                                //canvas.DrawHand(handRight, _sensor.CoordinateMapper);
                                //canvas.DrawHand(handLeft, _sensor.CoordinateMapper);
                                //canvas.DrawThumb(thumbRight, _sensor.CoordinateMapper);
                                //canvas.DrawThumb(thumbLeft, _sensor.CoordinateMapper);

                                #region Notif State

                                // Find the hand states
                                string rightHandState = "-";
                                string leftHandState = "-";

                                switch (body.HandRightState)
                                {
                                    case HandState.Open:
                                        rightHandState = "Open";
                                        break;
                                    case HandState.Closed:
                                        rightHandState = "Closed";
                                        break;
                                    case HandState.Lasso:
                                        rightHandState = "Lasso";
                                        break;
                                    case HandState.Unknown:
                                        rightHandState = "Unknown...";
                                        break;
                                    case HandState.NotTracked:
                                        rightHandState = "Not tracked";
                                        break;
                                    default:
                                        break;
                                }

                                switch (body.HandLeftState)
                                {
                                    case HandState.Open:
                                        leftHandState = "Open";
                                        break;
                                    case HandState.Closed:
                                        leftHandState = "Closed";
                                        break;
                                    case HandState.Lasso:
                                        leftHandState = "Lasso";
                                        break;
                                    case HandState.Unknown:
                                        leftHandState = "Unknown...";
                                        break;
                                    case HandState.NotTracked:
                                        leftHandState = "Not tracked";
                                        break;
                                    default:
                                        break;
                                }

                                tblRightHandState.Text = rightHandState;
                                tblLeftHandState.Text = leftHandState;

                                #endregion

                                if (body.HandRightState == HandState.Open)
                                {
                                    //hitung-hitung

                                    x = (handRight.Position.X);
                                    y = (handRight.Position.Y);
                                    z = (handRight.Position.Z);

                                    if (flag1 == 0)
                                    {    
                                        deltax = x; deltay = y; deltaz = z;
                                        flag1 = 1;
                                    }
                                    else
                                    {
                                        deltax = x - tmpx;
                                        deltay = y - tmpy;
                                        deltaz = z - tmpz;
                                    }

                                    tmpx = x; tmpy = y; tmpz = z;

                                    var stringx = x.ToString();
                                    var stringy = y.ToString();
                                    var stringz = z.ToString();
                                    var stringdeltax = deltax.ToString();
                                    var stringdeltay = deltay.ToString();
                                    var stringdeltaz = deltaz.ToString();

                                    var newLine = string.Format("{0},{1},{2},{3},{4},{5}",
                                        stringx, stringy, stringz,
                                        stringdeltax, stringdeltay, stringdeltaz);
                                    csv.AppendLine(newLine);
                                }
                                //flag = 1;
                            }
                        }
                    }
                    File.AppendAllText(filePath, csv.ToString());
                }
            }
        }

        #endregion
    }
}
