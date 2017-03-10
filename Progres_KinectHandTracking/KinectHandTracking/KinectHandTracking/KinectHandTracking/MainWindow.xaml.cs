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

        //Tangan Kiri
        private double a = 0, b = 0, c = 0, deltaa = 0, deltab = 0, deltac = 0, alpha1 = 0, tmpa = 0, tmpb = 0, tmpc = 0;
        private int[] kuant1 = new int[999];
        //Tangan Kanan
        private double x = 0, y = 0, z = 0, deltax = 0, deltay = 0, deltaz = 0, alpha2 = 0, tmpx = 0, tmpy = 0, tmpz = 0;
        private int[] kuant2 = new int[999];
        //Penentuan Posisi
        private double leher = 0, tengah = 0;
        //Posisinya
        private string tanganKanan = "", tanganKiri = "";

        //Penanda
        private int flag = 0;
        private int i = 0;

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

                    // Deklarasi Variabel untuk Write Data
                    var csv = new StringBuilder();
                    string filePath = "F:\\Eka\\ITS\\KULIAH\\SEMESTER 7\\TAkisgan\\Realtime\\DataSet 2\\Percobaan.csv";


                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Identifikasi Tulang
                                //Joint shoulderRight = body.Joints[JointType.ShoulderRight];
                                Joint handRight = body.Joints[JointType.HandRight];
                                //Joint thumbRight = body.Joints[JointType.ThumbRight];
                                //Joint shoulderLeft = body.Joints[JointType.ShoulderLeft];
                                Joint handLeft = body.Joints[JointType.HandLeft];
                                //Joint thumbLeft = body.Joints[JointType.ThumbLeft];
                                Joint neck = body.Joints[JointType.Neck];
                                Joint spineMid = body.Joints[JointType.SpineMid];
                  
                                // Menggambar Skeleton
                                canvas.DrawSkeleton(body, _sensor.CoordinateMapper);

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

                                #region Untuk Membuat Dataset Baru
                                // Jika Tangan Kanan & Kiri Terbuka
                                if (body.HandRightState == HandState.Open && body.HandLeftState == HandState.Open)
                                {
                                    // Koordinat Tangan
                                    a = (handLeft.Position.X); b = (handLeft.Position.Y); //c = (handLeft.Position.Z);
                                    x = (handRight.Position.X); y = (handRight.Position.Y); //z = (handRight.Position.Z);

                                    // Frame Pertama
                                    if (flag == 0)
                                    {
                                        deltaa = a; deltab = b; //deltac = c;
                                        deltax = x; deltay = y; //deltaz = z;
                                        flag = 1;
                                    }
                                    else
                                    {
                                        deltaa = a - tmpa; deltab = b - tmpy; //deltac = c - tmpc;
                                        deltax = x - tmpx; deltay = y - tmpy; //deltaz = z - tmpz;
                                    }
                                    tmpa = a; tmpb = b; //tmpc = c;
                                    tmpx = x; tmpy = y; //tmpz = z;

                                    if (i < 40)
                                    {
                                        if (i == 1)
                                        {
                                            leher = (neck.Position.Y);
                                            tengah = (spineMid.Position.Y);
                                        }

                                        if (i == 19)
                                        {
                                            //b handleft tanganKiri //y handright tanganKanan
                                            if (b > leher) tanganKiri = "Atas Kepala";
                                            else if (b < tengah) tanganKiri = "Perut";
                                            else tanganKiri = "Dada";

                                            if (y > leher) tanganKanan = "Atas Kepala";
                                            else if (y < tengah) tanganKanan = "Perut";
                                            else tanganKanan = "Dada";
                                        }

                                        //ekstraksi fitur dinamis
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

                                        //fitur dinamisnya
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

                                        //write ke csv
                                        if (i == 38)
                                        {
                                            var stringkuant0 = kuant1[0].ToString(); var stringkuant1 = kuant1[1].ToString(); var stringkuant2 = kuant1[2].ToString();
                                            var stringkuant3 = kuant1[3].ToString(); var stringkuant4 = kuant1[4].ToString(); var stringkuant5 = kuant1[5].ToString();
                                            var stringkuant6 = kuant1[6].ToString(); var stringkuant7 = kuant1[7].ToString(); var stringkuant8 = kuant1[8].ToString();
                                            var stringkuant9 = kuant1[9].ToString(); var stringkuant10 = kuant1[10].ToString(); var stringkuant11 = kuant1[11].ToString();
                                            var stringkuant12 = kuant1[12].ToString(); var stringkuant13 = kuant1[13].ToString(); var stringkuant14 = kuant1[14].ToString();
                                            var stringkuant15 = kuant1[15].ToString(); var stringkuant16 = kuant1[16].ToString(); var stringkuant17 = kuant1[17].ToString();
                                            var stringkuant18 = kuant1[18].ToString(); var stringkuant19 = kuant1[19].ToString(); var stringkuant20 = kuant1[20].ToString();
                                            var stringkuant21 = kuant1[21].ToString(); var stringkuant22 = kuant1[22].ToString(); var stringkuant23 = kuant1[23].ToString();
                                            var stringkuant24 = kuant1[24].ToString(); var stringkuant25 = kuant1[25].ToString(); var stringkuant26 = kuant1[26].ToString();
                                            var stringkuant27 = kuant1[27].ToString(); var stringkuant28 = kuant1[28].ToString(); var stringkuant29 = kuant1[29].ToString();
                                            var stringkuant30 = kuant1[30].ToString(); var stringkuant31 = kuant1[31].ToString(); var stringkuant32 = kuant1[32].ToString();
                                            var stringkuant33 = kuant1[33].ToString(); var stringkuant34 = kuant1[34].ToString(); var stringkuant35 = kuant1[35].ToString();
                                            var stringkuant36 = kuant1[36].ToString(); var stringkuant37 = kuant1[37].ToString(); var stringkuant38 = kuant1[38].ToString();
                                            var stringkuant39 = kuant2[0].ToString(); var stringkuant40 = kuant2[1].ToString(); var stringkuant41 = kuant2[2].ToString();
                                            var stringkuant42 = kuant2[3].ToString(); var stringkuant43 = kuant2[4].ToString(); var stringkuant44 = kuant2[5].ToString();
                                            var stringkuant45 = kuant2[6].ToString(); var stringkuant46 = kuant2[7].ToString(); var stringkuant47 = kuant2[8].ToString();
                                            var stringkuant48 = kuant2[9].ToString(); var stringkuant49 = kuant2[10].ToString(); var stringkuant50 = kuant2[11].ToString();
                                            var stringkuant51 = kuant2[12].ToString(); var stringkuant52 = kuant2[13].ToString(); var stringkuant53 = kuant2[14].ToString();
                                            var stringkuant54 = kuant2[15].ToString(); var stringkuant55 = kuant2[16].ToString(); var stringkuant56 = kuant2[17].ToString();
                                            var stringkuant57 = kuant2[18].ToString(); var stringkuant58 = kuant2[19].ToString(); var stringkuant59 = kuant2[20].ToString();
                                            var stringkuant60 = kuant2[21].ToString(); var stringkuant61 = kuant2[22].ToString(); var stringkuant62 = kuant2[23].ToString();
                                            var stringkuant63 = kuant2[24].ToString(); var stringkuant64 = kuant2[25].ToString(); var stringkuant65 = kuant2[26].ToString();
                                            var stringkuant66 = kuant2[27].ToString(); var stringkuant67 = kuant2[28].ToString(); var stringkuant68 = kuant2[29].ToString();
                                            var stringkuant69 = kuant2[30].ToString(); var stringkuant70 = kuant2[31].ToString(); var stringkuant71 = kuant2[32].ToString();
                                            var stringkuant72 = kuant2[33].ToString(); var stringkuant73 = kuant2[34].ToString(); var stringkuant74 = kuant2[35].ToString();
                                            var stringkuant75 = kuant2[36].ToString(); var stringkuant76 = kuant2[37].ToString(); var stringkuant77 = kuant2[38].ToString();
                                            var stringtangankiri = tanganKiri; var stringtangankanan = tanganKanan;

                                            var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43},{44},{45},{46},{47},{48},{49},{50},{51},{52},{53},{54},{55},{56},{57},{58},{59},{60},{61},{62},{63},{64},{65},{66},{67},{68},{69},{70},{71},{72},{73},{74},{75},{76},{77},{78},{79}",
                                                stringkuant0, stringkuant1, stringkuant2, stringkuant3, stringkuant4, stringkuant5, stringkuant6, stringkuant7, stringkuant8,
                                                stringkuant9, stringkuant10, stringkuant11, stringkuant12, stringkuant13, stringkuant14, stringkuant15, stringkuant16, stringkuant17,
                                                stringkuant18, stringkuant19, stringkuant20, stringkuant21, stringkuant22, stringkuant23, stringkuant24, stringkuant25, stringkuant26,
                                                stringkuant27, stringkuant28, stringkuant29, stringkuant30, stringkuant31, stringkuant32, stringkuant33, stringkuant34, stringkuant35,
                                                stringkuant36, stringkuant37, stringkuant38, stringkuant39, stringkuant40, stringkuant41, stringkuant42, stringkuant43, stringkuant44,
                                                stringkuant45, stringkuant46, stringkuant47, stringkuant48, stringkuant49, stringkuant50, stringkuant51, stringkuant52, stringkuant53,
                                                stringkuant54, stringkuant55, stringkuant56, stringkuant57, stringkuant58, stringkuant59, stringkuant60, stringkuant61, stringkuant62,
                                                stringkuant63, stringkuant64, stringkuant65, stringkuant66, stringkuant67, stringkuant68, stringkuant69, stringkuant70, stringkuant71,
                                                stringkuant72, stringkuant73, stringkuant74, stringkuant75, stringkuant76, stringkuant77, stringtangankiri, stringtangankanan);

                                            //memasukkan ke dalam baris
                                            csv.AppendLine(newLine);
                                        }
                                        i++;
                                    }
                                }
                                #endregion
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
