using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ROV.Serial;


namespace WpfApp1
{
    public enum DVLType
    {
        Serial = 0,
        Network = 1
    }

    public class DVLStatus
    {
        public DVLType dvltype { get; set; } = DVLType.Serial;
        public bool connection { get; set; } = false;
        public bool satellitefix { get; set; } = false;
        public double Longitude { get; set; } = 0; //39.056531162330714;
        public double Latitude { get; set; } = 0; //117.06059910052493;
        public double UTMEasting { get; set; } = 0;
        public double UTMNorthing { get; set; } = 0;
        public string UTMZone { get; set; } = "";
        public double HeadingDistance { get; set; } = 0;

        public DVLStatus()
        {
            Latitude = Convert.ToDouble(SelectXMLData.GetConfiguration("DefaultLat", "value"));
            Longitude = Convert.ToDouble(SelectXMLData.GetConfiguration("DefaultLng", "value"));
        }
    }

    public class RecordDVL
    {
        public double time { get; set; } = 0.0;
        public double vx { get; set; } = 0.0;
        public double vy { get; set; } = 0.0;
        public double vz { get; set; } = 0.0;
        public double heading { get; set; } = 0.0;
    }

    public class DVL
    {
        private char devid = '0';
        private SerialSendData serialport1;

        double mdbltime = 0.0;
        double mdblvx = 0.0;
        double mdblvy = 0.0;
        double mdblvz = 0.0;
        double mdblhesudu = 0.0;
        double mdblfom = 0.0;
        double mdblaltitude = 0.0;
        bool mblnvalid = false;
        int mintstatus = 0;
        bool mblnisCalcDeadReckoned = true;
        bool mblnsatellitefix = false;

        public double time  //Milliseconds since last velocity report (ms)
        {
            get { return mdbltime; }
            set { mdbltime = value; }
        }

        public double vx  //Measured velocity in x direction (m/s)
        {
            get { return mdblvx; }
            set { mdblvx = value; }
        }

        public double vy  //Measured velocity in y direction (m/s)
        {
            get { return mdblvy; }
            set { mdblvy = value; }
        }

        public double vz  //Measured velocity in z direction (m/s)
        {
            get { return mdblvz; }
            set { mdblvz = value; }
        }

        public double vhesudu  //Measured velocity in z direction (m/s)
        {
            get { return mdblhesudu; }
            set { mdblhesudu = value; }
        }

        public double fom  //Figure of merit, a measure of the accuracy of the measured velocities (m/s)
        {
            get { return mdblfom; }
            set { mdblfom = value; }
        }

        public double altitude  //Measured altitude to the bottom (m)
        {
            get { return mdblaltitude; }
            set { mdblaltitude = value; }
        }

        public bool valid  //If valid is "y" the DVL has lock on the bottom and the altitude and velocities are valid (y/n)
        {
            get { return mblnvalid; }
            set { mblnvalid = value; }
        }

        public int status  //0 for normal operation, 1 for high temperature warning
        {
            get { return mintstatus; }
            set { mintstatus = value; }
        }

        public bool IsCalcDeadReckoned  //0 for normal operation, 1 for high temperature warning
        {
            get { return mblnisCalcDeadReckoned; }
            set { mblnisCalcDeadReckoned = value; }
        }

        public DVL(char _devid, ref SerialSendData _sp)
        {
            this.devid = _devid;
            serialport1 = _sp;
        }
    }

    public class DVLWRZ
    {

        public double time { get; set; }
        public double vx { get; set; }
        public double vy { get; set; }
        public double vz { get; set; }
        public double fom { get; set; }
        //public string covariance { get; set; }
        public double altitude { get; set; }
        //public Transducers transducers { get; set; }
        public bool velocity_valid { get; set; }
        public int status { get; set; }
        public string format { get; set; }
        public string type { get; set; }
        public string time_of_validity { get; set; }
        public string time_of_transmission { get; set; }
    }

    public class DVLWRP
    {
        public double ts { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
        public double std { get; set; }
        public double roll { get; set; }
        public double pitch { get; set; }
        public double yaw { get; set; }
        public string type { get; set; }
        public int status { get; set; }
        public string format { get; set; }
    }

    public class Transducers
    {
        public int id { get; set; }
        public double velocity { get; set; }
        public double distance { get; set; }
        public double rssi { get; set; }
        public double nsd { get; set; }
        public bool beam_valid { get; set; }
    }
}
