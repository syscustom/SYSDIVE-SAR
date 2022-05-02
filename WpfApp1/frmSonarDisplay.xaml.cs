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
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ProViewer4.Models;
using BlueView.Sonar.Model;
using BlueView.Sonar.Interfaces;
using System.Runtime.InteropServices;
using System.Windows.Forms.Integration;
using System.Windows.Forms;

namespace WpfApp1
{
    /// <summary>
    /// frmSonarDisplay.xaml 的交互逻辑
    /// </summary>
    public partial class frmSonarDisplay : Window
    {
        [DllImport("OculusSonar.dll", CallingConvention = CallingConvention.Cdecl)]//导入qtdialog.dll
        public static extern void InitialDll();//声明qtdialog.dll里面的一个接口

        [DllImport("OculusSonar.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]//qtdialog.dll
        public static extern bool showDialog(IntPtr parent,int w, int h);//声明qtdialog.dll里面的另一个接口
        [DllImport("OculusSonar.dll")]//qtdialog.dll
        public static extern int OculusResize(int w, int h);//声明qtdialog.dll里面的另一个接口
        [DllImport("OculusSonar.dll")]//qtdialog.dll
        public static extern void ExitDll();//声明qtdialog.dll里面的另一个接口

        [DllImport("OculusSonar.dll", EntryPoint = "OculusToggleOpen")]//导入qtdialog.dll
        public static extern void OculusToggleOpen();//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "OculusToogleFrequency")]//导入qtdialog.dll
        public static extern void OculusToogleFrequency();//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "OculusIncreaseRange")]//导入qtdialog.dll
        public static extern void OculusIncreaseRange();//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "OculusDecreaseRange")]//导入qtdialog.dll
        public static extern void OculusDecreaseRange();//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "OculusIncreaseGain")]//导入qtdialog.dll
        public static extern void OculusIncreaseGain();//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "OculusDecreaseGain")]//导入qtdialog.dll
        public static extern void OculusDecreaseGain();//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "OculusToggleRecord")]//导入qtdialog.dll
        public static extern void OculusToggleRecord();//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "OculusOpenFile", CharSet = CharSet.Ansi)]//导入qtdialog.dll
        public static extern void OculusOpenFile(IntPtr path, bool ischecked);//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "OculusRecordFile", CharSet = CharSet.Ansi)]//导入qtdialog.dll
        public static extern void OculusRecordFile(IntPtr path, bool ischecked);//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "OculusPlayChanged", CharSet = CharSet.Ansi)]//导入qtdialog.dll
        public static extern void OculusPlayChanged(bool ischecked);//声明qtdialog.dll里面的一个接口



        [DllImport("OculusSonar.dll", EntryPoint = "OculusFlipY", CharSet = CharSet.Ansi)]//导入qtdialog.dll
        public static extern void OculusFlipY(bool ischecked);//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "DLLIsLoaded")]//导入qtdialog.dll
        public static extern bool DLLIsLoaded();//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "OculusToggleConnect")]//导入qtdialog.dll
        public static extern void OculusToggleConnect();//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "IsEnableConnect")]//导入qtdialog.dll
        public static extern bool IsEnableConnect();//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "GetDemandMode", CharSet = CharSet.Ansi)]//导入qtdialog.dll
        public static extern int GetDemandMode();//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "GetDemandRange", CharSet = CharSet.Ansi)]//导入qtdialog.dll
        public static extern double GetDemandRange();//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "GetDemandGain", CharSet = CharSet.Ansi)]//导入qtdialog.dll
        public static extern int GetDemandGain();//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "IsConnected")]//导入qtdialog.dll
        public static extern bool IsConnected();//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", EntryPoint = "RepeatChanged")]//导入qtdialog.dll
        public static extern void RepeatChanged(bool ischecked);//声明qtdialog.dll里面的一个接口

        enum ConnectStatus
        {
            Detect = 0,
            Connect = 1,
        }



        DispatcherTimer tmrButtonCheck = new DispatcherTimer();
        DispatcherTimer tmrSonarApp = new DispatcherTimer();
        DispatcherTimer tmrSonarAppSecond = new DispatcherTimer();
        DispatcherTimer tmrSonarAppThird = new DispatcherTimer();
        DispatcherTimer tmrSonarInfoGet = new DispatcherTimer();

        DispatcherTimer tmrFormMonitor = new DispatcherTimer();

        DispatcherTimer tmrSonarMonitor = new DispatcherTimer();

        DispatcherTimer tmrTopMost = new DispatcherTimer();

        private WindowsFormsHost host = new WindowsFormsHost();

        ConnectStatus connectstatus = ConnectStatus.Detect;

        ElementHost MapHost;
        ElementHost RangeHostTitle;
        ElementHost RangeHost;
        ElementHost FreqHostTitle;
        ElementHost FreqHost;

        ElementHost GainTitle;
        ElementHost GainHost;

        System.Windows.Controls.Label lblRangeTitle;
        System.Windows.Controls.Label lblRange;
        System.Windows.Controls.Label lblFreqTitle;
        System.Windows.Controls.Label lblFreq;

        System.Windows.Controls.Label lblGainTitle;
        System.Windows.Controls.Label lblGain;

        bool IsStartRecordLog = false;

        public frmSonarDisplay()
        {
            InitializeComponent();


            SoanrInfoContainer.Visibility = Visibility.Hidden;
            SoanrGainInfoContainer.Visibility = Visibility.Hidden;
            //Content_Map.Content = Global.globalMap;
            //Video_Content.Content = Global.host;
            //if (Global.VisionSwitch)
            //    Global.OpenPreviewVideo();

            if (GlobalSonar.isInstalled && GlobalSonar.SonarSwitch)
            {
                GlobalSonar.mainModel.Sonar.ImageDataReceived += new EventHandler<ImageEventArgs>(Sonar_ImageDataReceived);
            }




            tmrSonarApp.Tick += new EventHandler(tmrSonarApp_Tick);
            tmrSonarApp.Interval = TimeSpan.FromSeconds(1);
            tmrSonarApp.Start();

            tmrSonarAppSecond.Tick += new EventHandler(tmrSonarAppSecond_Tick);
            tmrSonarAppSecond.Interval = TimeSpan.FromSeconds(2);

            tmrSonarAppThird.Tick += new EventHandler(tmrSonarAppThird_Tick);
            tmrSonarAppThird.Interval = TimeSpan.FromSeconds(2);

            tmrFormMonitor.Tick += new EventHandler(tmrFormMonitor_Tick);
            tmrFormMonitor.Interval = TimeSpan.FromSeconds(2);

            tmrSonarInfoGet.Tick += new EventHandler(tmrSonarInfoGet_Tick);
            tmrSonarInfoGet.Interval = TimeSpan.FromSeconds(1);

            tmrSonarMonitor.Tick += new EventHandler(tmrSonarMonitor_Tick);
            tmrSonarMonitor.Interval = TimeSpan.FromSeconds(1);

            tmrButtonCheck.Tick += new EventHandler(tmrButtonCheck_Tick);
            tmrButtonCheck.Interval = TimeSpan.FromMilliseconds(5);

            tmrTopMost.Tick += new EventHandler(tmrTopMost_Tick);
            tmrTopMost.Interval = TimeSpan.FromSeconds(2);
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

        private void Power_Press()
        {
            DisposeAllComponentForHide();
            frmPower frmPower = new frmPower("frmSonarDisplay");
            frmPower.Show();
            this.Hide();
        }

        private void Lbl_Power_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Power_Press();
        }

        private void RangePlus_Press()
        {
            if (GlobalOculus.isInstalled && GlobalOculus.SonarSwitch)
            {
                OculusIncreaseRange();
                tmrSonarInfoGet.Start();
            }

            if (GlobalSonar.isInstalled && GlobalSonar.SonarSwitch)
            {
                if (GlobalSonar.mainModel != null)
                {
                    if (GlobalSonar.mainModel.Sonar.Head.StopRange < GlobalSonar.mainModel.Sonar.Head.MaximumRange)
                        GlobalSonar.mainModel.Sonar.Head.StopRange += 5;

                }
            }
        }

        private void Lbl_RangePlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RangePlus_Press();
        }

        private void RangeMinus_Press()
        {
            if (GlobalOculus.isInstalled && GlobalOculus.SonarSwitch)
            {
                OculusDecreaseRange();
                tmrSonarInfoGet.Start();
            }
                

            if (GlobalSonar.isInstalled && GlobalSonar.SonarSwitch)
            {
                if (GlobalSonar.mainModel != null)
                {
                    if (GlobalSonar.SonarSwitch)
                        if (GlobalSonar.mainModel.Sonar.Head.StopRange > GlobalSonar.mainModel.Sonar.Head.MinimumRange)
                            GlobalSonar.mainModel.Sonar.Head.StopRange -= 5;
                }
            }
        }

        private void Lbl_RangeMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RangeMinus_Press();
        }

        private void Markers_Press()
        {
            DisposeAllComponentForHide();
            frmMarkers frmMarkers = new frmMarkers();
            frmMarkers.Show();
            this.Hide();
        }

        private void Lbl_Markers_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Markers_Press();
        }

        private void Home_Press()
        {
            DisposeAllComponentForHide();
            MainWindow mainwindow = new MainWindow();
            mainwindow.Show();
            this.Hide();

        }

        private void Lbl_Home_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Home_Press();
        }

        private void GainPlus_Press()
        {
            if (GlobalOculus.isInstalled && GlobalOculus.SonarSwitch)
            {
                OculusIncreaseGain();
                tmrSonarInfoGet.Start();
            } 

            if (GlobalSonar.isInstalled && GlobalSonar.SonarSwitch)
            {
                if (GlobalSonar.mainModel != null)
                {
                    GlobalSonar.mainModel.ColorMapper.AutoMode = false;
                    GlobalSonar.mainModel.ColorMapper.SetThresholds(GlobalSonar.mainModel.ColorMapper.TopThreshold, GlobalSonar.mainModel.ColorMapper.BottomThreshold - 10);
                }
            }
        }

        private void Lbl_GainPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
             GainPlus_Press();
        }

        private void GainMinus_Press()
        {
            if (GlobalOculus.isInstalled && GlobalOculus.SonarSwitch)
            {
                OculusDecreaseGain();
                tmrSonarInfoGet.Start();
            }


            if (GlobalSonar.isInstalled && GlobalSonar.SonarSwitch)
            {
                if (GlobalSonar.mainModel != null)
                {
                    GlobalSonar.mainModel.ColorMapper.AutoMode = false;
                    GlobalSonar.mainModel.ColorMapper.SetThresholds(GlobalSonar.mainModel.ColorMapper.TopThreshold, GlobalSonar.mainModel.ColorMapper.BottomThreshold + 10);
                }
            }
        }

        private void Lbl_GainMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GainMinus_Press();
        }

        private void FreqToggle_Press()
        {
            if (GlobalOculus.isInstalled && GlobalOculus.SonarSwitch)
            {
                OculusToogleFrequency();
                tmrSonarInfoGet.Start();
            }

        }

        private void Lbl_Freq_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FreqToggle_Press();
        }

        private void Display_Press()
        {
            DisposeAllComponentForHide();
            frmNavigation frmnav = new frmNavigation();
            frmnav.Show();
            this.Hide();
        }

        private void Lbl_Display_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Display_Press();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tmrFormMonitor.Stop();
            if (Global.MountVision && Global.VisionSwitch)
                Global.ClosePreviewVideo();
        }

        private void DisposeAllComponentForHide()
        {
            tmrButtonCheck.Stop();
            tmrTopMost.Stop();

            Content_Nav.Content = null;
            if (MapHost != null)
            {
                MapHost.Child = null;
                MapContainer.ReturnHPanel().Controls.Remove(MapHost);
            }
        }

        private void DisposeAllComponent()
        {
            tmrSonarApp.Stop();
            tmrSonarAppSecond.Stop();
            tmrSonarAppThird.Stop();
            tmrSonarInfoGet.Stop();
            tmrButtonCheck.Stop();
            tmrSonarMonitor.Stop();
            tmrFormMonitor.Start();
            tmrTopMost.Stop();

            if (MapHost != null)
            {
                MapHost.Child = null;
                MapContainer.ReturnHPanel().Controls.Remove(MapHost);
            }

            if(RangeHostTitle != null)
            {
                RangeHostTitle.Child = null;
                SoanrInfoContainer.ReturnHPanel().Controls.Remove(RangeHostTitle);
            }

            if (RangeHost != null)
            {
                RangeHost.Child = null;
                SoanrInfoContainer.ReturnHPanel().Controls.Remove(RangeHost);
            }

            if (FreqHostTitle != null)
            {
                FreqHostTitle.Child = null;
                SoanrInfoContainer.ReturnHPanel().Controls.Remove(FreqHostTitle);
            }

            if (FreqHost != null)
            {
                FreqHost.Child = null;
                SoanrInfoContainer.ReturnHPanel().Controls.Remove(FreqHost);
            }

            if (GainTitle != null)
            {
                GainTitle.Child = null;
                SoanrGainInfoContainer.ReturnHPanel().Controls.Remove(GainTitle);
            }

            if (GainHost != null)
            {
                GainHost.Child = null;
                SoanrGainInfoContainer.ReturnHPanel().Controls.Remove(GainHost);
            }

            //if (GlobalOculus.isInstalled && GlobalOculus.SonarSwitch)
            if (GlobalOculus.isInstalled)
                {
                if (DLLIsLoaded())
                {
                    if (Global.IsStartRecordLog)
                    {
                        IntPtr init = Marshal.StringToHGlobalAnsi(Global.RecordLogFileName);
                        OculusRecordFile(init, false);
                    }

                    if (IsConnected() && IsEnableConnect())
                        OculusToggleConnect();
                    ExitDll();
                }
            }

            if (GlobalSonar.isInstalled && GlobalSonar.SonarSwitch)
            {
                if (GlobalSonar.mainModel != null)
                {
                    GlobalSonar.mainModel.Sonar.ImageDataReceived -= Sonar_ImageDataReceived;
                }
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


                if (GlobalUpBoard.GPIOLevel[1] == 0 && GlobalUpBoard.ButtonState[1] == false) //Pressed RangePlus Button
                {
                    RangePlus_Press();
                    GlobalUpBoard.ButtonState[1] = true;
                }
                if (GlobalUpBoard.GPIOLevel[1] == 1 && GlobalUpBoard.ButtonState[1] == true)
                    GlobalUpBoard.ButtonState[1] = false;

                if (GlobalUpBoard.GPIOLevel[2] == 0 && GlobalUpBoard.ButtonState[2] == false) //Pressed RangeMinus Button
                {
                    RangeMinus_Press();
                    GlobalUpBoard.ButtonState[2] = true;
                }
                if (GlobalUpBoard.GPIOLevel[2] == 1 && GlobalUpBoard.ButtonState[2] == true)
                    GlobalUpBoard.ButtonState[2] = false;

                if (GlobalUpBoard.GPIOLevel[3] == 0 && GlobalUpBoard.ButtonState[3] == false) //Pressed Freq Button
                {
                    FreqToggle_Press();
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

                if (GlobalUpBoard.GPIOLevel[6] == 0 && GlobalUpBoard.ButtonState[6] == false) //Pressed GainPlus Button
                {
                    GainPlus_Press();
                    GlobalUpBoard.ButtonState[6] = true;
                }
                if (GlobalUpBoard.GPIOLevel[6] == 1 && GlobalUpBoard.ButtonState[6] == true)
                    GlobalUpBoard.ButtonState[6] = false;

                if (GlobalUpBoard.GPIOLevel[7] == 0 && GlobalUpBoard.ButtonState[7] == false) //Pressed GainMinus Button
                {
                    GainMinus_Press();
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

                if (GlobalUpBoard.GPIOLevel[9] == 0 && GlobalUpBoard.ButtonState[9] == false) //Pressed Display Button
                {
                    Display_Press();
                    GlobalUpBoard.ButtonState[9] = true;
                }
                if (GlobalUpBoard.GPIOLevel[9] == 1 && GlobalUpBoard.ButtonState[9] == true)
                    GlobalUpBoard.ButtonState[9] = false;

            }

        }

        void tmrSonarApp_Tick(object sender, EventArgs e)
        {
            tmrSonarApp.Stop();
            tmrSonarAppSecond.Start();
            tmrSonarAppThird.Start();
            //if (GlobalOculus.isInstalled && GlobalOculus.SonarSwitch)
            if (GlobalOculus.isInstalled)
            {
                double w = Convert.ToInt16(OculusContainer.Width);
                double h = Convert.ToInt16(OculusContainer.Height);
                w *= 1;
                h *= 1;
                showDialog(OculusContainer.ReturnHostPanel(), Convert.ToInt32(w), Convert.ToInt32(h));
                
            }
        }

        void tmrSonarAppSecond_Tick(object sender, EventArgs e)
        {
            tmrSonarAppSecond.Stop();

            //MapHost = new ElementHost();
            //MapHost.Dock = DockStyle.Fill;
            //MapContainer.ReturnHPanel().Controls.Add(MapHost);
            //MapHost.Child = Global.globalMap;
            
            RangeHostTitle = new ElementHost();
            RangeHost = new ElementHost();
            FreqHostTitle = new ElementHost();
            FreqHost = new ElementHost();


            SoanrInfoContainer.ReturnHPanel().Controls.Add(RangeHostTitle);
            RangeHostTitle.Width = 200;
            RangeHostTitle.Height = 26;
            RangeHostTitle.Location = new System.Drawing.Point(0, 0);

            SoanrInfoContainer.ReturnHPanel().Controls.Add(RangeHost);
            RangeHost.Width = 200;
            RangeHost.Height = 35;
            RangeHost.Location = new System.Drawing.Point(0, 20);

            SoanrInfoContainer.ReturnHPanel().Controls.Add(FreqHostTitle);
            FreqHostTitle.Width = 200;
            FreqHostTitle.Height = 26;
            FreqHostTitle.Location = new System.Drawing.Point(0, 55);

            SoanrInfoContainer.ReturnHPanel().Controls.Add(FreqHost);
            FreqHost.Width = 200;
            FreqHost.Height = 35;
            FreqHost.Location = new System.Drawing.Point(0, 75);

            lblRangeTitle = new System.Windows.Controls.Label();
            lblRangeTitle.Content = "量程";
            lblRangeTitle.Foreground = Brushes.White;
            lblRangeTitle.FontSize = 14;
            RangeHostTitle.Child = lblRangeTitle;

            lblRange = new System.Windows.Controls.Label();
            lblRange.Content = "50m";
            lblRange.Foreground = Brushes.White;
            lblRange.FontSize = 24;
            RangeHost.Child = lblRange;

            lblFreqTitle = new System.Windows.Controls.Label();
            lblFreqTitle.Content = "频率";
            lblFreqTitle.Foreground = Brushes.White;
            lblFreqTitle.FontSize = 14;
            FreqHostTitle.Child = lblFreqTitle;

            lblFreq = new System.Windows.Controls.Label();
            lblFreq.Content = "750kHz";
            lblFreq.Foreground = Brushes.White;
            lblFreq.FontSize = 24;
            FreqHost.Child = lblFreq;

            SoanrInfoContainer.ReturnHPanel().Refresh();


            GainTitle = new ElementHost();
            GainHost = new ElementHost();

            SoanrGainInfoContainer.ReturnHPanel().Controls.Add(GainTitle);
            GainTitle.Width = 200;
            GainTitle.Height = 26;
            GainTitle.Location = new System.Drawing.Point(0, 0);

            SoanrGainInfoContainer.ReturnHPanel().Controls.Add(GainHost);
            GainHost.Width = 200;
            GainHost.Height = 35;
            GainHost.Location = new System.Drawing.Point(0, 20);

            lblGainTitle = new System.Windows.Controls.Label();
            //lblGainTitle.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            lblGainTitle.Content = "增益";
            lblGainTitle.Foreground = Brushes.White;
            lblGainTitle.FontSize = 14;
            GainTitle.Child = lblGainTitle;

            lblGain = new System.Windows.Controls.Label();
           // lblGain.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            lblGain.Content = "20%";
            lblGain.Foreground = Brushes.White;
            lblGain.FontSize = 24;
            GainHost.Child = lblGain;

            //if (Global.globalMap.Parent != null)
            //    Global.globalMap.Parent.SetValue(ContentPresenter.ContentProperty, null);


            if (Global.MountVision && Global.VisionSwitch)
            {
                Video_Content.Content = Global.host;
                Global.OpenPreviewVideo();
            }
        }

        void tmrSonarAppThird_Tick(object sender, EventArgs e)
        {
            //if (GlobalOculus.isInstalled && GlobalOculus.SonarSwitch)
            if (GlobalOculus.isInstalled)
            {
                if(Global.SonarDemoShow == false)
                {
                    if (IsEnableConnect()) //代表已经探测到声呐，可以进行连接了
                    {
                        switch (connectstatus)
                        {
                            case ConnectStatus.Detect:
                                OculusToggleConnect();
                                connectstatus = ConnectStatus.Connect;
                                break;
                            case ConnectStatus.Connect:
                                connectstatus = ConnectStatus.Detect;
                                if (IsConnected())
                                {
                                    tmrSonarAppThird.Stop();
                                    SoanrInfoContainer.Visibility = Visibility.Visible;
                                    SoanrGainInfoContainer.Visibility = Visibility.Visible;
                                    tmrSonarInfoGet.Start();
                                    tmrSonarMonitor.Start();
                                }
                                break;
                        }
                    }
                }
                else
                {
                    string path = System.Environment.CurrentDirectory + "\\" + "Oculus_M750d_1200kHz_River_Fish.oculus";
                    IntPtr init = Marshal.StringToHGlobalAnsi(path);

                    OculusOpenFile(init, true);
                    OculusFlipY(true);
                    OculusPlayChanged(true);
                    RepeatChanged(true);
                    tmrSonarAppThird.Stop();
                    SoanrInfoContainer.Visibility = Visibility.Visible;
                    SoanrGainInfoContainer.Visibility = Visibility.Visible;
                    tmrSonarInfoGet.Start();
                }
            }
        }

        void tmrSonarInfoGet_Tick(object sender, EventArgs e)
        {
            tmrSonarInfoGet.Stop();
            if (IsConnected())
            {
                try
                {
                    int demandmode = GetDemandMode();
                    double demandrange = GetDemandRange();
                    int demandgain = GetDemandGain();
                    if (demandmode == 1) lblFreq.Content = "750kHz";
                    if (demandmode == 2) lblFreq.Content = "1200kHz";
                    lblRange.Content = demandrange.ToString("0.0");
                    lblGain.Content = demandgain.ToString() + "%";
                }
                catch { }
            }
        }

        void tmrFormMonitor_Tick(object sender, EventArgs e)
        {
            this.Close();
        }
        

        void tmrSonarMonitor_Tick(object sender, EventArgs e)
        {
            
            if(!(IsEnableConnect()))
            {
                //OculusToggleConnect();
                SoanrInfoContainer.Visibility = Visibility.Hidden;
                SoanrGainInfoContainer.Visibility = Visibility.Hidden;
                tmrSonarMonitor.Stop();
                tmrSonarAppThird.Start();
                connectstatus = ConnectStatus.Detect;
            }
            

            if(IsEnableConnect() && IsConnected())
            {
                if (Global.IsStartRecordLog != IsStartRecordLog)
                {
                    IsStartRecordLog = Global.IsStartRecordLog;
                    if(IsStartRecordLog == true)
                    {
                        IntPtr init = Marshal.StringToHGlobalAnsi(Global.RecordLogFileName);
                        OculusRecordFile(init, true);
                    }
                    else
                    {
                        IntPtr init = Marshal.StringToHGlobalAnsi(Global.RecordLogFileName);
                        OculusRecordFile(init, false);
                    }
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = Global.TopMost;
        }

        public void DoShow()
        {
            this.Show();

            tmrButtonCheck.Start();

            Content_Nav.Content = GlobalNavigation.NavCommUserControl;

            MapHost = new ElementHost();
            MapHost.Dock = DockStyle.Fill;
            MapContainer.ReturnHPanel().Controls.Add(MapHost);
            MapHost.Child = Global.globalMap;

            tmrTopMost.Start();

        }

        public void DoClose()
        {
            DisposeAllComponent();
            this.Close();
        }

        public void DisconnectSonar()
        {
            if (IsEnableConnect() && IsConnected())
            {
                if (IsStartRecordLog == true)
                {
                    IntPtr init = Marshal.StringToHGlobalAnsi(Global.RecordLogFileName);
                    OculusRecordFile(init, false);
                }
                tmrSonarMonitor.Stop();
                tmrSonarInfoGet.Stop();
                SoanrInfoContainer.Visibility = Visibility.Hidden;
                SoanrGainInfoContainer.Visibility = Visibility.Hidden;
                OculusToggleConnect();
                connectstatus = ConnectStatus.Detect;
                tmrSonarAppThird.Start();
            }
        }


        public void ConnectSonar()
        {
            if (IsEnableConnect() && !(IsConnected()))
            {
                OculusToggleConnect();
                SoanrInfoContainer.Visibility = Visibility.Visible;
                SoanrGainInfoContainer.Visibility = Visibility.Visible;
                if (IsStartRecordLog == true)
                {
                    IntPtr init = Marshal.StringToHGlobalAnsi(Global.RecordLogFileName);
                    OculusRecordFile(init, true);
                }
                tmrSonarMonitor.Start();
                tmrSonarInfoGet.Start();
            }
        }


        void tmrTopMost_Tick(object sender, EventArgs e)
        {
            this.Topmost = Global.TopMost;
            this.Focus();
        }
    }
}
