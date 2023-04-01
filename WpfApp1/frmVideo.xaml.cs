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
using System.Drawing;
using ProViewer4.Models;
using BlueView.Sonar.Model;
using BlueView.Sonar.Interfaces;
using System.Windows.Threading;


namespace WpfApp1
{
    /// <summary>
    /// frmVideo.xaml 的交互逻辑
    /// </summary>
    public partial class frmVideo : Window
    {

        DispatcherTimer tmrButtonCheck = new DispatcherTimer();
        DispatcherTimer tmrFormMonitor = new DispatcherTimer();

        public frmVideo()
        {
            InitializeComponent();
            Content_Nav.Content = GlobalNavigation.NavCommUserControl;
            Content_Map.Content = Global.globalMap;

            if (Global.MountVision)
                Video_Content.Content = Global.videohost;


            if (GlobalSonar.isInstalled && GlobalSonar.SonarSwitch)
                GlobalSonar.mainModel.Sonar.ImageDataReceived += new EventHandler<ImageEventArgs>(Sonar_ImageDataReceived);

            // Global.host.Child.Controls.z


            /*
            Bitmap b = new Bitmap(160, 120);
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.Black);
            g.DrawEllipse(new Pen(Color.Red, 3), 0, 0, 100, 100);
            g.DrawString("Sample bitmap", new Font("Arial", 13), new SolidBrush(Color.White), 0, 0);
            Global.host.SetBitmapOverlay(0, b.GetHbitmap().ToInt32(), 0, 0, 160, 100, 0, 255);
            */
            tmrButtonCheck.Tick += new EventHandler(tmrButtonCheck_Tick);
            tmrButtonCheck.Interval = TimeSpan.FromMilliseconds(5);
            tmrButtonCheck.Start();

            tmrFormMonitor.Tick += new EventHandler(tmrFormMonitor_Tick);
            tmrFormMonitor.Interval = TimeSpan.FromSeconds(2);

        }

        private void Power_Press()
        {
            DisposeAllComponent();
            frmPower frmPower = new frmPower("frmVideo");
            frmPower.Show();
            this.Close();
        }

        private void Lbl_Power_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Power_Press();
        }

        private void Light_Press()
        {


        }

        private void Lbl_Light_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Light_Press();
        }

        private void Markers_Press()
        {
            DisposeAllComponent();
            frmMarkers frmMarkers = new frmMarkers();
            frmMarkers.Show();
            this.Close();
        }

        private void Lbl_Markers_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Markers_Press();
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
            DisposeAllComponent();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close(); ;
        }

        private void VisionOnOff()
        {
            Global.VisionSwitch = !Global.VisionSwitch;
            if (Global.VisionSwitch)
            {
                SelectXMLData.SaveConfiguration("VisionSwitch", "value", "1");
                Global.CreateVideo();
            }
            else
            {
                SelectXMLData.SaveConfiguration("VisionSwitch", "value", "0");
                Global.CloseVideo();
            }
        }

        private void Camera_Press()
        {
            if (Global.MountVision)
            {
                VisionOnOff();
            }

        }

        private void Lbl_Camera_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Camera_Press();
        }

        private void SnapShot_Press()
        {
            if (Global.MountVision && Global.VisionSwitch)
                Global.VideoSnapshot();
        }

        private void Lbl_SnapShot_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SnapShot_Press();
        }

        private void Sonar_ImageDataReceived(object sender, ImageEventArgs e)
        {

            //e.Image.SavePPM("cimgsys" + i +".ppm");
            //i++;
            //Sonar_Img.Source = e.Image.GetBitmap();


            this.Sonar_Img.Dispatcher.Invoke(
                  new Action(
                       delegate
                       {
                           Sonar_Img.Source = e.Image.GetBitmap();
                           //Sonar_Img.Margin = new Thickness(0, -207, Grid_Sonar.Width - Sonar_Img.Width, 207);
                           //image.Source = imgSource;
                       }
                  )
            );
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tmrFormMonitor.Stop();
            tmrButtonCheck.Stop();
        }

        private void DisposeAllComponent()
        {
            tmrFormMonitor.Start();
            Content_Map.Content = null;
            Content_Nav.Content = null;
            Video_Content.Content = null;

            if (GlobalSonar.isInstalled && GlobalSonar.SonarSwitch)
                if (GlobalSonar.mainModel != null)
                {
                    GlobalSonar.mainModel.Sonar.ImageDataReceived -= Sonar_ImageDataReceived;
                }
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


                if (GlobalUpBoard.GPIOLevel[1] == 0 && GlobalUpBoard.ButtonState[1] == false) //Pressed Light Button
                {
                    Light_Press();
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

                if (GlobalUpBoard.GPIOLevel[4] == 0 && GlobalUpBoard.ButtonState[4] == false) //Pressed Markers Button
                {
                    Markers_Press();
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

                if (GlobalUpBoard.GPIOLevel[6] == 0 && GlobalUpBoard.ButtonState[6] == false) //Pressed Camera Button
                {
                    Camera_Press();
                    GlobalUpBoard.ButtonState[6] = true;
                }
                if (GlobalUpBoard.GPIOLevel[6] == 1 && GlobalUpBoard.ButtonState[6] == true)
                    GlobalUpBoard.ButtonState[6] = false;

                if (GlobalUpBoard.GPIOLevel[7] == 0 && GlobalUpBoard.ButtonState[7] == false) //Pressed SnapShot Button
                {
                    SnapShot_Press();
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
                    Display_Press();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = Global.TopMost;
        }

        private void Lbl_Display_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Display_Press();
        }

        private void Display_Press()
        {
            DisposeAllComponent();
            Global.SonarWindow.DoShow();
            this.Close();
        }
    }
}
