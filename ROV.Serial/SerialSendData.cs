using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Collections;
using System.Threading;


namespace ROV.Serial
{
    public enum CRC8_POLY
    {
        CRC8 = 0xd5,
        CRC8_CCITT = 0x07,
        CRC8_DALLAS_MAXIM = 0x31,
        CRC8_SAE_J1850 = 0x1D,
        CRC_8_WCDMA = 0x9b,
    };

    /// 
    /// Class for calculating CRC8 checksums...
    /// 
    public class CRC8Calc
    {
        private byte[] table = new byte[256];

        public byte Checksum(params byte[] val)
        {
            if (val == null)
                throw new ArgumentNullException("val");

            byte c = 0;

            foreach (byte b in val)
            {
                c = table[c ^ b];
            }

            return c;
        }

        public byte[] Table
        {
            get
            {
                return this.table;
            }
            set
            {
                this.table = value;
            }
        }

        public byte[] GenerateTable(CRC8_POLY polynomial)
        {
            byte[] csTable = new byte[256];

            for (int i = 0; i < 256; ++i)
            {
                int curr = i;

                for (int j = 0; j < 8; ++j)
                {
                    if ((curr & 0x80) != 0)
                    {
                        curr = (curr << 1) ^ (int)polynomial;
                    }
                    else
                    {
                        curr <<= 1;
                    }
                }

                csTable[i] = (byte)curr;
            }

            return csTable;
        }

        public CRC8Calc(CRC8_POLY polynomial)
        {
            this.table = this.GenerateTable(polynomial);
        }
    }


    class SerialCMD : IComparable
    {
        public char[] Cmds { get; set; }
        public int Priority { get; set; }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            SerialCMD serialcmd = obj as SerialCMD;
            return Priority.CompareTo(serialcmd.Priority);
        }
        #endregion
    }

    public class SerialSendData
    {
        private enum DataReceiveStatus
        {
            Head1 = 0,
            Head2 = 1,
            Command1 = 2,
            Command2 = 3,
            DataLen = 4,
            HeaderCRC = 5,
            Data = 6,
            DataCRC1 = 7,
            DataCRC2 = 8,
            End1 = 9,
            End2 = 10,
            None = 11
        }

        private enum AH500DataReceiveStatus
        {
            Head = 0,
            DataLen = 1,
            Address = 2,
            Data = 3,
            DataCRC = 4,
            None = 5
        }

        private enum DCM250BDataReceiveStatus
        {
            Head = 0,
            DataLen = 1,
            Address = 2,
            Command = 3,
            Data = 4,
            DataCRC = 5,
            None = 6
        }

        public enum DataType
        {
            Motor = 0,
            Lights = 1,
            Tilt = 2,
            Nav = 3,
            JB = 4,
            SingleManip = 5,
            Altimeter = 6,
            LinearActuator = 7,
            ExternalBoard = 8,
            AH500 = 9
        }

        public enum SerialType
        {
            NavTelemetry = 0,
            GPSPORT = 1,
            EXTPORT = 2,
            AUXPORT = 3,
            BATPORT = 4,
            DVLPORT = 5,
            None = 6,
            AH500Telemetry = 7,
            DCM250BTelemetry = 8
        }

        private int WaitingACKTimeOut = 10; //等待ACK超时时间
        private int BaseWaitingACKTimeOut = 10; //等待ACK超时时间

        private SerialPort serialPort1;
        private Thread threadSending, threadReceiving;

        //private bool mblnWaitingACK = false;
        private ArrayList malCmdQueue = new ArrayList();
        private ArrayList malCmdQueueWithPriority = new ArrayList();
        private ArrayList malCmdQueueWithThruster = new ArrayList();
        private ArrayList malrxcurrent = new ArrayList();
        private ArrayList maltxcurrent = new ArrayList();

        private SerialPort comm = new SerialPort();
        private StringBuilder builder = new StringBuilder();//避免在事件处理方法中反复的创建，定义到外面。  

        private SerialType serialtype = SerialType.None;

        private char[] m_current = new char[4] { 'u', '1', '+', '0' };

        private int mintWaitingThrusterACKTimeOut = 0;

        public enum SerialLineWaiting
        {
            Motor = 0,
            Sensor = 1,
            Free = 2
        };

        private SerialLineWaiting mblnWaitingACK = SerialLineWaiting.Free;

        public delegate void UserRequest(object sender, ReceivedEventArgs e);
        public delegate void UserLogRequest(object sender, LogReceivedEventArgs e);
        public delegate void ByteUserRequest(object sender, ByteReceivedEventArgs e);

        public event UserRequest OnDataReceived;
        public event ByteUserRequest OnByteDataReceived;
        public event UserRequest OnSuccessfulDataReceived;
        public event ByteUserRequest OnByteSuccessfulDataReceived;
        public event UserRequest OnNavDataReceived;
        public event ByteUserRequest OnAH500NavDataReceived;
        public event ByteUserRequest OnDCM250BNavDataReceived;
        public event UserRequest OnAltimeterDataReceived;
        public event UserLogRequest OnDataLogReceived;
        public event UserRequest OnJunctionBoardDataReceived;
        public event UserRequest OnTiltDataReceived;
        public event UserRequest OnLinearDataReceived;
        public event UserRequest OnMotorDataReceived;
        public event UserRequest OnCMDConfirmationReceived;

        DataReceiveStatus drs;
        AH500DataReceiveStatus ah500drs;
        DCM250BDataReceiveStatus dcm250bdrs;

        ArrayList messagedata = new ArrayList();
        byte messagecmd1 = 0;
        byte messagecmd2 = 0;
        byte[] messageend = new byte[2];

        byte messagelength = 0;
        byte messageaddress = 0;
        byte messagecommand = 0;
        byte messagelengthcounter = 0;

        public int WaitingThrusterACKTimeOut
        {
            get { return mintWaitingThrusterACKTimeOut; }
            set { mintWaitingThrusterACKTimeOut = value; }
        }

        public SerialSendData()
        {
            char[] emptycmds = new char[2] { '0', '0' };
            serialPort1 = new System.IO.Ports.SerialPort();
            if (serialPort1.IsOpen == false)
            {
                serialPort1.ReadTimeout = 5;
                serialPort1.ReadBufferSize = 1024;
                serialPort1.DiscardNull = false;
                serialPort1.WriteBufferSize = 512;
                serialPort1.BaudRate = 57600;
                serialPort1.ReceivedBytesThreshold = 1024;
                serialPort1.NewLine = "\r";
            }
            malCmdQueueWithThruster.Add(emptycmds);
            malCmdQueueWithThruster.Add(emptycmds);
            malCmdQueueWithThruster.Add(emptycmds);
            malCmdQueueWithThruster.Add(emptycmds);
            malCmdQueueWithThruster.Add(emptycmds);
            malCmdQueueWithThruster.Add(emptycmds);
        }

        public void SetWaitingACKTimeOut(int _timeout)
        {
            BaseWaitingACKTimeOut = _timeout;
            WaitingACKTimeOut = _timeout;
        }

        public void StartSending() //检测发送队列并发送
        {
            int CmdQueueWithThrusterIndex = 0;
            while (true)
            {
                Thread.Sleep(1);
                switch (serialtype)
                {
                    case SerialType.NavTelemetry:

                        if (malCmdQueueWithPriority.Count > 50) malCmdQueueWithPriority.Clear();
                        if (malCmdQueueWithThruster.Count > 50) malCmdQueueWithThruster.Clear();
                        if (maltxcurrent.Count > 50) maltxcurrent.Clear();


                        if (malCmdQueueWithPriority.Count > 0 && mblnWaitingACK == SerialLineWaiting.Free)
                        {
                            char[] cmds;
                            //malCmdQueueWithPriority.Sort();
                            SerialCMD serialcmd = (SerialCMD)malCmdQueueWithPriority[0];
                            if (serialcmd == null)
                            {
                                malCmdQueueWithPriority.RemoveAt(0);
                                break;
                            }
                            cmds = serialcmd.Cmds;
                            malCmdQueueWithPriority.RemoveAt(0);
                            m_current[0] = cmds[0];
                            m_current[1] = cmds[1];
                            m_current[2] = cmds[3];
                            m_current[3] = cmds[4];
                            maltxcurrent.Add(m_current);

                            WaitingACKTimeOut = BaseWaitingACKTimeOut;

                            //serialPort1.DiscardInBuffer();
                            if (serialPort1.IsOpen == true)
                                serialPort1.Write(cmds, 0, cmds.Length);

                            mblnWaitingACK = SerialLineWaiting.Sensor;
                        }
                        break;
                    case SerialType.EXTPORT:
                        if (malCmdQueue.Count > 200) malCmdQueue.Clear();
                        if (maltxcurrent.Count > 50) maltxcurrent.Clear();
                        if (malCmdQueue.Count > 0)
                        {
                            char[] cmds;
                            cmds = (char[])malCmdQueue[0];
                            malCmdQueue.RemoveAt(0);
                            m_current[0] = cmds[0];
                            m_current[1] = cmds[1];
                            m_current[2] = cmds[3];
                            m_current[3] = cmds[4];
                            maltxcurrent.Add(m_current);
                            serialPort1.Write(cmds, 0, cmds.Length);
                        }
                        break;
                    case SerialType.BATPORT:
                        if (malCmdQueue.Count > 200) malCmdQueue.Clear();
                        if (malCmdQueue.Count > 0)
                        {
                            byte[] cmds;
                            cmds = (byte[])malCmdQueue[0];
                            malCmdQueue.RemoveAt(0);
                            serialPort1.Write(cmds, 0, cmds.Length);
                        }
                        break;
                    case SerialType.AH500Telemetry:
                        if (malCmdQueue.Count > 200) malCmdQueue.Clear();
                        if (malCmdQueue.Count > 0)
                        {
                            byte[] cmds;
                            cmds = (byte[])malCmdQueue[0];
                            malCmdQueue.RemoveAt(0);
                            serialPort1.Write(cmds, 0, cmds.Length);
                        }
                        break;
                    case SerialType.DCM250BTelemetry:
                        if (malCmdQueue.Count > 200) malCmdQueue.Clear();
                        if (malCmdQueue.Count > 0)
                        {
                            byte[] cmds;
                            cmds = (byte[])malCmdQueue[0];
                            malCmdQueue.RemoveAt(0);
                            serialPort1.Write(cmds, 0, cmds.Length);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void StartReceiving() //接收队列 
        {
            string ackmessage = "";
            byte[] Buff = new byte[20];
            byte[] temp = new byte[20];
            int index = 0, bytestoread = 0;
            int timeoutcounter = 0;
            char[] cmds;

            while (true)
            {
                Thread.Sleep(1);
                switch (serialtype)
                {
                    case SerialType.DCM250BTelemetry:
                        if (serialPort1.BytesToRead > 0)
                        {
                            byte[] receivedbyte = new byte[serialPort1.BytesToRead];
                            serialPort1.Read(receivedbyte, 0, receivedbyte.Length);
                            for (int i = 0; i < receivedbyte.Length; i++)
                            {
                                switch (dcm250bdrs)
                                {
                                    case DCM250BDataReceiveStatus.Head:
                                        if (receivedbyte[i] == 0x68)
                                        {
                                            dcm250bdrs = DCM250BDataReceiveStatus.DataLen;
                                            messagelengthcounter = 0;
                                        }
                                        break;
                                    case DCM250BDataReceiveStatus.DataLen:
                                        messagelength = receivedbyte[i];
                                        messagelengthcounter++;
                                        dcm250bdrs = DCM250BDataReceiveStatus.Address;
                                        break;
                                    case DCM250BDataReceiveStatus.Address:
                                        messageaddress = receivedbyte[i];
                                        messagelengthcounter++;
                                        dcm250bdrs = DCM250BDataReceiveStatus.Data;
                                        break;
                                    case DCM250BDataReceiveStatus.Data:
                                        if (messagelengthcounter < (messagelength - 1))
                                        {
                                            messagedata.Add(receivedbyte[i]);
                                            messagelengthcounter++;
                                        }
                                        if (messagelengthcounter >= (messagelength - 1))
                                        {
                                            messagelengthcounter = 0;
                                            dcm250bdrs = DCM250BDataReceiveStatus.DataCRC;
                                        }
                                        break;
                                    case DCM250BDataReceiveStatus.DataCRC:
                                        byte dcm250bbytescrc = receivedbyte[i];
                                        ArrayList crcmessagedata = new ArrayList();
                                        crcmessagedata.Add(messagelength);
                                        crcmessagedata.Add(messageaddress);
                                        for (int j = 0; j < messagedata.Count; j++)
                                        {
                                            crcmessagedata.Add(messagedata[j]);
                                        }

                                        byte[] crcmessagedatabyte = (byte[])crcmessagedata.ToArray(typeof(byte));

                                        byte dcm250bbytescrcresult = 0;

                                        unchecked // Let overflow occur without exceptions
                                        {
                                            foreach (byte b in crcmessagedatabyte)
                                            {
                                                dcm250bbytescrcresult += b;
                                            }
                                        }

                                        //byte ah500bytescrcresult = ah500crc.Checksum(crcmessagedatabyte);                         

                                        if (dcm250bbytescrcresult == dcm250bbytescrc) //CRC校验成功
                                        {
                                            OnDCM250BNavDataReceived(this, new ByteReceivedEventArgs((byte[])messagedata.ToArray(typeof(byte))));
                                        }
                                        messagedata.Clear();
                                        dcm250bdrs = DCM250BDataReceiveStatus.Head;
                                        break;
                                    default:
                                        break;
                                }

                            }
                        }
                        break;
                    case SerialType.AH500Telemetry:
                        if (serialPort1.BytesToRead > 0)
                        {
                            byte[] receivedbyte = new byte[serialPort1.BytesToRead];
                            serialPort1.Read(receivedbyte, 0, receivedbyte.Length);
                            for (int i = 0; i < receivedbyte.Length; i++)
                            {
                                switch (ah500drs)
                                {
                                    case AH500DataReceiveStatus.Head:
                                        if (receivedbyte[i] == 0x77)
                                        {
                                            ah500drs = AH500DataReceiveStatus.DataLen;
                                            messagelengthcounter = 0;
                                        }
                                        break;
                                    case AH500DataReceiveStatus.DataLen:
                                        messagelength = receivedbyte[i];
                                        messagelengthcounter++;
                                        ah500drs = AH500DataReceiveStatus.Address;
                                        break;
                                    case AH500DataReceiveStatus.Address:
                                        messageaddress = receivedbyte[i];
                                        messagelengthcounter++;
                                        ah500drs = AH500DataReceiveStatus.Data;
                                        break;
                                    case AH500DataReceiveStatus.Data:
                                        if (messagelengthcounter < (messagelength - 1))
                                        {
                                            messagedata.Add(receivedbyte[i]);
                                            messagelengthcounter++;
                                        }
                                        if (messagelengthcounter >= (messagelength - 1))
                                        {
                                            messagelengthcounter = 0;
                                            ah500drs = AH500DataReceiveStatus.DataCRC;
                                        }
                                        break;
                                    case AH500DataReceiveStatus.DataCRC:
                                        byte ah500bytescrc = receivedbyte[i];
                                        CRC8Calc ah500crc = new CRC8Calc(CRC8_POLY.CRC8_CCITT);
                                        ArrayList crcmessagedata = new ArrayList();
                                        crcmessagedata.Add(messagelength);
                                        crcmessagedata.Add(messageaddress);
                                        for (int j = 0; j < messagedata.Count; j++)
                                        {
                                            crcmessagedata.Add(messagedata[j]);
                                        }

                                        byte[] crcmessagedatabyte = (byte[])crcmessagedata.ToArray(typeof(byte));

                                        byte ah500bytescrcresult = 0;

                                        unchecked // Let overflow occur without exceptions
                                        {
                                            foreach (byte b in crcmessagedatabyte)
                                            {
                                                ah500bytescrcresult += b;
                                            }
                                        }

                                        //byte ah500bytescrcresult = ah500crc.Checksum(crcmessagedatabyte);                         

                                        if (ah500bytescrcresult == ah500bytescrc) //CRC校验成功
                                        {
                                            OnAH500NavDataReceived(this, new ByteReceivedEventArgs((byte[])messagedata.ToArray(typeof(byte))));
                                        }
                                        messagedata.Clear();
                                        ah500drs = AH500DataReceiveStatus.Head;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        break;
                    case SerialType.NavTelemetry:
                        if (mblnWaitingACK == SerialLineWaiting.Sensor)
                        {
                            try
                            {
                                if (serialPort1.BytesToRead > 0)
                                {
                                    ackmessage = serialPort1.ReadLine();
                                }
                            }
                            catch { }


                            cmds = ackmessage.ToCharArray();
                            ackmessage = "";

                            if (cmds.Length >= 3)
                            {
                                if (cmds[0] == char.ToUpper(m_current[0]) && cmds[1] == m_current[1])
                                {
                                    maltxcurrent.Add(cmds);
                                    switch (m_current[0])
                                    {
                                        case 'u':
                                            switch (m_current[2])
                                            {
                                                case '+':
                                                case '-':
                                                    break;
                                                case '?':
                                                    int len = 0;
                                                    char[] val;
                                                    switch (m_current[3])
                                                    {
                                                        case 'R':
                                                            if (cmds[2] >= 'A' && cmds[2] <= 'Z')
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 55;
                                                            }
                                                            else
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 48;
                                                            }
                                                            try
                                                            {
                                                                if (len < 3) break;
                                                                val = new char[len - 2];
                                                                val[0] = m_current[3];
                                                                val[1] = cmds[1];
                                                                for (int i = 0; i < len - 5; i++)
                                                                {
                                                                    val[i + 2] = cmds[i + 4];
                                                                }
                                                                OnMotorDataReceived(this, new ReceivedEventArgs(val));
                                                            }
                                                            catch { }
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            break;
                                        case 'l':
                                            switch (m_current[2])
                                            {
                                                case '+':
                                                    break;
                                                default:
                                                    break;
                                            }
                                            break;
                                        case 'a':
                                            switch (m_current[2])
                                            {
                                                case '?':
                                                    int len = 0;
                                                    char[] val;
                                                    switch (m_current[3])
                                                    {
                                                        case 'l':
                                                            if (cmds[2] >= 'A' && cmds[2] <= 'Z')
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 55;
                                                            }
                                                            else
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 48;
                                                            }
                                                            try
                                                            {
                                                                if (len < 3) break;
                                                                val = new char[len - 3];
                                                                val[0] = m_current[3];
                                                                for (int i = 0; i < len - 4; i++)
                                                                {
                                                                    val[i + 1] = cmds[i + 3];
                                                                }
                                                                OnAltimeterDataReceived(this, new ReceivedEventArgs(val));
                                                            }
                                                            catch { }
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            break;
                                        case 'c':
                                            switch (m_current[2])
                                            {
                                                case 'o':
                                                case 'r':
                                                    char[] _cmdconfirmation = new char[3];
                                                    _cmdconfirmation[0] = m_current[0];
                                                    _cmdconfirmation[1] = m_current[2];
                                                    _cmdconfirmation[2] = m_current[3];
                                                    OnCMDConfirmationReceived(this, new ReceivedEventArgs(_cmdconfirmation));
                                                    break;
                                                case '?':
                                                    int len = 0;
                                                    char[] val;
                                                    switch (m_current[3])
                                                    {
                                                        case 'l':
                                                            if (cmds[2] >= 'A' && cmds[2] <= 'Z')
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 55;
                                                            }
                                                            else
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 48;
                                                            }
                                                            try
                                                            {
                                                                if (len < 3) break;
                                                                val = new char[len - 3];
                                                                val[0] = m_current[3];
                                                                for (int i = 0; i < len - 4; i++)
                                                                {
                                                                    val[i + 1] = cmds[i + 3];
                                                                }
                                                                OnNavDataReceived(this, new ReceivedEventArgs(val));
                                                            }
                                                            catch { }
                                                            break;
                                                        case 'a':
                                                            if (cmds[2] >= 'A' && cmds[2] <= 'Z')
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 55;
                                                            }
                                                            else
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 48;
                                                            }
                                                            try
                                                            {
                                                                if (len < 3) break;
                                                                val = new char[len - 3];
                                                                val[0] = m_current[3];
                                                                for (int i = 0; i < len - 4; i++)
                                                                {
                                                                    val[i + 1] = cmds[i + 3];
                                                                }
                                                                OnNavDataReceived(this, new ReceivedEventArgs(val));
                                                            }
                                                            catch { }
                                                            break;
                                                        case 'p':
                                                            if (cmds[2] >= 'A' && cmds[2] <= 'Z')
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 55;
                                                            }
                                                            else
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 48;
                                                            }
                                                            try
                                                            {
                                                                if (len < 3) break;
                                                                val = new char[len - 3];
                                                                val[0] = m_current[3];
                                                                for (int i = 0; i < len - 4; i++)
                                                                {
                                                                    val[i + 1] = cmds[i + 3];
                                                                }
                                                                OnNavDataReceived(this, new ReceivedEventArgs(val));
                                                            }
                                                            catch { }
                                                            break;
                                                        case 'r':
                                                            if (cmds[2] >= 'A' && cmds[2] <= 'Z')
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 55;
                                                            }
                                                            else
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 48;
                                                            }
                                                            try
                                                            {
                                                                if (len < 3) break;
                                                                val = new char[len - 3];
                                                                val[0] = m_current[3];
                                                                for (int i = 0; i < len - 4; i++)
                                                                {
                                                                    val[i + 1] = cmds[i + 3];
                                                                }
                                                                OnNavDataReceived(this, new ReceivedEventArgs(val));
                                                            }
                                                            catch { }
                                                            break;
                                                        case 'd':
                                                            if (cmds[2] >= 'A' && cmds[2] <= 'Z')
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 55;
                                                            }
                                                            else
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 48;
                                                            }
                                                            try
                                                            {
                                                                if (len < 3) break;
                                                                val = new char[len - 3];
                                                                val[0] = m_current[3];
                                                                for (int i = 0; i < len - 4; i++)
                                                                {
                                                                    val[i + 1] = cmds[i + 3];
                                                                }
                                                                OnNavDataReceived(this, new ReceivedEventArgs(val));
                                                            }
                                                            catch { }
                                                            break;
                                                        case 'z':
                                                            if (cmds[2] >= 'A' && cmds[2] <= 'Z')
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 55;
                                                            }
                                                            else
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 48;
                                                            }
                                                            try
                                                            {
                                                                if (len < 3) break;
                                                                val = new char[len - 3];
                                                                val[0] = m_current[3];
                                                                for (int i = 0; i < len - 4; i++)
                                                                {
                                                                    val[i + 1] = cmds[i + 3];
                                                                }
                                                                OnNavDataReceived(this, new ReceivedEventArgs(val));
                                                            }
                                                            catch { }
                                                            break;
                                                        case 'o':
                                                            if (cmds[2] >= 'A' && cmds[2] <= 'Z')
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 55;
                                                            }
                                                            else
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 48;
                                                            }
                                                            try
                                                            {
                                                                if (len < 3) break;
                                                                val = new char[len - 3];
                                                                val[0] = m_current[3];
                                                                for (int i = 0; i < len - 4; i++)
                                                                {
                                                                    val[i + 1] = cmds[i + 3];
                                                                }
                                                                OnNavDataReceived(this, new ReceivedEventArgs(val));
                                                            }
                                                            catch { }
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            break;
                                        case 'j':
                                            switch (m_current[2])
                                            {
                                                case '?':
                                                    int len = 0;
                                                    char[] val;
                                                    switch (m_current[3])
                                                    {
                                                        case 'P':
                                                            if (cmds[2] >= 'A' && cmds[2] <= 'Z')
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 55;
                                                            }
                                                            else
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 48;
                                                            }
                                                            try
                                                            {
                                                                if (len < 3) break;
                                                                val = new char[len - 3];
                                                                val[0] = m_current[3];
                                                                for (int i = 0; i < len - 4; i++)
                                                                {
                                                                    val[i + 1] = cmds[i + 3];
                                                                }
                                                                OnJunctionBoardDataReceived(this, new ReceivedEventArgs(val));
                                                            }
                                                            catch { }
                                                            break;
                                                    }
                                                    break;
                                            }
                                            break;
                                        case 'p':
                                            switch (m_current[2])
                                            {
                                                case 't':
                                                    break;
                                                case '?':
                                                    switch (m_current[3])
                                                    {
                                                        case 't':
                                                            int len = 0;
                                                            char[] val;
                                                            if (cmds[2] >= 'A' && cmds[2] <= 'Z')
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 55;
                                                            }
                                                            else
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 48;
                                                            }
                                                            try
                                                            {
                                                                if (len < 4) break;
                                                                val = new char[len - 4];
                                                                for (int i = 0; i < len - 4; i++)
                                                                {
                                                                    val[i] = cmds[i + 3];
                                                                }
                                                                OnTiltDataReceived(this, new ReceivedEventArgs(val));
                                                            }
                                                            catch { }
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            break;
                                        case 'm':
                                            if (m_current[1] == '2')
                                            {
                                                switch (m_current[2])
                                                {
                                                    case 'd':
                                                        char[] _cmdconfirmation = new char[4];
                                                        _cmdconfirmation[0] = m_current[0];
                                                        _cmdconfirmation[1] = m_current[1];
                                                        _cmdconfirmation[2] = m_current[2];
                                                        _cmdconfirmation[3] = m_current[3];
                                                        OnCMDConfirmationReceived(this, new ReceivedEventArgs(_cmdconfirmation));
                                                        break;
                                                }
                                            }
                                            break;
                                        case 'n':
                                            switch (m_current[2])
                                            {
                                                case 't':
                                                    break;
                                                case '?':
                                                    switch (m_current[3])
                                                    {
                                                        case 't':
                                                            int len = 0;
                                                            char[] val;
                                                            if (cmds[2] >= 'A' && cmds[2] <= 'Z')
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 55;
                                                            }
                                                            else
                                                            {
                                                                len = Convert.ToInt32(cmds[2]);
                                                                len = len - 48;
                                                            }
                                                            try
                                                            {
                                                                if (len < 4) break;
                                                                val = new char[len - 4];
                                                                for (int i = 0; i < len - 4; i++)
                                                                {
                                                                    val[i] = cmds[i + 3];
                                                                }
                                                                OnLinearDataReceived(this, new ReceivedEventArgs(val));
                                                            }
                                                            catch { }
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                    OnSuccessfulDataReceived(this, new ReceivedEventArgs(cmds));
                                    timeoutcounter = 0;
                                    mblnWaitingACK = SerialLineWaiting.Free;
                                }
                                else
                                {
                                    timeoutcounter++;
                                    if (timeoutcounter >= WaitingACKTimeOut)
                                    {
                                        OnDataReceived(this, new ReceivedEventArgs(m_current));
                                        //serialPort1.DiscardInBuffer();
                                        timeoutcounter = 0;
                                        mblnWaitingACK = SerialLineWaiting.Free;

                                    }
                                }

                            }
                            else if (cmds.Length == 0)
                            {
                                //timeoutcounter = 0;
                                timeoutcounter++;
                                if (timeoutcounter >= WaitingACKTimeOut)
                                {
                                    OnDataReceived(this, new ReceivedEventArgs(m_current));
                                    //serialPort1.DiscardInBuffer();
                                    timeoutcounter = 0;
                                    mblnWaitingACK = SerialLineWaiting.Free;
                                }
                            }
                            else
                            {
                                timeoutcounter++;
                                if (timeoutcounter >= WaitingACKTimeOut)
                                {
                                    OnDataReceived(this, new ReceivedEventArgs(m_current));
                                    //serialPort1.DiscardInBuffer();
                                    timeoutcounter = 0;
                                    mblnWaitingACK = SerialLineWaiting.Free;
                                }
                            }
                        }
                        break;
                    case SerialType.EXTPORT:
                        try
                        {
                            if (serialPort1.BytesToRead > 0)
                            {
                                ackmessage = serialPort1.ReadLine();
                            }
                        }
                        catch { }

                        cmds = ackmessage.ToCharArray();
                        ackmessage = "";

                        if (cmds.Length >= 3)
                        {
                            if (cmds[0] == char.ToUpper(m_current[0]) && cmds[1] == m_current[1])
                            {
                                maltxcurrent.Add(cmds);
                                switch (m_current[0])
                                {
                                    case 'c':
                                        switch (m_current[2])
                                        {
                                            case '?':
                                                int len = 0;
                                                char[] val;
                                                switch (m_current[3])
                                                {
                                                    case 'l':
                                                        if (cmds[2] >= 'A' && cmds[2] <= 'Z')
                                                        {
                                                            len = Convert.ToInt32(cmds[2]);
                                                            len = len - 55;
                                                        }
                                                        else
                                                        {
                                                            len = Convert.ToInt32(cmds[2]);
                                                            len = len - 48;
                                                        }
                                                        try
                                                        {
                                                            if (len < 3) break;
                                                            val = new char[len - 3];
                                                            val[0] = m_current[3];
                                                            for (int i = 0; i < len - 4; i++)
                                                            {
                                                                val[i + 1] = cmds[i + 3];
                                                            }
                                                            OnNavDataReceived(this, new ReceivedEventArgs(val));
                                                        }
                                                        catch { }
                                                        break;
                                                    default:
                                                        break;
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                OnSuccessfulDataReceived(this, new ReceivedEventArgs(cmds));
                            }

                        }
                        break;
                    case SerialType.BATPORT:
                        if (serialPort1.BytesToRead > 0)
                        {
                            byte[] receivedbyte = new byte[serialPort1.BytesToRead];
                            serialPort1.Read(receivedbyte, 0, receivedbyte.Length);

                            for (int i = 0; i < receivedbyte.Length; i++)
                            {
                                switch (drs)
                                {
                                    case DataReceiveStatus.Head1:
                                        if (receivedbyte[i] == 0xAA)
                                        {
                                            drs = DataReceiveStatus.Head2;
                                        }
                                        break;
                                    case DataReceiveStatus.Head2:
                                        if (receivedbyte[i] == 0xFF)
                                        {
                                            drs = DataReceiveStatus.Command1;
                                        }
                                        break;
                                    case DataReceiveStatus.Command1:
                                        messagecmd1 = receivedbyte[i];
                                        drs = DataReceiveStatus.Command2;
                                        break;
                                    case DataReceiveStatus.Command2:
                                        messagecmd2 = receivedbyte[i];
                                        drs = DataReceiveStatus.Data;
                                        break;
                                    case DataReceiveStatus.Data:
                                        if (messagecmd1 == 0x01 && messagecmd2 == 0x10)
                                        {
                                            messagedata.Add(receivedbyte[i]);
                                            if (messagedata.Count == 16)
                                                drs = DataReceiveStatus.End1;
                                        }
                                        else
                                            drs = DataReceiveStatus.Head1;

                                        break;
                                    case DataReceiveStatus.End1:
                                        messageend[0] = receivedbyte[i];
                                        if (messageend[0] == 0x55)
                                            drs = DataReceiveStatus.End2;
                                        else
                                            drs = DataReceiveStatus.Head1;
                                        break;
                                    case DataReceiveStatus.End2:
                                        messageend[1] = receivedbyte[i];
                                        if (messageend[1] == 0xFF)
                                        {
                                            OnByteSuccessfulDataReceived(this, new ByteReceivedEventArgs((byte[])messagedata.ToArray(typeof(byte))));
                                        }
                                        messagedata.Clear();
                                        drs = DataReceiveStatus.Head1;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        break;
                    case SerialType.GPSPORT:
                        //int tempbytestoread = serialPort1.BytesToRead;
                        try
                        {
                            string gpsstr = serialPort1.ReadLine();
                            string[] gpssubstr = (gpsstr.Split('*'));
                            gpssubstr[0] = gpssubstr[0].Substring(1);

                            char[] tempbyte = gpsstr.ToCharArray();
                            byte[] gpsbytes = System.Text.Encoding.UTF8.GetBytes(gpssubstr[0]);

                            byte checksum = 0;
                            for (int i = 0; i < gpsbytes.Length; i++)
                            {
                                checksum ^= gpsbytes[i];
                            }

                            if (checksum.ToString("x2") == gpssubstr[1]) //CRC校验成功
                            {
                                OnDataReceived(this, new ReceivedEventArgs(tempbyte));
                            }
                        }
                        catch
                        { }
                        break;
                    case SerialType.DVLPORT:
                        try
                        {
                            string dvlstr = serialPort1.ReadLine();
                            string[] dvlsubstr = (dvlstr.Split('*'));

                            char[] tempbyte1 = dvlstr.ToCharArray();
                            byte[] dvlbytes = System.Text.Encoding.UTF8.GetBytes(dvlsubstr[0]);

                            CRC8Calc dvlcrc = new CRC8Calc(CRC8_POLY.CRC8_CCITT);
                            byte dvlbytescrc = dvlcrc.Checksum(dvlbytes);

                            if (dvlbytescrc.ToString("x2") == dvlsubstr[1]) //CRC校验成功
                            {
                                OnDataReceived(this, new ReceivedEventArgs(tempbyte1));
                            }
                        }
                        catch { }
                        break;
                }
            }

        }

        public void OpenPort(string port, int baud, SerialType _serialtype)
        {
            try
            {
                serialPort1.PortName = port;
                serialPort1.BaudRate = baud;
                if (_serialtype == SerialType.GPSPORT)
                {
                    serialPort1.ReadTimeout = 100;
                    serialPort1.NewLine = "\r\n";
                }

                if (_serialtype == SerialType.DVLPORT)
                {
                    serialPort1.ReadTimeout = 100;
                    serialPort1.NewLine = "\r\n";
                }
                if (_serialtype == SerialType.EXTPORT)
                {
                    serialPort1.DtrEnable = true;

                }

                serialtype = _serialtype;

                serialPort1.Open();
                if (serialPort1.IsOpen == true)
                {
                    threadSending = new Thread(new ThreadStart(StartSending));
                    threadSending.Start();

                    threadReceiving = new Thread(new ThreadStart(StartReceiving));
                    threadReceiving.Start();
                }
            }
            catch { }
        }

        public bool IsOpen()
        {
            if (serialPort1.IsOpen == true)
                return true;
            else
                return false;
        }

        public void ClosePort()
        {
            if (IsOpen())
            {
                if (threadSending != null) threadSending.Abort();
                if (threadReceiving != null) threadReceiving.Abort();
                serialPort1.DiscardInBuffer();
                serialPort1.DiscardOutBuffer();
                serialPort1.Close();
            }
        }

        public void SendMessage(byte[] cmds) //str 前引导符; str1 数值
        {
            if (serialPort1.IsOpen == false) return;
            switch (serialtype)
            {
                case SerialType.EXTPORT:
                    malCmdQueue.Add(cmds);
                    break;
                case SerialType.BATPORT:
                    malCmdQueue.Add(cmds);
                    break;
                case SerialType.AH500Telemetry:
                    malCmdQueue.Add(cmds);
                    break;
                case SerialType.DCM250BTelemetry:
                    malCmdQueue.Add(cmds);
                    break;
            }
        }

        public void SendMessage(char[] cmds) //str 前引导符; str1 数值
        {
            if (serialPort1.IsOpen == false) return;
            switch (serialtype)
            {
                case SerialType.EXTPORT:
                    malCmdQueue.Add(cmds);
                    break;
                case SerialType.BATPORT:
                    malCmdQueue.Add(cmds);
                    break;
            }
        }


        public void SendMessage(byte[] cmds, int _priority) //str 前引导符; str1 数值
        {
            if (serialPort1.IsOpen == false) return;
            switch (serialtype)
            {
                case SerialType.NavTelemetry:
                    char[] cpara = System.Text.Encoding.Default.GetChars(cmds);
                    malCmdQueueWithPriority.Add(new SerialCMD() { Cmds = cpara, Priority = _priority });
                    break;
            }
        }

        public void SendMessage(char[] cmds, int _priority) //直接发送格式化好的指令串
        {
            if (serialPort1.IsOpen == false) return;
            switch (serialtype)
            {
                case SerialType.NavTelemetry:
                    malCmdQueueWithPriority.Add(new SerialCMD() { Cmds = cmds, Priority = _priority });
                    break;
            }
        }

        public void SendMessage(DataType _dt, char _dev, char _cmd, String _val, int _priority)
        {
            if (serialPort1.IsOpen == false) return;

            ArrayList charlist = new ArrayList();
            char[] cc = _val.ToCharArray();
            char _len = '0';
            if ((cc.Length + 5) >= 0 && (cc.Length + 5) <= 9)
                _len = (char)(48 + cc.Length + 5);

            if ((cc.Length + 5) > 9)
                _len = (char)(55 + cc.Length + 5);

            char[] cmds;
            switch (_dt)
            {
                case DataType.Motor:
                    charlist.Add('u');
                    charlist.Add(_dev);
                    charlist.Add(_len);
                    charlist.Add(_cmd);
                    for (int i = 0; i < cc.Length; i++)
                        charlist.Add(cc[i]);
                    charlist.Add((char)(0x0D));
                    cmds = (char[])charlist.ToArray(typeof(char));
                    switch (_dev)
                    {
                        case '1':
                            malCmdQueueWithThruster[0] = cmds;
                            break;
                        case '2':
                            malCmdQueueWithThruster[1] = cmds;
                            break;
                        case '3':
                            malCmdQueueWithThruster[2] = cmds;
                            break;
                        case '4':
                            malCmdQueueWithThruster[3] = cmds;
                            break;
                        case '5':
                            malCmdQueueWithThruster[4] = cmds;
                            break;
                        case '8':
                            malCmdQueueWithThruster[5] = cmds;
                            break;
                    }
                    break;
                case DataType.Lights:
                    charlist.Add('l');
                    charlist.Add(_dev);
                    charlist.Add(_len);
                    charlist.Add(_cmd);
                    for (int i = 0; i < cc.Length; i++)
                        charlist.Add(cc[i]);
                    charlist.Add((char)(0x0D));
                    cmds = (char[])charlist.ToArray(typeof(char));
                    //malCmdQueue.Add(cmds);
                    malCmdQueueWithPriority.Add(new SerialCMD() { Cmds = cmds, Priority = _priority });
                    break;
                case DataType.Tilt:
                    charlist.Add('p');
                    charlist.Add(_dev);
                    charlist.Add(_len);
                    charlist.Add(_cmd);
                    for (int i = 0; i < cc.Length; i++)
                        charlist.Add(cc[i]);
                    charlist.Add((char)(0x0D));
                    cmds = (char[])charlist.ToArray(typeof(char));
                    //malCmdQueue.Add(cmds);
                    malCmdQueueWithPriority.Add(new SerialCMD() { Cmds = cmds, Priority = _priority });
                    break;
                case DataType.Nav:
                    charlist.Add('c');
                    charlist.Add(_dev);
                    charlist.Add(_len);
                    charlist.Add(_cmd);
                    for (int i = 0; i < cc.Length; i++)
                        charlist.Add(cc[i]);
                    charlist.Add((char)(0x0D));
                    cmds = (char[])charlist.ToArray(typeof(char));
                    //malCmdQueue.Add(cmds);
                    malCmdQueueWithPriority.Add(new SerialCMD() { Cmds = cmds, Priority = _priority });
                    break;
                case DataType.AH500:

                    break;
                case DataType.ExternalBoard:
                    charlist.Add('c');
                    charlist.Add(_dev);
                    charlist.Add(_len);
                    charlist.Add(_cmd);
                    for (int i = 0; i < cc.Length; i++)
                        charlist.Add(cc[i]);
                    charlist.Add((char)(0x0D));
                    cmds = (char[])charlist.ToArray(typeof(char));
                    malCmdQueue.Add(cmds);
                    break;
                case DataType.Altimeter:
                    charlist.Add('a');
                    charlist.Add(_dev);
                    charlist.Add(_len);
                    charlist.Add(_cmd);
                    for (int i = 0; i < cc.Length; i++)
                        charlist.Add(cc[i]);
                    charlist.Add((char)(0x0D));
                    cmds = (char[])charlist.ToArray(typeof(char));
                    //malCmdQueue.Add(cmds);
                    malCmdQueueWithPriority.Add(new SerialCMD() { Cmds = cmds, Priority = _priority });
                    break;
                case DataType.JB:
                    charlist.Add('j');
                    charlist.Add(_dev);
                    charlist.Add(_len);
                    charlist.Add(_cmd);
                    for (int i = 0; i < cc.Length; i++)
                        charlist.Add(cc[i]);
                    charlist.Add((char)(0x0D));
                    cmds = (char[])charlist.ToArray(typeof(char));
                    //malCmdQueue.Add(cmds);
                    malCmdQueueWithPriority.Add(new SerialCMD() { Cmds = cmds, Priority = _priority });
                    break;
                case DataType.SingleManip:
                    charlist.Add('m');
                    charlist.Add(_dev);
                    charlist.Add(_len);
                    charlist.Add(_cmd);
                    for (int i = 0; i < cc.Length; i++)
                        charlist.Add(cc[i]);
                    charlist.Add((char)(0x0D));
                    cmds = (char[])charlist.ToArray(typeof(char));
                    //malCmdQueue.Add(cmds);
                    malCmdQueueWithPriority.Add(new SerialCMD() { Cmds = cmds, Priority = _priority });
                    break;
                case DataType.LinearActuator:
                    charlist.Add('n');
                    charlist.Add(_dev);
                    charlist.Add(_len);
                    charlist.Add(_cmd);
                    for (int i = 0; i < cc.Length; i++)
                        charlist.Add(cc[i]);
                    charlist.Add((char)(0x0D));
                    cmds = (char[])charlist.ToArray(typeof(char));
                    //malCmdQueue.Add(cmds);
                    malCmdQueueWithPriority.Add(new SerialCMD() { Cmds = cmds, Priority = _priority });
                    break;
            }

        }

        public void ClearCmdQueueWithPriorityTransit()
        {
            malCmdQueueWithPriority.Clear();
        }

        public void ClearCmdQueueWithThrusterTransit()
        {
            //malCmdQueueWithThruster.Clear();
            char[] emptycmds = new char[2] { '0', '0' };
            malCmdQueueWithThruster[0] = emptycmds;
            malCmdQueueWithThruster[1] = emptycmds;
            malCmdQueueWithThruster[2] = emptycmds;
            malCmdQueueWithThruster[3] = emptycmds;
            malCmdQueueWithThruster[4] = emptycmds;
            malCmdQueueWithThruster[5] = emptycmds;
        }

        public int GetCmdQueueWithPriorityTransitCount()
        {
            //return malCmdQueueWithPriority.Count + malCmdQueueWithThruster.Count;
            return malCmdQueueWithPriority.Count;
        }

        public int GetCmdQueueTransitCount()
        {
            return malCmdQueue.Count;
        }

        public int GetCmdQueueReceiveByteCount()
        {
            return serialPort1.BytesToRead;
        }

        public ArrayList GetLastTX()
        {
            ArrayList temp = new ArrayList(maltxcurrent);
            maltxcurrent.Clear();
            return temp;
        }

        public ArrayList GetLastRX()
        {
            ArrayList temp = new ArrayList(malrxcurrent);
            malrxcurrent.Clear();
            return temp;
        }
    }

    public class ReceivedEventArgs : EventArgs
    {
        private char[] chars;
        public ReceivedEventArgs(char[] chars)
        {
            this.chars = chars;
        }

        public char[] DataReceived
        {
            get
            {
                return chars;
            }
        }
    }

    public class ByteReceivedEventArgs : EventArgs
    {
        private byte[] chars;
        public ByteReceivedEventArgs(byte[] chars)
        {
            this.chars = chars;
        }

        public byte[] DataReceived
        {
            get
            {
                return chars;
            }
        }
    }

    public class LogReceivedEventArgs : EventArgs
    {
        private string strlog;
        public LogReceivedEventArgs(string strlog)
        {
            this.strlog = strlog;
        }

        public string LogDataReceived
        {
            get
            {
                return strlog;
            }
        }
    }
}
