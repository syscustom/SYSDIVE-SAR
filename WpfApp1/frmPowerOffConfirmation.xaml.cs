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
    /// frmPowerOffConfirmation.xaml 的交互逻辑
    /// </summary>
    public partial class frmPowerOffConfirmation : Window
    {
        string para;
        DispatcherTimer tmrButtonCheck = new DispatcherTimer();
        DispatcherTimer tmrFormMonitor = new DispatcherTimer();
        DispatcherTimer tmrTopMost = new DispatcherTimer();

        public frmPowerOffConfirmation()
        {
            InitializeComponent();
        }

        public frmPowerOffConfirmation(string _para)
        {
            InitializeComponent();
            para = _para;

            tmrButtonCheck.Tick += new EventHandler(tmrButtonCheck_Tick);
            tmrButtonCheck.Interval = TimeSpan.FromMilliseconds(5);
            tmrButtonCheck.Start();

            tmrFormMonitor.Tick += new EventHandler(tmrFormMonitor_Tick);
            tmrFormMonitor.Interval = TimeSpan.FromSeconds(1);

            tmrTopMost.Tick += new EventHandler(tmrTopMost_Tick);
            tmrTopMost.Interval = TimeSpan.FromSeconds(2);
        }

        private void ReturnForm()
        {
            DisposeAllComponent();
            frmPower frmPower = new frmPower(para);
            frmPower.Show();
            this.Close();
        }

        private void DisposeAllComponent()
        {
            tmrFormMonitor.Start();
        }

        void tmrButtonCheck_Tick(object sender, EventArgs e)
        {
            if (this.IsActive)
            {
                if (GlobalUpBoard.GPIOLevel[0] == 0 && GlobalUpBoard.ButtonState[0] == false) //Pressed Return Button
                {
                    GlobalUpBoard.ButtonState[0] = true;
                }
                if (GlobalUpBoard.GPIOLevel[0] == 1 && GlobalUpBoard.ButtonState[0] == true)
                    GlobalUpBoard.ButtonState[0] = false;


                if (GlobalUpBoard.GPIOLevel[1] == 0 && GlobalUpBoard.ButtonState[1] == false) //Pressed PowerOff Button
                {
                    GlobalUpBoard.ButtonState[1] = true;
                }
                if (GlobalUpBoard.GPIOLevel[1] == 1 && GlobalUpBoard.ButtonState[1] == true)
                    GlobalUpBoard.ButtonState[1] = false;

                if (GlobalUpBoard.GPIOLevel[2] == 0 && GlobalUpBoard.ButtonState[2] == false) //Pressed Exit Button
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

                if (GlobalUpBoard.GPIOLevel[4] == 0 && GlobalUpBoard.ButtonState[4] == false) //Pressed Button
                {
                    PowerOff();
                    GlobalUpBoard.ButtonState[4] = true;
                }
                if (GlobalUpBoard.GPIOLevel[4] == 1 && GlobalUpBoard.ButtonState[4] == true)
                    GlobalUpBoard.ButtonState[4] = false;


                if (GlobalUpBoard.GPIOLevel[5] == 0 && GlobalUpBoard.ButtonState[5] == false) //Pressed Home Button
                {
                    GlobalUpBoard.ButtonState[5] = true;
                }
                if (GlobalUpBoard.GPIOLevel[5] == 1 && GlobalUpBoard.ButtonState[5] == true)
                    GlobalUpBoard.ButtonState[5] = false;

                if (GlobalUpBoard.GPIOLevel[6] == 0 && GlobalUpBoard.ButtonState[6] == false) //Pressed SonarOnOff Button
                {
                    GlobalUpBoard.ButtonState[6] = true;
                }
                if (GlobalUpBoard.GPIOLevel[6] == 1 && GlobalUpBoard.ButtonState[6] == true)
                    GlobalUpBoard.ButtonState[6] = false;

                if (GlobalUpBoard.GPIOLevel[7] == 0 && GlobalUpBoard.ButtonState[7] == false) //Pressed VisionOnOff Button
                {
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

                if (GlobalUpBoard.GPIOLevel[9] == 0 && GlobalUpBoard.ButtonState[9] == false) //Pressed Button
                {
                    ReturnForm();
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

        void tmrTopMost_Tick(object sender, EventArgs e)
        {
            this.Topmost = Global.TopMost;
            this.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = Global.TopMost;
            tmrTopMost.Start();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tmrFormMonitor.Stop();
            tmrButtonCheck.Stop();
            tmrTopMost.Stop();
        }

        private void Lbl_Cancel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ReturnForm();
        }

        private void Lbl_PowerOff_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PowerOff();
        }

        private void PowerOff()
        {
            DisposeAllComponent();

            /*
            GlobalNavigation.CloseNav();

            if (GlobalSonar.isInstalled && GlobalSonar.SonarSwitch)
                GlobalSonar.CloseSonar();

            //if (GlobalOculus.isInstalled && GlobalOculus.SonarSwitch)
            //    GlobalOculus.CloseSonar();

            if (Global.VisionSwitch)
                Global.CloseVideo();

            GlobalNavigation.CloseGPS();

            GlobalExternal.CloseExternal();

            GlobalDVL.CloseDVL();
            */

            if (Global.MountVision && Global.VisionSwitch)
                Global.CloseVideo();

            Global.SonarWindow.DisconnectSonar();
            GlobalUpBoard.CloseUpBoard();

            //GlobalBattery.CloseBattery();

            //Global.EndCallService();
            Global.SonarWindow.DoClose();

            GlobalNavigation.CloseNavComm();

            Global.shutdowntype = Global.ShutDownType.Shutdown;
            //ShutDownSys.DoExitWin(ShutDownSys.EWX_FORCE | ShutDownSys.EWX_POWEROFF); //关机
        }

    }
}
