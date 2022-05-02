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

namespace WpfApp1
{
    /// <summary>
    /// frmMarkersRemoveConfirmation.xaml 的交互逻辑
    /// </summary>
    public partial class frmMarkersRemoveConfirmation : Window
    {
        int intlstFocusIndex = 0;
        bool selected = false;

        DispatcherTimer tmrButtonCheck = new DispatcherTimer();
        DispatcherTimer tmrFormMonitor = new DispatcherTimer();

        public frmMarkersRemoveConfirmation()
        {
            InitializeComponent();
        }

        public frmMarkersRemoveConfirmation(int _intlstFocusIndex, bool _selected)
        {
            InitializeComponent();
            intlstFocusIndex = _intlstFocusIndex;
            selected = _selected;
            RereshInforamation(_intlstFocusIndex);
            tmrButtonCheck.Tick += new EventHandler(tmrButtonCheck_Tick);
            tmrButtonCheck.Interval = TimeSpan.FromMilliseconds(5);
            tmrButtonCheck.Start();

            tmrFormMonitor.Tick += new EventHandler(tmrFormMonitor_Tick);
            tmrFormMonitor.Interval = TimeSpan.FromSeconds(2);
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

        private void Home_Press()
        {
            DisposeAllComponent();
            MainWindow mainwindows = new MainWindow();
            mainwindows.Show();
            this.Close();
        }

        private void Lbl_Home_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Home_Press();
        }

        private void Yes_Press()
        {
            if (selected)
            {

            }

            SelectXMLData.DeleteConfiguration(Global.MissionFileFullName, Global.LstWayPoints[intlstFocusIndex].ID);
            Global.LstWayPoints.RemoveAt(intlstFocusIndex);
            Global.RemoveAllRoute();
            Global.RemoveAllWaypoints();
            Global.WaysPointsModified();
            DisposeAllComponent();
            frmMarkers frmMarkers = new frmMarkers();
            frmMarkers.Show();
            this.Close();
        }

        private void Lbl_Yes_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Yes_Press();
        }

        private void No_Press()
        {
            DisposeAllComponent();
            frmMarkers frmMarkers = new frmMarkers();
            frmMarkers.Show();
            this.Close();
        }

        private void Lbl_No_MouseUp(object sender, MouseButtonEventArgs e)
        {
            No_Press();
        }

        private void RereshInforamation(int _index)
        {
            lblNameInfo.Content = Global.LstWayPoints[_index].Name;
            lblLat.Content = Global.LstWayPoints[_index].PointLATLNG.Lat.ToString();
            lblLng.Content = Global.LstWayPoints[_index].PointLATLNG.Lng.ToString();
            lblDepthInfo.Content = Global.LstWayPoints[_index].Depth.ToString();
            switch (Global.LstWayPoints[_index].Type)
            {
                case 1:
                    lblTypeInfo.Content = "路径点";
                    break;
                case 2:
                    lblTypeInfo.Content = "目标点";
                    break;
            }
            lblDistanceToMarkerInfo.Content = Global.CalcDistance(Global.currentMarker.Position, Global.LstWayPoints[_index].PointLATLNG).ToString("0.0") + "米";
            lblBearingToMarkerInfo.Content = Global.CalcBearing(Global.currentMarker.Position, Global.LstWayPoints[_index].PointLATLNG).ToString("0.0") + "度";
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
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


                if (GlobalUpBoard.GPIOLevel[1] == 0 && GlobalUpBoard.ButtonState[1] == false) //Pressed  Button
                {

                    GlobalUpBoard.ButtonState[1] = true;
                }
                if (GlobalUpBoard.GPIOLevel[1] == 1 && GlobalUpBoard.ButtonState[1] == true)
                    GlobalUpBoard.ButtonState[1] = false;

                if (GlobalUpBoard.GPIOLevel[2] == 0 && GlobalUpBoard.ButtonState[2] == false) //Pressed  Button
                {
                    GlobalUpBoard.ButtonState[2] = true;
                }
                if (GlobalUpBoard.GPIOLevel[2] == 1 && GlobalUpBoard.ButtonState[2] == true)
                    GlobalUpBoard.ButtonState[2] = false;

                if (GlobalUpBoard.GPIOLevel[3] == 0 && GlobalUpBoard.ButtonState[3] == false) //Pressed  Button
                {

                    GlobalUpBoard.ButtonState[3] = true;
                }
                if (GlobalUpBoard.GPIOLevel[3] == 1 && GlobalUpBoard.ButtonState[3] == true)
                    GlobalUpBoard.ButtonState[3] = false;

                if (GlobalUpBoard.GPIOLevel[4] == 0 && GlobalUpBoard.ButtonState[4] == false) //Pressed  Button
                {

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

                if (GlobalUpBoard.GPIOLevel[6] == 0 && GlobalUpBoard.ButtonState[6] == false) //Pressed Yes Button
                {
                    Yes_Press();
                    GlobalUpBoard.ButtonState[6] = true;
                }
                if (GlobalUpBoard.GPIOLevel[6] == 1 && GlobalUpBoard.ButtonState[6] == true)
                    GlobalUpBoard.ButtonState[6] = false;

                if (GlobalUpBoard.GPIOLevel[7] == 0 && GlobalUpBoard.ButtonState[7] == false) //Pressed No Button
                {
                    No_Press();
                    GlobalUpBoard.ButtonState[7] = true;
                }
                if (GlobalUpBoard.GPIOLevel[7] == 1 && GlobalUpBoard.ButtonState[7] == true)
                    GlobalUpBoard.ButtonState[7] = false;

                if (GlobalUpBoard.GPIOLevel[8] == 0 && GlobalUpBoard.ButtonState[8] == false) //Pressed  Button
                {

                    GlobalUpBoard.ButtonState[8] = true;
                }
                if (GlobalUpBoard.GPIOLevel[8] == 1 && GlobalUpBoard.ButtonState[8] == true)
                    GlobalUpBoard.ButtonState[8] = false;

                if (GlobalUpBoard.GPIOLevel[9] == 0 && GlobalUpBoard.ButtonState[9] == false) //Pressed  Button
                {

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = Global.TopMost;
        }
    }
}
