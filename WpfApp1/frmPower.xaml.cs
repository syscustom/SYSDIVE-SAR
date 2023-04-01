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
    /// frmPower.xaml 的交互逻辑
    /// </summary>
    public partial class frmPower : Window
    {
        string para;
        DispatcherTimer tmrButtonCheck = new DispatcherTimer();
        DispatcherTimer tmrFormMonitor = new DispatcherTimer();
        DispatcherTimer tmrDeviceCheck = new DispatcherTimer();
        DispatcherTimer tmrTopMost = new DispatcherTimer();

        public frmPower()
        {
            InitializeComponent();
        }

        public frmPower(string _para)
        {
            InitializeComponent();
            para = _para;
            if (SelectXMLData.GetConfiguration("SonarSwitch", "value") == "1")
            {
                lblSonarSwitch.Content = "开启";
                Sonar_Img.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/Sonar_Icon.png"));
            }
            else
            {
                lblSonarSwitch.Content = "关闭";
                Sonar_Img.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/Sonar_Icon_RED.png"));
            }


            if (SelectXMLData.GetConfiguration("VisionSwitch", "value") == "1")
                lblVisionSwitch.Content = "开启";
            else
                lblVisionSwitch.Content = "关闭";


            Content_Nav.Content = GlobalNavigation.NavCommUserControl;

            tmrButtonCheck.Tick += new EventHandler(tmrButtonCheck_Tick);
            tmrButtonCheck.Interval = TimeSpan.FromMilliseconds(5);
            tmrButtonCheck.Start();

            tmrFormMonitor.Tick += new EventHandler(tmrFormMonitor_Tick);
            tmrFormMonitor.Interval = TimeSpan.FromSeconds(1);

            tmrDeviceCheck.Tick += new EventHandler(tmrDeviceCheck_Tick);
            tmrDeviceCheck.Interval = TimeSpan.FromSeconds(1);
            tmrDeviceCheck.Start();
            RefreshValidAUXDevice();

            if(GlobalDVL.isInstalled)
            {
                brdDVL.Visibility = Visibility.Hidden;
                lbl_DVL.Visibility = Visibility.Visible;
                grdDVLNav.Visibility = Visibility.Visible;
            }
            else
            {
                brdDVL.Visibility = Visibility.Visible;
                lbl_DVL.Visibility = Visibility.Hidden;
                grdDVLNav.Visibility = Visibility.Hidden;
            }

            if (GlobalDVL.isInstalled)
            {
                if (GlobalDVL.DVLNavigationMode)
                {
                    lblDVLNavSwitch.Content = "开启";
                }
                else
                {
                    lblDVLNavSwitch.Content = "关闭";
                }
            }

            if(Global.MountVision)
            {
                brdCamera.Visibility = Visibility.Hidden;
                lbl_Camera.Visibility = Visibility.Visible;
                grdCamera.Visibility = Visibility.Visible;
            }
            else
            {
                brdCamera.Visibility = Visibility.Visible;
                lbl_Camera.Visibility = Visibility.Hidden;
                grdCamera.Visibility = Visibility.Hidden;
            }

            tmrTopMost.Tick += new EventHandler(tmrTopMost_Tick);
            tmrTopMost.Interval = TimeSpan.FromSeconds(2);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tmrFormMonitor.Stop();
            tmrButtonCheck.Stop();
            tmrTopMost.Stop();
        }

        private void ReturnForm()
        {
            DisposeAllComponent();
            switch (para)
            {
                case "frmSonarDisplay":
                    Global.SonarWindow.DoShow();
                    //frmSonarDisplay frmSonarDisplay = new frmSonarDisplay();
                    //frmSonarDisplay.Show();
                    break;
                case "MainWindow":
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    break;
                case "frmMarkers":
                    frmMarkers frmMarkers = new frmMarkers();
                    frmMarkers.Show();
                    break;
                case "frmMissions":
                    frmMissions frmMissions = new frmMissions();
                    frmMissions.Show();
                    break;
                case "frmNavigation":
                    frmNavigation frmNavigation = new frmNavigation();
                    frmNavigation.Show();
                    break;
                case "frmTools":
                    frmTools frmTools = new frmTools();
                    frmTools.Show();
                    break;
                case "frmVideo":
                    frmVideo frmVideo = new frmVideo();
                    frmVideo.Show();
                    break;
                case "frmSettings":
                    frmSettings frmSettings = new frmSettings();
                    frmSettings.Show();
                    break;
                case "frmNavSettings":
                    frmNavigationSettings frmNavigationSettings = new frmNavigationSettings();
                    frmNavigationSettings.Show();
                    break;
                case "frmSensorSettings":
                    frmSensorSettings frmSensorSettings = new frmSensorSettings();
                    frmSensorSettings.Show();
                    break;
                case "frmDiveLog":
                    frmDiveLog frmDiveLog = new frmDiveLog();
                    frmDiveLog.Show();
                    break;
                case "frmGNSS":
                    frmGNSSInformation frmGNSSInformation = new frmGNSSInformation();
                    frmGNSSInformation.Show();
                    break;
            }
            this.Close();
        }

        private void Lbl_Return_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ReturnForm();
        }

        private void PowerOff()
        {
            DisposeAllComponent();
            frmPowerOffConfirmation frmpoweroffconfirmation = new frmPowerOffConfirmation(para);
            frmpoweroffconfirmation.Show();

            this.Close();
        }

        private void Lbl_PowerOff_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PowerOff();
        }

        private void Exit_Press()
        {
            DisposeAllComponent();
            frmExitConfirmation frmexitconfirmation = new frmExitConfirmation(para);
            frmexitconfirmation.Show();

            this.Close();
        }

        private void Lbl_Exit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Exit_Press();
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

        private void SonarOnOff()
        {
            if (GlobalSonar.isInstalled)
            {
                GlobalSonar.SonarSwitch = !GlobalSonar.SonarSwitch;
                if (GlobalSonar.SonarSwitch)
                    GlobalSonar.CreateSonar();
                else
                    GlobalSonar.CloseSonar();
            }

            if (GlobalOculus.isInstalled)
            {
                GlobalOculus.SonarSwitch = !GlobalOculus.SonarSwitch;
                if(Global.IsStartRecordLog == false)
                {
                    if (GlobalOculus.SonarSwitch == true)
                    {
                        GlobalUpBoard.SetPinState(18, GlobalUpBoard.HIGH);
                        SelectXMLData.SaveConfiguration("SonarSwitch", "value", "1");
                        lblSonarSwitch.Content = "开启";
                        Sonar_Img.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/Sonar_Icon.png"));
                        //Global.SonarWindow.ConnectSonar();
                    }
                    else
                    {
                        Global.SonarWindow.DisconnectSonar();
                        SelectXMLData.SaveConfiguration("SonarSwitch", "value", "0");
                        lblSonarSwitch.Content = "关闭";
                        Sonar_Img.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/Sonar_Icon_RED.png"));
                        GlobalUpBoard.SetPinState(18, GlobalUpBoard.LOW);
                    }
                }
            }
        }

        private void Lbl_Sonar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SonarOnOff();
        }

        private void VisionOnOff()
        {
            Global.VisionSwitch = !Global.VisionSwitch;
            if (Global.VisionSwitch)
            {
                SelectXMLData.SaveConfiguration("VisionSwitch", "value", "1");
                lblVisionSwitch.Content = "开启";
                Global.CreateVideo();
            }
            else
            {
                SelectXMLData.SaveConfiguration("VisionSwitch", "value", "0");
                lblVisionSwitch.Content = "关闭";
                Global.CloseVideo();
            }
        }

        void tmrButtonCheck_Tick(object sender, EventArgs e)
        {
            if (this.IsActive)
            {
                if (GlobalUpBoard.GPIOLevel[0] == 0 && GlobalUpBoard.ButtonState[0] == false) //Pressed Return Button
                {
                    ReturnForm();
                    GlobalUpBoard.ButtonState[0] = true;
                }
                if (GlobalUpBoard.GPIOLevel[0] == 1 && GlobalUpBoard.ButtonState[0] == true)
                    GlobalUpBoard.ButtonState[0] = false;


                if (GlobalUpBoard.GPIOLevel[1] == 0 && GlobalUpBoard.ButtonState[1] == false) //Pressed PowerOff Button
                {
                    PowerOff();
                    GlobalUpBoard.ButtonState[1] = true;
                }
                if (GlobalUpBoard.GPIOLevel[1] == 1 && GlobalUpBoard.ButtonState[1] == true)
                    GlobalUpBoard.ButtonState[1] = false;

                if (GlobalUpBoard.GPIOLevel[2] == 0 && GlobalUpBoard.ButtonState[2] == false) //Pressed Exit Button
                {
                    Exit_Press();
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

                if (GlobalUpBoard.GPIOLevel[4] == 0 && GlobalUpBoard.ButtonState[4] == false) //Pressed Button
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

                if (GlobalUpBoard.GPIOLevel[6] == 0 && GlobalUpBoard.ButtonState[6] == false) //Pressed SonarOnOff Button
                {
                    SonarOnOff();
                    GlobalUpBoard.ButtonState[6] = true;
                }
                if (GlobalUpBoard.GPIOLevel[6] == 1 && GlobalUpBoard.ButtonState[6] == true)
                    GlobalUpBoard.ButtonState[6] = false;

                if (GlobalUpBoard.GPIOLevel[7] == 0 && GlobalUpBoard.ButtonState[7] == false) //Pressed VisionOnOff Button
                {
                    if (Global.MountVision)
                        VisionOnOff();
                    GlobalUpBoard.ButtonState[7] = true;
                }
                if (GlobalUpBoard.GPIOLevel[7] == 1 && GlobalUpBoard.ButtonState[7] == true)
                    GlobalUpBoard.ButtonState[7] = false;

                if (GlobalUpBoard.GPIOLevel[8] == 0 && GlobalUpBoard.ButtonState[8] == false) //Pressed  Button
                {
                    DVLOnOff();
                    GlobalUpBoard.ButtonState[8] = true;
                }
                if (GlobalUpBoard.GPIOLevel[8] == 1 && GlobalUpBoard.ButtonState[8] == true)
                    GlobalUpBoard.ButtonState[8] = false;

                if (GlobalUpBoard.GPIOLevel[9] == 0 && GlobalUpBoard.ButtonState[9] == false) //Pressed Button
                {

                    GlobalUpBoard.ButtonState[9] = true;
                }
                if (GlobalUpBoard.GPIOLevel[9] == 1 && GlobalUpBoard.ButtonState[9] == true)
                    GlobalUpBoard.ButtonState[9] = false;

            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
        }

        private void DisposeAllComponent()
        {
            tmrFormMonitor.Start();
        }

        void tmrFormMonitor_Tick(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = Global.TopMost;
            tmrTopMost.Start();
        }

        void tmrDeviceCheck_Tick(object sender, EventArgs e)
        {
            RefreshValidAUXDevice();
        }

        private void RefreshValidAUXDevice()
        {
            if (GlobalNavigation.nav1.GPSValid)
                GNSS_Img.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/GNSS_Icon.png"));
            else
                GNSS_Img.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/GNSS_Icon_YEL.png"));

            if (GlobalDVL.isInstalled)
            {
                if (GlobalDVL.dvl1.valid)
                    DVL_Img.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/DVL_Icon.png"));
                else
                    DVL_Img.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/DVL_Icon_YEL.png"));
            }
            else
            {
                DVL_Img.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/DVL_Icon_RED.png"));
            }
        }

        private void Lbl_DVL_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DVLOnOff();
        }

        private void DVLOnOff()
        {

            if (GlobalDVL.isInstalled)
            {
                GlobalDVL.DVLNavigationMode = !GlobalDVL.DVLNavigationMode;
                if(GlobalDVL.DVLNavigationMode)
                {
                    lblDVLNavSwitch.Content = "开启";
                    GlobalDVL.dvl1.satellitefix = false;
                }
                else
                {
                    lblDVLNavSwitch.Content = "关闭";

                }
            }
        }

        private void Lbl_Camera_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Global.MountVision)
                VisionOnOff();
        }

        void tmrTopMost_Tick(object sender, EventArgs e)
        {
            this.Topmost = Global.TopMost;
            this.Focus();
        }
    }
}
