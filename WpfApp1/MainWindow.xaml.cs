using System;
using System.Collections;
using System.IO;
using System.Drawing.Imaging;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Threading;


namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]

        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer tmrButtonCheck = new DispatcherTimer();
        DispatcherTimer tmrFormMonitor = new DispatcherTimer();
        DispatcherTimer tmrTopMost = new DispatcherTimer();

        private string datePatt = @"yyyy-MM-dd";
        private string timePatt = @"HH:mm:ss";


        public MainWindow()
        {
            
            InitializeComponent();



            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();

            tmrButtonCheck.Tick += new EventHandler(tmrButtonCheck_Tick);
            tmrButtonCheck.Interval = TimeSpan.FromMilliseconds(5);
            tmrButtonCheck.Start();

            tmrFormMonitor.Tick += new EventHandler(tmrFormMonitor_Tick);
            tmrFormMonitor.Interval = TimeSpan.FromSeconds(2);

            tmrTopMost.Tick += new EventHandler(tmrTopMost_Tick);
            tmrTopMost.Interval = TimeSpan.FromSeconds(2);


        }

        void timer_Tick(object sender, EventArgs e)
        {
            lblYear.Content = DateTime.Now.ToString(datePatt);
            lblTime.Content = DateTime.Now.ToString(timePatt);
        }

        void tmrButtonCheck_Tick(object sender, EventArgs e)
        {

            if (this.IsActive)
            {

                if (GlobalUpBoard.GPIOLevel[10] == 0 && GlobalUpBoard.ButtonState[10] == false) //BRIGHTNESS
                {
                    GlobalExternal.ToggleBright();
                    GlobalUpBoard.ButtonState[10] = true;
                }
                if (GlobalUpBoard.GPIOLevel[10] == 1 && GlobalUpBoard.ButtonState[10] == true)
                    GlobalUpBoard.ButtonState[10] = false;

                if (GlobalUpBoard.GPIOLevel[11] == 0 && GlobalUpBoard.ButtonState[11] == false) //SNAPSHOT
                {
                    
                    GlobalUpBoard.ButtonState[11] = true;
                }
                if (GlobalUpBoard.GPIOLevel[11] == 1 && GlobalUpBoard.ButtonState[11] == true)
                    GlobalUpBoard.ButtonState[11] = false;


                if (GlobalUpBoard.GPIOLevel[0] == 0 && GlobalUpBoard.ButtonState[0] == false) //Pressed Power Button
                {
                    btnPower.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    GlobalUpBoard.ButtonState[0] = true;
                }
                if (GlobalUpBoard.GPIOLevel[0] == 1 && GlobalUpBoard.ButtonState[0] == true)
                    GlobalUpBoard.ButtonState[0] = false;


                if (GlobalUpBoard.GPIOLevel[1] == 0 && GlobalUpBoard.ButtonState[1] == false) //Pressed Sonar Button
                {
                    btnSonar.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    GlobalUpBoard.ButtonState[1] = true;
                }
                if (GlobalUpBoard.GPIOLevel[1] == 1 && GlobalUpBoard.ButtonState[1] == true)
                    GlobalUpBoard.ButtonState[1] = false;

                if (GlobalUpBoard.GPIOLevel[2] == 0 && GlobalUpBoard.ButtonState[2] == false) //Pressed Nav Button
                {
                    btnNav.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    GlobalUpBoard.ButtonState[2] = true;
                }
                if (GlobalUpBoard.GPIOLevel[2] == 1 && GlobalUpBoard.ButtonState[2] == true)
                    GlobalUpBoard.ButtonState[2] = false;

                if (GlobalUpBoard.GPIOLevel[3] == 0 && GlobalUpBoard.ButtonState[3] == false) //Pressed Video Button
                {
                    //btnVideo.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    GlobalUpBoard.ButtonState[3] = true;
                }
                if (GlobalUpBoard.GPIOLevel[3] == 1 && GlobalUpBoard.ButtonState[3] == true)
                    GlobalUpBoard.ButtonState[3] = false;

                if (GlobalUpBoard.GPIOLevel[4] == 0 && GlobalUpBoard.ButtonState[4] == false) //Pressed Diver Button
                {
                    //btnDiver.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    GlobalUpBoard.ButtonState[4] = true;
                }
                if (GlobalUpBoard.GPIOLevel[4] == 1 && GlobalUpBoard.ButtonState[4] == true)
                    GlobalUpBoard.ButtonState[4] = false;


                if (GlobalUpBoard.GPIOLevel[5] == 0 && GlobalUpBoard.ButtonState[5] == false) //Pressed Tools Button
                {
                    btnTools.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    GlobalUpBoard.ButtonState[5] = true;
                }
                if (GlobalUpBoard.GPIOLevel[5] == 1 && GlobalUpBoard.ButtonState[5] == true)
                    GlobalUpBoard.ButtonState[5] = false;

                if (GlobalUpBoard.GPIOLevel[6] == 0 && GlobalUpBoard.ButtonState[6] == false) //Pressed Missions Button
                {
                    btn_Missions.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    GlobalUpBoard.ButtonState[6] = true;
                }
                if (GlobalUpBoard.GPIOLevel[6] == 1 && GlobalUpBoard.ButtonState[6] == true)
                    GlobalUpBoard.ButtonState[6] = false;

                if (GlobalUpBoard.GPIOLevel[7] == 0 && GlobalUpBoard.ButtonState[7] == false) //Pressed Markers Button
                {
                    btn_Markers.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    GlobalUpBoard.ButtonState[7] = true;
                }
                if (GlobalUpBoard.GPIOLevel[7] == 1 && GlobalUpBoard.ButtonState[7] == true)
                    GlobalUpBoard.ButtonState[7] = false;

                if (GlobalUpBoard.GPIOLevel[8] == 0 && GlobalUpBoard.ButtonState[8] == false) //Pressed Dive Log Button
                {
                    btn_DiveLog.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    GlobalUpBoard.ButtonState[8] = true;
                }
                if (GlobalUpBoard.GPIOLevel[8] == 1 && GlobalUpBoard.ButtonState[8] == true)
                    GlobalUpBoard.ButtonState[8] = false;

                if (GlobalUpBoard.GPIOLevel[9] == 0 && GlobalUpBoard.ButtonState[9] == false) //Pressed Start Dive Button
                {
                    btn_StartDive.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));            
                    GlobalUpBoard.ButtonState[9] = true;
                }
                if (GlobalUpBoard.GPIOLevel[9] == 1 && GlobalUpBoard.ButtonState[9] == true)
                    GlobalUpBoard.ButtonState[9] = false;

            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = Global.TopMost;
            Uri u = new Uri("pack://SiteOfOrigin:,,,/Resources/logo.jpg", UriKind.RelativeOrAbsolute);
            Logo_Img.Source = new BitmapImage(u);

            //if (frmnav == null)
            //    frmnav = new frmNavigation(this);

            if (Global.IsStartRecordLog == false)
            {
                btn_StartDive.Content = "开始潜水";
                btn_StartDive.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF009700"));
            }
            else
            {
                btn_StartDive.Content = "停止潜水";
                btn_StartDive.Background = System.Windows.Media.Brushes.Red;
            }

            //frmSonarDisplay frmSonarDisplay = new frmSonarDisplay();
            //frmSonarDisplay.Show();
            //frmSonarDisplay.Hide();
            //Global.SetWnd(frmSonarDisplay);
            tmrTopMost.Start();

        }

        private void ListView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnTools_Click(object sender, RoutedEventArgs e)
        {
            DisposeAllComponent();
            frmTools frmtools = new frmTools();
            frmtools.Show();
            this.Close();

        }

        private void btnNav_Click(object sender, RoutedEventArgs e)
        {
            DisposeAllComponent();
            frmNavigation frmnav = new frmNavigation();
            frmnav.Show();
            this.Close();
        }

        private void Btn_Missions_Click(object sender, RoutedEventArgs e)
        {
            DisposeAllComponent();
            frmMissions frmmis = new frmMissions();
            frmmis.Show();
            this.Close();
        }

        private void BtnSonar_Click(object sender, RoutedEventArgs e)
        {
            DisposeAllComponent();
            //frmSonarDisplay frmsonardisplay = new frmSonarDisplay();
            Global.SonarWindow.DoShow();
            this.Close();
        }

        private void btnVideo_Click(object sender, RoutedEventArgs e)
        {
            DisposeAllComponent();
            frmVideo frmVideo = new frmVideo();
            frmVideo.Show();
            this.Close();
        }

        private void BtnPower_Click(object sender, RoutedEventArgs e)
        {
            DisposeAllComponent();
            frmPower frmPower = new frmPower("MainWindow");
            frmPower.Show();
            this.Close();
        }

        private void Btn_Markers_Click(object sender, RoutedEventArgs e)
        {
            DisposeAllComponent();
            frmMarkers frmMarkers = new frmMarkers();
            frmMarkers.Show();
            this.Close();
        }

        private void btnDiver_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
        }

        private void Btn_DiveLog_Click(object sender, RoutedEventArgs e)
        {
            DisposeAllComponent();
            frmDiveLog frmDiveLog = new frmDiveLog();
            frmDiveLog.Show();
            this.Close();
        }

        private void Btn_StartDive_Click(object sender, RoutedEventArgs e)
        {
            DisposeAllComponent();
            if(Global.IsStartRecordLog == false)
            {
                frmStartDiveConfirmation frmStartDive = new frmStartDiveConfirmation();
                frmStartDive.Show();
            }
            else
            {
                frmStopDiveConfirmation frmStopDiveConfirmation = new frmStopDiveConfirmation();
                frmStopDiveConfirmation.Show();
            }
            this.Close();
        }

        void tmrFormMonitor_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tmrFormMonitor.Stop();
            tmrButtonCheck.Stop();
            timer.Stop();
            tmrTopMost.Stop();
        }

        private void DisposeAllComponent()
        {
            tmrFormMonitor.Start();
        }

        void tmrTopMost_Tick(object sender, EventArgs e)
        {
            this.Topmost = Global.TopMost;
            this.Focus();
        }
    }

    public class ImageWork
    {

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);
        /// <summary>
        /// Bitmap->BitmapSource
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapSource getBitMapSourceFromBitmap(Bitmap bitmap)
        {
            IntPtr intPtrl = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(intPtrl,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(intPtrl);
            return bitmapSource;
        }


        /// <summary>
        ///  Bitmap --> BitmapImage
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);
                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
    }
}
