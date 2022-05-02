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
    /// frmMarkers.xaml 的交互逻辑
    /// </summary>
    public partial class frmMarkers : Window
    {

        enum ScrollStatus
        {
            Normal = 0,
            UpDownTop = 1,
            New = 2
        }

        private List<WayPoint> LstWayPoints;
        int intlstFocusIndex = -1;

        private ScrollStatus scrollstatus = ScrollStatus.Normal;

        DispatcherTimer tmrButtonCheck = new DispatcherTimer();
        DispatcherTimer tmrFormMonitor = new DispatcherTimer();
        DispatcherTimer tmrTopMost = new DispatcherTimer();


        public frmMarkers()
        {
            InitializeComponent();
            Content_Nav.Content = GlobalNavigation.NavCommUserControl;
            tmrButtonCheck.Tick += new EventHandler(tmrButtonCheck_Tick);
            tmrButtonCheck.Interval = TimeSpan.FromMilliseconds(5);
            tmrButtonCheck.Start();

            tmrFormMonitor.Tick += new EventHandler(tmrFormMonitor_Tick);
            tmrFormMonitor.Interval = TimeSpan.FromSeconds(2);

            tmrTopMost.Tick += new EventHandler(tmrTopMost_Tick);
            tmrTopMost.Interval = TimeSpan.FromSeconds(2);
        }

        private void RefreshListview()
        {
            LstWayPoints = Global.LstWayPoints;
            if (LstWayPoints.Count > 0)
            {
                for (int i = 0; i < LstWayPoints.Count; i++)
                {
                    lstviewMarkers.Items.Add((i + 1).ToString() + ". " + LstWayPoints[i].Name);
                    if (GlobalNavigation.nav1.SelectedMarker.ID == LstWayPoints[i].ID)
                        lstviewMarkers.SelectedIndex = lstviewMarkers.Items.Count - 1;
                }
            }

            foreach (GridViewColumn c in dataGridView.Columns)
            {
                c.Width = 0; //set it to no width
                c.Width = double.NaN; //resize it automatically
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = Global.TopMost;
            RefreshListview();
            lstviewMarkers.ItemContainerGenerator.StatusChanged += new EventHandler(ItemContainerGenerator_StatusChanged);
            tmrTopMost.Start();

        }

        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (lstviewMarkers.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                switch (scrollstatus)
                {
                    case ScrollStatus.Normal:
                        if (lstviewMarkers.Items.Count > 0 && intlstFocusIndex == -1)
                        {
                            intlstFocusIndex = 0;
                            lstviewMarkers.ScrollIntoView(lstviewMarkers.Items[lstviewMarkers.Items.Count - 1]);
                            intlstFocusIndex = lstviewMarkers.Items.Count - 1;
                            ListViewItem item = (ListViewItem)lstviewMarkers.ItemContainerGenerator.ContainerFromIndex(intlstFocusIndex);
                            item.Focus();
                            RereshInforamation(intlstFocusIndex);
                        }
                        break;
                    case ScrollStatus.New:
                        scrollstatus = ScrollStatus.Normal;
                        ImgDownArrow_Press();
                        break;
                    case ScrollStatus.UpDownTop:
                        scrollstatus = ScrollStatus.Normal;
                        ImgDownArrow_Press();
                        break;
                }
            }
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

        private void ImgUpArrow_Press()
        {
            if (lstviewMarkers.Items.Count > 0)
            {
                if (lstviewMarkers.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                {
                    if (intlstFocusIndex > 0)
                        intlstFocusIndex--;
                    else if (intlstFocusIndex == 0)
                    {
                        //    intlstFocusIndex = lstviewMarkers.Items.Count - 1;
                        lstviewMarkers.ScrollIntoView(lstviewMarkers.Items[lstviewMarkers.Items.Count - 1]);
                        intlstFocusIndex = lstviewMarkers.Items.Count - 1;
                    }

                    ListViewItem item = (ListViewItem)lstviewMarkers.ItemContainerGenerator.ContainerFromIndex(intlstFocusIndex);
                    item.Focus();
                    RereshInforamation(intlstFocusIndex);
                }
            }
        }

        private void ImgUpArrow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ImgUpArrow_Press();
        }

        private void ImgDownArrow_Press()
        {
            if (lstviewMarkers.Items.Count > 0)
            {
                if (lstviewMarkers.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                {
                    if (intlstFocusIndex < lstviewMarkers.Items.Count - 1)
                        intlstFocusIndex++;
                    else if (intlstFocusIndex == lstviewMarkers.Items.Count - 1)
                    {
                        lstviewMarkers.ScrollIntoView(lstviewMarkers.Items[0]);
                        intlstFocusIndex = 0;
                        //    intlstFocusIndex = 0;
                    }


                    ListViewItem item = (ListViewItem)lstviewMarkers.ItemContainerGenerator.ContainerFromIndex(intlstFocusIndex);
                    item.Focus();
                    RereshInforamation(intlstFocusIndex);
                }
            }
        }

        private void ImgDownArrow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ImgDownArrow_Press();
        }

        private void Missions_Press()
        {
            DisposeAllComponent();
            frmMissions frmmissions = new frmMissions();
            frmmissions.Show();
            this.Close();
        }

        private void Lbl_Missions_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Missions_Press();
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

        private void Select_Press()
        {
            if (lstviewMarkers.Items.Count > 0)
            {
                lstviewMarkers.SelectedIndex = intlstFocusIndex;
                Global.ActiveWayPointAt(intlstFocusIndex);
            }
        }

        private void Lbl_Select_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Select_Press();
        }

        private void Delete_Press()
        {
            if (LstWayPoints.Count > 0 && lstviewMarkers.Items.Count > 0)
            {
                DisposeAllComponent();
                frmMarkersRemoveConfirmation frmMarkersRemoveConfirmation;
                if (intlstFocusIndex == lstviewMarkers.SelectedIndex)
                {
                    frmMarkersRemoveConfirmation = new frmMarkersRemoveConfirmation(intlstFocusIndex, true);
                }
                else
                    frmMarkersRemoveConfirmation = new frmMarkersRemoveConfirmation(intlstFocusIndex, false);
                frmMarkersRemoveConfirmation.Show();
                this.Close();
            }
        }

        private void Lbl_Delete_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Delete_Press();
        }

        private void Edit_Press()
        {
            if (LstWayPoints.Count > 0 && lstviewMarkers.Items.Count > 0)
            {
                DisposeAllComponent();
                frmMarkerEdit frmMarkerEdit;
                if (intlstFocusIndex == lstviewMarkers.SelectedIndex)
                {
                    frmMarkerEdit = new frmMarkerEdit(intlstFocusIndex, true);
                }
                else
                    frmMarkerEdit = new frmMarkerEdit(intlstFocusIndex, false);

                frmMarkerEdit.Show();
                this.Close();
            }
        }

        private void Lbl_Edit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Edit_Press();
        }

        private void ADD_Press()
        {
            if (!(Global.MissionFileFullName == ""))
            {
                int maxid = 0;
                foreach (WayPoint wp in Global.LstWayPoints)
                {
                    if (maxid < wp.ID)
                        maxid = wp.ID;
                }
                maxid++;

                SelectXMLData.InsertConfiguration(Global.MissionFileFullName, "WayPoint", maxid.ToString(), "添加点" + maxid.ToString(),
                    GlobalNavigation.nav1.Latitude.ToString(), GlobalNavigation.nav1.Longitude.ToString(),
                    GlobalNavigation.nav1.GetDepth().ToString(), "1");


                Global.RemoveAllRoute();
                Global.RemoveAllWaypoints();
                Global.LstWayPoints = SelectXMLData.GetWayPoints(Global.MissionFileFullName);
                Global.WaysPointsModified();

                lstviewMarkers.Items.Clear();
                LstWayPoints.Clear();

                RefreshListview();
                /*
                LstWayPoints = Global.LstWayPoints;
                if (LstWayPoints.Count > 0)
                {
                    for (int i = 0; i < LstWayPoints.Count; i++)
                        lstviewMarkers.Items.Add((i + 1).ToString() + ". " + LstWayPoints[i].Name);

                    foreach (GridViewColumn c in dataGridView.Columns)
                    {
                        c.Width = 0; //set it to no width
                        c.Width = double.NaN; //resize it automatically
                    }
                }
                */
                scrollstatus = ScrollStatus.New;
            }
        }

        private void Lbl_ADD_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ADD_Press();
        }

        private void RereshInforamation(int _index)
        {
            lblNameInfo.Content = LstWayPoints[_index].Name;
            lblLat.Content = LstWayPoints[_index].PointLATLNG.Lat.ToString();
            lblLng.Content = LstWayPoints[_index].PointLATLNG.Lng.ToString();
            lblDepthInfo.Content = LstWayPoints[_index].Depth.ToString();
            switch (LstWayPoints[_index].Type)
            {
                case 1:
                    lblTypeInfo.Content = "路径点";
                    break;
                case 2:
                    lblTypeInfo.Content = "目标点";
                    break;
            }
            lblDistanceToMarkerInfo.Content = Global.CalcDistance(Global.currentMarker.Position, LstWayPoints[_index].PointLATLNG).ToString("0.0") + "米";
            lblBearingToMarkerInfo.Content = Global.CalcBearing(Global.currentMarker.Position, LstWayPoints[_index].PointLATLNG).ToString("0.0") + "度";
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

                if (GlobalUpBoard.GPIOLevel[3] == 0 && GlobalUpBoard.ButtonState[3] == false) //Pressed  Button
                {

                    GlobalUpBoard.ButtonState[3] = true;
                }
                if (GlobalUpBoard.GPIOLevel[3] == 1 && GlobalUpBoard.ButtonState[3] == true)
                    GlobalUpBoard.ButtonState[3] = false;

                if (GlobalUpBoard.GPIOLevel[4] == 0 && GlobalUpBoard.ButtonState[4] == false) //Pressed Missions Button
                {
                    Missions_Press();
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

                if (GlobalUpBoard.GPIOLevel[6] == 0 && GlobalUpBoard.ButtonState[6] == false) //Pressed Select Button
                {
                    Select_Press();
                    GlobalUpBoard.ButtonState[6] = true;
                }
                if (GlobalUpBoard.GPIOLevel[6] == 1 && GlobalUpBoard.ButtonState[6] == true)
                    GlobalUpBoard.ButtonState[6] = false;

                if (GlobalUpBoard.GPIOLevel[7] == 0 && GlobalUpBoard.ButtonState[7] == false) //Pressed Delete Button
                {
                    Delete_Press();
                    GlobalUpBoard.ButtonState[7] = true;
                }
                if (GlobalUpBoard.GPIOLevel[7] == 1 && GlobalUpBoard.ButtonState[7] == true)
                    GlobalUpBoard.ButtonState[7] = false;

                if (GlobalUpBoard.GPIOLevel[8] == 0 && GlobalUpBoard.ButtonState[8] == false) //Pressed Edit Button
                {
                    Edit_Press();
                    GlobalUpBoard.ButtonState[8] = true;
                }
                if (GlobalUpBoard.GPIOLevel[8] == 1 && GlobalUpBoard.ButtonState[8] == true)
                    GlobalUpBoard.ButtonState[8] = false;

                if (GlobalUpBoard.GPIOLevel[9] == 0 && GlobalUpBoard.ButtonState[9] == false) //Pressed ADD Button
                {
                    ADD_Press();
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

        void tmrTopMost_Tick(object sender, EventArgs e)
        {
            this.Topmost = Global.TopMost;
            this.Focus();
        }
    }
}
