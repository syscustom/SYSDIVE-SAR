﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ROV.Serial;
using System.Threading;

namespace WpfApp1
{
    public class Navigation
    {
        private char devid = '0';
        private SerialSendData serialport1;
        private double mdblheading = 0.0, mdblpitch = 0.0, mdblroll = 0.0, mdbldepth = 0.0, mdbltargetheading = 0.0, mdbltargetdepth = 0.0, mdblgyro = 0;
        private int noresponsecounter = 0;
        private bool installed = false;
        private bool installedsuccessfully = false;


        public delegate void UserRequest(object sender, ReceivedEventArgs e);


        private double mdblTurnHeading = 0, mdblTurnHeading1 = 0;
        private double mdblTurnCount = 0;
        private bool mblnGYROStabThruster = true;

        private double Start_Angel = -1, Last_Angel = -1, Current_Angel = -1;
        private double Start_Angel1 = -1, Last_Angel1 = -1, Current_Angel1 = -1;

        private int txpacks = 0, rxpacks = 0;

        public enum Dircetion
        {
            Zheng = 0,
            Fan = 1,
            Stop = 2
        };

        public enum NavType
        {
            Self = 0,
            Mti = 1,
            AH500 = 2,
            DCM250B = 3
        }

        private Dircetion turndirection = Dircetion.Stop;
        private NavType navtype = NavType.Self;

        private double mdblDepthZero = 0.0;
        private bool mblnDepthZeroSwitch = false;
        private double mdblHeadingZero = 0.0;
        private bool mblnHeadingZeroSwitch = false;
        private double mdblDistance = -1;
        private double mdblBearing = -1;
        private WayPoint waypoint = new WayPoint();

        private double mdbllatitude = 0.0; //39.056531162330714;
        private double mdbllongitude = 0.0; //117.06059910052493;
        private bool mblngpsvalid = false;
        private double fluiddensity = 997.0;

        public double ClockAngel { get; set; } = -1;
        public double AntiClockAngel { get; set; } = -1;

        public double FluidDensity
        {
            get { return fluiddensity; }
            set { fluiddensity = value; }
        }

        public double Longitude
        {
            get { return mdbllongitude; }
            set { mdbllongitude = value; }
        }

        public double Latitude
        {
            get { return mdbllatitude; }
            set { mdbllatitude = value; }
        }

        public bool GPSValid
        {
            get { return mblngpsvalid; }
            set { mblngpsvalid = value; }

        }

        public WayPoint SelectedMarker
        {
            get { return waypoint; }
            set { waypoint = value; }
        }

        public double Distance
        {
            get { return mdblDistance; }
            set { mdblDistance = value; }
        }

        public double Bearing
        {
            get { return mdblBearing; }
            set { mdblBearing = value; }
        }

        public double DepthZero
        {
            get { return mdblDepthZero; }
            set { mdblDepthZero = value; }
        }

        public bool DepthZeroSwitch
        {
            get { return mblnDepthZeroSwitch; }
            set { mblnDepthZeroSwitch = value; }
        }

        public double HeadingZero
        {
            get { return mdblHeadingZero; }
            set { mdblHeadingZero = value; }
        }

        public bool HeadingZeroSwitch
        {
            get { return mblnHeadingZeroSwitch; }
            set { mblnHeadingZeroSwitch = value; }
        }

        public int TxPacks
        {
            get { return txpacks; }
            set { txpacks = value; }
        }

        public int RxPacks
        {
            get { return rxpacks; }
            set { rxpacks = value; }
        }

        public bool GYROStabThruster
        {
            get { return mblnGYROStabThruster; }
            set
            {
                mblnGYROStabThruster = value;
            }
        }

        public double TurnHeading1
        {
            get { return mdblTurnHeading1; }
            set { mdblTurnHeading1 = value; }
        }

        public double TurnHeading
        {
            get { return mdblTurnHeading; }
            set
            {
                mdblTurnHeading = value;
            }
        }

        public double TurnCount
        {
            get { return mdblTurnCount; }
            set { mdblTurnCount = value; }
        }

        public Dircetion TurnDirection
        {
            get { return turndirection; }
            set { turndirection = value; }
        }

        public NavType NavigationType
        {
            get { return navtype; }
            set { navtype = value; }
        }

        public Navigation(char _devid)
        {
            this.devid = _devid;
            mdbllatitude = Convert.ToDouble(SelectXMLData.GetConfiguration("DefaultLat", "value"));
            mdbllongitude = Convert.ToDouble(SelectXMLData.GetConfiguration("DefaultLng", "value"));
        }

        public Navigation(char _devid, ref SerialSendData _sp) // 
        {
            this.devid = _devid;
            serialport1 = _sp;
            mdbllatitude = Convert.ToDouble(SelectXMLData.GetConfiguration("DefaultLat", "value"));
            mdbllongitude = Convert.ToDouble(SelectXMLData.GetConfiguration("DefaultLng", "value"));
        }

        private double GetTurnHeading()
        {
            return mdblTurnHeading;
        }

        public void ResetTurnHeading()
        {
            Start_Angel = -1;
            Current_Angel = -1;
            Last_Angel = -1;
            TurnHeading = 0;
        }

        public void ResetTurnHeading1()
        {
            Start_Angel1 = -1;
            Current_Angel1 = -1;
            Last_Angel1 = -1;
            TurnCount = 0;
            TurnHeading1 = 0;
        }

        public void ResetHeading()
        {
            mdblheading = 0.0;
            mdblpitch = 0.0;
            mdblroll = 0.0;
            mdbldepth = 0.0;
            mdbltargetheading = 0.0;
            mdbltargetdepth = 0.0;
            mdblgyro = 520.0;
        }

        public void SetTrueHeading(double _heading)
        {
            if (Start_Angel1 == -1 && Current_Angel1 == -1 && Last_Angel1 == -1 && _heading != 0) //Init the heading position
            {
                Current_Angel1 = 0;
                Last_Angel1 = Start_Angel1 = _heading;
            }
            Get_Rotation1(_heading);
            if (Current_Angel1 > 720)
            {
                TurnDirection = Navigation.Dircetion.Zheng;
                TurnCount = Math.Floor(Current_Angel1 / 360);
                //lblTurnDirection.Text = "顺时针";
                //lblTurnsCountInfo.Text = ">2";
            }
            else if (Current_Angel1 < -720)
            {
                TurnDirection = Navigation.Dircetion.Fan;
                TurnCount = Math.Floor(Math.Abs(Current_Angel) / 360) * -1;
                //lblTurnDirection.Text = "逆时针";
                //lblTurnsCountInfo.Text = ">-2";
            }
            else
            {
                if (Current_Angel1 > 0)
                {
                    //lblTurnDirection.Text = "顺时针";
                    TurnDirection = Navigation.Dircetion.Zheng;
                    TurnCount = Math.Floor(Current_Angel1 / 360);
                    //lblTurnsCountInfo.Text = Math.Floor(Current_Angel / 360).ToString();
                }
                if (Current_Angel1 < 0)
                {
                    //lblTurnDirection.Text = "逆时针";
                    TurnDirection = Navigation.Dircetion.Fan;
                    TurnCount = Math.Floor(Math.Abs(Current_Angel1) / 360) * -1;
                    //lblTurnsCountInfo.Text = (Math.Floor(Math.Abs(Current_Angel) / 360) * -1).ToString();
                }
                if (Current_Angel == 0)
                {
                    //lblTurnDirection.Text = "无";
                    TurnDirection = Navigation.Dircetion.Stop;
                    TurnCount = 0;
                    //lblTurnsCountInfo.Text = "0";
                }
            }
            //lblTurnCountsAngel.Text = string.Format("{0:0}", Current_Angel);

            //if (Math.Abs(PreviousTurnHeading - Current_Angel) > 30) return;
            TurnHeading1 = Current_Angel1;
            mdblheading = _heading;
        }

        public void SetHeading(double _heading)
        {
            if (Start_Angel == -1 && Current_Angel == -1 && Last_Angel == -1 && _heading != 0) //Init the heading position
            {
                Current_Angel = 0;
                Last_Angel = Start_Angel = _heading;
            }

            Get_Rotation(_heading);
            //lblTurnCountsAngel.Text = string.Format("{0:0}", Current_Angel);

            //if (Math.Abs(PreviousTurnHeading - Current_Angel) > 30) return;
            TurnHeading = Current_Angel;
            mdblheading = _heading;
        }

        private void Get_Rotation(double Current_Pos)
        {
            // will take the current reading,
            // compute the Current_Angle,
            // then update Last_Angle, and Current_Angle
            double Delta;

            // compute delta angle
            Delta = Current_Pos - Last_Angel;

            // put Delta into range of
            // -180 <= angle <= 180
            if (Delta > 180) Delta -= 360;
            if (Delta < -180) Delta += 360;

            // update Current_Angle
            Current_Angel += Delta;

            // update last reading with our current reading
            Last_Angel = Current_Pos;
        }

        private void Get_Rotation1(double Current_Pos)
        {
            // will take the current reading,
            // compute the Current_Angle,
            // then update Last_Angle, and Current_Angle
            double Delta;

            // compute delta angle
            Delta = Current_Pos - Last_Angel1;

            // put Delta into range of
            // -180 <= angle <= 180
            if (Delta > 180) Delta -= 360;
            if (Delta < -180) Delta += 360;

            // update Current_Angle
            Current_Angel1 += Delta;

            // update last reading with our current reading
            Last_Angel1 = Current_Pos;
        }

        public double GetHeading()
        {
            return mdblheading;
        }

        private double GetReviseHeading()
        {
            double dblHeading = 0.0, dblHeadingPrimary = 0.0;
            dblHeading = mdblheading;//get currentHeading
            dblHeadingPrimary = dblHeading;
            dblHeading = mdbltargetheading - dblHeading;

            if (dblHeading > 180)
                dblHeading = dblHeading - 360;
            else if (dblHeading < -180)
                dblHeading = dblHeading + 360;

            return (mdbltargetheading - dblHeading);
        }

        public double GetTargetHeading()
        {
            return mdbltargetheading;
        }

        public double GetTargetDepth()
        {
            return mdbltargetdepth;
        }

        public void SetPitch(double _pitch)
        {
            mdblpitch = _pitch;
        }

        public double GetPitch()
        {
            return mdblpitch;
        }

        public void SetRoll(double _roll)
        {
            mdblroll = _roll;
        }

        public double GetRoll()
        {
            return mdblroll;
        }

        public void SetDepth(double _depth)
        {
            mdbldepth = _depth;
        }

        public double GetDepth()
        {
            return mdbldepth;
        }

        public void SetGyro(double _gyro)
        {
            mdblgyro = _gyro;
        }

        public double GetGyro()
        {
            return mdblgyro;
        }

        public void EnableCompassOutput()
        {
            //serialport1.SendMessage(SerialSendData.DataType.Nav, '1', 'o', "1", 2);
        }

        public void DisableCompassOutput()
        {
            //serialport1.SendMessage(SerialSendData.DataType.Nav, '1', 'o', "0", 2);
        }

        public void InquiryCompassOutput()
        {
            //serialport1.SendMessage(SerialSendData.DataType.Nav, '1', '?', "o", 2);
        }

        public void StartCompassCalib()
        {
            //serialport1.SendMessage(SerialSendData.DataType.Nav, '1', 'r', "0", 2);
        }

        public void SaveCompassCalib()
        {
            //serialport1.SendMessage(SerialSendData.DataType.Nav, '1', 'r', "1", 2);
        }

        public bool IsInstalled()
        {
            return installed;
        }

        public void SetInstall(bool _install)
        {
            installed = _install;
        }

        public void SetNoResponseCounter()
        {
            noresponsecounter++;
        }

        public void SetNoResponseCounter(int _counter)
        {
            noresponsecounter = _counter;
        }

        public int GetNoResponseCounter()
        {
            return noresponsecounter;
        }

        public void InquiryData(char _id)
        {
            if (serialport1 != null)
            {
                if (NavigationType == NavType.Self)
                {
                    switch (_id)
                    {
                        case 'l':
                            serialport1.SendMessage(SerialSendData.DataType.Nav, devid, '?', "l", 1);
                            break;
                        case 'a':
                            serialport1.SendMessage(SerialSendData.DataType.Nav, devid, '?', "a", 1);
                            break;
                        case 'd':
                            serialport1.SendMessage(SerialSendData.DataType.Nav, devid, '?', "d", 1);
                            break;
                        case 'p':
                            serialport1.SendMessage(SerialSendData.DataType.Nav, devid, '?', "p", 1);
                            break;
                        case 'r':
                            serialport1.SendMessage(SerialSendData.DataType.Nav, devid, '?', "r", 1);
                            break;
                        case 'g':
                            serialport1.SendMessage(SerialSendData.DataType.Nav, devid, '?', "z", 1);
                            break;
                    }
                }
                if (NavigationType == NavType.AH500)
                {
                    byte[] cmds = new byte[5];
                    switch (_id)
                    {
                        case 'l':
                            cmds[0] = 0x77;
                            cmds[1] = 0x04;
                            cmds[2] = 0x00;
                            cmds[3] = 0x04;
                            cmds[4] = 0x08;
                            serialport1.SendMessage(cmds);
                            break;
                    }
                }
                if (NavigationType == NavType.DCM250B)
                {
                    byte[] cmds = new byte[5];
                    switch (_id)
                    {
                        case 'l':
                            cmds[0] = 0x68;
                            cmds[1] = 0x04;
                            cmds[2] = 0x00;
                            cmds[3] = 0x04;
                            cmds[4] = 0x08;
                            serialport1.SendMessage(cmds);
                            break;
                    }
                }
            }
        }


        public bool IsInstalledSuccessfully()
        {
            return installedsuccessfully;
        }

        public void InstalledSuccessfully(bool _install)
        {
            installedsuccessfully = _install;
        }

    }
}
