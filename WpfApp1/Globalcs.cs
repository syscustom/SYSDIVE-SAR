using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Device.Location;
using System.Windows.Threading;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Demo.WindowsPresentation.CustomMarkers;
using AxVIDEOCAPXLib;
using System.Windows.Forms.Integration;
using System.Threading;
using System.Runtime.InteropServices;

using ProViewer4.Models;
using BlueView.Sonar.Model;
using BlueView.Sonar.Interfaces;
using ROV.Serial;
using System.IO;
using System.Text.RegularExpressions;
using System.ServiceProcess;
using AppContainers;
using System.Collections;
using Xsens_Device_API;

internal enum EAPIStatus_t : uint
{
    EAPI_STATUS_SUCCESS = 0,
    EAPI_STATUS_ERROR = 0xFFFFF0FF,
    EAPI_STATUS_INITIALIZED = 0xFFFFFFFE,
    EAPI_STATUS_NOT_INITIALIZED = 0xFFFFFFFF,
};

namespace WpfApp1
{
    public static class Global // static 不是必须
    {
        public enum SonarType
        {
            None = 0,
            Blueview = 1,
            Oculus = 2
        }

        public enum GNSSType
        {
            Internal = 0,
            Float = 1
        }

        public enum DiveType
        {
            Float = 0,
            DeadReckoning = 1

        }

        public enum ShutDownType
        {
            Exit = 0,
            Shutdown = 1
        }

        public enum MapNorth
        {
            North = 0,
            Diver = 1
        }

        public static bool TopMost = true;

        public static List<WayPoint> LstWayPoints = new List<WayPoint>();
        public static string MissionFileFullName = "";
        public static string MissionFileNameWithoutExtension = "";
        public static bool IsStartRecordLog = false;
        public static string RecordLogFileName = "";
        public static string SavingMainDirectory = "";
        public static string SavingRecordDirectory = "";
        public static string SavingMissionDirectory = "";
        public static string SavingImagesDirectory = "";
        public static string SavingVideosDirectory = "";
        public static int RecordSamplingRate = 0;

        public static Map globalMap = new Map();



        private static GeoCoordinateWatcher watcher;

        private static DispatcherTimer timer = new DispatcherTimer();
        private static DispatcherTimer tmrWatcherTimer;
        private static DispatcherTimer tmrCurrentPosition;
        private static DispatcherTimer tmrCallService;


        public static GMapMarker currentMarker;
        public static GMapMarker currentMarker1;

        private static GMapMarker WaypointActiveMarker = null;

        private static int intWaypointActiveIndex = 0;

        public static ShutDownType shutdowntype = ShutDownType.Exit;
        public static MapNorth mapnorth = MapNorth.North;

        public static bool SonarDemoShow = false;

        private static double dblDefaultLat = 0.0;
        private static double dblDefaultLng = 0.0;

        private static bool blnLocationConfirmed = false;

        public static bool LocationConfirmed
        {
            get { return blnLocationConfirmed; }
            set { blnLocationConfirmed = value; }
        }

        public static void CreatMap()
        {
            globalMap.Manager.Mode = AccessMode.ServerAndCache;
            globalMap.MaxZoom = 18;
            globalMap.MinZoom = 3;
            globalMap.Zoom = 13;
            watcher = new GeoCoordinateWatcher();
            watcher.PositionChanged += watcher_PositionChanged;
            watcher.StatusChanged += watcher_StatusChanged;
            watcher.Start();

            // config map 
            globalMap.MapProvider = GMapProviders.AmapMap;

            //globalMap.MapProvider = GMapProviders.GoogleChinaSatelliteMap;
            //globalMap.MapProvider = GMapProviders.GoogleChinaMap;
            currentMarker = new GMapMarker(globalMap.Position);
            {
                currentMarker.Shape = new CustomMarkerRed(currentMarker, "custom position marker");
                //currentMarker.Offset = new System.Windows.Point(-15, -15);
                currentMarker.ZIndex = int.MaxValue;
                globalMap.Markers.Add(currentMarker);
            }

            /*
            currentMarker1 = new GMapMarker(globalMap.Position);
            {
                currentMarker1.Shape = new CustomMarkerRed(currentMarker1, "custom position marker");
                //currentMarker.Offset = new System.Windows.Point(-15, -15);
                currentMarker1.ZIndex = int.MaxValue;
                globalMap.Markers.Add(currentMarker1);
            }
            */

            if (globalMap.Markers.Count > 1)
            {
                globalMap.ZoomAndCenterMarkers(null);
            }

            // perfromance test
            timer.Interval = TimeSpan.FromMilliseconds(44);
            timer.Tick += new EventHandler(timer_Tick);

            dblDefaultLat = Convert.ToDouble(SelectXMLData.GetConfiguration("DefaultLat", "value"));
            dblDefaultLng = Convert.ToDouble(SelectXMLData.GetConfiguration("DefaultLng", "value"));


            tmrCurrentPosition = new DispatcherTimer();
            tmrCurrentPosition.Interval = TimeSpan.FromSeconds(1);
            tmrCurrentPosition.Tick += new EventHandler(tmrCurrentPosition_Tick);
            tmrCurrentPosition.Start();

            tmrWatcherTimer = new DispatcherTimer();
            tmrWatcherTimer.Interval = TimeSpan.FromSeconds(5);
            tmrWatcherTimer.Tick += new EventHandler(tmrWatcherTimer_Tick);
            tmrWatcherTimer.Start();

        }

        public static void WaysPointsModified()
        {
            PaintAllWayPoints();
            PaintAllWayPointRoutes();
        }

        public static RenderTargetBitmap ToImageSource(FrameworkElement obj)
        {
            // Save current canvas transform
            Transform transform = obj.LayoutTransform;
            obj.LayoutTransform = null;

            // fix margin offset as well
            Thickness margin = obj.Margin;
            obj.Margin = new Thickness(0, 0, margin.Right - margin.Left, margin.Bottom - margin.Top);

            // Get the size of canvas
            System.Windows.Size size = new System.Windows.Size(obj.Width, obj.Height);

            // force control to Update
            obj.Measure(size);
            obj.Arrange(new Rect(size));

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(obj);

            if (bmp.CanFreeze)
            {
                bmp.Freeze();
            }

            // return values as they were before
            obj.LayoutTransform = transform;
            obj.Margin = margin;

            return bmp;
        }

        private static double NextDouble(Random rng, double min, double max)
        {
            return min + (rng.NextDouble() * (max - min));
        }

        private static Random r = new Random();
        private static int tt = 0;

        public static SonarType sonartype = SonarType.None;

        private static void tmrWatcherTimer_Tick(object sender, EventArgs e)
        {
            tmrWatcherTimer.Stop();
            if (watcher.Status == GeoPositionStatus.NoData)
            {
                watcher.Stop();
                currentMarker.Position = new PointLatLng(dblDefaultLat, dblDefaultLng);
                globalMap.Position = currentMarker.PositionGCJ02;
            }
            if (watcher.Status == GeoPositionStatus.Ready)
            {
                watcher.Stop();
                if (watcher.Position.Location.IsUnknown)
                {
                    currentMarker.Position = new PointLatLng(dblDefaultLat, dblDefaultLng);
                    globalMap.Position = currentMarker.PositionGCJ02;

                }
                else
                {
                    currentMarker.Position = new PointLatLng(watcher.Position.Location.Latitude, watcher.Position.Location.Longitude);
                    globalMap.Position = currentMarker.PositionGCJ02;
                }
            }
            blnLocationConfirmed = true;
        }


        private static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        private static double ConvertToDegrees(double radian)
        {
            return (180 / Math.PI) * radian;
        }

        private static void tmrCurrentPosition_Tick(object sender, EventArgs e)
        {
            if (GlobalDVL.DVLNavigationMode == false) //GlobalNavigation.nav1.GPSValid && 
            {
                currentMarker.Position = new PointLatLng(GlobalNavigation.nav1.Latitude, GlobalNavigation.nav1.Longitude);
                globalMap.Position = currentMarker.PositionGCJ02;
            }

            if (GlobalDVL.isInstalled && GlobalDVL.DVLNavigationMode)
            {

                double lat1 = 0.0;
                double lng1 = 0.0;
                if (GlobalDVL.dvl1.satellitefix == false)
                {
                    if (GlobalNavigation.nav1.GPSValid)
                    {
                        lat1 = GlobalNavigation.nav1.Latitude;
                        lng1 = GlobalNavigation.nav1.Longitude;
                        GlobalDVL.dvl1.Latitude = lat1;
                        GlobalDVL.dvl1.Longitude = lng1;
                        GlobalDVL.dvl1.satellitefix = true;
                        return;
                    }
                }
                else
                {
                    if (GlobalNavigation.nav1.GPSValid)
                    {
                        lat1 = GlobalNavigation.nav1.Latitude;
                        lng1 = GlobalNavigation.nav1.Longitude;
                        GlobalDVL.dvl1.Latitude = lat1;
                        GlobalDVL.dvl1.Longitude = lng1;
                    }
                    else
                    {
                        if (GlobalDVL.dvl1.valid)
                        {
                            lat1 = GlobalDVL.dvl1.Latitude;
                            lng1 = GlobalDVL.dvl1.Longitude;

                            double vx2 = GlobalDVL.dvl1.vx * GlobalDVL.dvl1.vx;
                            double vy2 = GlobalDVL.dvl1.vy * GlobalDVL.dvl1.vy;

                            double hesudu = Math.Sqrt(vx2 + vy2);

                            double d = hesudu * GlobalDVL.dvl1.time * 0.001;


                            const double R = 6371000;
                            double θ = GlobalNavigation.nav1.GetHeading();

                            double δ = d / R;
                            θ = ConvertToRadians(θ);
                            lat1 = ConvertToRadians(lat1);
                            lng1 = ConvertToRadians(lng1);


                            double sinφ2 = Math.Sin(lat1) * Math.Cos(δ) + Math.Cos(lat1) * Math.Sin(δ) * Math.Cos(θ);
                            double φ2 = Math.Asin(sinφ2);
                            double y = Math.Sin(θ) * Math.Sin(δ) * Math.Cos(lat1);
                            double x = Math.Cos(δ) - Math.Sin(lat1) * sinφ2;
                            double λ2 = lng1 + Math.Atan2(y, x);

                            double lat2 = ConvertToDegrees(φ2);
                            double lon2 = ConvertToDegrees(λ2);

                            GlobalDVL.dvl1.Latitude = lat2;
                            GlobalDVL.dvl1.Longitude = lon2;
                        }
                    }
                    currentMarker.Position = new PointLatLng(GlobalDVL.dvl1.Latitude, GlobalDVL.dvl1.Longitude);
                    globalMap.Position = currentMarker.PositionGCJ02;
                }
            }

            if (blnLocationConfirmed)
            {
                if (LstWayPoints.Count > 0)
                {
                    if (WaypointActiveMarker != null)
                    {
                        GlobalNavigation.nav1.Distance = CalcDistance(currentMarker.Position, GlobalNavigation.nav1.SelectedMarker.PointLATLNG);
                        GlobalNavigation.nav1.Bearing = CalcBearing(currentMarker.Position, GlobalNavigation.nav1.SelectedMarker.PointLATLNG);
                        PrintCurrentToActiveRoute(currentMarker.Position, GlobalNavigation.nav1.SelectedMarker.PointLATLNG);
                    }
                    else
                    {
                        GlobalNavigation.nav1.Distance = 0;
                        GlobalNavigation.nav1.Bearing = 0;
                    }
                }
                else
                {
                    if (WaypointActiveMarker == null)
                    {
                        GlobalNavigation.nav1.Distance = 0;
                        GlobalNavigation.nav1.Bearing = 0;
                    }
                }
            }

        }

        public static void CreateServiceMonitor()
        {
            tmrCallService = new DispatcherTimer();
            tmrCallService.Interval = TimeSpan.FromSeconds(1);
            tmrCallService.Tick += new EventHandler(tmrCallService_Tick);
            tmrCallService.Start();

        }

        private static void tmrCallService_Tick(object sender, EventArgs e)
        {
            CallService();
        }


        private static void timer_Tick(object sender, EventArgs e)
        {
            var pos = new PointLatLng(NextDouble(r, globalMap.ViewArea.Top, globalMap.ViewArea.Bottom), NextDouble(r, globalMap.ViewArea.Left, globalMap.ViewArea.Right));
            GMapMarker m = new GMapMarker(pos);
            {
                var s = new Test((tt++).ToString());

                var image = new Image();
                {
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.LowQuality);
                    image.Stretch = Stretch.None;
                    image.Opacity = s.Opacity;

                    image.Source = ToImageSource(s);
                }

                m.Shape = image;

                m.Offset = new Point(-s.Width, -s.Height);
            }
            globalMap.Markers.Add(m);

            if (tt >= 333)
            {
                timer.Stop();
                tt = 0;
            }
        }

        private static void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (e.Position.Location.HorizontalAccuracy > 500)
            {
                return;
            }

            globalMap.Position = new PointLatLng(e.Position.Location.Latitude, e.Position.Location.Longitude);
            watcher.Stop();
        }

        private static void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            /*
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    MessageBox.Show("Location Service is not enabled on the device");
                    break;

                case GeoPositionStatus.NoData:
                    MessageBox.Show(" The Location Service is working, but it cannot get location data.");
                    break;
            }
            */
        }

        private static void PaintAllWayPoints()
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
                        ToolTipText = currentMarker.Position.ToString();
                    }

                    m.Shape = new CustomMarkerDemo(m, wp.Name);
                    m.ZIndex = 55;

                }
                globalMap.Markers.Add(m);
            }
        }

        private static void PaintAllWayPointRoutes()
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
                gmRoute.RegenerateShape(globalMap);
                ((System.Windows.Shapes.Path)gmRoute.Shape).Stroke = new SolidColorBrush(Colors.Blue);
                ((System.Windows.Shapes.Path)gmRoute.Shape).StrokeThickness = 2;
                globalMap.Markers.Add(gmRoute);
            }
        }

        private static void PrintCurrentToActiveRoute(PointLatLng _point1, PointLatLng _point2)
        {
            GMapRoute CTARoute = new GMapRoute(new List<PointLatLng>() { _point1, _point2 })
            {
                Tag = "CTARoute",
                Shape = new Line()
                {
                    StrokeThickness = 2,
                    Stroke = System.Windows.Media.Brushes.Black
                }
            };

            for (int i = 0; i < globalMap.Markers.Count; i++)
            {
                if ((string)globalMap.Markers[i].Tag == "CTARoute")
                {
                    globalMap.Markers.RemoveAt(i);
                    break;
                }

            }
            CTARoute.RegenerateShape(globalMap);
            ((System.Windows.Shapes.Path)CTARoute.Shape).Stroke = new SolidColorBrush(Colors.Black);
            ((System.Windows.Shapes.Path)CTARoute.Shape).StrokeThickness = 5;
            globalMap.Markers.Add(CTARoute);

        }

        public static void RemoveAllRoute()
        {
            for (int i = 0; i < globalMap.Markers.Count; i++)
            {
                if (globalMap.Markers[i].GetType() == typeof(GMapRoute))
                {
                    globalMap.Markers.RemoveAt(i);
                    if (i > 0) i -= 1;
                }
            }
        }

        public static void RemoveAllWaypoints()
        {
            for (int i = 0; i < globalMap.Markers.Count; i++)
            {
                if (globalMap.Markers[i].GetType() == typeof(GMapMarker) &&
                    (globalMap.Markers[i].Shape.GetType() == typeof(CustomMarkerDemo)
                    || globalMap.Markers[i].Shape.GetType() == typeof(WayPointMarkerActive)))
                {
                    globalMap.Markers.RemoveAt(i);
                    if (i > 0) i -= 1;
                }
            }
            if (WaypointActiveMarker != null)
                WaypointActiveMarker.Clear();
            WaypointActiveMarker = null;
        }

        public static void RemoveAllMarker()
        {
            for (int i = 0; i < globalMap.Markers.Count; i++)
            {
                if (globalMap.Markers[i].GetType() == typeof(GMapMarker) &&
                    (globalMap.Markers[i].Shape.GetType() == typeof(CustomMarkerDemo)
                    || globalMap.Markers[i].Shape.GetType() == typeof(WayPointMarkerActive)))
                {
                    globalMap.Markers.RemoveAt(i);
                    if (i > 0) i -= 1;
                }
            }

            LstWayPoints.Clear();
            if (WaypointActiveMarker != null)
                WaypointActiveMarker.Clear();
            WaypointActiveMarker = null;

        }

        public static void RemoveActiveWayPoint()
        {
            for (int i = 0; i < globalMap.Markers.Count; i++)
            {
                if (globalMap.Markers[i].GetType() == typeof(GMapMarker) &&
                    globalMap.Markers[i].Shape.GetType() == typeof(WayPointMarkerActive))
                {
                    globalMap.Markers.RemoveAt(i);
                    if (i > 0) i -= 1;
                }
            }
            if (WaypointActiveMarker != null)
                WaypointActiveMarker.Clear();
            WaypointActiveMarker = null;
        }

        public static void ActiveWayPointAt(int _index)
        {
            if (LstWayPoints.Count == 0) return;
            intWaypointActiveIndex = _index;
            if (WaypointActiveMarker == null)
            {
                WaypointActiveMarker = new GMapMarker(LstWayPoints[intWaypointActiveIndex].PointLATLNG);
                {
                    Placemark? p = null;
                    string ToolTipText;
                    if (p != null)
                    {
                        ToolTipText = p.Value.Address;
                    }
                    else
                    {
                        ToolTipText = LstWayPoints[intWaypointActiveIndex].PointLATLNG.ToString();
                    }

                    WaypointActiveMarker.Shape = new WayPointMarkerActive(WaypointActiveMarker, ToolTipText);
                    WaypointActiveMarker.ZIndex = 55;
                }

                globalMap.Markers.Add(WaypointActiveMarker);
                WayPoint wp = LstWayPoints[intWaypointActiveIndex];
                GlobalNavigation.nav1.SelectedMarker = wp;
            }
            else
            {
                if (intWaypointActiveIndex >= 0 && intWaypointActiveIndex <= (LstWayPoints.Count - 1))
                {
                    WaypointActiveMarker.Position = LstWayPoints[intWaypointActiveIndex].PointLATLNG;
                    WayPoint wp = LstWayPoints[intWaypointActiveIndex];
                    GlobalNavigation.nav1.SelectedMarker = wp;
                }
            }
        }

        public static void ActiveWayPointNext()
        {
            if (LstWayPoints.Count == 0) return;
            if (WaypointActiveMarker == null)
            {
                WaypointActiveMarker = new GMapMarker(LstWayPoints[LstWayPoints.Count - 1].PointLATLNG);
                {
                    Placemark? p = null;
                    string ToolTipText;
                    if (p != null)
                    {
                        ToolTipText = p.Value.Address;
                    }
                    else
                    {
                        ToolTipText = LstWayPoints[LstWayPoints.Count - 1].PointLATLNG.ToString();
                    }

                    WaypointActiveMarker.Shape = new WayPointMarkerActive(WaypointActiveMarker, ToolTipText);
                    WaypointActiveMarker.ZIndex = 55;
                }
                intWaypointActiveIndex = LstWayPoints.Count - 1;
                globalMap.Markers.Add(WaypointActiveMarker);
                WayPoint wp = LstWayPoints[LstWayPoints.Count - 1];
                GlobalNavigation.nav1.SelectedMarker = wp;
            }
            else
            {
                if (intWaypointActiveIndex >= 0 && intWaypointActiveIndex < (LstWayPoints.Count - 1))
                {
                    intWaypointActiveIndex += 1;
                    WaypointActiveMarker.Position = LstWayPoints[intWaypointActiveIndex].PointLATLNG;
                    //MainMap.Markers[MainMap.Markers.Count - 1].Position = LstPoints[intWaypointActiveIndex];
                    WayPoint wp = LstWayPoints[intWaypointActiveIndex];
                    GlobalNavigation.nav1.SelectedMarker = wp;
                }
            }

        }

        public static void ActiveWayPointPrev()
        {
            if (LstWayPoints.Count == 0) return;
            if (WaypointActiveMarker == null)
            {
                WaypointActiveMarker = new GMapMarker(LstWayPoints[0].PointLATLNG);
                {
                    Placemark? p = null;
                    string ToolTipText;
                    if (p != null)
                    {
                        ToolTipText = p.Value.Address;
                    }
                    else
                    {
                        ToolTipText = LstWayPoints[0].PointLATLNG.ToString();
                    }

                    WaypointActiveMarker.Shape = new WayPointMarkerActive(WaypointActiveMarker, ToolTipText);
                    WaypointActiveMarker.ZIndex = 55;
                }
                intWaypointActiveIndex = 0;
                globalMap.Markers.Add(WaypointActiveMarker);
                WayPoint wp = LstWayPoints[0];
                GlobalNavigation.nav1.SelectedMarker = wp;
            }
            else
            {
                if (intWaypointActiveIndex > 0 && intWaypointActiveIndex <= (LstWayPoints.Count - 1))
                {
                    intWaypointActiveIndex -= 1;
                    WaypointActiveMarker.Position = LstWayPoints[intWaypointActiveIndex].PointLATLNG;
                    //MainMap.Markers[MainMap.Markers.Count - 1].]Position = LstPoints[intWaypointActiveIndex];
                    WayPoint wp = LstWayPoints[intWaypointActiveIndex];
                    GlobalNavigation.nav1.SelectedMarker = wp;
                }
            }

        }

        public static double CalcDistance(PointLatLng _point1, PointLatLng _point2)
        {
            const double R = 6371000; //earth’s radius  radius = 6,371km
            double φ1 = _point1.Lat * Math.PI / 180;
            double φ2 = _point2.Lat * Math.PI / 180;
            double Δφ = (_point2.Lat - _point1.Lat) * Math.PI / 180;
            double Δλ = (_point2.Lng - _point1.Lng) * Math.PI / 180;

            double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                      Math.Cos(φ1) * Math.Cos(φ2) *
                      Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double d = R * c; // in metres
            return d;
        }

        public static double CalcBearing(PointLatLng _point1, PointLatLng _point2)
        {
            //where	φ1,λ1 is the start point, φ2,λ2 the end point (Δλ is the difference in longitude)

            double y = Math.Sin(_point2.Lng - _point1.Lng) * Math.Cos(_point2.Lat);
            double x = Math.Cos(_point1.Lat) * Math.Sin(_point2.Lat) -
                      Math.Sin(_point1.Lat) * Math.Cos(_point2.Lat) * Math.Cos(_point2.Lng - _point1.Lng);
            double θ = Math.Atan2(y, x);
            double brng = (θ * 180 / Math.PI + 360) % 360; // in degrees
            //brng = (brng + 180.0) % 360.0;
            return brng;

        }

        public static bool VisionSwitch = false;
        public static bool MountVision = false;

        public static GNSSType GNSSMode = GNSSType.Float;

        public static DiveType DiveMode = DiveType.Float;

        public static AxVideoCapX vcx = new AxVideoCapX();
        public static WindowsFormsHost videohost = new WindowsFormsHost();

        private static Thread threadVideoMonitoring;
        private static bool videoStartRecorded = false;
        private static int videoRecordedIndex = 1;
        private static string videoRecordedName = "";

        public static void InitVideo()
        {


        }



        public static void CreateVideo()
        {
            
            vcx.BeginInit();
            vcx.Width = 352;
            vcx.Height = 288;
            videohost.Child = vcx;
            vcx.EndInit();
            vcx.BackColor = System.Drawing.Color.Black;

            try
            {
                if (vcx.GetVideoDeviceCount() == 0)
                {
                    //没有检测到 摄像机
                    throw new Exception();
                }
                int n = vcx.GetVideoDeviceCount();
                for (int i = 1; i <= n; i++)
                {

                    if (vcx.GetVideoDeviceName(i - 1) == "USB2.0 Camera")
                    {
                        if (vcx.Connected) vcx.Connected = false;
                        vcx.VideoDeviceIndex = i - 1;
                        vcx.CaptureAudio = false;
                        vcx.CaptureRate = 30;
                        vcx.Connected = true;
                        vcx.Preview = false;

                        if (vcx.Connected)
                        {
                            for (int j = 0; j < vcx.GetVideoInputCount(); j++)
                            {
                                if (vcx.GetVideoInputName(j) == "Video Composite")
                                    vcx.VideoInputIndex = j;
                            }
                        }
                        break;
                    }
                }

                if (!vcx.Connected)
                {
                    throw new Exception();
                }

                for (int i = 0; i < vcx.GetVideoCodecCount(); i++)
                {
                    if (vcx.GetVideoCodecName(i).IndexOf("x264vfw") > -1)
                        vcx.VideoCodecIndex = i;
                }
            }
            catch
            {

            }

            OpenPreviewVideo();

            threadVideoMonitoring = new Thread(new ThreadStart(StartVideoMonitoring));
            threadVideoMonitoring.Start();
        }

        private static void StartVideoMonitoring()
        {
            while (true)
            {
                Thread.Sleep(500);
                if (Global.IsStartRecordLog == true && videoStartRecorded == false)
                {
                    if (videoRecordedName != Global.RecordLogFileName)
                    {
                        videoRecordedName = Global.RecordLogFileName;
                        videoRecordedIndex = 1;
                    }

                    videoStartRecorded = true;
                    vcx.CapFilename = Global.SavingVideosDirectory + Global.RecordLogFileName + videoRecordedIndex.ToString() + ".avi";
                    vcx.StartCapture();

                    videoRecordedIndex++;
                }
                else if (Global.IsStartRecordLog == false && videoStartRecorded == true)
                {
                    videoStartRecorded = false;
                    vcx.StopCapture();
                }
            }
        }

        public static void ToggleVideo()
        {
            if (vcx.Connected)
                vcx.Preview = !vcx.Preview;
        }

        public static void OpenPreviewVideo()
        {
            if (vcx.Connected)
                vcx.Preview = true;
        }

        public static void ClosePreviewVideo()
        {
            if (vcx.Connected)
                vcx.Preview = false;
        }

        public static void ResizeVideo(int _width, int _height)
        {
            //Array a = (Array)vcx.GetVideoCaps();
            // vcx.Width = Convert.ToInt32(a.GetValue(a.GetLowerBound(0), 0));
            //vcx.Height = Convert.ToInt32(a.GetValue(a.GetLowerBound(0), 1));
            //vcx.Width = _width;
            //vcx.Height = _height;
            vcx.SetVideoFormat(_width, _height);
        }

        public static void VideoSnapshot()
        {
            if (vcx.Connected)
            {
                string path = Global.SavingImagesDirectory + DateTime.Now.ToFileTime() + ".jpg";
                vcx.GrabFrame().Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
            }

        }

        public static void CloseVideo()
        {
            if (threadVideoMonitoring != null) threadVideoMonitoring.Abort();

            if (videoStartRecorded == true)
            {
                videoStartRecorded = false;
                vcx.StopCapture();
            }


            if (vcx.Connected)
            {
                vcx.Preview = false;
                vcx.Connected = false;
            }

            if (videohost != null)
            {
                videohost.Child = null;
            }
        }

        public static void CallService()
        {
            int SmartRestart = 222;
            TimeSpan timeout = new TimeSpan(0, 0, 0, 0, 200);
            ServiceController service = new ServiceController("SARServiceMonitor");
            //service.Start();
            try
            {
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                service.ExecuteCommand(SmartRestart);
            }
            catch (Exception)
            { }
        }

        public static void EndCallService()
        {
            int SmartRestart = 130;
            TimeSpan timeout = new TimeSpan(0, 0, 0, 0, 200);
            ServiceController service = new ServiceController("SARServiceMonitor");
            //service.Start();
            try
            {
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                service.ExecuteCommand(SmartRestart);
            }
            catch (Exception)
            { }

        }

        internal static frmSonarDisplay m_Wnd = null;

        public static void SetWnd(frmSonarDisplay wnd)
        {
            m_Wnd = wnd;
        }

        public static frmSonarDisplay SonarWindow { get { return m_Wnd; } }
    }

    public static class GlobalUpBoard
    {
        #region Import DLL Calls
        [DllImport("aaeonEAPI.dll", EntryPoint = "EApiGPIOGetDirectionCaps")]
        public static extern UInt32 EApiGPIOGetDirectionCaps(uint Id, ref UInt32 pInputs, ref UInt32 pOutputs);
        [DllImport("aaeonEAPI.dll", EntryPoint = "EApiGPIOGetDirection")]
        public static extern UInt32 EApiGPIOGetDirection(UInt32 Id, UInt32 Bitmask, ref UInt32 pDirection);
        [DllImport("aaeonEAPI.dll", EntryPoint = "EApiGPIOSetDirection")]
        public static extern UInt32 EApiGPIOSetDirection(UInt32 Id, UInt32 Bitmask, UInt32 Direction);
        [DllImport("aaeonEAPI.dll", EntryPoint = "EApiGPIOGetLevel")]
        public static extern UInt32 EApiGPIOGetLevel(UInt32 Id, UInt32 Bitmask, ref UInt32 pLevel);
        [DllImport("aaeonEAPI.dll", EntryPoint = "EApiGPIOSetLevel")]
        public static extern UInt32 EApiGPIOSetLevel(UInt32 Id, UInt32 Bitmask, UInt32 Level);
        [DllImport("aaeonEAPI.dll", EntryPoint = "EApiGPIOGetCaps")]
        public static extern UInt32 EApiGPIOGetCaps(UInt32 Id, ref UInt32 PinCount, ref UInt32 pDioDisable);
        [DllImport("aaeonEAPI.dll", EntryPoint = "EApiLibInitialize")]
        public static extern UInt32 EApiLibInitialize();
        [DllImport("aaeonEAPI.dll", EntryPoint = "EApiLibUnInitialize")]
        public static extern UInt32 EApiLibUnInitialize();
        [DllImport("aaeonEAPI.dll", EntryPoint = "EApiI2CReadTransfer")]
        public static extern UInt32 EApiI2CReadTransfer(uint Id, UInt32 Addr, UInt32 Cmd, out IntPtr pBuffer, UInt32 BufLen, UInt32 ByteCnt);
        [DllImport("aaeonEAPI.dll", EntryPoint = "EApiI2CWriteTransfer")]
        public static extern UInt32 EApiI2CWriteTransfer(uint Id, UInt32 Addr, UInt32 Cmd, out byte pBuffer, UInt32 ByteCnt);

        /*
        IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
            // Call unmanaged code
            Marshal.FreeHGlobal(unmanagedPointer);
         */

        #endregion


        #region My Variables
        public static uint HIGH = 1;
        public static uint LOW = 0;
        private static uint INPUT = 1;
        private static uint OUTPUT = 0;
        #endregion

        #region Status Codes
        public const UInt32 EAPI_STATUS_NOT_INITIALIZED = 0xFFFFFFFF;
        public const UInt32 EAPI_STATUS_INITIALIZED = 0xFFFFFFFE;
        public const UInt32 EAPI_STATUS_ALLOC_ERROR = 0xFFFFFFFD;
        public const UInt32 EAPI_STATUS_DRIVER_TIMEOUT = 0xFFFFFFFC;
        public const UInt32 EAPI_STATUS_INVALID_PARAMETER = 0xFFFFFEFF;
        public const UInt32 EAPI_STATUS_INVALID_BLOCK_ALIGNMENT = 0xFFFFFEFE;
        public const UInt32 EAPI_STATUS_INVALID_BLOCK_LENGTH = 0xFFFFFEFD;
        public const UInt32 EAPI_STATUS_INVALID_DIRECTION = 0xFFFFFEFC;
        public const UInt32 EAPI_STATUS_INVALID_BITMASK = 0xFFFFFEFB;
        public const UInt32 EAPI_STATUS_RUNNING = 0xFFFFFEFA;
        public const UInt32 EAPI_STATUS_UNSUPPORTED = 0xFFFFFCFF;
        public const UInt32 EAPI_STATUS_NOT_FOUND = 0xFFFFFBFF;
        public const UInt32 EAPI_STATUS_TIMEOUT = 0xFFFFFBFE;
        public const UInt32 EAPI_STATUS_BUSY_COLLISION = 0xFFFFFBFD;
        public const UInt32 EAPI_STATUS_READ_ERROR = 0xFFFFFAFF;
        public const UInt32 EAPI_STATUS_WRITE_ERROR = 0xFFFFFAFE;
        public const UInt32 EAPI_STATUS_MORE_DATA = 0xFFFFF9FF;
        public const UInt32 EAPI_STATUS_ERROR = 0xFFFFF0FF;
        public const UInt32 EAPI_STATUS_SUCCESS = 0x00000000;
        #endregion
        #region Board Hardware Info Strings
        public const UInt32 EAPI_ID_BOARD_MANUFACTURER_STR = 0;
        public const UInt32 EAPI_ID_BOARD_NAME_STR = 1;
        public const UInt32 EAPI_ID_BOARD_REVISION_STR = 2;
        public const UInt32 EAPI_ID_BOARD_SERIAL_STR = 3;
        public const UInt32 EAPI_ID_BOARD_BIOS_REVISION_STR = 4;
        public const UInt32 EAPI_ID_BOARD_HW_REVISION_STR = 5;
        public const UInt32 EAPI_ID_BOARD_PLATFORM_TYPE_STR = 6;
        public const UInt32 EAPI_ID_GET_EAPI_SPEC_VERSION = 0;
        public const UInt32 EAPI_ID_BOARD_BOOT_COUNTER_VAL = 1;
        public const UInt32 EAPI_ID_BOARD_RUNNING_TIME_METER_VAL = 2;
        public const UInt32 EAPI_ID_BOARD_PNPID_VAL = 3;
        public const UInt32 EAPI_ID_BOARD_PLATFORM_REV_VAL = 4;
        public const UInt32 EAPI_ID_AONCUS_HISAFE_FUCTION = 5;
        public const UInt32 EAPI_ID_BOARD_DRIVER_VERSION_VAL = 0x10000;
        public const UInt32 EAPI_ID_BOARD_LIB_VERSION_VAL = 0x10001;
        public const UInt32 EAPI_ID_HWMON_CPU_TEMP = 0x20000;
        public const UInt32 EAPI_ID_HWMON_CHIPSET_TEMP = 0x20001;
        public const UInt32 EAPI_ID_HWMON_SYSTEM_TEMP = 0x20002;
        public const UInt32 EAPI_KELVINS_OFFSET = 2731;
        public static UInt32 EAPI_ENCODE_CELCIUS(UInt32 Celsius)
        {
            return (Celsius * 10) + EAPI_KELVINS_OFFSET;
        }
        public static UInt32 EAPI_DECODE_CELCIUS(UInt32 Celsius)
        {
            return ((Celsius) - EAPI_KELVINS_OFFSET) / 10;
        }
        public const UInt32 EAPI_ID_HWMON_VOLTAGE_VCORE = 0x21004;
        public const UInt32 EAPI_ID_HWMON_VOLTAGE_2V5 = 0x21008;
        public const UInt32 EAPI_ID_HWMON_VOLTAGE_3V3 = 0x2100C;
        public const UInt32 EAPI_ID_HWMON_VOLTAGE_VBAT = 0x21010;
        public const UInt32 EAPI_ID_HWMON_VOLTAGE_5V = 0x21014;
        public const UInt32 EAPI_ID_HWMON_VOLTAGE_5VSB = 0x21018;
        public const UInt32 EAPI_ID_HWMON_VOLTAGE_12V = 0x2101C;
        public const UInt32 EAPI_ID_HWMON_VOLTAGE_DIMM = 0x21020;
        public const UInt32 EAPI_ID_HWMON_VOLTAGE_3VSB = 0x21024;
        public const UInt32 EAPI_ID_HWMON_FAN_CPU = 0x22000;
        public const UInt32 EAPI_ID_HWMON_FAN_CHIPSET = 0x22001;
        public const UInt32 EAPI_ID_HWMON_FAN_SYSTEM = 0x22002;
        public const UInt32 EAPI_ID_BACKLIGHT_1 = 0;
        public const UInt32 EAPI_ID_BACKLIGHT_2 = 1;
        public const UInt32 EAPI_ID_BACKLIGHT_3 = 2;
        public const UInt32 EAPI_BACKLIGHT_SET_ON = 0;
        public const UInt32 EAPI_BACKLIGHT_SET_OFF = 0xFFFFFFFF;
        public const UInt32 EAPI_BACKLIGHT_SET_DIMEST = 0;
        public const UInt32 EAPI_BACKLIGHT_SET_BRIGHTEST = 255;
        public const UInt32 EAPI_ID_STORAGE_STD = 0;
        public const UInt32 EAPI_ID_I2C_EXTERNAL = 0;
        public const UInt32 EAPI_ID_I2C_LVDS_1 = 1;
        public const UInt32 EAPI_ID_I2C_LVDS_2 = 2;
        public static UInt32 EAPI_I2C_ENC_7BIT_ADDR(UInt32 x)
        {
            return ((x) & 0x07F) << 1;
        }
        public static UInt32 EAPI_I2C_DEC_7BIT_ADDR(UInt32 x)
        {
            return ((x) >> 1) & 0x07F;
        }
        #endregion
        #region GPIO
        public static UInt32 EAPI_GPIO_GPIO_ID(UInt32 GPIO_NUM)
        {
            return GPIO_NUM;
        }
        public const UInt32 EAPI_GPIO_GPIO_BITMASK = 1;
        public static UInt32 EAPI_ID_GPIO_GPIO00 = EAPI_GPIO_GPIO_ID(0);
        public static UInt32 EAPI_ID_GPIO_GPIO01 = EAPI_GPIO_GPIO_ID(1);
        public static UInt32 EAPI_ID_GPIO_GPIO02 = EAPI_GPIO_GPIO_ID(2);
        public static UInt32 EAPI_ID_GPIO_GPIO03 = EAPI_GPIO_GPIO_ID(3);
        public static UInt32 EAPI_ID_GPIO_GPIO04 = EAPI_GPIO_GPIO_ID(4);
        public static UInt32 EAPI_ID_GPIO_GPIO05 = EAPI_GPIO_GPIO_ID(5);
        public static UInt32 EAPI_ID_GPIO_GPIO06 = EAPI_GPIO_GPIO_ID(6);
        public static UInt32 EAPI_ID_GPIO_GPIO07 = EAPI_GPIO_GPIO_ID(7);
        public static UInt32 EAPI_GPIO_BANK_ID(UInt32 GPIO_NUM)
        {
            return (0x10000 | ((GPIO_NUM) >> 5));
        }
        public static int EAPI_GPIO_BANK_MASK(int GPIO_NUM)
        {
            return (0x01 << (GPIO_NUM & 0x1F));
        }
        public static UInt32 EAPI_ID_GPIO_BANK00 = EAPI_GPIO_BANK_ID(0);
        public static UInt32 EAPI_ID_GPIO_BANK01 = EAPI_GPIO_BANK_ID(32);
        public static UInt32 EAPI_ID_GPIO_BANK02 = EAPI_GPIO_BANK_ID(64);
        public const UInt32 EAPI_GPIO_BITMASK_SELECT = 1;
        public const UInt32 EAPI_GPIO_BITMASK_NOSELECT = 0;
        public const UInt32 EAPI_GPIO_LOW = 0;
        public const UInt32 EAPI_GPIO_HIGH = 1;
        public const UInt32 EAPI_GPIO_INPUT = 1;
        public const UInt32 EAPI_GPIO_OUTPUT = 0;
        #endregion

        #region Helpers
        private static uint EnableSDIO()
        {
            uint err = EApiLibInitialize();

            return err;
        }

        public static int SetPinDirection(int pin, uint direction)
        {
            uint err;
            pin = pin - 1;
            err = EApiGPIOSetDirection(EAPI_GPIO_GPIO_ID((UInt32)(pin)), 0xFFFFFFFF, direction);
            if (err > 0)
            {
                //Status.Text = "Error #" + err;
            }
            return (int)err;
        }
        public static int GetPinDirection(int pin, uint direction)
        {
            uint err;
            pin = pin - 1;
            err = EApiGPIOGetDirection(EAPI_GPIO_GPIO_ID((UInt32)(pin)), 0xFFFFFFFF, ref direction);
            if (err > 0)
            {
                //Status.Text = "Error #" + err;
                return (int)err;
            }
            return (int)direction;
        }
        public static int SetPinState(int pin, uint state)
        {
            uint err;
            pin = pin - 1;
            err = EApiGPIOSetLevel(EAPI_GPIO_GPIO_ID((UInt32)(pin)), 0xFFFFFFFF, state);
            if (err > 0)
            {
                //Status.Text = "Error #" + err;
            }
            return (int)err;
        }
        public static int GetPinState(int pin, uint state)
        {
            uint err;
            pin = pin - 1;
            err = EApiGPIOGetLevel(EAPI_GPIO_GPIO_ID((UInt32)(pin)), 0xFFFFFFFF, ref state);
            if (err > 0)
            {
                //Status.Text = "Error #" + err;
                return (int)-1;
            }
            return (int)state;
        }
        #endregion



        public static string ManufactureName { get; set; } = "";
        public static string BoardName { get; set; } = "";
        public static string BIOSVersion { get; set; } = "";

        public static bool GPIOSwitch = false;
        public static uint[] GPIOLevel;
        public static bool[] ButtonState;
        public static int[] ButtonCounter;
        public static int ButtonCounterTotal = 3;

        private static Thread threadGPIOCollecting;

        public static void CreateUpBoard()
        {
            GPIOLevel = new uint[28];
            ButtonState = new bool[28];
            ButtonCounter = new int[28];

            for (int i = 0; i < 28; i++)
            {
                GPIOLevel[i] = 1;
                ButtonState[i] = false;
                ButtonCounter[i] = 0;
            }

            if (EnableSDIO() != EAPI_STATUS_INITIALIZED)
            {
                GPIOSwitch = false;
                return;
            }
            GPIOSwitch = true;

            SetPinDirection(19, INPUT);
            SetPinDirection(21, INPUT);
            SetPinDirection(23, INPUT);
            SetPinDirection(27, INPUT);
            SetPinDirection(29, INPUT);

            SetPinDirection(22, INPUT);
            SetPinDirection(24, INPUT);
            SetPinDirection(26, INPUT);
            SetPinDirection(28, INPUT);
            SetPinDirection(32, INPUT);

            SetPinDirection(31, OUTPUT);
            SetPinDirection(18, OUTPUT);
            SetPinState(31, LOW);
            SetPinState(18, LOW);
            //byte pbuffer;

            //EApiI2CWriteTransfer(EAPI_ID_I2C_EXTERNAL, EAPI_I2C_ENC_7BIT_ADDR(238), 0x1E, out pbuffer, 1);


            threadGPIOCollecting = new Thread(new ThreadStart(StartGPIOCollecting));
            threadGPIOCollecting.Start();

        }

        public static void CloseUpBoard()
        {
            SetPinState(18, LOW);
            if (threadGPIOCollecting != null) threadGPIOCollecting.Abort();
            if (GPIOSwitch)
            {
                GPIOSwitch = false;
                EApiLibUnInitialize();
            }
        }

        private static void StartI2CCollecting()
        {
            //EApiI2CWriteTransfer(EAPI_ID_I2C_EXTERNAL, EAPI_I2C_ENC_7BIT_ADDR(0xEE), 0x48, out i2cdata, 0);

            byte[] bytes = new byte[10];
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
            EApiI2CReadTransfer(EAPI_ID_I2C_EXTERNAL, EAPI_I2C_ENC_7BIT_ADDR(0xEF), 0x48, out unmanagedPointer, 10, 3);
            Marshal.FreeHGlobal(unmanagedPointer);
            int _D1 = (bytes[0] << 16) + (bytes[1] << 8) + bytes[2];

            //EApiI2CWriteTransfer(EAPI_ID_I2C_EXTERNAL, EAPI_I2C_ENC_7BIT_ADDR(0xEE), 0x58, out i2cdata, 0);

            byte[] bytes1 = new byte[10];
            IntPtr unmanagedPointer1 = Marshal.AllocHGlobal(bytes1.Length);
            Marshal.Copy(bytes1, 0, unmanagedPointer1, bytes1.Length);
            EApiI2CReadTransfer(EAPI_ID_I2C_EXTERNAL, EAPI_I2C_ENC_7BIT_ADDR(0xEF), 0x58, out unmanagedPointer1, 10, 3);
            Marshal.FreeHGlobal(unmanagedPointer1);
            int _D2 = (bytes1[0] << 16) + (bytes1[1] << 8) + bytes1[2];

        }

        private static void StartGPIOCollecting() //采集AD数据
        {
            while (GPIOSwitch)
            {
                Thread.Sleep(10);
                int[] state = new int[10];
                state[0] = GetPinState(19, GPIOLevel[0]);
                state[1] = GetPinState(21, GPIOLevel[1]);
                state[2] = GetPinState(23, GPIOLevel[2]);
                state[3] = GetPinState(27, GPIOLevel[3]);
                state[4] = GetPinState(29, GPIOLevel[4]);

                state[5] = GetPinState(22, GPIOLevel[5]);
                state[6] = GetPinState(24, GPIOLevel[6]);
                state[7] = GetPinState(26, GPIOLevel[7]);
                state[8] = GetPinState(28, GPIOLevel[8]);
                state[9] = GetPinState(32, GPIOLevel[9]);

                for (int i = 0; i < 10; i++)
                {
                    if (state[i] == -1) continue;
                    if (state[i] == 0)
                    {
                        ButtonCounter[i]++;
                        if (ButtonCounter[i] == ButtonCounterTotal)
                        {
                            GPIOLevel[i] = Convert.ToUInt32(state[i]);
                            ButtonCounter[i] = 0;
                        }

                    }
                    else
                    {
                        ButtonCounter[i] = 0;
                        GPIOLevel[i] = Convert.ToUInt32(state[i]);
                    }
                }

                if (GPIOLevel[2] == 0 && GPIOLevel[7] == 0)
                {
                    GPIOLevel[10] = 0;
                    GPIOLevel[2] = 1;
                    GPIOLevel[7] = 1;
                }
                else
                    GPIOLevel[10] = 1;

                if (GPIOLevel[1] == 0 && GPIOLevel[6] == 0)
                {
                    GPIOLevel[11] = 0;
                    GPIOLevel[1] = 1;
                    GPIOLevel[6] = 1;
                }
                else
                    GPIOLevel[11] = 1;
            }
        }
    }

    public static class GlobalSonar
    {
        public static bool SonarSwitch = false;
        public static MainModel mainModel;
        public static bool isInstalled = false;

        public static void CreateSonar()
        {
            mainModel = new MainModel();
            //mainModel.OpenSonarFile("swimmer.son");
            mainModel.OpenSonarOnNetwork("192.168.1.45");

            //mainModel.OpenSonarFile("M900-130-Bridge-pilings-and-exposed-cabling.son");
            mainModel.ColorMapper.Load("copper.cmap");
        }

        public static void CloseSonar()
        {
            /*
            FieldInfo f1 = typeof(Control).GetField("EventImageDataReceived",
    BindingFlags.Static | BindingFlags.NonPublic);
            object obj = f1.GetValue(mainModel.Sonar);
            PropertyInfo pi = mainModel.Sonar.GetType().GetProperty("Events",
                BindingFlags.NonPublic | BindingFlags.Instance);
            EventHandlerList list = (EventHandlerList)pi.GetValue(mainModel.Sonar, null);
            list.RemoveHandler(obj, list[obj]);
            */
            mainModel.DisconnectSonar();
            mainModel.Close();
        }
    }

    public static class GlobalOculus
    {
        [DllImport("OculusSonar.dll", CallingConvention = CallingConvention.Cdecl)]//导入qtdialog.dll
        public static extern void InitialDll();//声明qtdialog.dll里面的一个接口
        [DllImport("OculusSonar.dll", CallingConvention = CallingConvention.Cdecl)]//导入qtdialog.dll
        public static extern void ExitDll();//声明qtdialog.dll里面的一个接口
        public static bool SonarSwitch = false;
        public static bool isInstalled = false;

        public static void CreateSonar()
        {
            InitialDll();
        }

        public static void CloseSonar()
        {
            ExitDll();
        }
    }

    public static class GlobalDVL
    {
        public static bool DVLNavigationMode = false;
        public static bool isInstalled = false;

        public static DVL dvl1;

        private static SerialSendData serialport1;

        public static void CreateDVL()
        {
            serialport1 = new SerialSendData();
            try
            {
                serialport1.OnDataReceived += new SerialSendData.UserRequest(DVLDataReceived);
                serialport1.OpenPort(SelectXMLData.GetConfiguration("DVLPORT", "value"), 115200, SerialSendData.SerialType.DVLPORT);

                dvl1 = new DVL('1', ref serialport1);
            }
            catch
            {

            }
        }

        public static void CloseDVL()
        {
            serialport1.ClosePort();
        }

        private static void DVLDataReceived(object sender, ReceivedEventArgs e)
        {
            string DVL = new string(e.DataReceived);
            //string[] GPSLine = GPS.Split('\r', '\n');
            string DVLLine = DVL;
            if (DVLLine.Length == 0)
                return;
            try
            {
                string[] DVLData = (DVLLine.Split(','));
                if (DVLData.Length == 9 && DVLData[0] == "wrx")
                {
                    dvl1.time = Convert.ToDouble(DVLData[1]);
                    dvl1.vx = Convert.ToDouble(DVLData[2]);
                    dvl1.vy = Convert.ToDouble(DVLData[3]);
                    dvl1.vz = Convert.ToDouble(DVLData[4]);
                    dvl1.fom = Convert.ToDouble(DVLData[5]);
                    dvl1.altitude = Convert.ToDouble(DVLData[6]);
                    if (DVLData[7] == "y")
                        dvl1.valid = true;
                    if (DVLData[7] == "n")
                        dvl1.valid = false;
                    dvl1.status = Convert.ToInt32(DVLData[8]);
                }
            }
            catch { }
        }


    }

    public static class GlobalNavigation
    {
        private static SerialSendData serialport1;

        public static Navigation nav1;

        private static Thread threadNavCollecting;

        public static NavCommUserControl NavCommUserControl;

        private static MTi mti;

        public static void CreateNav()
        {
            if (SelectXMLData.GetConfiguration("NavType", "value") == "0")
            {
                serialport1 = new SerialSendData();
                serialport1.OnDataReceived += new SerialSendData.UserRequest(DataReceived);
                serialport1.OnSuccessfulDataReceived += new SerialSendData.UserRequest(SuccessfulDataReceived);
                serialport1.OnNavDataReceived += new SerialSendData.UserRequest(NavDataReceived);
                serialport1.OpenPort(SelectXMLData.GetConfiguration("NavTelemetry", "value"), 57600, SerialSendData.SerialType.NavTelemetry); //主通信
                nav1 = new Navigation('1', ref serialport1);
                nav1.NavigationType = Navigation.NavType.Self;

            }

            if (SelectXMLData.GetConfiguration("NavType", "value") == "1")
            {
                nav1 = new Navigation('1');
                mti = new MTi();
                mti.OpenPort(SelectXMLData.GetConfiguration("NavTelemetry", "value"));
                nav1.NavigationType = Navigation.NavType.Mti;
            }

            if (SelectXMLData.GetConfiguration("NavType", "value") == "2")
            {
                serialport1 = new SerialSendData();
                serialport1.OnDataReceived += new SerialSendData.UserRequest(DataReceived);
                serialport1.OnSuccessfulDataReceived += new SerialSendData.UserRequest(SuccessfulDataReceived);
                serialport1.OnAH500NavDataReceived += new SerialSendData.ByteUserRequest(AH500NavDataReceived);
                serialport1.OpenPort(SelectXMLData.GetConfiguration("NavTelemetry", "value"), 9600, SerialSendData.SerialType.AH500Telemetry); //主通信
                nav1 = new Navigation('1', ref serialport1);
                nav1.NavigationType = Navigation.NavType.AH500;
            }

            if (SelectXMLData.GetConfiguration("NavType", "value") == "3")
            {
                serialport1 = new SerialSendData();
                serialport1.OnDataReceived += new SerialSendData.UserRequest(DataReceived);
                serialport1.OnSuccessfulDataReceived += new SerialSendData.UserRequest(SuccessfulDataReceived);
                serialport1.OnDCM250BNavDataReceived += new SerialSendData.ByteUserRequest(DCM250BNavDataReceived);
                serialport1.OpenPort(SelectXMLData.GetConfiguration("NavTelemetry", "value"), 9600, SerialSendData.SerialType.DCM250BTelemetry); //主通信
                nav1 = new Navigation('1', ref serialport1);
                nav1.NavigationType = Navigation.NavType.DCM250B;
            }

            if (nav1 != null)
            {
                nav1.SetInstall(true);

                nav1.SetHeading(0);
                nav1.SetTrueHeading(0);
            }


            threadNavCollecting = new Thread(new ThreadStart(StartNavInquiry));
            threadNavCollecting.Start();
        }

        private static void Serialport1_OnDataReceived(object sender, ReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static void CloseNav()
        {
            serialport1.ClosePort();
            if (nav1.NavigationType == Navigation.NavType.Mti)
                if (mti != null)
                    mti.ClosePort();
            if (threadNavCollecting != null) threadNavCollecting.Abort();
        }

        private static void DataReceived(object sender, ROV.Serial.ReceivedEventArgs e)
        {

        }

        private static void DCM250BNavDataReceived(object sender, ROV.Serial.ByteReceivedEventArgs e)
        {
            byte[] bytes = e.DataReceived;
            byte[] pitchbytes = new byte[3];
            byte[] rollbytes = new byte[3];
            byte[] yawbytes = new byte[3];

            pitchbytes[0] = bytes[1];
            pitchbytes[1] = bytes[2];
            pitchbytes[2] = bytes[3];

            rollbytes[0] = bytes[4];
            rollbytes[1] = bytes[5];
            rollbytes[2] = bytes[6];

            yawbytes[0] = bytes[7];
            yawbytes[1] = bytes[8];
            yawbytes[2] = bytes[9];


            if (nav1.NavigationType == Navigation.NavType.DCM250B)
            {
                int pitchsize1 = 10 * (pitchbytes[0] / 16) + (pitchbytes[0] % 16);
                int pitchshiwei = pitchsize1 / 10 % 10;
                int pitchgewei = pitchsize1 / 1 % 10;

                int pitchsize2 = 10 * (pitchbytes[1] / 16) + (pitchbytes[1] % 16);
                int pitchsize3 = 10 * (pitchbytes[2] / 16) + (pitchbytes[2] % 16);
                double pitch = Convert.ToDouble(pitchgewei * 100) + Convert.ToDouble(pitchsize2) + Convert.ToDouble(pitchsize3 * 0.01);
                if (pitchshiwei == 1)
                    pitch *= -1;

                int rollsize1 = 10 * (rollbytes[0] / 16) + (rollbytes[0] % 16);
                int rollshiwei = rollsize1 / 10 % 10;
                int rollgewei = rollsize1 / 1 % 10;

                int rollsize2 = 10 * (rollbytes[1] / 16) + (rollbytes[1] % 16);
                int rollsize3 = 10 * (rollbytes[2] / 16) + (rollbytes[2] % 16);
                double roll = Convert.ToDouble(rollgewei * 100) + Convert.ToDouble(rollsize2) + Convert.ToDouble(rollsize3 * 0.01);
                if (rollshiwei == 1)
                    roll *= -1;

                int yawsize1 = 10 * (yawbytes[0] / 16) + (yawbytes[0] % 16);
                int yawshiwei = yawsize1 / 10 % 10;
                int yawgewei = yawsize1 / 1 % 10;

                int yawsize2 = 10 * (yawbytes[1] / 16) + (yawbytes[1] % 16);
                int yawsize3 = 10 * (yawbytes[2] / 16) + (yawbytes[2] % 16);
                double yaw = Convert.ToDouble(yawgewei * 100) + Convert.ToDouble(yawsize2) + Convert.ToDouble(yawsize3 * 0.01);
                if (yawshiwei == 1)
                    yaw *= -1;
                double heading = 0.0;
                heading = yaw;
                if (nav1.HeadingZeroSwitch == true)
                {
                    heading -= nav1.HeadingZero;
                    if (heading > 360)
                        heading = heading - 360;
                    if (heading < 0)
                        heading = heading + 360;
                }
                nav1.SetTrueHeading(heading);
                nav1.SetHeading(heading);

                nav1.SetPitch(pitch);
                nav1.SetRoll(roll);

            }
        }

        private static void AH500NavDataReceived(object sender, ROV.Serial.ByteReceivedEventArgs e)
        {
            byte[] bytes = e.DataReceived;
            byte[] pitchbytes = new byte[3];
            byte[] rollbytes = new byte[3];
            byte[] yawbytes = new byte[3];

            pitchbytes[0] = bytes[1];
            pitchbytes[1] = bytes[2];
            pitchbytes[2] = bytes[3];

            rollbytes[0] = bytes[4];
            rollbytes[1] = bytes[5];
            rollbytes[2] = bytes[6];

            yawbytes[0] = bytes[7];
            yawbytes[1] = bytes[8];
            yawbytes[2] = bytes[9];


            if (nav1.NavigationType == Navigation.NavType.AH500)
            {
                int pitchsize1 = 10 * (pitchbytes[0] / 16) + (pitchbytes[0] % 16);
                int pitchshiwei = pitchsize1 / 10 % 10;
                int pitchgewei = pitchsize1 / 1 % 10;

                int pitchsize2 = 10 * (pitchbytes[1] / 16) + (pitchbytes[1] % 16);
                int pitchsize3 = 10 * (pitchbytes[2] / 16) + (pitchbytes[2] % 16);
                double pitch = Convert.ToDouble(pitchgewei * 100) + Convert.ToDouble(pitchsize2) + Convert.ToDouble(pitchsize3 * 0.01);
                if (pitchshiwei == 1)
                    pitch *= -1;

                int rollsize1 = 10 * (rollbytes[0] / 16) + (rollbytes[0] % 16);
                int rollshiwei = rollsize1 / 10 % 10;
                int rollgewei = rollsize1 / 1 % 10;

                int rollsize2 = 10 * (rollbytes[1] / 16) + (rollbytes[1] % 16);
                int rollsize3 = 10 * (rollbytes[2] / 16) + (rollbytes[2] % 16);
                double roll = Convert.ToDouble(rollgewei * 100) + Convert.ToDouble(rollsize2) + Convert.ToDouble(rollsize3 * 0.01);
                if (rollshiwei == 1)
                    roll *= -1;

                int yawsize1 = 10 * (yawbytes[0] / 16) + (yawbytes[0] % 16);
                int yawshiwei = yawsize1 / 10 % 10;
                int yawgewei = yawsize1 / 1 % 10;

                int yawsize2 = 10 * (yawbytes[1] / 16) + (yawbytes[1] % 16);
                int yawsize3 = 10 * (yawbytes[2] / 16) + (yawbytes[2] % 16);
                double yaw = Convert.ToDouble(yawgewei * 100) + Convert.ToDouble(yawsize2) + Convert.ToDouble(yawsize3 * 0.01);
                if (yawshiwei == 1)
                    yaw *= -1;
                double heading = 0.0;
                heading = yaw;
                if (nav1.HeadingZeroSwitch == true)
                {
                    heading -= nav1.HeadingZero;
                    if (heading > 360)
                        heading = heading - 360;
                    if (heading < 0)
                        heading = heading + 360;
                }
                nav1.SetTrueHeading(heading);
                nav1.SetHeading(heading);

                nav1.SetPitch(pitch);
                nav1.SetRoll(roll);

            }
        }

        private static void NavDataReceived(object sender, ROV.Serial.ReceivedEventArgs e)
        {
            char[] chars = e.DataReceived;
            string str;
            double heading = 0.0;
            double depth = 0.0;
            double zgyro = 0.0;
            double pitch = 0.0;
            double roll = 0.0;

            if (nav1.NavigationType == Navigation.NavType.Self)
            {
                switch (chars[0])
                {
                    case 'l':
                        str = new string(chars, 1, chars.Length - 1);
                        string[] sArray = str.Split(',');

                        heading = Convert.ToDouble(sArray[0]);
                        if (heading == 0) return;
                        if (nav1.HeadingZeroSwitch == true)
                        {
                            heading += nav1.HeadingZero;
                            if (heading > 360)
                                heading = heading - 360;
                            if (heading < 0)
                                heading = heading + 360;
                        }
                        nav1.SetTrueHeading(heading);
                        nav1.SetHeading(heading);

                        depth = Convert.ToDouble(sArray[1]);
                        if (nav1.DepthZeroSwitch == true)
                            depth = depth - nav1.DepthZero;
                        depth = Math.Round(depth, 2);
                        //if (Math.Abs(depth - nav1.GetDepth()) > 10) return;
                        nav1.SetDepth(depth);

                        zgyro = Convert.ToDouble(sArray[2]);
                        nav1.SetGyro(zgyro);

                        pitch = Convert.ToDouble(sArray[3]);
                        nav1.SetPitch(pitch);

                        roll = Convert.ToDouble(sArray[4]);
                        nav1.SetRoll(roll);
                        break;
                    case 'a':
                        str = new string(chars, 1, chars.Length - 1);
                        heading = Convert.ToDouble(str);
                        if (heading == 0) return;
                        nav1.SetTrueHeading(heading);
                        nav1.SetHeading(heading);
                        str = string.Format("{0:000.0}", heading);
                        break;
                    case 'p':
                        byte[] temppitchangel = new byte[3];
                        byte[] pitchangel = new byte[3];
                        str = new string(chars, 1, chars.Length - 1);
                        pitch = Convert.ToDouble(str);
                        nav1.SetPitch(pitch);
                        str = string.Format("{0:00}", pitch);
                        temppitchangel = System.Text.Encoding.Default.GetBytes(str);
                        if (temppitchangel[0] == 45)
                        {
                            pitchangel[0] = 0x01;
                            pitchangel[1] = temppitchangel[1];
                            pitchangel[2] = temppitchangel[2];
                        }
                        if (temppitchangel[0] >= 48 && temppitchangel[0] <= 57)
                        {
                            pitchangel[0] = 0x02;
                            pitchangel[1] = temppitchangel[0];
                            pitchangel[2] = temppitchangel[1];
                        }

                        if (temppitchangel[0] == 48 && temppitchangel[1] == 48)
                        {
                            pitchangel[0] = 0x00;
                            pitchangel[1] = temppitchangel[0];
                            pitchangel[2] = temppitchangel[1];
                        }
                        break;
                    case 'r':
                        byte[] temprollangel = new byte[3];
                        byte[] rollangel = new byte[3];
                        str = new string(chars, 1, chars.Length - 1);
                        roll = Convert.ToDouble(str);
                        nav1.SetRoll(roll);
                        str = string.Format("{0:00}", roll);
                        temprollangel = System.Text.Encoding.Default.GetBytes(str);
                        if (temprollangel[0] == 45)
                        {
                            rollangel[0] = 0x01;
                            rollangel[1] = temprollangel[1];
                            rollangel[2] = temprollangel[2];
                        }
                        if (temprollangel[0] >= 48 && temprollangel[0] <= 57)
                        {
                            rollangel[0] = 0x02;
                            rollangel[1] = temprollangel[0];
                            rollangel[2] = temprollangel[1];
                        }

                        if (temprollangel[0] == 48 && temprollangel[1] == 48)
                        {
                            rollangel[0] = 0x00;
                            rollangel[1] = temprollangel[0];
                            rollangel[2] = temprollangel[1];
                        }
                        //lblRollInfo.Text = str;
                        break;
                    case 'd':
                        str = new string(chars, 1, chars.Length - 1);
                        depth = Convert.ToDouble(str);
                        str = string.Format("{0:000.0}", depth);
                        if (Math.Abs(depth - nav1.GetDepth()) > 10) return;
                        nav1.SetDepth(depth);
                        break;
                    case 'z':
                        str = new string(chars, 1, chars.Length - 1);
                        zgyro = Convert.ToDouble(str);
                        str = string.Format("{0:000}", zgyro);
                        nav1.SetGyro(zgyro);

                        break;
                    case 'o':
                        /*
                        if (chars[1] == '0')
                        {
                            lblCompassOutputInquiryInfo.Text = "关闭";
                            btnCompassStartCalib.Enabled = true;
                            btnCompassSaveCalib.Enabled = false;
                        }
                        if (chars[1] == '1')
                        {
                            lblCompassOutputInquiryInfo.Text = "开启";
                            btnCompassStartCalib.Enabled = false;
                            btnCompassSaveCalib.Enabled = false;
                        }
                         */
                        break;
                    default:
                        break;
                }
            }
        }

        private static void SuccessfulDataReceived(object sender, ROV.Serial.ReceivedEventArgs e)
        {
            char[] chars = e.DataReceived;
            switch (chars[0])
            {
                case 'C': //导航单元
                    switch (chars[1])
                    {
                        case '1':
                            nav1.SetInstall(true);
                            nav1.SetNoResponseCounter(0);
                            nav1.InstalledSuccessfully(true);
                            //lblDaoHangInfo.Text = "通信正常";
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        private static void StartNavInquiry()
        {
            while (true)
            {
                Thread.Sleep(50);
                if (nav1.NavigationType == Navigation.NavType.Mti)
                {
                    double heading = 0.0;
                    double pitch = 0.0;
                    double roll = 0.0;
                    heading = mti.Yaw;
                    if (heading == 0) continue;
                    if (nav1.HeadingZeroSwitch == true)
                    {
                        heading += nav1.HeadingZero;
                        if (heading > 360)
                            heading = heading - 360;
                        if (heading < 0)
                            heading = heading + 360;
                    }
                    nav1.SetTrueHeading(heading);
                    nav1.SetHeading(heading);

                    pitch = mti.Pitch;
                    nav1.SetPitch(pitch);

                    roll = mti.Roll;
                    nav1.SetRoll(roll);
                }

                if (nav1.NavigationType == Navigation.NavType.Self)
                {
                    if (nav1.IsInstalled())
                    {
                        nav1.InquiryData('l');
                    }
                }

                if (nav1.NavigationType == Navigation.NavType.AH500)
                {
                    if (nav1.IsInstalled())
                    {
                        nav1.InquiryData('l');
                    }
                }

                if (nav1.NavigationType == Navigation.NavType.DCM250B)
                {
                    if (nav1.IsInstalled())
                    {
                        nav1.InquiryData('l');
                    }
                }
            }
        }

        public static void CreateNavComm()
        {
            NavCommUserControl = new NavCommUserControl();
        }

        public static void CloseNavComm()
        {
            GlobalNavigation.NavCommUserControl.Close();
            NavCommUserControl = null;
        }

        private static SerialSendData serialport2;

        public static void CreateGPS()
        {
            serialport2 = new SerialSendData();
            try
            {
                serialport2.OnDataReceived += new SerialSendData.UserRequest(GPSDataReceived);
                serialport2.OpenPort(SelectXMLData.GetConfiguration("GPSPORT", "value"), 9600, SerialSendData.SerialType.GPSPORT);
            }
            catch
            {
                //tmrGPSCommCheck.Enabled = true;
            }

        }

        private static void GPSDataReceived(object sender, ReceivedEventArgs e)
        {
            string GPS = new string(e.DataReceived);
            //string[] GPSLine = GPS.Split('\r', '\n');
            string GPSLine = GPS;
            if (GPSLine.Length == 0)
                return;
            try
            {
                string[] GPSData = (GPSLine.Split(','));
                if (GPSData.Length < 10)
                { }
                else
                {
                    switch (GPSData[0])
                    {
                        case "$GNGGA":
                            /*
                            try
                            {
                                //double _jingdu = Convert.ToDouble(GPSData[4]) * 0.01;
                                //double _weidu = Convert.ToDouble(GPSData[2]) * 0.01;


                                double _jingdu1 = Nmea2DecDeg(GPSData[4], "E");
                                double _weidu1 = Nmea2DecDeg(GPSData[2], "N");

                                if (_weidu1 == 0.0 || _jingdu1 == 0.0) return;

                                nav1.Latitude = _weidu1;
                                nav1.Longitude = _jingdu1;
                                //double[] latlng = wgs84togcj02(_weidu, _jingdu);
                                //nav1.Latitude = latlng[0];
                                //nav1.Longitude = latlng[1];
                                //lblJingDuInfo.Text = _jingdu.ToString("f7");
                                //lblWeiDuInfo.Text = _weidu.ToString("f7");
                                //axiAnalogDisplayHeight.Value = Convert.ToDouble(Convert.ToDouble(GPSData[9]).ToString("f1"));
                            }
                            catch (FormatException fex)
                            {
                                //serialport1.DiscardInBuffer();
                                return;
                            }
                            */
                            break;
                        case "$GNRMC":
                            try
                            {
                                if (GPSData[2] == "A") nav1.GPSValid = true;
                                if (GPSData[2] == "V") nav1.GPSValid = false;

                                double _jingdu2 = Nmea2DecDeg(GPSData[5], "E");
                                double _weidu2 = Nmea2DecDeg(GPSData[3], "N");

                                if (_weidu2 == 0.0 || _jingdu2 == 0.0) return;

                                if (nav1.GPSValid)
                                {
                                    nav1.Latitude = _weidu2;
                                    nav1.Longitude = _jingdu2;
                                }
                            }
                            catch (FormatException fex)
                            {
                                //serialport1.DiscardInBuffer();
                                return;
                            }
                            break;
                        case "$GPGGA":
                            /*
                            try
                            {
                                double _jingdu2 = Nmea2DecDeg(GPSData[4], "E");
                                double _weidu2 = Nmea2DecDeg(GPSData[2], "N");

                                if (_weidu2 == 0.0 || _jingdu2 == 0.0) return;

                                nav1.Latitude = _weidu2;
                                nav1.Longitude = _jingdu2;
                            }
                            catch (FormatException fex)
                            {
                                //serialport1.DiscardInBuffer();
                                return;
                            }
                            */
                            break;
                        case "$GPRMC":
                            /*
                            4、 Recommended Minimum Specific GPS/TRANSIT Data（RMC）推荐定位信息
                            $GPRMC,<1>,<2>,<3>,<4>,<5>,<6>,<7>,<8>,<9>,<10>,<11>,<12>*hh<CR><LF>
                            <1> UTC时间，hhmmss（时分秒）格式
                            <2> 定位状态，A=有效定位，V=无效定位
                            <3> 纬度ddmm.mmmm（度分）格式（前面的0也将被传输）
                            <4> 纬度半球N（北半球）或S（南半球）
                            <5> 经度dddmm.mmmm（度分）格式（前面的0也将被传输）
                            <6> 经度半球E（东经）或W（西经）
                            <7> 地面速率（000.0~999.9节，前面的0也将被传输）
                            <8> 地面航向（000.0~359.9度，以真北为参考基准，前面的0也将被传输）
                            <9> UTC日期，ddmmyy（日月年）格式
                            <10> 磁偏角（000.0~180.0度，前面的0也将被传输）
                            <11> 磁偏角方向，E（东）或W（西）
                            <12> 模式指示（仅NMEA0183 3.00版本输出，A=自主定位，D=差分，E=估算，N=数据无效）
                            */
                            try
                            {
                                if (GPSData[2] == "A") nav1.GPSValid = true;
                                if (GPSData[2] == "V") nav1.GPSValid = false;

                                double _jingdu2 = Nmea2DecDeg(GPSData[5], "E");
                                double _weidu2 = Nmea2DecDeg(GPSData[3], "N");

                                if (_weidu2 == 0.0 || _jingdu2 == 0.0) return;

                                if (nav1.GPSValid)
                                {
                                    nav1.Latitude = _weidu2;
                                    nav1.Longitude = _jingdu2;
                                }
                            }
                            catch (FormatException fex)
                            {
                                //serialport1.DiscardInBuffer();
                                return;
                            }
                            break;
                        case "$GPGSV":
                            /*
                            $GPGSV, <1>,<2>,<3>,<4>,<5>,<6>,<7>,?<4>,<5>,<6>,<7>,<8><CR><LF>
                            <1> GSV语句的总数 
                            <2> 本句GSV的编号 
                            <3> 可见卫星的总数，00 至 12。 
                            <4> 卫星编号， 01 至 32。 
                            <5>卫星仰角， 00 至 90 度。 
                            <6>卫星方位角， 000 至 359 度。实际值。 
                            <7>讯号噪声比（C/No）， 00 至 99 dB；无表未接收到讯号。 
                            <8>Checksum.(检查位).
                            第<4>,<5>,<6>,<7>项个别卫星会重复出现，每行最多有四颗卫星。其余卫星信息会于次一行出现，若未使用，这些字段会空白。
                             */
                            //$GPGSV,4,1,13,    02,02,213,,   03,-3,000,,  11,00,121,,  14,13,172,05   *67

                            break;
                    }
                }
            }
            catch { }
        }

        public static void CloseGPS()
        {
            serialport2.ClosePort();


        }

        public static double Nmea2DecDeg(string NmeaLonLat, string Hemisphere)
        {
            int inx = NmeaLonLat.IndexOf(".");
            if (inx == -1)
            {
                return 0;    // Invalid syntax
            }
            string minutes_str = NmeaLonLat.Substring(inx - 2);
            double minutes = Double.Parse(minutes_str, new
                      System.Globalization.CultureInfo("en-US"));
            string degrees_str = NmeaLonLat.Substring(0, inx - 2);
            double degrees = Convert.ToDouble(degrees_str) + minutes / 60.0;
            if (Hemisphere.Equals("W") || Hemisphere.Equals("S"))
            {
                degrees = -degrees;

            }
            return degrees;
        }
    }

    public static class GlobalExternal
    {
        private static Thread threadExternalCollecting;
        private static SerialSendData serialport1;
        public static int BrightLevel = 3;
        public static int LightBrightLevel = 0;

        public static void CreateExternal()
        {
            serialport1 = new SerialSendData();
            serialport1.OnSuccessfulDataReceived += new SerialSendData.UserRequest(SuccessfulDataReceived);
            serialport1.OnNavDataReceived += new SerialSendData.UserRequest(NavDataReceived);
            serialport1.OpenPort(SelectXMLData.GetConfiguration("EXTPORT", "value"), 57600, SerialSendData.SerialType.EXTPORT); //主通信

            if (SelectXMLData.GetConfiguration("NavType", "value") != "0")
            {
                SendBright(BrightLevel);
                if (Global.MountVision)
                    GlobalExternal.MinLightBright();
                threadExternalCollecting = new Thread(new ThreadStart(StartExternalInquiry));
                threadExternalCollecting.Start();
            }
            else
                SendOldBright(BrightLevel);


        }

        private static void SuccessfulDataReceived(object sender, ROV.Serial.ReceivedEventArgs e)
        {
            char[] chars = e.DataReceived;
            switch (chars[0])
            {
                case 'C': //深度计
                    switch (chars[1])
                    {
                        case '1':
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        private static void NavDataReceived(object sender, ROV.Serial.ReceivedEventArgs e)
        {
            char[] chars = e.DataReceived;
            string str;
            double depth = 0.0;
            double temperature = 0.0;
            switch (chars[0])
            {
                case 'l':
                    str = new string(chars, 1, chars.Length - 1);
                    string[] sArray = str.Split(',');

                    depth = Convert.ToDouble(sArray[0]);

                    depth = (depth - 101300) / (GlobalNavigation.nav1.FluidDensity * 9.80665);

                    if (GlobalNavigation.nav1.DepthZeroSwitch == true)
                        depth = depth - GlobalNavigation.nav1.DepthZero;
                    depth = Math.Round(depth, 2);
                    //if (Math.Abs(depth - nav1.GetDepth()) > 10) return;
                    GlobalNavigation.nav1.SetDepth(depth);

                    temperature = Convert.ToDouble(sArray[1]);
                    break;
                default:
                    break;
            }
        }


        private static void StartExternalInquiry()
        {
            while (true)
            {
                Thread.Sleep(100);
                InquiryDepth();
            }
        }

        public static void CloseExternal()
        {
            serialport1.ClosePort();
        }

        public static void PlusBright()
        {
            BrightLevel++;
            if (BrightLevel > 7) BrightLevel = 7;
            SendBright(BrightLevel);
        }

        public static void MinusBright()
        {
            BrightLevel--;
            if (BrightLevel < 0) BrightLevel = 0;
            SendBright(BrightLevel);
        }

        public static int ToggleBright()
        {
            BrightLevel++;
            if (BrightLevel > 7) BrightLevel = 0;
            SendBright(BrightLevel);
            return BrightLevel;
        }

        public static int ToggleOldBright()
        {
            BrightLevel++;
            if (BrightLevel > 7) BrightLevel = 0;
            SendOldBright(BrightLevel);
            return BrightLevel;
        }

        public static void SendBright(int _level)
        {
            ArrayList charlist = new ArrayList();

            char[] cmds;
            charlist.Add('M');
            charlist.Add('+');
            charlist.Add('B');
            charlist.Add(char.Parse(_level.ToString()));
            charlist.Add((char)(0x0D));
            cmds = (char[])charlist.ToArray(typeof(char));
            serialport1.SendMessage(cmds);
        }

        public static void SendOldBright(int _level)
        {
            ArrayList charlist = new ArrayList();

            char[] cmds;
            charlist.Add('M');
            charlist.Add('+');
            charlist.Add(char.Parse(_level.ToString()));
            charlist.Add((char)(0x0D));
            cmds = (char[])charlist.ToArray(typeof(char));
            serialport1.SendMessage(cmds);
        }

        public static int ToggleLightBright()
        {
            LightBrightLevel++;
            if (LightBrightLevel > 2) LightBrightLevel = 0;
            SendLightBright(LightBrightLevel);
            return LightBrightLevel;
        }

        public static void MinLightBright()
        {
            LightBrightLevel = 0;
            SendLightBright(LightBrightLevel);
        }

        public static void SendLightBright(int _level)
        {
            ArrayList charlist = new ArrayList();

            char[] cmds;
            charlist.Add('L');
            charlist.Add('+');
            charlist.Add('B');
            charlist.Add(char.Parse(_level.ToString()));
            charlist.Add((char)(0x0D));
            cmds = (char[])charlist.ToArray(typeof(char));
            serialport1.SendMessage(cmds);
        }

        public static void InquiryDepth()
        {
            serialport1.SendMessage(SerialSendData.DataType.ExternalBoard, '1', '?', "l", 1);
        }
    }

    public static class GlobalBattery
    {
        private static SerialSendData serialport1;
        private static float BatteryPowerPercent = 0;
        private static float BatteryPowerVoltage = 0;
        private static Thread threadBATCollecting;

        public static void CreateBattery()
        {
            serialport1 = new SerialSendData();
            try
            {
                serialport1.OnByteSuccessfulDataReceived += new SerialSendData.ByteUserRequest(BATDataReceived);

                serialport1.OpenPort(SelectXMLData.GetConfiguration("BATPORT", "value"), 4800, SerialSendData.SerialType.BATPORT); //电池电量通信
                BatteryPowerPercent = 0;
                BatteryPowerVoltage = 0;

                threadBATCollecting = new Thread(new ThreadStart(StartInquiryBattery));
                threadBATCollecting.Start();
            }
            catch
            {

            }
        }

        public static void CloseBattery()
        {
            try
            {
                serialport1.ClosePort();
                if (threadBATCollecting != null) threadBATCollecting.Abort();
            }
            catch
            {

            }
        }

        public static void InquiryBattery()
        {
            byte[] cmds = new byte[6];
            cmds[0] = 0xAA;
            cmds[1] = 0xFF;
            cmds[2] = 0x01;
            cmds[3] = 0x10;
            cmds[4] = 0x55;
            cmds[5] = 0xFF;
            serialport1.SendMessage(cmds);
        }

        private static void StartInquiryBattery()
        {
            while (true)
            {
                Thread.Sleep(1000);
                InquiryBattery();
            }
        }

        public static float GetBatteryPowerPercent()
        {
            return BatteryPowerPercent;
        }

        public static double GetBatteryVoltage()
        {
            return BatteryPowerVoltage;
        }

        private static void BATDataReceived(object sender, ByteReceivedEventArgs e)
        {
            BatteryPowerPercent = 0;
            BatteryPowerVoltage = 0;
            byte[] byteBatteryPowerVoltage = new byte[4];
            byteBatteryPowerVoltage[0] = e.DataReceived[0];
            byteBatteryPowerVoltage[1] = e.DataReceived[1];
            byteBatteryPowerVoltage[2] = 0;
            byteBatteryPowerVoltage[3] = 0;
            BatteryPowerVoltage = BitConverter.ToSingle(byteBatteryPowerVoltage, 0);
            BatteryPowerVoltage /= 1000;
            BatteryPowerPercent = Convert.ToSingle(e.DataReceived[8]);

        }
    }

    public class ShutDownSys
    {

        //C#关机代码
        // 这个结构体将会传递给API。使用StructLayout
        //(...特性，确保其中的成员是按顺序排列的，C#编译器不会对其进行调整。
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count; public long Luid; public int Attr;
        }
        // 以下使用DllImport特性导入了所需的Windows API。
        // 导入的方法必须是static extern的，并且没有方法体。
        //调用这些方法就相当于调用Windows API。
        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValueA
       (string host, string name, ref long pluid);
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool
         AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool ExitWindowsEx(int flg, int rea);
        //C#关机代码
        // 以下定义了在调用WinAPI时需要的常数。
        //这些常数通常可以从Platform SDK的包含文件（头文件）中找到
        public const int SE_PRIVILEGE_ENABLED = 0x00000002;
        public const int TOKEN_QUERY = 0x00000008;
        public const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        public const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        public const int EWX_LOGOFF = 0x00000000;
        public const int EWX_SHUTDOWN = 0x00000001;
        public const int EWX_REBOOT = 0x00000002;
        public const int EWX_FORCE = 0x00000004;
        public const int EWX_POWEROFF = 0x00000008;
        public const int EWX_FORCEIFHUNG = 0x00000010;
        // 通过调用WinAPI实现关机，主要代码再最后一行ExitWindowsEx  //这调用了同名的WinAPI，正好是关机用的。
        //C#关机代码
        public static void DoExitWin(int flg)
        {
            bool ok;
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            ok = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            ok = LookupPrivilegeValueA(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            ok = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            ok = ExitWindowsEx(flg, 0);
        }
    }


}
