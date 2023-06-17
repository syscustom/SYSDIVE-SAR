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
using System.Windows.Forms.Integration;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Threading;

namespace WpfApp1
{
    /// <summary>
    /// frmVideo.xaml 的交互逻辑
    /// </summary>
    public partial class frmVideo : Window
    {

        DispatcherTimer tmrButtonCheck = new DispatcherTimer();
        DispatcherTimer tmrFormMonitor = new DispatcherTimer();
        DispatcherTimer tmrTopMost = new DispatcherTimer();
        DispatcherTimer tmrDrawSonar = new DispatcherTimer();

        //Thread threadGNSSMessage;

        ElementHost MapHost;
        ElementHost SonarHost;

        System.Windows.Controls.Image Sonar_Image;

        public frmVideo()
        {
            InitializeComponent();
            Content_Nav.Content = GlobalNavigation.NavCommUserControl;
            //Content_Map.Content = Global.globalMap;

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

            tmrTopMost.Tick += new EventHandler(tmrTopMost_Tick);
            tmrTopMost.Interval = TimeSpan.FromSeconds(2);

            tmrDrawSonar.Tick += new EventHandler(tmrDrawSonar_Tick);
            tmrDrawSonar.Interval = TimeSpan.FromMilliseconds(40);

            //threadGNSSMessage = new Thread(new ThreadStart(StartCollectingMessage));
        }
/*
        private void StartCollectingMessage()
        {
            while (true)
            {
                Thread.Sleep(50);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate
                {
                    byte[] bytesgrabwindows = Global.SonarWindow.SonarGrabWindows();
                    Sonar_Image.Source = Convert(LoadImage(bytesgrabwindows));
                });

                //this.Dispatcher.Invoke((EventHandler)delegate {
                //    txtGNSSMessage.Text = "真的只要一行代码"; });
            }
        }
*/

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
            GlobalExternal.LightBrightLevel = GlobalExternal.ToggleLightBright();
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

            tmrTopMost.Stop();
        }

        private void DisposeAllComponent()
        {
            //tmrDrawSonar.Stop();
            tmrFormMonitor.Start();
            //Content_Map.Content = null;
            Content_Nav.Content = null;
            Video_Content.Content = null;

            if (Global.LittlePreviewSwitch)
            {
                if (MapHost != null)
                {
                    MapHost.Child = null;
                    MapContainer.ReturnHPanel().Controls.Remove(MapHost);
                }

                if (SonarHost != null)
                {
                    SonarHost.Child = null;
                    SonarContainer.ReturnHPanel().Controls.Remove(SonarHost);
                }
            }


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
            tmrTopMost.Start();
            
            //tmrDrawSonar.Start();

            if (Global.LittlePreviewSwitch)
            {
                MapHost = new ElementHost();
                MapHost.Dock = DockStyle.Fill;
                MapContainer.ReturnHPanel().Controls.Add(MapHost);
                MapHost.Child = Global.globalMap;


                Sonar_Image = new System.Windows.Controls.Image();
                Sonar_Image.Visibility = Visibility.Hidden;
                Sonar_Image.Height = 119;
                Sonar_Image.Width = 190;
                Thickness thickness = new Thickness(49, 370, 0, 0);
                Sonar_Image.Margin = thickness;

                SonarHost = new ElementHost();
                SonarHost.Dock = DockStyle.Fill;
                SonarContainer.ReturnHPanel().Controls.Add(SonarHost);
                SonarHost.Child = Sonar_Image;
                Sonar_Image.Visibility = Visibility.Visible;

                //threadGNSSMessage.Start();
            }

            
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

        void tmrTopMost_Tick(object sender, EventArgs e)
        {
            this.Topmost = Global.TopMost;
            this.Focus();
        }

        private BitmapImage LoadImage1(byte[] imageData)
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

        private Bitmap LoadImage(byte[] imageData)
        {

            using (MemoryStream stream = new MemoryStream(imageData))
            {
                Bitmap bitmap = new Bitmap(stream);
                return bitmap;
            }     
        }

        void tmrDrawSonar_Tick(object sender, EventArgs e)
        {
            byte[] bytesgrabwindows = Global.SonarWindow.SonarGrabWindows();
            Sonar_Image.Source = Convert(LoadImage(bytesgrabwindows));
        }

        public static BitmapSource Convert(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                ConvertPixelFormat(bitmap.PixelFormat), null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;

        }

        private static System.Windows.Media.PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat sourceFormat)
        {
            switch (sourceFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormats.Bgr24;

                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return PixelFormats.Bgra32;

                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return PixelFormats.Bgr32;

                    // .. as many as you need...
            }
            return new System.Windows.Media.PixelFormat();
        }

    }

    [ValueConversion(typeof(Bitmap), typeof(ImageSource))]
    public class BitmapToImageSourceConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Bitmap bitmap = value as Bitmap;
            if (bitmap == null)
                return null;

            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height, 96, 96, ConvertPixelFormat(bitmap.PixelFormat), null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        private static System.Windows.Media.PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat sourceFormat)
        {
            switch (sourceFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormats.Bgr24;

                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return PixelFormats.Bgra32;

                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return PixelFormats.Bgr32;

                    // .. as many as you need...
            }
            return new System.Windows.Media.PixelFormat();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
