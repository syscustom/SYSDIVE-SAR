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
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Demo.WindowsPresentation.CustomMarkers;
using System.Data;
using System.Windows.Threading;
using System.Drawing;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace WpfApp1
{
    /// <summary>
    /// frmDiveLogReplay.xaml 的交互逻辑
    /// </summary>
    public partial class frmDiveLogReplay : Window
    {
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]

        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public enum PlayStatus
        {
            Stop = 0,
            Pause = 1,
            Play = 2
        }

        Map ReplayMap = new Map();
        string strFileName = "";
        List<WayPoint> LstWayPoints = new List<WayPoint>();
        List<WayPoint> LstAllWayPoints = new List<WayPoint>();
        List<WayPoint> LstMarkers = new List<WayPoint>();

        GMapMarker currentMarker;

        DispatcherTimer tmrButtonCheck = new DispatcherTimer();
        DispatcherTimer tmrFormMonitor = new DispatcherTimer();
        DispatcherTimer tmrPlayerTimer = new DispatcherTimer();
        DispatcherTimer tmrBatteryCheck = new DispatcherTimer();

        int PlayTimeIndex = 0;

        PlayStatus playstatus = PlayStatus.Stop;

        public frmDiveLogReplay()
        {
            InitializeComponent();
        }

        public frmDiveLogReplay(string _filename)
        {
            InitializeComponent();
            strFileName = _filename;

            tmrButtonCheck.Tick += new EventHandler(tmrButtonCheck_Tick);
            tmrButtonCheck.Interval = TimeSpan.FromMilliseconds(5);
            tmrButtonCheck.Start();

            tmrFormMonitor.Tick += new EventHandler(tmrFormMonitor_Tick);
            tmrFormMonitor.Interval = TimeSpan.FromSeconds(2);

            tmrPlayerTimer.Tick += new EventHandler(tmrPlayerTimer_Tick);
            tmrPlayerTimer.Interval = TimeSpan.FromSeconds(Global.RecordSamplingRate);

            tmrBatteryCheck.Tick += new EventHandler(tmrBatteryCheck_Tick);
            tmrBatteryCheck.Interval = TimeSpan.FromSeconds(5);
            tmrBatteryCheck.Start();

            CreatMap();
            Content_Map.Content = ReplayMap;
            Paint();

        }


        public void CreatMap()
        {
            ReplayMap.Manager.Mode = AccessMode.ServerAndCache;
            ReplayMap.MaxZoom = 18;
            ReplayMap.MinZoom = 3;
            ReplayMap.Zoom = 13;

            // config map 
            ReplayMap.MapProvider = GMapProviders.AmapMap;

            currentMarker = new GMapMarker(ReplayMap.Position);
            {
                currentMarker.Shape = new CustomMarkerRed(currentMarker, "custom position marker");
                currentMarker.Offset = new System.Windows.Point(-15, -15);
                currentMarker.ZIndex = int.MaxValue;
                ReplayMap.Markers.Add(currentMarker);
            }

        }

        private void Paint()
        {
            try
            {
                int spaceindex = 0;
                int spaceindextotal = 0;
                DataTable dt = CSVFileHelper.GetCSVALL(strFileName);


                if (dt.Rows.Count > 0 && dt.Rows.Count <= 5)
                    spaceindextotal = 1;

                if (dt.Rows.Count > 5 && dt.Rows.Count <= 10)
                    spaceindextotal = 2;

                if (dt.Rows.Count > 10 && dt.Rows.Count <= 50)
                    spaceindextotal = 3;

                if (dt.Rows.Count > 50 && dt.Rows.Count <= 100)
                    spaceindextotal = 5;

                if (dt.Rows.Count > 100 && dt.Rows.Count <= 500)
                    spaceindextotal = 8;

                if (dt.Rows.Count > 500 && dt.Rows.Count <= 1000)
                    spaceindextotal = 10;

                if (dt.Rows.Count > 1000 && dt.Rows.Count <= 1500)
                    spaceindextotal = 15;

                if (dt.Rows.Count > 1500)
                    spaceindextotal = 20;

                if (dt.Columns[0].ColumnName != "")
                    LstMarkers = SelectXMLData.GetWayPoints(Global.SavingMissionDirectory + dt.Columns[0].ColumnName + ".mis");


                foreach (DataRow dr in dt.Rows)
                {
                    WayPoint _waypoint = new WayPoint();
                    PointLatLng _pointlatlng = new PointLatLng();


                    _pointlatlng.Lat = Convert.ToDouble(dr[0]);
                    _pointlatlng.Lng = Convert.ToDouble(dr[1]);
                    _waypoint.PointLATLNG = _pointlatlng;
                    _waypoint.Heading = Convert.ToDouble(dr[2]);
                    _waypoint.Depth = Convert.ToDouble(dr[3]);
                    _waypoint.Distance = Convert.ToDouble(dr[4]);
                    _waypoint.SelectedMarkerName = Convert.ToString(dr[5]);
                    _waypoint.Bearing = Convert.ToDouble(dr[6]);

                    if (_pointlatlng.Lat == 0 || _pointlatlng.Lng == 0) continue;

                    LstAllWayPoints.Add(_waypoint);
                    spaceindex++;
                    if (spaceindex == spaceindextotal)
                    {
                        LstWayPoints.Add(_waypoint);
                        spaceindex = 0;
                    }
                }

                PaintAllWayPoints();
                PrintAllMarkers();
                PaintAllWayPointRoutes();
                if (LstWayPoints.Count > 0)
                    ReplayMap.Position = LstWayPoints[0].PointLATLNGGCJ02;
            }
            catch { }

        }

        private void PaintAllWayPoints()
        {
            foreach (WayPoint wp in LstWayPoints)
            {
                GMapMarker m = new GMapMarker(wp.PointLATLNG);
                {
                    Placemark? p = null;
                    string ToolTipText;
                    if (p != null)
                    {
                        ToolTipText = p.Value.Address;
                    }
                    else
                    {
                        ToolTipText = "";
                    }

                    //m.Shape = new CustomMarkerDemo(m, wp.Name);
                    m.ZIndex = 55;

                }
                ReplayMap.Markers.Add(m);
            }
        }

        private void PrintAllMarkers()
        {
            foreach (WayPoint wp in LstMarkers)
            {
                GMapMarker m = new GMapMarker(wp.PointLATLNG);
                {
                    Placemark? p = null;
                    string ToolTipText;
                    if (p != null)
                    {
                        ToolTipText = p.Value.Address;
                    }
                    else
                    {
                        ToolTipText = "";
                    }

                    m.Shape = new CustomMarkerDemo(m, wp.Name);
                    m.ZIndex = 55;

                }
                ReplayMap.Markers.Add(m);
            }
        }

        private void PaintAllWayPointRoutes()
        {
            RemoveAllRoute();
            for (int i = 0; i < LstWayPoints.Count - 1; i++)
            {
                GMapRoute gmRoute = new GMapRoute(new List<PointLatLng>() { LstWayPoints[i].PointLATLNG, (LstWayPoints.Count - 1) == i ? LstWayPoints[i].PointLATLNG : LstWayPoints[i + 1].PointLATLNG })
                {
                    Shape = new Line()
                    {
                        StrokeThickness = 2,
                        //Stroke = System.Windows.Media.Brushes.BlueViolet
                        Stroke = System.Windows.Media.Brushes.White,
                    }
                };
                gmRoute.RegenerateShape(ReplayMap);
                ((System.Windows.Shapes.Path)gmRoute.Shape).Stroke = new SolidColorBrush(Colors.Blue);
                ((System.Windows.Shapes.Path)gmRoute.Shape).StrokeThickness = 5;
                ReplayMap.Markers.Add(gmRoute);
            }
        }

        public void RemoveAllRoute()
        {
            for (int i = 0; i < ReplayMap.Markers.Count; i++)
            {
                if (ReplayMap.Markers[i].GetType() == typeof(GMapRoute))
                {
                    ReplayMap.Markers.RemoveAt(i);
                    if (i > 0) i -= 1;
                }
            }
        }

        private void DisposeAllComponent()
        {
            tmrFormMonitor.Start();
            Content_Map.Content = null;
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

        private void Play_Press()
        {
            switch(playstatus)
            {
                case PlayStatus.Stop:
                    playstatus = PlayStatus.Play;
                    if (LstWayPoints.Count > 0)
                    {
                        tmrPlayerTimer.Start();
                        lbl_Play.Content = "暂 停";
                    }
                    break;
                case PlayStatus.Play:
                    playstatus = PlayStatus.Pause;
                    if (LstWayPoints.Count > 0)
                    {
                        tmrPlayerTimer.Stop();
                        lbl_Play.Content = "播 放";
                    }
                    break;
                case PlayStatus.Pause:
                    playstatus = PlayStatus.Play;
                    if (LstWayPoints.Count > 0)
                    {
                        tmrPlayerTimer.Start();
                        lbl_Play.Content = "暂 停";
                    }
                    break;
            }
        }

        private void Lbl_Play_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Play_Press();
        }

        private void Stop_Press()
        {
            switch (playstatus)
            {
                case PlayStatus.Play:
                case PlayStatus.Pause:
                    tmrPlayerTimer.Stop();
                    PlayTimeIndex = 0;
                    playstatus = PlayStatus.Stop;
                    if (LstWayPoints.Count > 0)
                    {
                        currentMarker.Position = new PointLatLng(LstWayPoints[0].PointLATLNG.Lat, LstWayPoints[0].PointLATLNG.Lng);
                        ReplayMap.Position = currentMarker.PositionGCJ02;
                    }
                    lbl_Play.Content = "播 放";
                    break;
                case PlayStatus.Stop:
                    break;
            }


        }

        private void Lbl_Stop_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Stop_Press();
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

        private void Return_Press()
        {
            DisposeAllComponent();
            frmDiveLog frmDiveLog = new frmDiveLog();
            frmDiveLog.Show();
            this.Close();
        }

        private void Lbl_Return_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Return_Press();
        }

        private void X1_Press()
        {
            switch(playstatus)
            {
                case PlayStatus.Play:
                    tmrPlayerTimer.Stop();
                    tmrPlayerTimer.Interval = TimeSpan.FromSeconds(Global.RecordSamplingRate);
                    tmrPlayerTimer.Start();
                    break;
                case PlayStatus.Pause:
                case PlayStatus.Stop:
                    tmrPlayerTimer.Interval = TimeSpan.FromSeconds(Global.RecordSamplingRate);
                    break;
            }
        }

        private void Lbl_X1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            X1_Press();
        }

        private void X10_Press()
        {
            switch (playstatus)
            {
                case PlayStatus.Play:
                    tmrPlayerTimer.Stop();
                    tmrPlayerTimer.Interval = TimeSpan.FromMilliseconds(Global.RecordSamplingRate * 100);
                    tmrPlayerTimer.Start();
                    break;
                case PlayStatus.Pause:
                case PlayStatus.Stop:
                    tmrPlayerTimer.Interval = TimeSpan.FromMilliseconds(Global.RecordSamplingRate * 100);
                    break;
            }
        }

        private void Lbl_X10_MouseUp(object sender, MouseButtonEventArgs e)
        {
            X10_Press();
        }

        private void ZoomPlus_Press()
        {
            if (ReplayMap.Zoom < ReplayMap.MaxZoom)
                ReplayMap.Zoom += 1;
        }


        private void Lbl_ZoomPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ZoomPlus_Press();
        }

        private void ZoomMinus_Press()
        {
            if (ReplayMap.Zoom > ReplayMap.MinZoom)
                ReplayMap.Zoom -= 1;
        }


        private void Lbl_ZoomMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ZoomMinus_Press();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tmrPlayerTimer.Stop();
            tmrFormMonitor.Stop();
            tmrButtonCheck.Stop();
            tmrBatteryCheck.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = Global.TopMost;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
        }

        void tmrPlayerTimer_Tick(object sender, EventArgs e)
        {
            lblHeadingInfo.Content = LstAllWayPoints[PlayTimeIndex].Heading;

            ReplayMap.Bearing = 0;
            ((CustomMarkerRed)currentMarker.Shape).Rotate(LstAllWayPoints[PlayTimeIndex].Heading);

            lblDepthInfo.Content = LstAllWayPoints[PlayTimeIndex].Depth;
            lblDistance.Content = LstAllWayPoints[PlayTimeIndex].Distance;
            lblWayPointName.Content = LstAllWayPoints[PlayTimeIndex].SelectedMarkerName;
            lblBearing.Content = LstAllWayPoints[PlayTimeIndex].Bearing;
            currentMarker.Position = new PointLatLng(LstAllWayPoints[PlayTimeIndex].PointLATLNG.Lat, LstAllWayPoints[PlayTimeIndex].PointLATLNG.Lng);
            ReplayMap.Position = currentMarker.PositionGCJ02;
            PlayTimeIndex++;
            if (PlayTimeIndex == LstAllWayPoints.Count)
            {
                PlayTimeIndex = 0;
                Stop_Press();
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


                if (GlobalUpBoard.GPIOLevel[1] == 0 && GlobalUpBoard.ButtonState[1] == false) //Pressed Play Button
                {
                    Play_Press();
                    GlobalUpBoard.ButtonState[1] = true;
                }
                if (GlobalUpBoard.GPIOLevel[1] == 1 && GlobalUpBoard.ButtonState[1] == true)
                    GlobalUpBoard.ButtonState[1] = false;

                if (GlobalUpBoard.GPIOLevel[2] == 0 && GlobalUpBoard.ButtonState[2] == false) //Pressed Stop Button
                {
                    Stop_Press();
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

                if (GlobalUpBoard.GPIOLevel[4] == 0 && GlobalUpBoard.ButtonState[4] == false) //Pressed Return Button
                {
                    Return_Press();
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

                if (GlobalUpBoard.GPIOLevel[6] == 0 && GlobalUpBoard.ButtonState[6] == false) //Pressed X1 Button
                {
                    X1_Press();
                    GlobalUpBoard.ButtonState[6] = true;
                }
                if (GlobalUpBoard.GPIOLevel[6] == 1 && GlobalUpBoard.ButtonState[6] == true)
                    GlobalUpBoard.ButtonState[6] = false;

                if (GlobalUpBoard.GPIOLevel[7] == 0 && GlobalUpBoard.ButtonState[7] == false) //Pressed X10 Button
                {
                    X10_Press();
                    GlobalUpBoard.ButtonState[7] = true;
                }
                if (GlobalUpBoard.GPIOLevel[7] == 1 && GlobalUpBoard.ButtonState[7] == true)
                    GlobalUpBoard.ButtonState[7] = false;

                if (GlobalUpBoard.GPIOLevel[8] == 0 && GlobalUpBoard.ButtonState[8] == false) //Pressed ZoomPlus Button
                {
                    ZoomPlus_Press();
                    GlobalUpBoard.ButtonState[8] = true;
                }
                if (GlobalUpBoard.GPIOLevel[8] == 1 && GlobalUpBoard.ButtonState[8] == true)
                    GlobalUpBoard.ButtonState[8] = false;

                if (GlobalUpBoard.GPIOLevel[9] == 0 && GlobalUpBoard.ButtonState[9] == false) //Pressed ZoomMinus Button
                {
                    ZoomMinus_Press();
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

        void tmrFormMonitor_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        void tmrBatteryCheck_Tick(object sender, EventArgs e)
        {
            System.Windows.Forms.PowerStatus status = System.Windows.Forms.SystemInformation.PowerStatus;
            //float percent = status.BatteryLifePercent;
            float percent = GlobalBattery.GetBatteryPowerPercent();
            string percent_text = percent.ToString("P0");
            /*
            if (status.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online)
            {
                if (percent < 1.0f)
                    lblStatus.Text = percent_text + ", charging";
                else
                    lblStatus.Text = "Online fully charged";
            }
            else
            {
                lblStatus.Text = "Offline, " + percent_text + " remaining";
            }
            */
            Battery_Img.Source = ImageSourceForBitmap(DrawBattery(percent, 100, 50, System.Drawing.Color.Black, System.Drawing.Color.White, System.Drawing.Color.Green, System.Drawing.Color.Black, true));

            GC.Collect();
        }

        private ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            IntPtr handle = bmp.GetHbitmap();
            try
            {
                ImageSource newSource = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(handle);
                return newSource;
            }
            catch (Exception ex)
            {
                DeleteObject(handle);
                return null;
            }
        }

        private Bitmap DrawBattery(
    float percent, int wid, int hgt,
    System.Drawing.Color bg_color, System.Drawing.Color outline_color,
    System.Drawing.Color charged_color, System.Drawing.Color uncharged_color,
    bool striped)
        {
            Bitmap bm = new Bitmap(wid, hgt);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                // If the battery has a horizontal orientation,
                // rotate so we can draw it vertically.
                if (wid > hgt)
                {
                    gr.RotateTransform(90, MatrixOrder.Append);
                    gr.TranslateTransform(wid, 0, MatrixOrder.Append);
                    int temp = wid;
                    wid = hgt;
                    hgt = temp;
                }

                // Draw the battery.
                DrawVerticalBattery(gr, percent, wid, hgt, bg_color,
                    outline_color, charged_color, uncharged_color,
                    striped);
            }
            return bm;
        }


        // Draw a vertically oriented battery with
        // the indicated percentage filled in.
        private void DrawVerticalBattery(Graphics gr,
            float percent, int wid, int hgt,
            System.Drawing.Color bg_color, System.Drawing.Color outline_color,
            System.Drawing.Color charged_color, System.Drawing.Color uncharged_color,
            bool striped)
        {
            gr.Clear(bg_color);
            gr.SmoothingMode = SmoothingMode.AntiAlias;

            // Make a rectangle for the main body.
            float thickness = hgt / 20f;
            RectangleF body_rect = new RectangleF(
                thickness * 0.5f, thickness * 1.5f,
                wid - thickness, hgt - thickness * 2f);

            using (System.Drawing.Pen pen = new System.Drawing.Pen(outline_color, thickness))
            {
                // Fill the body with the uncharged color.
                using (System.Drawing.Brush brush = new SolidBrush(uncharged_color))
                {
                    gr.FillRectangle(brush, body_rect);
                }

                // Fill the charged area.
                float charged_hgt = body_rect.Height * percent;
                RectangleF charged_rect = new RectangleF(
                    body_rect.Left, body_rect.Bottom - charged_hgt,
                    body_rect.Width, charged_hgt);
                using (System.Drawing.Brush brush = new SolidBrush(charged_color))
                {
                    gr.FillRectangle(brush, charged_rect);
                }

                // Optionally stripe multiples of 25%
                if (striped)
                    for (int i = 1; i <= 3; i++)
                    {
                        float y = body_rect.Bottom -
                            i * 0.25f * body_rect.Height;
                        gr.DrawLine(pen, body_rect.Left, y,
                            body_rect.Right, y);
                    }

                // Draw the main body.
                gr.DrawPath(pen, MakeRoundedRect(
                    body_rect, thickness, thickness,
                    true, true, true, true));

                // Draw the positive terminal.
                RectangleF terminal_rect = new RectangleF(
                    wid / 2f - thickness, 0,
                    2 * thickness, thickness);
                gr.DrawPath(pen, MakeRoundedRect(
                    terminal_rect, thickness / 2f, thickness / 2f,
                    true, true, false, false));
            }
        }

        // Draw a rectangle in the indicated Rectangle
        // rounding the indicated corners.
        private GraphicsPath MakeRoundedRect(
            RectangleF rect, float xradius, float yradius,
            bool round_ul, bool round_ur, bool round_lr, bool round_ll)
        {
            // Make a GraphicsPath to draw the rectangle.
            PointF point1, point2;
            GraphicsPath path = new GraphicsPath();

            // Upper left corner.
            if (round_ul)
            {
                RectangleF corner = new RectangleF(
                    rect.X, rect.Y,
                    2 * xradius, 2 * yradius);
                path.AddArc(corner, 180, 90);
                point1 = new PointF(rect.X + xradius, rect.Y);
            }
            else point1 = new PointF(rect.X, rect.Y);

            // Top side.
            if (round_ur)
                point2 = new PointF(rect.Right - xradius, rect.Y);
            else
                point2 = new PointF(rect.Right, rect.Y);
            path.AddLine(point1, point2);

            // Upper right corner.
            if (round_ur)
            {
                RectangleF corner = new RectangleF(
                    rect.Right - 2 * xradius, rect.Y,
                    2 * xradius, 2 * yradius);
                path.AddArc(corner, 270, 90);
                point1 = new PointF(rect.Right, rect.Y + yradius);
            }
            else point1 = new PointF(rect.Right, rect.Y);

            // Right side.
            if (round_lr)
                point2 = new PointF(rect.Right, rect.Bottom - yradius);
            else
                point2 = new PointF(rect.Right, rect.Bottom);
            path.AddLine(point1, point2);

            // Lower right corner.
            if (round_lr)
            {
                RectangleF corner = new RectangleF(
                    rect.Right - 2 * xradius,
                    rect.Bottom - 2 * yradius,
                    2 * xradius, 2 * yradius);
                path.AddArc(corner, 0, 90);
                point1 = new PointF(rect.Right - xradius, rect.Bottom);
            }
            else point1 = new PointF(rect.Right, rect.Bottom);

            // Bottom side.
            if (round_ll)
                point2 = new PointF(rect.X + xradius, rect.Bottom);
            else
                point2 = new PointF(rect.X, rect.Bottom);
            path.AddLine(point1, point2);

            // Lower left corner.
            if (round_ll)
            {
                RectangleF corner = new RectangleF(
                    rect.X, rect.Bottom - 2 * yradius,
                    2 * xradius, 2 * yradius);
                path.AddArc(corner, 90, 90);
                point1 = new PointF(rect.X, rect.Bottom - yradius);
            }
            else point1 = new PointF(rect.X, rect.Bottom);

            // Left side.
            if (round_ul)
                point2 = new PointF(rect.X, rect.Y + yradius);
            else
                point2 = new PointF(rect.X, rect.Y);
            path.AddLine(point1, point2);

            // Join with the start point.
            path.CloseFigure();

            return path;
        }
    }
}
