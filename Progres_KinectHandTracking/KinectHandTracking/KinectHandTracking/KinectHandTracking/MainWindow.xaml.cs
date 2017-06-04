using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        //Tangan Kiri
        private double a = 0, b = 0, deltaa = 0, deltab = 0, alpha1 = 0, tmpa = 0, tmpb = 0;
        private int[] kuant1 = new int[999];
        //Tangan Kanan
        private double x = 0, y = 0, deltax = 0, deltay = 0, alpha2 = 0, tmpx = 0, tmpy = 0;
        private int[] kuant2 = new int[999];
        //Penentuan Posisi
        private double leher = 0, tengah = 0;
        //Posisinya
        private string tanganKanan = "", tanganKiri = "";

        //Penanda
        private int flag = 0, flag2 = 1;
        private int i = 0;
        private int statusAmbil = 0;
        private string namaGerakan = "";

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
                statusDetail.Content = "Idle";
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

        async void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            #region Acquire Frame Color
            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    camera.Source = frame.ToBitmap();
                }
            }
            #endregion

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();
                    _bodies = new Body[frame.BodyFrameSource.BodyCount];
                    frame.GetAndRefreshBodyData(_bodies);

                    // Deklarasi Variabel untuk Write Data
                    var csv = new StringBuilder();
                    string filePath = "F:\\Eka\\ITS\\KULIAH\\SEMESTER 7\\TAkisgan\\Realtime\\DataSet\\Percobaan.csv";
                    string imagePath = "F:\\Eka\\ITS\\KULIAH\\SEMESTER 7\\TAkisgan\\Realtime\\GambarIsyarat\\";

                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Identifikasi Tulang
                                Joint handRight = body.Joints[JointType.HandRight];
                                Joint handLeft = body.Joints[JointType.HandLeft];
                                Joint neck = body.Joints[JointType.Neck];
                                Joint spineMid = body.Joints[JointType.SpineMid];

                                // Menggambar Skeleton
                                //canvas.DrawSkeleton(body, _sensor.CoordinateMapper);

                                // COORDINATE MAPPING
                                foreach (Joint joint in body.Joints.Values)
                                {
                                    if (joint.TrackingState == TrackingState.Tracked)
                                    {
                                        // 3D space point
                                        CameraSpacePoint jointPosition = joint.Position;
                                        // 2D space point
                                        Point point = new Point();

                                        ColorSpacePoint colorPoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(jointPosition);
                                        point.X = float.IsInfinity(colorPoint.X) ? 0 : colorPoint.X;
                                        point.Y = float.IsInfinity(colorPoint.Y) ? 0 : colorPoint.Y;

                                        Ellipse ellipse = new Ellipse
                                        {
                                            Fill = Brushes.Blue,
                                            Width = 30,
                                            Height = 30
                                        };

                                        Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
                                        Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);

                                        canvas.Children.Add(ellipse);
                                    }
                                }

                                //if (flag2 == 0)
                                //{
                                //    await Task.Delay(3000);
                                //    flag2 = 1;
                                //}
                                
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

                                tblRightHandState.Content = rightHandState;
                                tblLeftHandState.Content = leftHandState;
                                #endregion

                                #region Untuk Membuat Dataset Baru
                                // Koordinat Tangan
                                a = (handLeft.Position.X); b = (handLeft.Position.Y);
                                x = (handRight.Position.X); y = (handRight.Position.Y);

                                if (flag == 0)
                                {
                                    deltaa = a; deltab = b;
                                    deltax = x; deltay = y;
                                    flag = 1;
                                }
                                else
                                {
                                    deltaa = a - tmpa; deltab = b - tmpy;
                                    deltax = x - tmpx; deltay = y - tmpy;
                                }
                                tmpa = a; tmpb = b;
                                tmpx = x; tmpy = y;

                                // kondisi idle

                                if (i < 40 && statusAmbil != 0)
                                {
                                    #region Status Frame
                                    if (i < 38)
                                    {
                                        ambilData.Content = (i + 1).ToString();
                                    }
                                    else ambilData.Content = "Done";
                                    #endregion

                                    #region Identifikasi Posisi
                                    if (i == 1)
                                    {
                                        leher = (neck.Position.Y);
                                        tengah = (spineMid.Position.Y);
                                    }
                                    #endregion

                                    #region Frame Tengah untuk Posisi
                                    if (i == 20)
                                    {
                                        //b handleft tanganKiri //y handright tanganKanan
                                        if (b > leher) tanganKiri = "Kepala";
                                        else if (b < tengah) tanganKiri = "Perut";
                                        else tanganKiri = "Dada";

                                        if (y > leher) tanganKanan = "Kepala";
                                        else if (y < tengah) tanganKanan = "Perut";
                                        else tanganKanan = "Dada";
                                    }
                                    #endregion

                                    #region Ekstraksi Fitur Sinamis
                                    //tangan kiri
                                    if (deltaa >= 0 && deltab >= 0)
                                    {
                                        alpha1 = (Math.Atan(deltab / deltaa)) * (180 / Math.PI);
                                    }
                                    else if (deltaa < 0)
                                    {
                                        alpha1 = (Math.Atan(deltab / deltaa)) * (180 / Math.PI) + 180;
                                    }
                                    else { alpha1 = (Math.Atan(deltab / deltaa)) * (180 / Math.PI) + 360; }

                                    //tangan kanan
                                    if (deltax >= 0 && deltay >= 0) { alpha2 = (Math.Atan(deltay / deltax)) * (180 / Math.PI); }
                                    else if (deltax < 0) { alpha2 = (Math.Atan(deltay / deltax)) * (180 / Math.PI) + 180; }
                                    else { alpha2 = (Math.Atan(deltay / deltax)) * (180 / Math.PI) + 360; }
                                    #endregion

                                    #region Fitur Dinamisnya
                                    //tangan kiri
                                    if (alpha1 > 314) { kuant1[i] = 8; }
                                    else if (alpha1 > 269) { kuant1[i] = 7; }
                                    else if (alpha1 > 224) { kuant1[i] = 6; }
                                    else if (alpha1 > 179) { kuant1[i] = 5; }
                                    else if (alpha1 > 134) { kuant1[i] = 4; }
                                    else if (alpha1 > 89) { kuant1[i] = 3; }
                                    else if (alpha1 > 44) { kuant1[i] = 2; }
                                    else { kuant1[i] = 1; }

                                    //tangan kanan
                                    if (alpha2 > 314) { kuant2[i] = 8; }
                                    else if (alpha2 > 269) { kuant2[i] = 7; }
                                    else if (alpha2 > 224) { kuant2[i] = 6; }
                                    else if (alpha2 > 179) { kuant2[i] = 5; }
                                    else if (alpha2 > 134) { kuant2[i] = 4; }
                                    else if (alpha2 > 89) { kuant2[i] = 3; }
                                    else if (alpha2 > 44) { kuant2[i] = 2; }
                                    else { kuant2[i] = 1; }
                                    #endregion

                                    #region Write Data
                                    if (i == 38)
                                    {
                                        var stringkuant4 = kuant1[4].ToString(); var stringkuant6 = kuant1[6].ToString(); var stringkuant8 = kuant1[8].ToString();
                                        var stringkuant10 = kuant1[10].ToString(); var stringkuant12 = kuant1[12].ToString(); var stringkuant14 = kuant1[14].ToString();
                                        var stringkuant16 = kuant1[16].ToString(); var stringkuant18 = kuant1[18].ToString(); var stringkuant20 = kuant1[20].ToString();
                                        var stringkuant22 = kuant1[22].ToString(); var stringkuant24 = kuant1[24].ToString(); var stringkuant26 = kuant1[26].ToString();
                                        var stringkuant28 = kuant1[28].ToString(); var stringkuant30 = kuant1[30].ToString(); var stringkuant32 = kuant1[32].ToString();
                                        var stringkuant34 = kuant1[34].ToString(); var stringkuant36 = kuant1[36].ToString(); var stringkuant38 = kuant1[38].ToString();

                                        var stringkuant43 = kuant2[4].ToString(); var stringkuant45 = kuant2[6].ToString(); var stringkuant47 = kuant2[8].ToString();
                                        var stringkuant49 = kuant2[10].ToString(); var stringkuant51 = kuant2[12].ToString(); var stringkuant53 = kuant2[14].ToString();
                                        var stringkuant55 = kuant2[16].ToString(); var stringkuant57 = kuant2[18].ToString(); var stringkuant59 = kuant2[20].ToString();
                                        var stringkuant61 = kuant2[22].ToString(); var stringkuant63 = kuant2[24].ToString(); var stringkuant65 = kuant2[26].ToString();
                                        var stringkuant67 = kuant2[28].ToString(); var stringkuant69 = kuant2[30].ToString(); var stringkuant71 = kuant2[32].ToString();
                                        var stringkuant73 = kuant2[34].ToString(); var stringkuant75 = kuant2[36].ToString(); var stringkuant77 = kuant2[38].ToString();
                                        var stringtangankiri = tanganKiri; var stringtangankanan = tanganKanan;
                                        var stringnamagerakan = namaGerakan;

                                        #region Training / Testing
                                        if (statusAmbil == 1)
                                        { 
                                            var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38}",
                                            stringkuant4, stringkuant6, stringkuant8, stringkuant10, stringkuant12, stringkuant14, stringkuant16,
                                            stringkuant18, stringkuant20, stringkuant22, stringkuant24, stringkuant26, stringkuant28, stringkuant30,
                                            stringkuant32, stringkuant34, stringkuant36, stringkuant38, stringkuant43, stringkuant45, stringkuant47,
                                            stringkuant49, stringkuant51, stringkuant53, stringkuant55, stringkuant57, stringkuant59, stringkuant61,
                                            stringkuant63, stringkuant65, stringkuant67, stringkuant69, stringkuant71, stringkuant73, stringkuant75,
                                            stringkuant77, stringtangankiri, stringtangankanan, stringnamagerakan);

                                            //memasukkan ke dalam baris
                                            csv.AppendLine(newLine);

                                            statusDetail.Content = "Data Created";
                                        }
                                        else if (statusAmbil == 2)
                                        {
                                            int kondisi = 3;
                                            if (kondisi == 1)
                                            {
                                                #region Data Sedikit
                                                if (stringtangankiri == "Perut")
                                                {
                                                    if (stringtangankanan == "Kepala")
                                                    {
                                                        if (kuant2[34] <= 3)
                                                        {
                                                            if (kuant2[22] <= 4)
                                                            {
                                                                outputText.Content = "Maklum";
                                                            }
                                                            else
                                                            {
                                                                if (kuant2[14] <= 7)
                                                                {
                                                                    outputText.Content = "Bingung";
                                                                }
                                                                else
                                                                {
                                                                    outputText.Content = "Awan";
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (kuant2[34] <= 6)
                                                            {
                                                                outputText.Content = "Topeng";
                                                            }
                                                            else
                                                            {
                                                                if (kuant2[18] <= 3)
                                                                {
                                                                    outputText.Content = "Maklum";
                                                                }
                                                                else
                                                                {
                                                                    outputText.Content = "Awan";
                                                                }
                                                            }
                                                        }

                                                    }
                                                    else if (stringtangankanan == "Perut")
                                                    {
                                                        if (kuant1[34] <= 6)
                                                        {
                                                            if (kuant2[14] <= 5)
                                                            {
                                                                outputText.Content = "Besar";
                                                            }
                                                            else
                                                            {
                                                                outputText.Content = "Badan";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (kuant1[18] <= 6)
                                                            {
                                                                if (kuant2[34] <= 6)
                                                                {
                                                                    outputText.Content = "Bola";
                                                                }
                                                                else
                                                                {
                                                                    outputText.Content = "Badan";
                                                                }
                                                            }
                                                            else
                                                            {
                                                                outputText.Content = "Anak";
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        outputText.Content = "Topeng";
                                                    }
                                                }
                                                else if (kuant1[10] <= 6)
                                                {
                                                    outputText.Content = "Bingkai";
                                                }
                                                else
                                                {
                                                    outputText.Content = "Rujuk";
                                                }
                                                #endregion
                                            }
                                            else if (kondisi == 2)
                                            {
                                                #region Data Banyak
                                                if (stringtangankiri == "Perut")
                                                {
                                                    if(stringtangankanan == "Kepala")
                                                    {
                                                        if (kuant2[34] <= 3)
                                                        {
                                                            if (kuant2[22] <= 4)
                                                            {
                                                                outputText.Content = "Maklum";
                                                            }
                                                            else
                                                            {
                                                                if (kuant2[14] <= 7)
                                                                {
                                                                    outputText.Content = "Bingung";
                                                                }
                                                                else
                                                                {
                                                                    outputText.Content = "Awan";
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (kuant2[34] <= 6)
                                                            {
                                                                outputText.Content = "Topeng";
                                                            }
                                                            else
                                                            {
                                                                if (kuant2[18] <= 3)
                                                                {
                                                                    outputText.Content = "Maklum";
                                                                }
                                                                else
                                                                {
                                                                    outputText.Content = "Awan";
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else if(stringtangankanan == "Perut")
                                                    {
                                                        if (kuant1[14] <= 6)
                                                        {
                                                            if (kuant1[34] <= 6)
                                                            {
                                                                if (kuant1[16] <= 6)
                                                                {
                                                                    outputText.Content = "Besar";
                                                                }
                                                                else
                                                                {
                                                                    outputText.Content = "Badan";
                                                                }
                                                            }
                                                            else
                                                            {
                                                                outputText.Content = "Bola";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (kuant1[34] <= 6)
                                                            {
                                                                if(kuant2[14] <= 5)
                                                                {
                                                                    outputText.Content = "Sempit";
                                                                }
                                                                else
                                                                {
                                                                    outputText.Content = "Badan";
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (kuant2[28] <= 6)
                                                                {
                                                                    if (kuant2[28] <= 2)
                                                                    {
                                                                        outputText.Content = "Samping";
                                                                    }
                                                                    else
                                                                    {
                                                                        if (kuant2[30] <= 5)
                                                                        {
                                                                            if (kuant1[26] <= 6)
                                                                            {
                                                                                outputText.Content = "Sempit";
                                                                            }
                                                                            else
                                                                            {
                                                                                outputText.Content = "Sama";
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            outputText.Content = "Sempit";
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if(kuant2[10] <= 6)
                                                                    {
                                                                        if(kuant2[34] <= 4)
                                                                        {
                                                                            outputText.Content = "Samping";
                                                                        }
                                                                        else
                                                                        {
                                                                            outputText.Content = "Anak";
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        outputText.Content = "Samping";
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (kuant2[16] <= 3)
                                                        {
                                                            if (kuant2[30] <= 4)
                                                            {
                                                                outputText.Content = "Lengkung";
                                                            }
                                                            else
                                                            {
                                                                outputText.Content = "Gelombang";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (kuant2[18] <= 6)
                                                            {
                                                                outputText.Content = "Faedah";
                                                            }
                                                            else
                                                            {
                                                                if (kuant2[16] <= 5)
                                                                {
                                                                    outputText.Content = "Faedah";
                                                                }
                                                                else
                                                                {
                                                                    outputText.Content = "Gelombang";
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (stringtangankiri == "Dada")
                                                {
                                                    if (kuant1[22] <= 6 )
                                                    {
                                                        outputText.Content = "Bingkai";
                                                    }
                                                    else
                                                    {
                                                        outputText.Content = "Rujuk";
                                                    }
                                                }
                                                else
                                                {
                                                    if (kuant2[36] <= 7)
                                                    {
                                                        if (kuant2[36] <= 3)
                                                        {
                                                            outputText.Content = "Kijang";
                                                        }
                                                        else
                                                        {
                                                            outputText.Content = "Selubung";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        outputText.Content = "Kijang";
                                                    }
                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                #region Data Banyak
                                                if (stringtangankiri == "Perut")
                                                {
                                                    if (stringtangankanan == "Kepala")
                                                    {
                                                        if (kuant2[34] <= 3)
                                                        {
                                                            if (kuant2[22] <= 4)
                                                            {
                                                                outputText.Content = "Maklum";
                                                            }
                                                            else
                                                            {
                                                                if (kuant2[14] <= 7)
                                                                {
                                                                    outputText.Content = "Bingung";
                                                                }
                                                                else
                                                                {
                                                                    outputText.Content = "Awan";
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (kuant2[34] <= 6)
                                                            {
                                                                outputText.Content = "Topeng";
                                                            }
                                                            else
                                                            {
                                                                if (kuant2[18] <= 3)
                                                                {
                                                                    outputText.Content = "Maklum";
                                                                }
                                                                else
                                                                {
                                                                    outputText.Content = "Awan";
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else if (stringtangankanan == "Perut")
                                                    {
                                                        if (kuant1[14] <= 6)
                                                        {
                                                            if (kuant1[34] <= 6)
                                                            {
                                                                if (kuant1[16] <= 6)
                                                                {
                                                                    outputText.Content = "Besar";
                                                                }
                                                                else
                                                                {
                                                                    outputText.Content = "Badan";
                                                                }
                                                            }
                                                            else
                                                            {
                                                                outputText.Content = "Bola";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (kuant1[34] <= 6)
                                                            {
                                                                if (kuant2[14] <= 5)
                                                                {
                                                                    outputText.Content = "Sempit";
                                                                }
                                                                else
                                                                {
                                                                    outputText.Content = "Badan";
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (kuant2[28] <= 6)
                                                                {
                                                                    if (kuant2[28] <= 2)
                                                                    {
                                                                        outputText.Content = "Samping";
                                                                    }
                                                                    else
                                                                    {
                                                                        if (kuant2[30] <= 5)
                                                                        {
                                                                            if (kuant1[26] <= 6)
                                                                            {
                                                                                outputText.Content = "Sempit";
                                                                            }
                                                                            else
                                                                            {
                                                                                outputText.Content = "Sama";
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            outputText.Content = "Sempit";
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (kuant2[10] <= 6)
                                                                    {
                                                                        if (kuant2[34] <= 4)
                                                                        {
                                                                            outputText.Content = "Samping";
                                                                        }
                                                                        else
                                                                        {
                                                                            outputText.Content = "Anak";
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        outputText.Content = "Samping";
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (kuant2[16] <= 3)
                                                        {
                                                            if (kuant2[30] <= 4)
                                                            {
                                                                outputText.Content = "Lengkung";
                                                            }
                                                            else
                                                            {
                                                                outputText.Content = "Gelombang";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (kuant2[18] <= 6)
                                                            {
                                                                outputText.Content = "Faedah";
                                                            }
                                                            else
                                                            {
                                                                if (kuant2[16] <= 5)
                                                                {
                                                                    outputText.Content = "Faedah";
                                                                }
                                                                else
                                                                {
                                                                    outputText.Content = "Gelombang";
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (stringtangankiri == "Dada")
                                                {
                                                    if (kuant2[24] <= 6)
                                                    {
                                                        if (kuant2[14] <= 3)
                                                        {
                                                            outputText.Content = "Gang";
                                                        }
                                                        else
                                                        {
                                                            if (kuant2[16] <= 6)
                                                            {
                                                                outputText.Content = "Rujuk";
                                                            }
                                                            else
                                                            {
                                                                outputText.Content = "Gang";
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (kuant2[22] <= 6)
                                                        {
                                                            outputText.Content = "Gang";
                                                        }
                                                        else
                                                        {
                                                            outputText.Content = "Bingkai";
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (kuant2[36] <= 7)
                                                    {
                                                        if (kuant2[36] <= 3)
                                                        {
                                                            outputText.Content = "Kijang";
                                                        }
                                                        else
                                                        {
                                                            outputText.Content = "Selubung";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        outputText.Content = "Kijang";
                                                    }
                                                }
                                                #endregion
                                            }

                                            string imageFullPath = imagePath + outputText.Content + ".bmp";
                                            string imageFullPath2 = imagePath + outputText.Content + ".jpg";

                                            if (File.Exists(imageFullPath))
                                                outputImage.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(imageFullPath);
                                            if (File.Exists(imageFullPath2))
                                                outputImage.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(imageFullPath2);
                                        }
                                        i = -1;
                                        flag = 0;
                                        flag2 = 0;
                                        #endregion
                                    }
                                    i++;
                                    #endregion
                                }
                                #endregion
                            }
                        }
                    }
                    if (statusAmbil == 1)
                    {
                        File.AppendAllText(filePath, csv.ToString());
                    }
                }
            }
        }
        #endregion

        private void OneTestButton_Click(object sender, RoutedEventArgs e)
        {
            statusDetail.Content = "Testing Data";
            statusAmbil = 2;
            flag2 = 0;
        }

        private void createButton_click(object sender, RoutedEventArgs e)
        {
            statusDetail.Content = "Create Dataset";
            statusAmbil = 1;
            flag2 = 0;

            namaGerakan = fileName.Text;
        }
    }
}
