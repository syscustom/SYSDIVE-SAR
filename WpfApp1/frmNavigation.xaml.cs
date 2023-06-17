using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Demo.WindowsForms;
using Demo.WindowsPresentation.CustomMarkers;
using System.Windows.Threading;
using System.Device.Location;
using ProViewer4.Models;
using BlueView.Sonar.Model;
using BlueView.Sonar.Interfaces;

namespace WpfApp1
{
    /// <summary>
    /// frmNavigation.xaml 的交互逻辑
    /// </summary>
    public partial class frmNavigation : Window
    {
        List<PointLatLng> LstPoints = new List<PointLatLng>();

        private List<WayPoint> LstWayPoints;

        DispatcherTimer tmrButtonCheck = new DispatcherTimer();
        DispatcherTimer tmrFormMonitor = new DispatcherTimer();
        DispatcherTimer tmrTopMost = new DispatcherTimer();
        DispatcherTimer tmrDrawSonar = new DispatcherTimer();
        DispatcherTimer tmrDrawArrow = new DispatcherTimer();

        public frmNavigation()
        {
            InitializeComponent();
            Content_Nav.Content = GlobalNavigation.NavCommUserControl;
            Content_Map.Content = Global.globalMap;

            if(Global.LittlePreviewSwitch)
            {
                if (Global.MountVision && Global.VisionSwitch)
                {
                    Video_Content.Content = Global.videohost;
                }
            }

            LstWayPoints = Global.LstWayPoints;

            if (GlobalSonar.isInstalled && GlobalSonar.SonarSwitch)
                GlobalSonar.mainModel.Sonar.ImageDataReceived += new EventHandler<ImageEventArgs>(Sonar_ImageDataReceived);

            tmrButtonCheck.Tick += new EventHandler(tmrButtonCheck_Tick);
            tmrButtonCheck.Interval = TimeSpan.FromMilliseconds(5);
            tmrButtonCheck.Start();

            tmrFormMonitor.Tick += new EventHandler(tmrFormMonitor_Tick);
            tmrFormMonitor.Interval = TimeSpan.FromSeconds(2);

            tmrTopMost.Tick += new EventHandler(tmrTopMost_Tick);
            tmrTopMost.Interval = TimeSpan.FromSeconds(2);


            tmrDrawSonar.Tick += new EventHandler(tmrDrawSonar_Tick);
            tmrDrawSonar.Interval = TimeSpan.FromMilliseconds(40);

            tmrDrawArrow.Tick += new EventHandler(tmrDrawArrow_Tick);
            tmrDrawArrow.Interval = TimeSpan.FromMilliseconds(500);
        }

        private void Power_Press()
        {
            DisposeAllComponent();
            frmPower frmPower = new frmPower("frmNavigation");
            frmPower.Show();
            this.Close();
        }

        private void Lbl_Power_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Power_Press();
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

        //Location

        private void Markers_Press()
        {
            DisposeAllComponent();
            frmMarkers frmmarkers = new frmMarkers();
            frmmarkers.Show();
            this.Close();
        }

        private void Lbl_Markers_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Markers_Press();
        }

        private void Home_Press()
        {
            DisposeAllComponent();
            MainWindow mainwindow = new MainWindow();
            mainwindow.Show();
            this.Close();
        }

        private void Lbl_Home_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Home_Press();
        }

        private void Lbl_Prev_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Global.ActiveWayPointPrev();
        }

        private void Lbl_Next_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Global.ActiveWayPointNext();
        }

        private void Display_Press()
        {
            DisposeAllComponent();
            if (Global.MountVision == true)
            {
                frmVideo frmVideo = new frmVideo();
                frmVideo.Show();
            }
            else
                Global.SonarWindow.DoShow();
            this.Close();
        }

        private void Lbl_Display_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Display_Press();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            tmrFormMonitor.Stop();
            tmrButtonCheck.Stop();  

            tmrTopMost.Stop();
        }

        private void Sonar_ImageDataReceived(object sender, ImageEventArgs e)
        {
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

        private void DisposeAllComponent()
        {
            // tmrDrawSonar.Stop();
            tmrDrawArrow.Stop();
            tmrFormMonitor.Start();

            Content_Nav.Content = null;
            Content_Map.Content = null;

            if (Global.LittlePreviewSwitch)
            {
                if (Global.MountVision && Global.VisionSwitch)
                {
                    Video_Content.Content = null;
                }
            }
  
            if (GlobalSonar.isInstalled && GlobalSonar.SonarSwitch)
                if (GlobalSonar.mainModel != null)
                {
                    GlobalSonar.mainModel.Sonar.ImageDataReceived -= Sonar_ImageDataReceived;
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


                if (GlobalUpBoard.GPIOLevel[1] == 0 && GlobalUpBoard.ButtonState[1] == false) //Pressed ZoomPlus Button
                {
                    ZoomPlus_Press();
                    GlobalUpBoard.ButtonState[1] = true;
                }
                if (GlobalUpBoard.GPIOLevel[1] == 1 && GlobalUpBoard.ButtonState[1] == true)
                    GlobalUpBoard.ButtonState[1] = false;

                if (GlobalUpBoard.GPIOLevel[2] == 0 && GlobalUpBoard.ButtonState[2] == false) //Pressed ZoomMinus Button
                {
                    ZoomMinus_Press();
                    GlobalUpBoard.ButtonState[2] = true;
                }
                if (GlobalUpBoard.GPIOLevel[2] == 1 && GlobalUpBoard.ButtonState[2] == true)
                    GlobalUpBoard.ButtonState[2] = false;

                if (GlobalUpBoard.GPIOLevel[3] == 0 && GlobalUpBoard.ButtonState[3] == false) //Pressed Location Button
                {
                    Position_Press();
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

                if (GlobalUpBoard.GPIOLevel[6] == 0 && GlobalUpBoard.ButtonState[6] == false) //Pressed Prev Button
                {
                    Global.ActiveWayPointPrev();
                    GlobalUpBoard.ButtonState[6] = true;
                }
                if (GlobalUpBoard.GPIOLevel[6] == 1 && GlobalUpBoard.ButtonState[6] == true)
                    GlobalUpBoard.ButtonState[6] = false;

                if (GlobalUpBoard.GPIOLevel[7] == 0 && GlobalUpBoard.ButtonState[7] == false) //Pressed Next Button
                {
                    Global.ActiveWayPointNext();
                    GlobalUpBoard.ButtonState[7] = true;
                }
                if (GlobalUpBoard.GPIOLevel[7] == 1 && GlobalUpBoard.ButtonState[7] == true)
                    GlobalUpBoard.ButtonState[7] = false;

                if (GlobalUpBoard.GPIOLevel[8] == 0 && GlobalUpBoard.ButtonState[8] == false) //Pressed Clear Button
                {
                    Clear_Press();
                    GlobalUpBoard.ButtonState[8] = true;
                }
                if (GlobalUpBoard.GPIOLevel[8] == 1 && GlobalUpBoard.ButtonState[8] == true)
                    GlobalUpBoard.ButtonState[8] = false;

                if (GlobalUpBoard.GPIOLevel[9] == 0 && GlobalUpBoard.ButtonState[9] == false) //Pressed Display Button
                {
                    Display_Press();
                    GlobalUpBoard.ButtonState[9] = true;
                }
                if (GlobalUpBoard.GPIOLevel[9] == 1 && GlobalUpBoard.ButtonState[9] == true)
                    GlobalUpBoard.ButtonState[9] = false;

            }

            /*
            if (GlobalUpBoard.GPIOLevel[2] == 0) //Pressed Nav Button
            {
                btnNav.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            }

            if (GlobalUpBoard.GPIOLevel[3] == 0) //Pressed Video Button
            {
                btnVideo.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            }

            if (GlobalUpBoard.GPIOLevel[4] == 0) //Pressed Diver Button
            {
                btnDiver.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            }

            if (GlobalUpBoard.GPIOLevel[15] == 0) //Pressed Tools Button
            {
                btnTools.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            }

            if (GlobalUpBoard.GPIOLevel[16] == 0) //Pressed Missions Button
            {
                btn_Missions.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            }

            if (GlobalUpBoard.GPIOLevel[17] == 0) //Pressed Markers Button
            {
                btn_Markers.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            }
            */
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
        }

        void tmrFormMonitor_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = Global.TopMost;
            tmrTopMost.Start();
            // tmrDrawSonar.Start();
            tmrDrawArrow.Start();
        }

        private void Clear_Press()
        {
            Global.CleartAllCurrentPointsRoute();
        }

        private void Lbl_Clear_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Clear_Press();
        }

        void tmrTopMost_Tick(object sender, EventArgs e)
        {
            this.Topmost = Global.TopMost;
            this.Focus();
        }

        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        void tmrDrawSonar_Tick(object sender, EventArgs e)
        {
            byte[] bytesgrabwindows = Global.SonarWindow.SonarGrabWindows();
            Sonar_Img.Source = LoadImage(bytesgrabwindows);
        }

        void tmrDrawArrow_Tick(object sender, EventArgs e)
        {
            if(GlobalNavigation.nav1.ClockAngel != -1 && GlobalNavigation.nav1.AntiClockAngel != -1)
            {
                if(GlobalNavigation.nav1.ClockAngel <= GlobalNavigation.nav1.AntiClockAngel)
                {
                    if (GlobalNavigation.nav1.ClockAngel >= 0 && GlobalNavigation.nav1.ClockAngel <= 5)
                    {
                        stackRightArrow1.Visibility = Visibility.Hidden;
                        stackRightArrow2.Visibility = Visibility.Hidden;
                        stackRightArrow3.Visibility = Visibility.Hidden;


                        stackLeftArrow1.Visibility = Visibility.Hidden;
                        stackLeftArrow2.Visibility = Visibility.Hidden;
                        stackLeftArrow3.Visibility = Visibility.Hidden;
                    }

                    if (GlobalNavigation.nav1.ClockAngel > 5 && GlobalNavigation.nav1.ClockAngel <= 30)
                    {
                        stackRightArrow1.Visibility = Visibility.Visible;
                        stackRightArrow2.Visibility = Visibility.Hidden;
                        stackRightArrow3.Visibility = Visibility.Hidden;


                        stackLeftArrow1.Visibility = Visibility.Hidden;
                        stackLeftArrow2.Visibility = Visibility.Hidden;
                        stackLeftArrow3.Visibility = Visibility.Hidden;
                    }

                    if (GlobalNavigation.nav1.ClockAngel > 30 && GlobalNavigation.nav1.ClockAngel <= 60)
                    {
                        stackRightArrow1.Visibility = Visibility.Visible;
                        stackRightArrow2.Visibility = Visibility.Visible;
                        stackRightArrow3.Visibility = Visibility.Hidden;

                        stackLeftArrow1.Visibility = Visibility.Hidden;
                        stackLeftArrow2.Visibility = Visibility.Hidden;
                        stackLeftArrow3.Visibility = Visibility.Hidden;
                    }

                    if (GlobalNavigation.nav1.ClockAngel > 60)
                    {
                        stackRightArrow1.Visibility = Visibility.Visible;
                        stackRightArrow2.Visibility = Visibility.Visible;
                        stackRightArrow3.Visibility = Visibility.Visible;

                        stackLeftArrow1.Visibility = Visibility.Hidden;
                        stackLeftArrow2.Visibility = Visibility.Hidden;
                        stackLeftArrow3.Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    if (GlobalNavigation.nav1.AntiClockAngel >= 0 && GlobalNavigation.nav1.AntiClockAngel <= 5)
                    {
                        stackLeftArrow1.Visibility = Visibility.Hidden;
                        stackLeftArrow2.Visibility = Visibility.Hidden;
                        stackLeftArrow3.Visibility = Visibility.Hidden;


                        stackRightArrow1.Visibility = Visibility.Hidden;
                        stackRightArrow2.Visibility = Visibility.Hidden;
                        stackRightArrow3.Visibility = Visibility.Hidden;
                    }

                    if (GlobalNavigation.nav1.AntiClockAngel > 5 && GlobalNavigation.nav1.AntiClockAngel <= 30)
                    {
                        stackLeftArrow1.Visibility = Visibility.Visible;
                        stackLeftArrow2.Visibility = Visibility.Hidden;
                        stackLeftArrow3.Visibility = Visibility.Hidden;


                        stackRightArrow1.Visibility = Visibility.Hidden;
                        stackRightArrow2.Visibility = Visibility.Hidden;
                        stackRightArrow3.Visibility = Visibility.Hidden;
                    }

                    if (GlobalNavigation.nav1.AntiClockAngel > 30 && GlobalNavigation.nav1.AntiClockAngel <= 60)
                    {
                        stackLeftArrow1.Visibility = Visibility.Visible;
                        stackLeftArrow2.Visibility = Visibility.Visible;
                        stackLeftArrow3.Visibility = Visibility.Hidden;

                        stackRightArrow1.Visibility = Visibility.Hidden;
                        stackRightArrow2.Visibility = Visibility.Hidden;
                        stackRightArrow3.Visibility = Visibility.Hidden;
                    }

                    if (GlobalNavigation.nav1.AntiClockAngel > 60)
                    {
                        stackLeftArrow1.Visibility = Visibility.Visible;
                        stackLeftArrow2.Visibility = Visibility.Visible;
                        stackLeftArrow3.Visibility = Visibility.Visible;

                        stackRightArrow1.Visibility = Visibility.Hidden;
                        stackRightArrow2.Visibility = Visibility.Hidden;
                        stackRightArrow3.Visibility = Visibility.Hidden;
                    }


                }
            }
        }

        private void Position_Press()
        {
            DisposeAllComponent();
            //frmSetDiverPosition frmsetDiverPosition = new frmSetDiverPosition();
            //mainwindow.Show();
            this.Close();


            GlobalDVL.dVLStatus.Latitude = Convert.ToDouble(SelectXMLData.GetConfiguration("DefaultLat", "value"));
            GlobalDVL.dVLStatus.Longitude = Convert.ToDouble(SelectXMLData.GetConfiguration("DefaultLng", "value"));
            GlobalDVL.dVLStatus.satellitefix = true;
        }

        private void Lbl_Position_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Position_Press();
        }
    }
}
