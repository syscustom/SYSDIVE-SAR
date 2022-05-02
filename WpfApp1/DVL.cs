using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ROV.Serial;


namespace WpfApp1
{
    public class DVL
    {
        private char devid = '0';
        private SerialSendData serialport1;

        double mdbltime = 0.0;
        double mdblvx = 0.0;
        double mdblvy = 0.0;
        double mdblvz = 0.0;
        double mdblfom = 0.0;
        double mdblaltitude = 0.0;
        bool mblnvalid = false;
        int mintstatus = 0;
        double mdbllatitude = 0.0; //39.056531162330714;
        double mdbllongitude = 0.0; //117.06059910052493;
        bool mblnsatellitefix = false;


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

        public DVL(char _devid, ref SerialSendData _sp)
        {
            this.devid = _devid;
            serialport1 = _sp;
        }

        public bool satellitefix  //If valid is "y" the DVL has lock on the bottom and the altitude and velocities are valid (y/n)
        {
            get { return mblnsatellitefix; }
            set { mblnsatellitefix = value; }
        }


    }
}
