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
    /// frmSettings.xaml 的交互逻辑
    /// </summary>
    public partial class frmSettings : Window
    {
        int intlstFocusIndex = -1;

        DispatcherTimer tmrButtonCheck = new DispatcherTimer();
        DispatcherTimer tmrFormMonitor = new DispatcherTimer();
        DispatcherTimer tmrTopMost = new DispatcherTimer();

        public frmSettings()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = Global.TopMost;
            lstviewSettings.ItemContainerGenerator.StatusChanged += new EventHandler(ItemContainerGenerator_StatusChanged);
            Content_Nav.Content = GlobalNavigation.NavCommUserControl;
            tmrButtonCheck.Tick += new EventHandler(tmrButtonCheck_Tick);
            tmrButtonCheck.Interval = TimeSpan.FromMilliseconds(5);
            tmrButtonCheck.Start();

            tmrFormMonitor.Tick += new EventHandler(tmrFormMonitor_Tick);
            tmrFormMonitor.Interval = TimeSpan.FromSeconds(2);

            tmrTopMost.Tick += new EventHandler(tmrTopMost_Tick);
            tmrTopMost.Interval = TimeSpan.FromSeconds(2);

            tmrTopMost.Start();
        }



        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (lstviewSettings.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                if (lstviewSettings.Items.Count > 0)
                {
                    intlstFocusIndex = 0;
                    ListViewItem item = (ListViewItem)lstviewSettings.ItemContainerGenerator.ContainerFromIndex(intlstFocusIndex);
                    item.Focus();

                }
            }
        }

        private void Power_Press()
        {
            DisposeAllComponent();
            frmPower frmPower = new frmPower("frmSettings");
            frmPower.Show();
            this.Close();
        }

        private void Lbl_Power_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Power_Press();
        }

        private void ImgUpArrow_Press()
        {
            if (lstviewSettings.Items.Count > 0)
            {
                if (lstviewSettings.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                {
                    if (intlstFocusIndex > 0)
                        intlstFocusIndex--;
                    else if (intlstFocusIndex == 0)
                        intlstFocusIndex = lstviewSettings.Items.Count - 1;

                    ListViewItem item = (ListViewItem)lstviewSettings.ItemContainerGenerator.ContainerFromIndex(intlstFocusIndex);
                    item.Focus();
                }
            }
        }

        private void ImgDownArrow_Press()
        {
            if (lstviewSettings.Items.Count > 0)
            {
                if (lstviewSettings.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                {
                    if (intlstFocusIndex < lstviewSettings.Items.Count - 1)
                        intlstFocusIndex++;
                    else if (intlstFocusIndex == lstviewSettings.Items.Count - 1)
                        intlstFocusIndex = 0;

                    ListViewItem item = (ListViewItem)lstviewSettings.ItemContainerGenerator.ContainerFromIndex(intlstFocusIndex);
                    item.Focus();
                }
            }
        }

        private void ImgUpArrow_1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ImgUpArrow_Press();
        }

        private void ImgDownArrow_1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ImgDownArrow_Press();
        }

        private void ImgUpArrow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ImgUpArrow_Press();
        }

        private void ImgDownArrow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ImgDownArrow_Press();
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

        private void Back_Press()
        {
            DisposeAllComponent();
            frmTools frmTools = new frmTools();
            frmTools.Show();
            this.Close();
        }

        private void Lbl_Back_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Back_Press();
        }

        private void Select_Press()
        {
            if (lstviewSettings.Items.Count > 0)
            {
                switch (intlstFocusIndex)
                {
                    case 0:
                        DisposeAllComponent();
                        frmSensorSettings frmSensorSettings = new frmSensorSettings();
                        frmSensorSettings.Show();
                        this.Close();
                        break;
                    case 1:
                        DisposeAllComponent();
                        frmNavigationSettings frmNavigationSettings = new frmNavigationSettings();
                        frmNavigationSettings.Show();
                        this.Close();
                        break;
                    case 2:
                        DisposeAllComponent();
                        frmDisplaySettings frmDisplaySettings = new frmDisplaySettings();
                        frmDisplaySettings.Show();
                        this.Close();
                        break;

                }
            }
        }

        private void Lbl_Select_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Select_Press();
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


                if (GlobalUpBoard.GPIOLevel[1] == 0 && GlobalUpBoard.ButtonState[1] == false) //Pressed ImgUpArrow Button
                {
                    ImgUpArrow_Press();
                    GlobalUpBoard.ButtonState[1] = true;
                }
                if (GlobalUpBoard.GPIOLevel[1] == 1 && GlobalUpBoard.ButtonState[1] == true)
                    GlobalUpBoard.ButtonState[1] = false;

                if (GlobalUpBoard.GPIOLevel[2] == 0 && GlobalUpBoard.ButtonState[2] == false) //Pressed ImgDownArrow Button
                {
                    ImgDownArrow_Press();
                    GlobalUpBoard.ButtonState[2] = true;
                }
                if (GlobalUpBoard.GPIOLevel[2] == 1 && GlobalUpBoard.ButtonState[2] == true)
                    GlobalUpBoard.ButtonState[2] = false;

                if (GlobalUpBoard.GPIOLevel[3] == 0 && GlobalUpBoard.ButtonState[3] == false) //Pressed Back Button
                {
                    Back_Press();
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

                if (GlobalUpBoard.GPIOLevel[6] == 0 && GlobalUpBoard.ButtonState[6] == false) //Pressed UpArrow Button
                {
                    ImgUpArrow_Press();
                    GlobalUpBoard.ButtonState[6] = true;
                }
                if (GlobalUpBoard.GPIOLevel[6] == 1 && GlobalUpBoard.ButtonState[6] == true)
                    GlobalUpBoard.ButtonState[6] = false;

                if (GlobalUpBoard.GPIOLevel[7] == 0 && GlobalUpBoard.ButtonState[7] == false) //Pressed DownArrow Button
                {
                    ImgDownArrow_Press();
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

        private void DisposeAllComponent()
        {
            tmrFormMonitor.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tmrFormMonitor.Stop();
            tmrButtonCheck.Stop();
            tmrTopMost.Stop();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
        }

        void tmrTopMost_Tick(object sender, EventArgs e)
        {
            this.Topmost = Global.TopMost;
            this.Focus();
        }
    }
}
