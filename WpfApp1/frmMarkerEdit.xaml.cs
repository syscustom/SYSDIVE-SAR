using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using GMap.NET;

namespace WpfApp1
{
    /// <summary>
    /// frmMarkerEdit.xaml 的交互逻辑
    /// </summary>
    public partial class frmMarkerEdit : Window
    {
        int intlstFocusIndex = 0;
        bool selected = false;
        int intCurrentSelectItem = 0;
        bool blnIsHome = true;
        DispatcherTimer tmrButtonCheck = new DispatcherTimer();
        DispatcherTimer tmrFormMonitor = new DispatcherTimer();

        public frmMarkerEdit()
        {
            InitializeComponent();
        }

        public frmMarkerEdit(int _intlstFocusIndex, bool _selected)
        {
            InitializeComponent();
            Content_Nav.Content = GlobalNavigation.NavCommUserControl;
            intlstFocusIndex = _intlstFocusIndex;
            selected = _selected;

            lblNameInfo.Content = Global.LstWayPoints[_intlstFocusIndex].Name;
            double decimal_degrees;
            decimal_degrees =Global.LstWayPoints[_intlstFocusIndex].PointLATLNG.Lat;
            double minutes;
            minutes = (decimal_degrees - Math.Floor(decimal_degrees)) * 60.0;
            double seconds;
            seconds = (minutes - Math.Floor(minutes)) * 60.0;
            double tenths;
            tenths = (seconds - Math.Floor(seconds)) * 100;

            decimal_degrees = Math.Floor(decimal_degrees);
            minutes = Math.Floor(minutes);
            seconds = Math.Floor(seconds);
            tenths = Math.Floor(tenths);

            lblLatDeg.Content = decimal_degrees.ToString();// + "°";
            lblLatMin.Content = minutes.ToString();// + "'";
            lblLatSec1.Content = seconds.ToString();// + "''";
            lblLatSec2.Content = tenths.ToString();

            decimal_degrees = Global.LstWayPoints[_intlstFocusIndex].PointLATLNG.Lng;
            minutes = (decimal_degrees - Math.Floor(decimal_degrees)) * 60.0;
            seconds = (minutes - Math.Floor(minutes)) * 60.0;
            tenths = (seconds - Math.Floor(seconds)) * 100;

            decimal_degrees = Math.Floor(decimal_degrees);
            minutes = Math.Floor(minutes);
            seconds = Math.Floor(seconds);
            tenths = Math.Floor(tenths);

            lblLngDeg.Content = decimal_degrees.ToString();// + "°";
            lblLngMin.Content = minutes.ToString();// + "'";
            lblLngSec1.Content = seconds.ToString();// + "''";
            lblLngSec2.Content = tenths.ToString();

            lblDepthInfo.Content = Global.LstWayPoints[_intlstFocusIndex].Depth.ToString();

            tmrButtonCheck.Tick += new EventHandler(tmrButtonCheck_Tick);
            tmrButtonCheck.Interval = TimeSpan.FromMilliseconds(5);
            tmrButtonCheck.Start();

            tmrFormMonitor.Tick += new EventHandler(tmrFormMonitor_Tick);
            tmrFormMonitor.Interval = TimeSpan.FromSeconds(2);
        }

        public frmMarkerEdit(int _intlstFocusIndex, bool _selected, WayPoint _wp)
        {
            InitializeComponent();
            Content_Nav.Content = GlobalNavigation.NavCommUserControl;
            intlstFocusIndex = _intlstFocusIndex;
            selected = _selected;

            lblNameInfo.Content = _wp.Name;
            double decimal_degrees;
            decimal_degrees = _wp.PointLATLNG.Lat;
            double minutes;
            minutes = (decimal_degrees - Math.Floor(decimal_degrees)) * 60.0;
            double seconds;
            seconds = (minutes - Math.Floor(minutes)) * 60.0;
            double tenths;
            tenths = (seconds - Math.Floor(seconds)) * 100;

            decimal_degrees = Math.Floor(decimal_degrees);
            minutes = Math.Floor(minutes);
            seconds = Math.Floor(seconds);
            tenths = Math.Floor(tenths);

            lblLatDeg.Content = decimal_degrees.ToString();// + "°";
            lblLatMin.Content = minutes.ToString();// + "'";
            lblLatSec1.Content = seconds.ToString();// + "''";
            lblLatSec2.Content = tenths.ToString();

            decimal_degrees = _wp.PointLATLNG.Lng;
            minutes = (decimal_degrees - Math.Floor(decimal_degrees)) * 60.0;
            seconds = (minutes - Math.Floor(minutes)) * 60.0;
            tenths = (seconds - Math.Floor(seconds)) * 100;

            decimal_degrees = Math.Floor(decimal_degrees);
            minutes = Math.Floor(minutes);
            seconds = Math.Floor(seconds);
            tenths = Math.Floor(tenths);

            lblLngDeg.Content = decimal_degrees.ToString();// + "°";
            lblLngMin.Content = minutes.ToString();// + "'";
            lblLngSec1.Content = seconds.ToString();// + "''";
            lblLngSec2.Content = tenths.ToString();

            lblDepthInfo.Content = _wp.Depth.ToString();

            tmrButtonCheck.Tick += new EventHandler(tmrButtonCheck_Tick);
            tmrButtonCheck.Interval = TimeSpan.FromMilliseconds(5);
            tmrButtonCheck.Start(); ;

            tmrFormMonitor.Tick += new EventHandler(tmrFormMonitor_Tick);
            tmrFormMonitor.Interval = TimeSpan.FromSeconds(2);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = Global.TopMost;
            RereshInforamation(0);
        }

        private void Power_Press()
        {
            DisposeAllComponent();
            frmPower frmPower = new frmPower("frmMarkers");
            frmPower.Show();
            this.Close();
        }

        private void Lbl_Power_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Power_Press();
        }

        private void ImgUpArrow()
        {
            intCurrentSelectItem--;
            if (intCurrentSelectItem < 0) intCurrentSelectItem = 10;
            RereshInforamation(intCurrentSelectItem);
        }

        private void ImgUpArrow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ImgUpArrow();
        }

        private void ImgDownArrow()
        {
            intCurrentSelectItem++;
            if (intCurrentSelectItem > 10) intCurrentSelectItem = 0;
            RereshInforamation(intCurrentSelectItem);
        }

        private void ImgDownArrow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ImgDownArrow();
        }

        private void Cancel_Press()
        {
            DisposeAllComponent();
            frmMarkers frmMarkers = new frmMarkers();
            frmMarkers.Show();
            this.Close();
        }

        private void Lbl_Cancel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Cancel_Press();
        }

        private void Done_Press()
        {
            double latsec2 = Convert.ToDouble(lblLatSec2.Content);
            latsec2 /= 100;
            double latsec1 = Convert.ToDouble(lblLatSec1.Content);
            latsec1 += latsec2;
            latsec1 /= 60;
            double latmin = Convert.ToDouble(lblLatMin.Content);
            latmin += latsec1;
            latmin /= 60;
            double latdeg = Convert.ToDouble(lblLatDeg.Content);
            latdeg += latmin;


            double lngsec2 = Convert.ToDouble(lblLngSec2.Content);
            lngsec2 /= 100;
            double lngsec1 = Convert.ToDouble(lblLngSec1.Content);
            lngsec1 += lngsec2;
            lngsec1 /= 60;
            double lngmin = Convert.ToDouble(lblLngMin.Content);
            lngmin += lngsec1;
            lngmin /= 60;
            double lngdeg = Convert.ToDouble(lblLngDeg.Content);
            lngdeg += lngmin;

            Global.LstWayPoints[intlstFocusIndex].PointLATLNG = new PointLatLng(latdeg, lngdeg);
            Global.LstWayPoints[intlstFocusIndex].Depth = Convert.ToDouble(lblDepthInfo.Content);
            Global.LstWayPoints[intlstFocusIndex].Type = Global.LstWayPoints[intlstFocusIndex].Type;

            SelectXMLData.EditConfiguration(Global.MissionFileFullName, Global.LstWayPoints[intlstFocusIndex].ID.ToString(),
                Global.LstWayPoints[intlstFocusIndex].PointLATLNG.Lat.ToString(),
                Global.LstWayPoints[intlstFocusIndex].PointLATLNG.Lng.ToString(),
                Global.LstWayPoints[intlstFocusIndex].Depth.ToString(),
                Global.LstWayPoints[intlstFocusIndex].Type.ToString());

            Global.RemoveAllRoute();
            Global.RemoveAllWaypoints();
            Global.WaysPointsModified();

            DisposeAllComponent();

            frmMarkers frmMarkers = new frmMarkers();
            frmMarkers.Show();
            this.Close();
        }

        private void Lbl_Done_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Done_Press();
        }

        private void Home_Press()
        {
            DisposeAllComponent();
            if (blnIsHome)
            {
                MainWindow mainwindow = new MainWindow();
                mainwindow.Show();
            }
            else
            {
                WayPoint wp = new WayPoint();
                wp.ID = Global.LstWayPoints[intlstFocusIndex].ID;
                wp.Name = Global.LstWayPoints[intlstFocusIndex].Name;
                wp.PointLATLNG  = Global.LstWayPoints[intlstFocusIndex].PointLATLNG;
                wp.PointLATLNGGCJ02 = Global.LstWayPoints[intlstFocusIndex].PointLATLNGGCJ02;
                wp.Type = Global.LstWayPoints[intlstFocusIndex].Type;
                frmMapCoordinatePicker frmMapCoordinatePicker = new frmMapCoordinatePicker(intlstFocusIndex, selected, wp);
                frmMapCoordinatePicker.Show();
            }
            this.Close();
        }

        private void Lbl_Home_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Home_Press();
        }

        private void MinusTen_Press()
        {
            ModifyInformation(intCurrentSelectItem, -10);
        }

        private void Lbl_MinusTen_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MinusTen_Press();
        }

        private void MinusOne_Press()
        {
            ModifyInformation(intCurrentSelectItem, -1);
        }

        private void Lbl_MinusOne_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MinusOne_Press();
        }

        private void PlusOne_Press()
        {
            ModifyInformation(intCurrentSelectItem, 1);
        }

        private void Lbl_PlusOne_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PlusOne_Press();
        }

        private void PlusTen_Press()
        {
            ModifyInformation(intCurrentSelectItem, 10);
        }

        private void Lbl_PlusTen_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PlusTen_Press();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
        }

        private void ModifyInformation(int _index, int _increment)
        {
            int temp = 0;
            double temp1 = 0.0;
            switch (_index)
            {
                case 0:
                    temp = Convert.ToInt32(lblLatDeg.Content);
                    temp += _increment;
                    if (temp > 89) temp = 89;
                    if (temp < 0) temp = 0;
                    lblLatDeg.Content = temp.ToString();
                    break;
                case 1:
                    temp = Convert.ToInt32(lblLatMin.Content);
                    temp += _increment;
                    if (temp > 59) temp = 59;
                    if (temp < 0) temp = 0;
                    lblLatMin.Content = temp.ToString();
                    break;
                case 2:
                    temp = Convert.ToInt32(lblLatSec1.Content);
                    temp += _increment;
                    if (temp > 59) temp = 59;
                    if (temp < 0) temp = 0;
                    lblLatSec1.Content = temp.ToString();
                    break;
                case 3:
                    temp = Convert.ToInt32(lblLatSec2.Content);
                    temp += _increment;
                    if (temp > 99) temp = 99;
                    if (temp < 0) temp = 0;
                    lblLatSec2.Content = temp.ToString();
                    break;
                case 4:
                    if (_increment < 0)
                        lblLatInfo.Content = "N";
                    else if (_increment > 0)
                        lblLatInfo.Content = "S";
                    break;
                case 5:
                    temp = Convert.ToInt32(lblLngDeg.Content);
                    temp += _increment;
                    if (temp > 119) temp = 119;
                    if (temp < 0) temp = 0;
                    lblLngDeg.Content = temp.ToString();
                    break;
                case 6:
                    temp = Convert.ToInt32(lblLngMin.Content);
                    temp += _increment;
                    if (temp > 59) temp = 59;
                    if (temp < 0) temp = 0;
                    lblLngMin.Content = temp.ToString();
                    break;
                case 7:
                    temp = Convert.ToInt32(lblLngSec1.Content);
                    temp += _increment;
                    if (temp > 59) temp = 59;
                    if (temp < 0) temp = 0;
                    lblLngSec1.Content = temp.ToString();
                    break;
                case 8:
                    temp = Convert.ToInt32(lblLatSec2.Content);
                    temp += _increment;
                    if (temp > 99) temp = 99;
                    if (temp < 0) temp = 0;
                    lblLngSec2.Content = temp.ToString();
                    break;
                case 9:
                    if (_increment < 0)
                        lblLngInfo.Content = "E";
                    else if (_increment > 0)
                        lblLngInfo.Content = "W";
                    break;
                case 10:
                    temp1 = Convert.ToDouble(lblDepthInfo.Content);
                    temp1 += _increment * 0.1;
                    if (temp1 > 99.0) temp1 = 99.0;
                    if (temp1 < 0.0) temp1 = 0.0;
                    lblDepthInfo.Content = temp1.ToString();
                    break;
            }
        }

        private void RereshInforamation(int _index)
        {
            lblLatDeg.Background = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
            lblLatMin.Background = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
            lblLatSec1.Background = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
            lblLatSec2.Background = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
            lblLatInfo.Background = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));

            lblLngDeg.Background = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
            lblLngMin.Background = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
            lblLngSec1.Background = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
            lblLngSec2.Background = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
            lblLngInfo.Background = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));

            lblDepthInfo.Background = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));

            
            switch (_index)
            {
                case 0:
                    lblLatDeg.Background = Brushes.Blue;
                    //lbl_Home.Content = "地 图 选 择";
                    //blnIsHome = false;
                    break;
                case 1:
                    lblLatMin.Background = Brushes.Blue;
                    //lbl_Home.Content = "地 图 选 择";
                    //blnIsHome = false;
                    break;
                case 2:
                    lblLatSec1.Background = Brushes.Blue;
                    //lbl_Home.Content = "地 图 选 择";
                    //blnIsHome = false;
                    break;
                case 3:
                    lblLatSec2.Background = Brushes.Blue;
                    //lbl_Home.Content = "地 图 选 择";
                    //blnIsHome = false;
                    break;
                case 4:
                    lblLatInfo.Background = Brushes.Blue;
                    //lbl_Home.Content = "地 图 选 择";
                    //blnIsHome = false;
                    break;
                case 5:
                    lblLngDeg.Background = Brushes.Blue;
                    //lbl_Home.Content = "地 图 选 择";
                    //blnIsHome = false;
                    break;
                case 6:
                    lblLngMin.Background = Brushes.Blue;
                    //lbl_Home.Content = "地 图 选 择";
                    //blnIsHome = false;
                    break;
                case 7:
                    lblLngSec1.Background = Brushes.Blue;
                    //lbl_Home.Content = "地 图 选 择";
                    //blnIsHome = false;
                    break;
                case 8:
                    lblLngSec2.Background = Brushes.Blue;
                    //lbl_Home.Content = "地 图 选 择";
                    //blnIsHome = false;
                    break;
                case 9:
                    lblLngInfo.Background = Brushes.Blue;
                    //lbl_Home.Content = "地 图 选 择";
                    //blnIsHome = false;
                    break;
                case 10:
                    lblDepthInfo.Background = Brushes.Blue;
                    //lbl_Home.Content = "主 页";
                    //blnIsHome = true;
                    break;
            }
            
        }

        void tmrButtonCheck_Tick(object sender, EventArgs e)
        {
            if (this.IsActive)
            {
                if (GlobalUpBoard.GPIOLevel[0] == 0 && GlobalUpBoard.ButtonState[0] == false) //Pressed Power Button
                {
                    Power_Press();
                    GlobalUpBoard.ButtonState[0] = true;
                }
                if (GlobalUpBoard.GPIOLevel[0] == 1 && GlobalUpBoard.ButtonState[0] == true)
                    GlobalUpBoard.ButtonState[0] = false;


                if (GlobalUpBoard.GPIOLevel[1] == 0 && GlobalUpBoard.ButtonState[1] == false) //Pressed ImgUpArrow Button
                {
                    ImgUpArrow();
                    GlobalUpBoard.ButtonState[1] = true;
                }
                if (GlobalUpBoard.GPIOLevel[1] == 1 && GlobalUpBoard.ButtonState[1] == true)
                    GlobalUpBoard.ButtonState[1] = false;

                if (GlobalUpBoard.GPIOLevel[2] == 0 && GlobalUpBoard.ButtonState[2] == false) //Pressed ImgDownArrow Button
                {
                    ImgDownArrow();
                    GlobalUpBoard.ButtonState[2] = true;
                }
                if (GlobalUpBoard.GPIOLevel[2] == 1 && GlobalUpBoard.ButtonState[2] == true)
                    GlobalUpBoard.ButtonState[2] = false;

                if (GlobalUpBoard.GPIOLevel[3] == 0 && GlobalUpBoard.ButtonState[3] == false) //Pressed Cancel Button
                {
                    Cancel_Press();
                    GlobalUpBoard.ButtonState[3] = true;
                }
                if (GlobalUpBoard.GPIOLevel[3] == 1 && GlobalUpBoard.ButtonState[3] == true)
                    GlobalUpBoard.ButtonState[3] = false;

                if (GlobalUpBoard.GPIOLevel[4] == 0 && GlobalUpBoard.ButtonState[4] == false) //Pressed Done Button
                {
                    Done_Press();
                    GlobalUpBoard.ButtonState[4] = true;
                }
                if (GlobalUpBoard.GPIOLevel[4] == 1 && GlobalUpBoard.ButtonState[4] == true)
                    GlobalUpBoard.ButtonState[4] = false;



                if (GlobalUpBoard.GPIOLevel[5] == 0 && GlobalUpBoard.ButtonState[5] == false) //Pressed Home Button
                {
                    Home_Press();
                    GlobalUpBoard.ButtonState[5] = true;
                }
                if (GlobalUpBoard.GPIOLevel[5] == 1 && GlobalUpBoard.ButtonState[5] == true)
                    GlobalUpBoard.ButtonState[5] = false;

                if (GlobalUpBoard.GPIOLevel[6] == 0 && GlobalUpBoard.ButtonState[6] == false) //Pressed -10 Button
                {
                    MinusTen_Press();
                    GlobalUpBoard.ButtonState[6] = true;
                }
                if (GlobalUpBoard.GPIOLevel[6] == 1 && GlobalUpBoard.ButtonState[6] == true)
                    GlobalUpBoard.ButtonState[6] = false;

                if (GlobalUpBoard.GPIOLevel[7] == 0 && GlobalUpBoard.ButtonState[7] == false) //Pressed -1 Button
                {
                    MinusOne_Press();
                    GlobalUpBoard.ButtonState[7] = true;
                }
                if (GlobalUpBoard.GPIOLevel[7] == 1 && GlobalUpBoard.ButtonState[7] == true)
                    GlobalUpBoard.ButtonState[7] = false;

                if (GlobalUpBoard.GPIOLevel[8] == 0 && GlobalUpBoard.ButtonState[8] == false) //Pressed +1 Button
                {
                    PlusOne_Press();
                    GlobalUpBoard.ButtonState[8] = true;
                }
                if (GlobalUpBoard.GPIOLevel[8] == 1 && GlobalUpBoard.ButtonState[8] == true)
                    GlobalUpBoard.ButtonState[8] = false;

                if (GlobalUpBoard.GPIOLevel[9] == 0 && GlobalUpBoard.ButtonState[9] == false) //Pressed +10 Button
                {
                    PlusTen_Press();
                    GlobalUpBoard.ButtonState[9] = true;
                }
                if (GlobalUpBoard.GPIOLevel[9] == 1 && GlobalUpBoard.ButtonState[9] == true)
                    GlobalUpBoard.ButtonState[9] = false;

            }

        }

        void tmrFormMonitor_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DisposeAllComponent()
        {
            tmrFormMonitor.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tmrFormMonitor.Stop();
            tmrButtonCheck.Stop();
        }
    }
}
