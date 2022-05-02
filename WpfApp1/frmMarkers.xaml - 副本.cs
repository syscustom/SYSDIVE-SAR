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

namespace WpfApp1
{
    /// <summary>
    /// frmMarkers.xaml 的交互逻辑
    /// </summary>
    public partial class frmMarkers : Window
    {
        private List<WayPoint> LstWayPoints;
        int intlstFocusIndex = 0;

        public frmMarkers()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LstWayPoints = Global.LstWayPoints;
            if (LstWayPoints.Count>0)
            {
                for (int i = 0; i < LstWayPoints.Count; i++)
                    lstviewMarkers.Items.Add((i+1).ToString() + ". " + LstWayPoints[i].Name);
            }
            lstviewMarkers.ItemContainerGenerator.StatusChanged += new EventHandler(ItemContainerGenerator_StatusChanged);

        }

        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (lstviewMarkers.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                if (lstviewMarkers.Items.Count > 0)
                {
                    intlstFocusIndex = 0;
                    ListViewItem item = (ListViewItem)lstviewMarkers.ItemContainerGenerator.ContainerFromIndex(intlstFocusIndex);
                    item.Focus();
                    RereshInforamation(intlstFocusIndex);
                }
            }
        }

            private void Lbl_Home_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow mainwindow = new MainWindow();
            mainwindow.Show();
            this.Close();
        }

        private void Lbl_Missions_MouseUp(object sender, MouseButtonEventArgs e)
        {
            frmMissions frmmissions = new frmMissions();
            frmmissions.Show();
            this.Close();
        }

        private void ImgUpArrow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lstviewMarkers.Items.Count > 0)
            {
                if (lstviewMarkers.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                {
                    if (intlstFocusIndex > 0)
                        intlstFocusIndex--;
                    else if (intlstFocusIndex == 0)
                        intlstFocusIndex = lstviewMarkers.Items.Count - 1;

                    ListViewItem item = (ListViewItem)lstviewMarkers.ItemContainerGenerator.ContainerFromIndex(intlstFocusIndex);
                    item.Focus();
                    RereshInforamation(intlstFocusIndex);
                }
            }
        }

        private void ImgDownArrow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lstviewMarkers.Items.Count > 0)
            {
                if (lstviewMarkers.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                {
                    if (intlstFocusIndex < lstviewMarkers.Items.Count - 1)
                        intlstFocusIndex++;
                    else if (intlstFocusIndex == lstviewMarkers.Items.Count - 1)
                        intlstFocusIndex = 0;

                    ListViewItem item = (ListViewItem)lstviewMarkers.ItemContainerGenerator.ContainerFromIndex(intlstFocusIndex);
                    item.Focus();
                    RereshInforamation(intlstFocusIndex);
                }
            }
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

        }

        private void Lbl_Power_MouseUp(object sender, MouseButtonEventArgs e)
        {
            frmPower frmPower = new frmPower("frmMarkers");
            frmPower.Show();
            this.Close();
        }
    }
}
