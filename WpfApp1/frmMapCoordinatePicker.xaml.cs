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
    /// frmMapCoordinatePicker.xaml 的交互逻辑
    /// </summary>
    public partial class frmMapCoordinatePicker : Window
    {
        double panFactor = 0.0005;
        int intlstFocusIndex = 0;
        bool selected = false;
        WayPoint wp = new WayPoint();
        DispatcherTimer tmrButtonCheck = new DispatcherTimer();
        DispatcherTimer tmrFormMonitor = new DispatcherTimer();

        //double panFactor = 0.025;

        public frmMapCoordinatePicker()
        {
            InitializeComponent();
            Global.LocationConfirmed = false;
            Content_Nav.Content = GlobalNavigation.NavCommUserControl;
            Content_Map.Content = Global.globalMap;
        }

        public frmMapCoordinatePicker(int _intlstFocusIndex, bool _selected, WayPoint _wp)
        {
            InitializeComponent();
            Global.LocationConfirmed = false;
            Content_Nav.Content = GlobalNavigation.NavCommUserControl;
            Content_Map.Content = Global.globalMap;
            intlstFocusIndex = _intlstFocusIndex;
            selected = _selected;
            wp = _wp;
            Global.globalMap.Position = wp.PointLATLNGGCJ02;

            tmrButtonCheck.Tick += new EventHandler(tmrButtonCheck_Tick);
            tmrButtonCheck.Interval = TimeSpan.FromMilliseconds(5);
            tmrButtonCheck.Start();

            tmrFormMonitor.Tick += new EventHandler(tmrFormMonitor_Tick);
            tmrFormMonitor.Interval = TimeSpan.FromSeconds(2);

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
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

        private void North_Press()
        {
            Global.globalMap.Position = new PointLatLng(Global.globalMap.Position.Lat + panFactor, Global.globalMap.Position.Lng);
        }

        private void Lbl_North_MouseUp(object sender, MouseButtonEventArgs e)
        {
            North_Press();
        }

        private void South_Press()
        {
            Global.globalMap.Position = new PointLatLng(Global.globalMap.Position.Lat - panFactor, Global.globalMap.Position.Lng);
        }

        private void Lbl_South_MouseUp(object sender, MouseButtonEventArgs e)
        {
            South_Press();
        }

        private void ZoomPlus_Press()
        {
            if (Global.globalMap.Zoom < Global.globalMap.MaxZoom)
                Global.globalMap.Zoom += 1;
        }

        private void Lbl_ZoomPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ZoomPlus_Press();
        }

        private void ZoomMinus_Press()
        {
            if (Global.globalMap.Zoom > Global.globalMap.MinZoom)
                Global.globalMap.Zoom -= 1;
        }

        private void Lbl_ZoomMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ZoomMinus_Press();
        }

        private void Home_Press()
        {
            DisposeAllComponent();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Lbl_Home_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Home_Press();
        }

        private void East_Press()
        {
            Global.globalMap.Position = new PointLatLng(Global.globalMap.Position.Lat, Global.globalMap.Position.Lng + panFactor);
        }

        private void Lbl_East_MouseUp(object sender, MouseButtonEventArgs e)
        {
            East_Press();
        }

        private void West_Press()
        {
            Global.globalMap.Position = new PointLatLng(Global.globalMap.Position.Lat, Global.globalMap.Position.Lng - panFactor);
        }

        private void Lbl_West_MouseUp(object sender, MouseButtonEventArgs e)
        {
            West_Press();
        }

        private void Select_Press()
        {
            wp.PointLATLNGGCJ02 = Global.globalMap.Position;
            //wp.PointLATLNG = Global.globalMap.Position;
            DisposeAllComponent();
            frmMarkerEdit frmMarkerEdit = new frmMarkerEdit(intlstFocusIndex, selected, wp);
            frmMarkerEdit.Show();
            this.Close();
        }

        private void Lbl_Select_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Select_Press();
        }

        private void Cancel_Press()
        {
            DisposeAllComponent();
            frmMarkerEdit frmMarkerEdit = new frmMarkerEdit(intlstFocusIndex, selected);
            frmMarkerEdit.Show();
            this.Close();
        }

        private void Lbl_Cancel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Cancel_Press();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tmrFormMonitor.Stop();
            tmrButtonCheck.Stop();
            Global.LocationConfirmed = true;
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


                if (GlobalUpBoard.GPIOLevel[1] == 0 && GlobalUpBoard.ButtonState[1] == false) //Pressed North Button
                {
                    North_Press();
                    GlobalUpBoard.ButtonState[1] = true;
                }
                if (GlobalUpBoard.GPIOLevel[1] == 1 && GlobalUpBoard.ButtonState[1] == true)
                    GlobalUpBoard.ButtonState[1] = false;

                if (GlobalUpBoard.GPIOLevel[2] == 0 && GlobalUpBoard.ButtonState[2] == false) //Pressed South Button
                {
                    South_Press();
                    GlobalUpBoard.ButtonState[2] = true;
                }
                if (GlobalUpBoard.GPIOLevel[2] == 1 && GlobalUpBoard.ButtonState[2] == true)
                    GlobalUpBoard.ButtonState[2] = false;

                if (GlobalUpBoard.GPIOLevel[3] == 0 && GlobalUpBoard.ButtonState[3] == false) //Pressed ZoomPlus Button
                {
                    ZoomPlus_Press();
                    GlobalUpBoard.ButtonState[3] = true;
                }
                if (GlobalUpBoard.GPIOLevel[3] == 1 && GlobalUpBoard.ButtonState[3] == true)
                    GlobalUpBoard.ButtonState[3] = false;

                if (GlobalUpBoard.GPIOLevel[4] == 0 && GlobalUpBoard.ButtonState[4] == false) //Pressed ZoomMinus Button
                {
                    ZoomMinus_Press();
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

                if (GlobalUpBoard.GPIOLevel[6] == 0 && GlobalUpBoard.ButtonState[6] == false) //Pressed East Button
                {
                    East_Press();
                    GlobalUpBoard.ButtonState[6] = true;
                }
                if (GlobalUpBoard.GPIOLevel[6] == 1 && GlobalUpBoard.ButtonState[6] == true)
                    GlobalUpBoard.ButtonState[6] = false;

                if (GlobalUpBoard.GPIOLevel[7] == 0 && GlobalUpBoard.ButtonState[7] == false) //Pressed West Button
                {
                    West_Press();
                    GlobalUpBoard.ButtonState[7] = true;
                }
                if (GlobalUpBoard.GPIOLevel[7] == 1 && GlobalUpBoard.ButtonState[7] == true)
                    GlobalUpBoard.ButtonState[7] = false;

                if (GlobalUpBoard.GPIOLevel[8] == 0 && GlobalUpBoard.ButtonState[8] == false) //Pressed Select Button
                {
                    Select_Press();
                    GlobalUpBoard.ButtonState[8] = true;
                }
                if (GlobalUpBoard.GPIOLevel[8] == 1 && GlobalUpBoard.ButtonState[8] == true)
                    GlobalUpBoard.ButtonState[8] = false;

                if (GlobalUpBoard.GPIOLevel[9] == 0 && GlobalUpBoard.ButtonState[9] == false) //Pressed Cancel Button
                {
                    Cancel_Press();
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
            Content_Map.Content = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = Global.TopMost;
        }
    }
}
