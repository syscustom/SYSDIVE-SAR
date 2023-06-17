using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using Demo.WindowsPresentation.CustomMarkers;

namespace WpfApp1
{
    /// <summary>
    /// NavCommUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class NavCommUserControl : UserControl
    {
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]

        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        private Iocomp.Instrumentation.Professional.SlidingCompass slidingCompass1;

        DispatcherTimer tmrNavCommCheck = new DispatcherTimer();
        DispatcherTimer tmrBatteryCheck = new DispatcherTimer();
        DispatcherTimer tmrNavStartRecordLog = new DispatcherTimer();

        bool IsStartRecordLog = false;

        string strRecordPath = "";

        double Nheading = 0.0;

        public NavCommUserControl()
        {
            InitializeComponent();
            tmrNavCommCheck.Tick += new EventHandler(tmrNavCommCheck_Tick);
            tmrNavCommCheck.Interval = TimeSpan.FromMilliseconds(10);
            tmrNavCommCheck.Start();

            tmrBatteryCheck.Tick += new EventHandler(tmrBatteryCheck_Tick);
            tmrBatteryCheck.Interval = TimeSpan.FromSeconds(5);
            tmrBatteryCheck.Start();

            tmrNavStartRecordLog.Tick += new EventHandler(tmrNavStartRecordLog_Tick);
            tmrNavStartRecordLog.Interval = TimeSpan.FromSeconds(2); //2秒钟间隔

            Iocomp.Classes.PointerSlidingScale pointerSlidingScale1 = new Iocomp.Classes.PointerSlidingScale();
            Iocomp.Classes.PointerSlidingScale pointerSlidingScale2 = new Iocomp.Classes.PointerSlidingScale();
            this.slidingCompass1 = new Iocomp.Instrumentation.Professional.SlidingCompass();
            this.slidingCompass1.LoadingBegin();
            this.slidingCompass1.BackColor = this.slidingCompass1.BackColor = System.Drawing.Color.Black;//System.Drawing.Color.Transparent;//System.Drawing.Color.FromArgb(0x2c, 0x2c, 0x2c);
            this.slidingCompass1.Font = new System.Drawing.Font("宋体", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.slidingCompass1.ForeColor = System.Drawing.Color.White;
            this.slidingCompass1.Name = "slidingCompass1";
            pointerSlidingScale1.LineColor = System.Drawing.Color.Yellow;
            pointerSlidingScale1.Style = Iocomp.Types.PointerStyleSlidingScale.Line;

            pointerSlidingScale2.LineColor = System.Drawing.Color.Yellow;
            pointerSlidingScale2.Style = Iocomp.Types.PointerStyleSlidingScale.Pointer;

            this.slidingCompass1.Pointers.Add(pointerSlidingScale1);
            this.slidingCompass1.Pointers.Add(pointerSlidingScale2);

            this.slidingCompass1.Rotation = Iocomp.Types.RotationQuad.X090;
            this.slidingCompass1.ScaleBackground.Style = Iocomp.Types.SlidingScaleBackgroundStyle.Clear;
            this.slidingCompass1.ScaleDisplay.Direction = Iocomp.Types.SideDirection.RightToLeft;
            this.slidingCompass1.ScaleDisplay.GeneratorAuto.MidIncluded = true;
            this.slidingCompass1.ScaleDisplay.GeneratorAuto.MinorCount = 10;
            this.slidingCompass1.ScaleDisplay.GeneratorFixed.MinorCount = 5;
            this.slidingCompass1.ScaleDisplay.LineInnerVisible = true;
            this.slidingCompass1.ScaleDisplay.LineThickness = 2;
            this.slidingCompass1.ScaleDisplay.TextFormatting.Precision = 0;
            this.slidingCompass1.ScaleDisplay.TextRotation = Iocomp.Types.TextRotation.X270;
            this.slidingCompass1.ScaleDisplay.TickMajor.Color = System.Drawing.Color.White;
            this.slidingCompass1.ScaleDisplay.TickMajor.Length = 45;
            this.slidingCompass1.ScaleDisplay.TickMid.Color = System.Drawing.Color.White;
            this.slidingCompass1.ScaleDisplay.TickMid.Length = 30;
            this.slidingCompass1.ScaleDisplay.TickMid.TextVisible = false;
            this.slidingCompass1.ScaleDisplay.TickMinor.Color = System.Drawing.Color.White;
            this.slidingCompass1.ScaleDisplay.TickMinor.Length = 15;
            this.slidingCompass1.ScaleRange.Min = -30D;
            this.slidingCompass1.ScaleRange.Span = 60D;
            this.slidingCompass1.Size = new System.Drawing.Size(543, 53);
            this.slidingCompass1.LoadingEnd();
            wfhCompass.Child = slidingCompass1;
        }

        public void Close()
        {
            tmrNavCommCheck.Stop();
            tmrBatteryCheck.Stop();
            tmrNavStartRecordLog.Stop();
            wfhCompass.Child = null;
            slidingCompass1.Dispose();
        }

        void tmrNavStartRecordLog_Tick(object sender, EventArgs e)
        {
            string path = "";
            //string assemblyFolder = System.Windows.Forms.Application.StartupPath;
            string[] para = new string[9];

            //path = assemblyFolder + "\\Record\\" + strRecordPath;
            path = Global.SavingRecordDirectory + strRecordPath;

            para[0] = GlobalNavigation.nav1.Latitude.ToString();
            para[1] = GlobalNavigation.nav1.Longitude.ToString();
            para[2] = GlobalNavigation.nav1.GetHeading().ToString();
            para[3] = GlobalNavigation.nav1.GetDepth().ToString();
            para[4] = GlobalNavigation.nav1.Distance.ToString("0.0");
            para[5] = GlobalNavigation.nav1.SelectedMarker.Name;
            para[6] = GlobalNavigation.nav1.Bearing.ToString("0.0");
            para[7] = "0.0";
            para[8] = DateTime.Now.ToString();

            try
            {
                using (StreamWriter w = File.AppendText(path))
                {
                    string data = "";

                    for (int i = 0; i < para.Length; i++)
                    {
                        //data += "\"" + para[i] + "\"";
                        data += para[i];
                        if (i < para.Length - 1)
                            data += ",";
                    }

                    w.WriteLine(data);
                    w.Flush();
                    // Close the writer and underlying file.
                    w.Close();
                }
            }
            catch { }
        }

        void tmrNavCommCheck_Tick(object sender, EventArgs e)
        {
            slidingCompass1.Pointers[0].Value = GlobalNavigation.nav1.GetHeading();
            lblHeadingInfo.Content = GlobalNavigation.nav1.GetHeading().ToString("0.0");// + "°";
            lblDepthInfo.Content = GlobalNavigation.nav1.GetDepth();// + "米";
            if(GlobalNavigation.nav1.Distance != -1)
            {
                lblDistance.Content = GlobalNavigation.nav1.Distance.ToString("0.0") + "米";
            } 
            lblWayPointName.Content = GlobalNavigation.nav1.SelectedMarker.Name;
            if(GlobalNavigation.nav1.Bearing != -1)
            {
                lblBearing.Content = GlobalNavigation.nav1.Bearing.ToString("0.0") + "°";
                slidingCompass1.Pointers[1].Value = GlobalNavigation.nav1.Bearing;
            }


            lblPitchInfo.Content = GlobalNavigation.nav1.GetPitch().ToString("0.0") + "°"; 
            lblRollInfo.Content = GlobalNavigation.nav1.GetRoll().ToString("0.0") + "°";


            if (GlobalDVL.isInstalled)
            {
                if (GlobalDVL.dvl1.valid)
                    lblAltitude.Content = GlobalDVL.dvl1.altitude.ToString();
                else
                    lblAltitude.Content = "NaN";

                lblUTMEasting.Content = GlobalDVL.dVLStatus.UTMEasting.ToString();
                lblUTMNorthing.Content = GlobalDVL.dVLStatus.UTMNorthing.ToString();
                lblUTMHeadingDistance.Content = GlobalDVL.dVLStatus.HeadingDistance.ToString();
                lblVx.Content = GlobalDVL.dvl1.vx.ToString();
                lblVy.Content = GlobalDVL.dvl1.vy.ToString();
            }



            if (Global.mapnorth == Global.MapNorth.Diver)
            {
                Global.globalMap.Bearing = Convert.ToSingle(GlobalNavigation.nav1.TurnHeading1);
                ((CustomMarkerRed)Global.currentMarker.Shape).Rotate(0);
            }

            else
            {
                Global.globalMap.Bearing = 0;
                ((CustomMarkerRed)Global.currentMarker.Shape).Rotate(GlobalNavigation.nav1.GetHeading());
            }



            if (Global.IsStartRecordLog == true && (Global.IsStartRecordLog != IsStartRecordLog))
            {
                string datePatt = @"yyyyMMdd";
                string timePatt = @"HHmmss";
                string datePatt1 = @"yyyy-MM-dd";
                string timePatt1 = @"HH:mm:ss";
                DateTime dt;
                dt = DateTime.Now;
                string[] para = new string[9];

                lblStartRecordLog.Content = "正在记录";
                IsStartRecordLog = Global.IsStartRecordLog;

                //if (Global.CurrentFileInfoWithoutExtension != "")
                //{
                //Global.RecordLogFileName = "SYSDIVE" + "_" + dt.ToString(datePatt) + "_" + dt.ToString(timePatt);
                strRecordPath = Global.RecordLogFileName + ".reclog";
                para[0] = Global.MissionFileNameWithoutExtension;

                //Global.CurrentFileInfoWithoutExtension + " " + DateTime.Now.ToFileTime() + ".csv";
                // }
                // else
                //{
                //    strRecordPath = DateTime.Now.ToFileTime() + ".csv";
                //     para[0] = " ";
                //}


                string path = "";
                //string assemblyFolder = System.Windows.Forms.Application.StartupPath;
                //path = assemblyFolder + "\\Record\\" + strRecordPath;

                path = Global.SavingRecordDirectory + strRecordPath;

                para[1] = dt.ToString(datePatt1);
                para[2] = dt.ToString(timePatt1);

                para[3] = Global.RecordSamplingRate.ToString();
                para[4] = "";
                para[5] = "";
                para[6] = "";
                para[7] = "";
                para[8] = "";

                try
                {
                    using (StreamWriter w = File.AppendText(path))
                    {
                        string data = "";

                        for (int i = 0; i < para.Length; i++)
                        {
                            //data += "\"" + para[i] + "\"";
                            data += para[i];
                            if (i < para.Length - 1)
                                data += ",";
                        }

                        w.WriteLine(data);
                        w.Flush();
                        // Close the writer and underlying file.
                        w.Close();
                    }
                }
                catch { }


                tmrNavStartRecordLog.Start();
            }
            else if (Global.IsStartRecordLog == false && (Global.IsStartRecordLog != IsStartRecordLog))
            {
                lblStartRecordLog.Content = "未记录";
                IsStartRecordLog = Global.IsStartRecordLog;
                tmrNavStartRecordLog.Stop();
            }
        }

        void tmrBatteryCheck_Tick(object sender, EventArgs e)
        {
            System.Windows.Forms.PowerStatus status = System.Windows.Forms.SystemInformation.PowerStatus;
            //float percent = status.BatteryLifePercent;
            float percent = GlobalBattery.GetBatteryPowerPercent();
            percent /= 100;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Nheading += 10;
            if (Nheading > 360) Nheading -= 360;

            GlobalNavigation.nav1.SetHeading(Nheading);
            GlobalNavigation.nav1.SetTrueHeading(Nheading);
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Nheading -= 10;
            if (Nheading < 0) Nheading += 360;

            GlobalNavigation.nav1.SetHeading(Nheading);
            GlobalNavigation.nav1.SetTrueHeading(Nheading);
        }
    }
}
