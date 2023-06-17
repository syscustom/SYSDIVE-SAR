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
    /// frmNavigationSettings.xaml 的交互逻辑
    /// </summary>
    public partial class frmNavigationSettings : Window
    {
        DispatcherTimer tmrButtonCheck = new DispatcherTimer();
        DispatcherTimer tmrFormMonitor = new DispatcherTimer();

        public frmNavigationSettings()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = Global.TopMost;
            if (Global.GNSSMode == Global.GNSSType.Internal)
                lblGNSSSwitch.Content = "内置";
            if (Global.GNSSMode == Global.GNSSType.Float)
                lblGNSSSwitch.Content = "外置";

            if (Global.mapnorth == Global.MapNorth.North)
                lblNorthDiver.Content = "正北";
            if (Global.mapnorth == Global.MapNorth.Diver)
                lblNorthDiver.Content = "潜水员";

            tmrButtonCheck.Tick += new EventHandler(tmrButtonCheck_Tick);
            tmrButtonCheck.Interval = TimeSpan.FromMilliseconds(5);
            tmrButtonCheck.Start();

            tmrFormMonitor.Tick += new EventHandler(tmrFormMonitor_Tick);
            tmrFormMonitor.Interval = TimeSpan.FromSeconds(2);
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tmrFormMonitor.Stop();
            tmrButtonCheck.Stop();
        }

        private void DisposeAllComponent()
        {
            tmrFormMonitor.Start();
        }

        private void Power_Press()
        {
            DisposeAllComponent();
            frmPower frmPower = new frmPower("frmNavSettings");
            frmPower.Show();
            this.Close();
        }

        private void Lbl_Power_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Power_Press();
        }

        private void Back_Press()
        {
            DisposeAllComponent();
            frmSettings frmSettings = new frmSettings();
            frmSettings.Show();
            this.Close();
        }

        private void Lbl_Back_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Back_Press();
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

        private void GNSS_Press()
        {
            switch(Global.GNSSMode)
            {
                case Global.GNSSType.Internal:
                    Global.GNSSMode = Global.GNSSType.Float;
                    lblGNSSSwitch.Content = "外置";
                    GlobalUpBoard.SetPinState(Global.NavPort, GlobalUpBoard.HIGH);
                    SelectXMLData.SaveConfiguration("GNSSMode", "value", "1");
                    break;
                case Global.GNSSType.Float:
                    Global.GNSSMode = Global.GNSSType.Internal;
                    lblGNSSSwitch.Content = "内置";
                    GlobalUpBoard.SetPinState(Global.NavPort, GlobalUpBoard.LOW);
                    SelectXMLData.SaveConfiguration("GNSSMode", "value", "0");
                    break;
            }
        }

        private void Lbl_GNSS_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GNSS_Press();
        }

        private void MapRotation_Press()
        {
            switch (Global.mapnorth)
            {
                case Global.MapNorth.North:
                    Global.mapnorth = Global.MapNorth.Diver;
                    lblNorthDiver.Content = "潜水员";
                    SelectXMLData.SaveConfiguration("MapNorth", "value", "1");
                    break;
                case Global.MapNorth.Diver:
                    Global.mapnorth = Global.MapNorth.North;
                    lblNorthDiver.Content = "正北";
                    SelectXMLData.SaveConfiguration("MapNorth", "value", "0");
                    break;
            }
        }


        private void lbl_MapRotation_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MapRotation_Press();
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


                if (GlobalUpBoard.GPIOLevel[1] == 0 && GlobalUpBoard.ButtonState[1] == false) //Pressed Back Button
                {
                    Back_Press();
                    GlobalUpBoard.ButtonState[1] = true;
                }
                if (GlobalUpBoard.GPIOLevel[1] == 1 && GlobalUpBoard.ButtonState[1] == true)
                    GlobalUpBoard.ButtonState[1] = false;

                if (GlobalUpBoard.GPIOLevel[2] == 0 && GlobalUpBoard.ButtonState[2] == false)
                {
                    GlobalUpBoard.ButtonState[2] = true;
                }
                if (GlobalUpBoard.GPIOLevel[2] == 1 && GlobalUpBoard.ButtonState[2] == true)
                    GlobalUpBoard.ButtonState[2] = false;

                if (GlobalUpBoard.GPIOLevel[3] == 0 && GlobalUpBoard.ButtonState[3] == false)
                {
                    GlobalUpBoard.ButtonState[3] = true;
                }
                if (GlobalUpBoard.GPIOLevel[3] == 1 && GlobalUpBoard.ButtonState[3] == true)
                    GlobalUpBoard.ButtonState[3] = false;

                if (GlobalUpBoard.GPIOLevel[4] == 0 && GlobalUpBoard.ButtonState[4] == false)
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

                if (GlobalUpBoard.GPIOLevel[6] == 0 && GlobalUpBoard.ButtonState[6] == false) //Pressed GNSS Button
                {
                    GNSS_Press();
                    GlobalUpBoard.ButtonState[6] = true;
                }
                if (GlobalUpBoard.GPIOLevel[6] == 1 && GlobalUpBoard.ButtonState[6] == true)
                    GlobalUpBoard.ButtonState[6] = false;

                if (GlobalUpBoard.GPIOLevel[7] == 0 && GlobalUpBoard.ButtonState[7] == false) //Pressed Map Button
                {
                    MapRotation_Press();
                    GlobalUpBoard.ButtonState[7] = true;
                }
                if (GlobalUpBoard.GPIOLevel[7] == 1 && GlobalUpBoard.ButtonState[7] == true)
                    GlobalUpBoard.ButtonState[7] = false;

                if (GlobalUpBoard.GPIOLevel[8] == 0 && GlobalUpBoard.ButtonState[8] == false)
                {
                    GlobalUpBoard.ButtonState[8] = true;
                }
                if (GlobalUpBoard.GPIOLevel[8] == 1 && GlobalUpBoard.ButtonState[8] == true)
                    GlobalUpBoard.ButtonState[8] = false;

                if (GlobalUpBoard.GPIOLevel[9] == 0 && GlobalUpBoard.ButtonState[9] == false)
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

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
        }
    }
}
