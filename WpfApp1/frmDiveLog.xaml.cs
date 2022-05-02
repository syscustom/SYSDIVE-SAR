﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
    /// frmDiveLog.xaml 的交互逻辑
    /// </summary>
    public partial class frmDiveLog : Window
    {
        enum ScrollStatus
        {
            Normal = 0,
            UpDownTop = 1,
            New = 2
        }

        static List<FileInfo> lst = new List<FileInfo>();
        List<FileInfo> lstFiles = new List<FileInfo>();
        int intlstFocusIndex = -1;
        private string datePatt = @"yyyy-MM-dd, HH:mm:ss";
        private ScrollStatus scrollstatus = ScrollStatus.Normal;

        DispatcherTimer tmrButtonCheck = new DispatcherTimer();
        DispatcherTimer tmrFormMonitor = new DispatcherTimer();
        DispatcherTimer tmrTopMost = new DispatcherTimer();

        public frmDiveLog()
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
            //string strPath = Environment.CurrentDirectory;
            string strPath = Global.SavingRecordDirectory;
            //strPath = strPath + "\\" + "Record";
            lst.Clear();
            lstFiles.Clear();
            lstFiles = getFile(strPath, ".reclog", lst);
            foreach (FileInfo shpFile in lstFiles)
            {
                lstviewDiveLogs.Items.Add(System.IO.Path.GetFileNameWithoutExtension(shpFile.Name));
                //if (shpFile.Name == SelectXMLData.GetConfiguration("MissionFile", "value"))
                //{
                //    lstviewDiveLogs.SelectedIndex = lstviewDiveLogs.Items.Count - 1;
                //}
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = Global.TopMost;
            RefreshListview();

            lstviewDiveLogs.ItemContainerGenerator.StatusChanged += new EventHandler(ItemContainerGenerator_StatusChanged);
            tmrTopMost.Start();
        }

        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (lstviewDiveLogs.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                switch (scrollstatus)
                {
                    case ScrollStatus.Normal:
                        if (lstviewDiveLogs.Items.Count > 0 && intlstFocusIndex == -1)
                        {
                            intlstFocusIndex = 0;
                            ListViewItem item = (ListViewItem)lstviewDiveLogs.ItemContainerGenerator.ContainerFromIndex(intlstFocusIndex);
                            item.Focus();

                            string _filename = System.IO.Path.GetFileNameWithoutExtension(lstFiles[intlstFocusIndex].Name);
                            lblFileNameInfo.Content = _filename.Replace("_", "__");

                            //lblFileDateInfo.Content = lstFiles[intlstFocusIndex].CreationTime.ToString(datePatt);
                            string[] dt = CSVFileHelper.GetCSVFirst(lstFiles[intlstFocusIndex].FullName);
                            dt[0].Replace("_", "__");
                            lblMissionFileUsedInfo.Content = dt[0];
                            lblFileDateInfo.Content = dt[1] + " , " + dt[2];
                            if (dt[0] != "")
                                lblMarkersCountInfo.Content = SelectXMLData.GetWayPoints(Global.SavingMissionDirectory + dt[0] + ".mis").Count.ToString();

                            DataTable dt1 = CSVFileHelper.GetCSVALL(lstFiles[intlstFocusIndex].FullName);
                            DataRow dr1 = dt1.Rows[0];
                            DataRow dr2 = dt1.Rows[dt1.Rows.Count - 1];
                            DateTime dtime1 = Convert.ToDateTime(dr1[8].ToString());
                            DateTime dtime2 = Convert.ToDateTime(dr2[8].ToString());

                            TimeSpan ts = dtime2 - dtime1;
                            lblDurationInfo.Content = ts.Hours.ToString() + " 小时 " + ts.Minutes.ToString() + " 分 " + ts.Seconds.ToString() + " 秒";

                            string filesize = string.Format("{0:0.0}", lstFiles[intlstFocusIndex].Length / 1024.0);
                            lbFileSizeInfo.Content = filesize + " KB";
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

        private static List<FileInfo> getFile(string path, string extName, List<FileInfo> lst)
        {
            try
            {
                string[] dir = Directory.GetDirectories(path); //文件夹列表  
                DirectoryInfo fdir = new DirectoryInfo(path);
                FileInfo[] file = fdir.GetFiles();
                //FileInfo[] file = Directory.GetFiles(path); //文件列表  
                if (file.Length != 0 || dir.Length != 0) //当前目录文件或文件夹不为空          
                {
                    foreach (FileInfo f in file) //显示当前目录所有文件  
                    {
                        if (extName.ToLower().IndexOf(f.Extension.ToLower()) >= 0)
                        {
                            lst.Add(f);
                        }
                    }
                    foreach (string d in dir)
                    {
                        getFile(d, extName, lst);//递归  
                    }
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Power_Press()
        {
            DisposeAllComponent();
            frmPower frmPower = new frmPower("frmDiveLog");
            frmPower.Show();
            this.Close();
        }

        private void Lbl_Power_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Power_Press();
        }

        private void ImgUpArrow_Press()
        {
            try
            {
                if (lstviewDiveLogs.Items.Count > 0)
                {
                    if (lstviewDiveLogs.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                    {
                        if (intlstFocusIndex > 0)
                            intlstFocusIndex--;
                        else if (intlstFocusIndex == 0)
                        {
                            //intlstFocusIndex = lstviewDiveLogs.Items.Count - 1;
                            lstviewDiveLogs.ScrollIntoView(lstviewDiveLogs.Items[lstviewDiveLogs.Items.Count - 1]);
                            intlstFocusIndex = lstviewDiveLogs.Items.Count - 1;
                        }

                        ListViewItem item = (ListViewItem)lstviewDiveLogs.ItemContainerGenerator.ContainerFromIndex(intlstFocusIndex);
                        item.Focus();

                        string _filename = System.IO.Path.GetFileNameWithoutExtension(lstFiles[intlstFocusIndex].Name);
                        lblFileNameInfo.Content = _filename.Replace("_", "__");

                        string[] dt = CSVFileHelper.GetCSVFirst(lstFiles[intlstFocusIndex].FullName);
                        dt[0].Replace("_", "__");
                        lblMissionFileUsedInfo.Content = dt[0];
                        lblFileDateInfo.Content = dt[1] + " , " + dt[2];
                        if (dt[0] != "")
                            lblMarkersCountInfo.Content = SelectXMLData.GetWayPoints(Global.SavingMissionDirectory + dt[0] + ".mis").Count.ToString();
                        else
                            lblMarkersCountInfo.Content = "0";

                        DataTable dt1 = CSVFileHelper.GetCSVALL(lstFiles[intlstFocusIndex].FullName);
                        DataRow dr1 = dt1.Rows[0];
                        DataRow dr2 = dt1.Rows[dt1.Rows.Count - 1];
                        DateTime dtime1 = Convert.ToDateTime(dr1[8].ToString());
                        DateTime dtime2 = Convert.ToDateTime(dr2[8].ToString());

                        TimeSpan ts = dtime2 - dtime1;
                        lblDurationInfo.Content = ts.Hours.ToString() + " 小时 " + ts.Minutes.ToString() + " 分 " + ts.Seconds.ToString() + " 秒";

                        string filesize = string.Format("{0:0.0}", lstFiles[intlstFocusIndex].Length / 1024.0);
                        lbFileSizeInfo.Content = filesize + " KB";

                    }
                }
            }
            catch { }

        }

        private void ImgUpArrow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ImgUpArrow_Press();
        }

        private void ImgDownArrow_Press()
        {
            try
            {
                if (lstviewDiveLogs.Items.Count > 0)
                {
                    if (lstviewDiveLogs.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                    {
                        if (intlstFocusIndex < lstviewDiveLogs.Items.Count - 1)
                            intlstFocusIndex++;
                        else if (intlstFocusIndex == lstviewDiveLogs.Items.Count - 1)
                        {
                            lstviewDiveLogs.ScrollIntoView(lstviewDiveLogs.Items[0]);
                            intlstFocusIndex = 0;
                            //intlstFocusIndex = 0;
                        }


                        ListViewItem item = (ListViewItem)lstviewDiveLogs.ItemContainerGenerator.ContainerFromIndex(intlstFocusIndex);
                        item.Focus();

                        string _filename = System.IO.Path.GetFileNameWithoutExtension(lstFiles[intlstFocusIndex].Name);
                        lblFileNameInfo.Content = _filename.Replace("_", "__");

                        string[] dt = CSVFileHelper.GetCSVFirst(lstFiles[intlstFocusIndex].FullName);
                        lblMissionFileUsedInfo.Content = dt[0];
                        lblFileDateInfo.Content = dt[1] + " , " + dt[2];
                        if (dt[0] != "")
                            lblMarkersCountInfo.Content = SelectXMLData.GetWayPoints(Global.SavingMissionDirectory + dt[0] + ".mis").Count.ToString();
                        else
                            lblMarkersCountInfo.Content = "0";

                        DataTable dt1 = CSVFileHelper.GetCSVALL(lstFiles[intlstFocusIndex].FullName);
                        DataRow dr1 = dt1.Rows[0];
                        DataRow dr2 = dt1.Rows[dt1.Rows.Count - 1];
                        DateTime dtime1 = Convert.ToDateTime(dr1[8].ToString());
                        DateTime dtime2 = Convert.ToDateTime(dr2[8].ToString());

                        TimeSpan ts = dtime2 - dtime1;
                        lblDurationInfo.Content = ts.Hours.ToString() + " 小时 " + ts.Minutes.ToString() + " 分 " + ts.Seconds.ToString() + " 秒";

                        string filesize = string.Format("{0:0.0}", lstFiles[intlstFocusIndex].Length / 1024.0);
                        lbFileSizeInfo.Content = filesize + " KB";
                    }
                }

            }
            catch { }

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

        private void Select_Press()
        {
            if (lstviewDiveLogs.Items.Count > 0)
            {
                DisposeAllComponent();
                frmDiveLogReplay frmDiveLogReplay;
                frmDiveLogReplay = new frmDiveLogReplay(lstFiles[intlstFocusIndex].FullName);
                frmDiveLogReplay.Show();
                this.Close();
            }
        }

        private void Lbl_Select_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Select_Press();
        }

        private void Delete_Press()
        {
            if (lstviewDiveLogs.Items.Count > 0)
            {
                DisposeAllComponent();
                frmDiveLogRemoveConfirmation frmDiveLogRemoveConfirmation;
                if (intlstFocusIndex == lstviewDiveLogs.SelectedIndex)
                {
                    frmDiveLogRemoveConfirmation = new frmDiveLogRemoveConfirmation(lstFiles[intlstFocusIndex].FullName, true);
                }
                else
                    frmDiveLogRemoveConfirmation = new frmDiveLogRemoveConfirmation(lstFiles[intlstFocusIndex].FullName, false);
                frmDiveLogRemoveConfirmation.Show();
                this.Close();
            }
        }

        private void Lbl_Delete_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Delete_Press();
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

                if (GlobalUpBoard.GPIOLevel[3] == 0 && GlobalUpBoard.ButtonState[3] == false)
                {

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

                if (GlobalUpBoard.GPIOLevel[8] == 0 && GlobalUpBoard.ButtonState[8] == false) //Pressed  Button
                {

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

        private void Lbl_SnapShots_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        void tmrTopMost_Tick(object sender, EventArgs e)
        {
            this.Topmost = Global.TopMost;
            this.Focus();
        }
    }
}
