using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TBVSonar
{
    public class MultibeamSonar
    {
        public struct BVTSonar { };
        public struct BVTHead { };
        public struct BVTMagImage { };
        public struct BVTPing { };

        public static string DataFile = "swimmer.son";


        [DllImport("bvtsdk3.dll", EntryPoint = "BVTSonar_Open")]
        public extern static int BVTSonar_Open(BVTSonar obj, string type, string type_params);
        [DllImport("bvtsdk3.dll", EntryPoint = "BVTSonar_Create")]
        public extern static BVTSonar BVTSonar_Create();
        [DllImport("bvtsdk3.dll", EntryPoint = "BVTSonar_GetHeadCount")]
        public extern static int BVTSonar_GetHeadCount(BVTSonar obj);
        [DllImport("bvtsdk3.dll", EntryPoint = "BVTSonar_GetHead")]
        unsafe public extern static int BVTSonar_GetHead(BVTSonar obj, int head_num, BVTHead* head);
        [DllImport("bvtsdk3.dll", EntryPoint = "BVTPing_GetImageXY")]
        unsafe public extern static int BVTPing_GetImageXY(BVTPing obj, BVTMagImage* img);
        [DllImport("bvtsdk3.dll", EntryPoint = "BVTHead_GetPing")]
        unsafe public extern static int BVTHead_GetPing(BVTHead obj, int ping_num, BVTPing* ping);
        [DllImport("bvtsdk3.dll", EntryPoint = "BVTHead_GetPingCount")]
        public extern static int BVTHead_GetPingCount(BVTHead obj);
        [DllImport("bvtsdk3.dll", EntryPoint = "BVTHead_GetMinimumRange")]
        public extern static int BVTHead_GetMinimumRange(BVTHead obj);
        [DllImport("bvtsdk3.dll", EntryPoint = "BVTHead_GetMaximumRange")]
        public extern static int BVTHead_GetMaximumRange(BVTHead obj);

        public void abc()
        {
            int ret;
            BVTSonar son = new BVTSonar();
            son = BVTSonar_Create();
            unsafe
            {
                ret = BVTSonar_Open(son, "FILE", DataFile);
            }
            int heads = -1;
            heads = BVTSonar_GetHeadCount(son);

            BVTHead head;
            unsafe
            {
                ret = BVTSonar_GetHead(son, 0, &head);

            }

            int pings = -1;
            pings = BVTHead_GetPingCount(head);
            Console.Write("BVTHead_GetPingCount: {0:D}\n", pings);
            Console.Write("BVTHead_GetMinimumRange: {0:f2}\n", BVTHead_GetMinimumRange(head));
            Console.Write("BVTHead_GetMaximumRange: {0:f2}\n", BVTHead_GetMaximumRange(head));



        }
    }
}
