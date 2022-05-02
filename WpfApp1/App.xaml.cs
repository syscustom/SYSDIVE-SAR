using System;
using System.Collections.Generic;
using System.Windows;
using GMap.NET;


namespace WpfApp1
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public List<WayPoint> LstWayPoints = new List<WayPoint>();

        [STAThread()]
        static void Main()
        {
            // Create the application.
            Application app = new Application();

            // Create the main window.
            MainWindow win = new MainWindow();

            Global.SavingMainDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + "SYSDIVESAR";
            if (System.IO.Directory.Exists(Global.SavingMainDirectory) == false)
                System.IO.Directory.CreateDirectory(Global.SavingMainDirectory);


            Global.SavingRecordDirectory = Global.SavingMainDirectory + "\\" + "Record" + "\\";
            if (System.IO.Directory.Exists(Global.SavingRecordDirectory) == false)
                System.IO.Directory.CreateDirectory(Global.SavingRecordDirectory);

            Global.SavingMissionDirectory = Global.SavingMainDirectory + "\\" + "Mission" + "\\";
            if (System.IO.Directory.Exists(Global.SavingMissionDirectory) == false)
                System.IO.Directory.CreateDirectory(Global.SavingMissionDirectory);

            Global.RecordSamplingRate = Convert.ToInt32(SelectXMLData.GetConfiguration("RecordSamplingRate", "value"));

            if (SelectXMLData.GetConfiguration("SonarDemoShow", "value") == "0")
                Global.SonarDemoShow = false;
            if (SelectXMLData.GetConfiguration("SonarDemoShow", "value") == "1")
                Global.SonarDemoShow = true;

            GlobalUpBoard.CreateUpBoard();

            Global.CreatMap();

            GlobalNavigation.CreateNav();

            GlobalNavigation.CreateNavComm();

            if (SelectXMLData.GetConfiguration("DepthZeroSwitch", "value") == "0")
            {
                GlobalNavigation.nav1.DepthZeroSwitch = false;
                GlobalNavigation.nav1.DepthZero = 0.0;
            }
            else if (SelectXMLData.GetConfiguration("DepthZeroSwitch", "value") == "1")
            {
                GlobalNavigation.nav1.DepthZeroSwitch = true;
                GlobalNavigation.nav1.DepthZero = Convert.ToDouble(SelectXMLData.GetConfiguration("DepthZero", "value"));
            }

            if (SelectXMLData.GetConfiguration("HeadingZeroSwitch", "value") == "0")
                GlobalNavigation.nav1.HeadingZeroSwitch = false;
            if (SelectXMLData.GetConfiguration("HeadingZeroSwitch", "value") == "1")
                GlobalNavigation.nav1.HeadingZeroSwitch = true;

            GlobalNavigation.nav1.HeadingZero = Convert.ToDouble(SelectXMLData.GetConfiguration("HeadingZero", "value"));

            GlobalNavigation.nav1.FluidDensity = Convert.ToDouble(SelectXMLData.GetConfiguration("FluidDensity", "value"));

            if (SelectXMLData.GetConfiguration("GNSSMode", "value") == "0")
            {
                Global.GNSSMode = Global.GNSSType.Internal;
                GlobalUpBoard.SetPinState(31, GlobalUpBoard.LOW);
            }
            else if (SelectXMLData.GetConfiguration("GNSSMode", "value") == "1")
            {
                Global.GNSSMode = Global.GNSSType.Float;
                GlobalUpBoard.SetPinState(31, GlobalUpBoard.HIGH);
            }

            if (SelectXMLData.GetConfiguration("MountVision", "value") == "0")
                Global.MountVision = false;
            else if (SelectXMLData.GetConfiguration("MountVision", "value") == "1")
                Global.MountVision = true;

            if (SelectXMLData.GetConfiguration("VisionSwitch", "value") == "0")
                Global.VisionSwitch = false;
            else if (SelectXMLData.GetConfiguration("VisionSwitch", "value") == "1")
                Global.VisionSwitch = true;

            if (Global.MountVision && Global.VisionSwitch)
                Global.CreateVideo();

            switch(SelectXMLData.GetConfiguration("SonarType", "value"))
            {
                case "0": //没有声呐
                    Global.sonartype = Global.SonarType.None;
                    GlobalSonar.isInstalled = false;
                    GlobalOculus.isInstalled = false;
                    break;
                case "1": //Blueview声呐
                    Global.sonartype = Global.SonarType.Blueview;
                    GlobalSonar.isInstalled = true;
                    GlobalOculus.isInstalled = false;
                    break;
                case "2": //Oculus声呐
                    Global.sonartype = Global.SonarType.Oculus;
                    GlobalSonar.isInstalled = false;
                    GlobalOculus.isInstalled = true;
                    break;
            }

            if (SelectXMLData.GetConfiguration("SonarSwitch", "value") == "0")
            {
                GlobalSonar.SonarSwitch = false;
                GlobalOculus.SonarSwitch = false;
                GlobalUpBoard.SetPinState(18, GlobalUpBoard.LOW);
            }

            else if (SelectXMLData.GetConfiguration("SonarSwitch", "value") == "1")
            {
                switch (Global.sonartype)
                {
                    case Global.SonarType.None: //没有声呐
                        GlobalSonar.SonarSwitch = false;
                        GlobalOculus.SonarSwitch = false;
                        GlobalUpBoard.SetPinState(18, GlobalUpBoard.LOW);
                        break;
                    case Global.SonarType.Blueview: //Blueview声呐
                        GlobalSonar.SonarSwitch = true;
                        GlobalOculus.SonarSwitch = false;
                        GlobalUpBoard.SetPinState(18, GlobalUpBoard.HIGH);
                        break;
                    case Global.SonarType.Oculus: //Oculus声呐
                        GlobalSonar.SonarSwitch = false;
                        GlobalOculus.SonarSwitch = true;
                        GlobalUpBoard.SetPinState(18, GlobalUpBoard.HIGH);
                        break;
                }
            }

            if (GlobalSonar.isInstalled && GlobalSonar.SonarSwitch)
            {
                GlobalSonar.CreateSonar();
            }

            //if(GlobalOculus.isInstalled && GlobalOculus.SonarSwitch)
            //{
                GlobalOculus.CreateSonar();
            //}

            GlobalNavigation.CreateGPS();

            GlobalExternal.CreateExternal();

            GlobalBattery.CreateBattery();

            if (SelectXMLData.GetConfiguration("ServiceMonitor", "value") == "1")
                Global.CreateServiceMonitor();

            GlobalExternal.BrightLevel = Convert.ToInt32(SelectXMLData.GetConfiguration("BrightLevel", "value"));

            if (SelectXMLData.GetConfiguration("MapNorth", "value") == "0")
                Global.mapnorth = Global.MapNorth.North;

            if (SelectXMLData.GetConfiguration("MapNorth", "value") == "1")
                Global.mapnorth = Global.MapNorth.Diver;

            if (SelectXMLData.GetConfiguration("MountDVL", "value") == "0")
            {
                GlobalDVL.isInstalled = false;
            }

            if (SelectXMLData.GetConfiguration("MountDVL", "value") == "1")
            {
                GlobalDVL.isInstalled = true;
                GlobalDVL.CreateDVL();
            }


            //frmMicronDST micronDST = new frmMicronDST();
            // Launch the application and show the main window.
            //app.Run(micronDST);

            frmSonarDisplay frmSonarDisplay = new frmSonarDisplay();
            frmSonarDisplay.Show();
            frmSonarDisplay.Hide();
            Global.SetWnd(frmSonarDisplay);



            app.Run(win);

            Global.EndCallService();
            if (Global.shutdowntype == Global.ShutDownType.Exit)
                Environment.Exit(0);
            if(Global.shutdowntype == Global.ShutDownType.Shutdown)
                ShutDownSys.DoExitWin(ShutDownSys.EWX_FORCE | ShutDownSys.EWX_POWEROFF);
        }
    }

    public class Dummy
    {

    }

    public struct PointAndInfo
    {
        public PointLatLng Point;
        public string Info;

        public PointAndInfo(PointLatLng point, string info)
        {
            Point = point;
            Info = info;
        }
    }
}
