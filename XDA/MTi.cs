using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using XDA;

namespace Xsens_Device_API
{

    public class DataAvailableArgs : EventArgs
    {
        public XsDevice Device { get; private set; }
        public XsDataPacket Packet { get; private set; }
        public DataAvailableArgs(XsDevice device, XsDataPacket packet)
        {
            Device = device;
            Packet = packet;
        }
    }

    internal class CallbackHandler : XsCallback
    {
        private uint _maxNumberOfPacketsInBuffer;
        private uint _numberOfPacketsInBuffer;
        private Queue<XsDataPacket> _packetBuffer;
        public event EventHandler<DataAvailableArgs> DataAvailable;
        private object _lockThis;

        private uint MaxNumberOfPacketsInBuffer
        {
            get { return _maxNumberOfPacketsInBuffer; }
            set { _maxNumberOfPacketsInBuffer = value; }
        }

        private uint NumberOfPacketsInBuffer
        {
            get { return _numberOfPacketsInBuffer; }
            set { _numberOfPacketsInBuffer = value; }
        }

        private Queue<XsDataPacket> PacketBuffer
        {
            get { return _packetBuffer; }
            set { _packetBuffer = value; }
        }

        public CallbackHandler(uint maxBufferSize = 5)
            : base()
        {
            MaxNumberOfPacketsInBuffer = maxBufferSize;
            NumberOfPacketsInBuffer = 0;
            PacketBuffer = new Queue<XsDataPacket>();
            _lockThis = new object();
        }

        public bool packetAvailable()
        {
            lock (_lockThis)
                return NumberOfPacketsInBuffer > 0;
        }

        public XsDataPacket getNextPacket()
        {
            lock (_lockThis)
            {
                XsDataPacket oldestPacket = PacketBuffer.Peek();
                PacketBuffer.Dequeue();
                --NumberOfPacketsInBuffer;
                return oldestPacket;
            }
        }

        protected override void onLiveDataAvailable(XsDevice dev, XsDataPacket packet)
        {
            lock (_lockThis)
            {
                while (NumberOfPacketsInBuffer >= MaxNumberOfPacketsInBuffer)
                    getNextPacket();

                PacketBuffer.Enqueue(new XsDataPacket(packet));
                ++NumberOfPacketsInBuffer;
            }
        }
    }

    public class MTi
    {
        private XsControl control;
        private Thread threadCollecting;
        CallbackHandler callback;
        XsDevice device;
        XsPortInfo mtPort;

        private double mdblYaw = 0.0;
        private double mdblRoll = 0.0;
        private double mdblPitch = 0.0;

        public void ClosePort()
        {
            if (threadCollecting != null) threadCollecting.Abort();
            control.closePort(mtPort.portName());
            control.close();
        }

        public void OpenPort(string port)
        {
            control = new XsControl();
            try
            {
                mtPort = new XsPortInfo();
                XsString xsString = new XsString(port);

                mtPort.setPortName(xsString);
                mtPort.setBaudrate(XsBaudRate.XBR_115k2);
                if (mtPort.empty())
                    return;
                if (!control.openPort(mtPort.portName(), mtPort.baudrate()))
                    return;

                // Get the device object
                XsDevice tempDevice = control.device(mtPort.deviceId());
                Debug.Assert(tempDevice != null);
                device = new XsDevice(tempDevice);

                // Create and attach callback handler to device
                callback = new CallbackHandler();


                device.addCallbackHandler(callback);

                if (!device.gotoConfig())
                    return;
                XsOutputConfigurationArray configArray = new XsOutputConfigurationArray();
                configArray.push_back(new XsOutputConfiguration(XsDataIdentifier.XDI_PacketCounter, 0));
                configArray.push_back(new XsOutputConfiguration(XsDataIdentifier.XDI_SampleTimeFine, 0));

                if (device.deviceId().isImu())
                {
                    configArray.push_back(new XsOutputConfiguration(XsDataIdentifier.XDI_Acceleration, 100));
                    configArray.push_back(new XsOutputConfiguration(XsDataIdentifier.XDI_RateOfTurn, 100));
                    configArray.push_back(new XsOutputConfiguration(XsDataIdentifier.XDI_MagneticField, 100));
                }
                else if (device.deviceId().isVru() || device.deviceId().isAhrs())
                {
                    configArray.push_back(new XsOutputConfiguration(XsDataIdentifier.XDI_Quaternion, 100));
                }
                else if (device.deviceId().isGnss())
                {
                    configArray.push_back(new XsOutputConfiguration(XsDataIdentifier.XDI_Quaternion, 100));
                    configArray.push_back(new XsOutputConfiguration(XsDataIdentifier.XDI_LatLon, 100));
                    configArray.push_back(new XsOutputConfiguration(XsDataIdentifier.XDI_AltitudeEllipsoid, 100));
                    configArray.push_back(new XsOutputConfiguration(XsDataIdentifier.XDI_VelocityXYZ, 100));
                }

                if (!device.setOutputConfiguration(configArray))
                    return;
                if (!device.gotoMeasurement())
                    return;
            }
            catch {}

            threadCollecting = new Thread(new ThreadStart(StartCollecting));
            threadCollecting.Start();
        }

        public void StartCollecting() //检测发送队列并发送
        {
           while (true)
            {
                Thread.Sleep(50);
                if (callback.packetAvailable())
                {
                    // Retrieve a packet
                    XsDataPacket packet = callback.getNextPacket();

                    if (packet.containsCalibratedData())
                    {
                        XsVector acc = packet.calibratedAcceleration();
                        //Console.Write("\rAcc X:{0,-5:f2}, Acc Y:{1,-5:f2}, Acc Z:{2,-5:f2}", acc.value(0), acc.value(1), acc.value(2));

                        XsVector gyr = packet.calibratedGyroscopeData();
                        //Console.Write(" |Gyr X:{0,-5:f2}, Gyr Y:{1,-5:f2}, Gyr Z:{2,-5:f2}", gyr.value(0), gyr.value(1), gyr.value(2));

                        XsVector mag = packet.calibratedMagneticField();
                        //Console.Write(" |Mag X:{0,-5:f2}, Mag Y:{1,-5:f2}, Mag Z:{2,-5:f2}", mag.value(0), mag.value(1), mag.value(2));
                    }

                    if (packet.containsOrientation())
                    {

                        XsQuaternion quaternion = packet.orientationQuaternion();
                        //Console.Write("\rq0:{0,-5:f2}, q1:{1,-5:f2}, q2:{2,-5:f2}, q3:{3,-5:f2}", quaternion.w(), quaternion.x(), quaternion.y(), quaternion.z());

                        XsEuler euler = packet.orientationEuler(XsDataIdentifier.XDI_CoordSysNwu);
                        //Console.Write(" |Roll:{0,-5:f2}, Pitch:{1,-5:f2}, Yaw:{2,-5:f2}", euler.roll(), euler.pitch(), euler.yaw());

                        double tempyaw = euler.yaw();
                        tempyaw = ((tempyaw * -1) + 360) % 360; //+180 -180 to 0 - 360 
                        mdblYaw = tempyaw;
                        mdblYaw = Math.Round(mdblYaw, 1);

                        mdblRoll = euler.roll();
                        mdblPitch = euler.pitch();
                    }

                    /*
                    if (packet.containsLatitudeLongitude())
                    {
                        XsVector latLon = packet.latitudeLongitude();
                        //Console.Write(" |Lat:{0,-5:f2}, Lon:{1,-5:f2}", latLon.value(0), latLon.value(1));
                    }

                    if (packet.containsAltitude())
                        Console.Write(" |Alt:{0,-5:f2}", packet.altitude());

                    if (packet.containsVelocity())
                    {
                        XsVector vel = packet.velocity(XsDataIdentifier.XDI_CoordSysEnu);
                        Console.Write(" |E:{0,-5:f2}, N:{1,-5:f2}, U:{2,-5:f2}", vel.value(0), vel.value(1), vel.value(2));
                    }
                    */

                    packet.Dispose();
                }
          }
        }

        public double Roll
        {
            get { return mdblRoll; }
            set { mdblRoll = value; }
        }
        public double Pitch
        {
            get { return mdblPitch; }
            set { mdblPitch = value; }
        }

        public double Yaw
        {
            get { return mdblYaw; }
            set { mdblYaw = value; }
        }
    }
}
