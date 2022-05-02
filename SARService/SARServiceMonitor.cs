using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cjwdev.WindowsApi;
using System.Runtime.InteropServices;

namespace SARService
{
    public partial class SARServiceMonitor : ServiceBase
    {
        public enum SimpleServiceCustomCommands { Command1 = 128, Command2 = 129, Command3 = 130, Command4 = 222};

        private int KeepingRunningCounter = 0;
        private bool IsMainAPPRuinning = false;

        public SARServiceMonitor()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Task.Factory.StartNew(Handle);

            /*
            Task.Factory.StartNew(() =>
            {

                //Console.WriteLine("任务开始工作……");
                Handle();
                //模拟工作过程
                Thread.Sleep(1000);

            });
            */
        }

        protected override void OnStop()
        {

        }

        //需要定时执行的代码段
        private void Handle()
        {
            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    //var path = AppDomain.CurrentDomain.BaseDirectory + "service.log";
                    //var context = "SARServiceMonitor: Service Stoped " + DateTime.Now + "\n";
                    //WriteLogs(path, context);

                    if (!IsMainAPPRuinning)
                        IsMainAPPRuinning = FindProcess("WpfApp1");

                    if(IsMainAPPRuinning)
                    {
                        KeepingRunningCounter++;
                        if (KeepingRunningCounter > 15)
                        {
                            KeepingRunningCounter = 0;
                            KillProcess("WpfApp1");
                            Thread.Sleep(3000);
                            ShutDownSys.DoExitWin(ShutDownSys.EWX_FORCE | ShutDownSys.EWX_REBOOT); //关机
                            //RunProcess();
                            IsMainAPPRuinning = false;
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private void KillProcess(string processName)
        {
            Process[] myproc = Process.GetProcesses();
            foreach (Process item in myproc)
            {
                if (item.ProcessName == processName)
                {
                    item.Kill();
                }
            }
        }

        private bool FindProcess(string processName)
        {
            Process[] myproc = Process.GetProcesses();
            foreach (Process item in myproc)
            {
                if (item.ProcessName == processName)
                {
                    return true;
                }
            }
            return false;
        }

        private void RunProcess()
        {
            string strProcess = @"D:\document\SYSDIVE-SAR\win iot\wpf\WpfApp1\WpfApp1\bin\Debug\WpfApp1.exe";
            try
            {

                string appStartPath = strProcess;
                IntPtr userTokenHandle = IntPtr.Zero;
                ApiDefinitions.WTSQueryUserToken(ApiDefinitions.WTSGetActiveConsoleSessionId(), ref userTokenHandle);

                ApiDefinitions.PROCESS_INFORMATION procInfo = new ApiDefinitions.PROCESS_INFORMATION();
                ApiDefinitions.STARTUPINFO startInfo = new ApiDefinitions.STARTUPINFO();
                startInfo.cb = (uint)Marshal.SizeOf(startInfo);

                ApiDefinitions.CreateProcessAsUser(
                    userTokenHandle,
                    appStartPath,
                    "",
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    0,
                    IntPtr.Zero,
                    null,
                    ref startInfo,
                    out procInfo);

                if (userTokenHandle != IntPtr.Zero)
                    ApiDefinitions.CloseHandle(userTokenHandle);

                int _currentAquariusProcessId = (int)procInfo.dwProcessId;
            }
            catch (Exception ex)
            {
            }

        }

        public class WinAPI_Interop
        {
            public static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;
            /// <summary>
            /// 服务程序执行消息提示,前台MessageBox.Show
            /// </summary>
            /// <param name="message">消息内容</param>
            /// <param name="title">标题</param>
            public static void ShowServiceMessage(string message, string title)
            {
                int resp = 0;
                WTSSendMessage(WTS_CURRENT_SERVER_HANDLE, WTSGetActiveConsoleSessionId(), title, title.Length, message, message.Length, 0, 0, out resp, false);
            }

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern int WTSGetActiveConsoleSessionId();

            [DllImport("wtsapi32.dll", SetLastError = true)]
            public static extern bool WTSSendMessage(IntPtr hServer, int SessionId, String pTitle, int TitleLength, String pMessage, int MessageLength, int Style, int Timeout, out int pResponse, bool bWait);
            #region P/Invoke WTS APIs
            private enum WTS_CONNECTSTATE_CLASS
            {
                WTSActive,
                WTSConnected,
                WTSConnectQuery,
                WTSShadow,
                WTSDisconnected,
                WTSIdle,
                WTSListen,
                WTSReset,
                WTSDown,
                WTSInit
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            private struct WTS_SESSION_INFO
            {
                public UInt32 SessionID;
                public string pWinStationName;
                public WTS_CONNECTSTATE_CLASS State;
            }

            [DllImport("WTSAPI32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
            static extern bool WTSEnumerateSessions(
                IntPtr hServer,
                [MarshalAs(UnmanagedType.U4)] UInt32 Reserved,
                [MarshalAs(UnmanagedType.U4)] UInt32 Version,
                ref IntPtr ppSessionInfo,
                [MarshalAs(UnmanagedType.U4)] ref UInt32 pSessionInfoCount
                );

            [DllImport("WTSAPI32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
            static extern void WTSFreeMemory(IntPtr pMemory);

            [DllImport("WTSAPI32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
            static extern bool WTSQueryUserToken(UInt32 sessionId, out IntPtr Token);
            #endregion

            #region P/Invoke CreateProcessAsUser
            /// <summary> 
            /// Struct, Enum and P/Invoke Declarations for CreateProcessAsUser. 
            /// </summary> 
            ///  

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            struct STARTUPINFO
            {
                public Int32 cb;
                public string lpReserved;
                public string lpDesktop;
                public string lpTitle;
                public Int32 dwX;
                public Int32 dwY;
                public Int32 dwXSize;
                public Int32 dwYSize;
                public Int32 dwXCountChars;
                public Int32 dwYCountChars;
                public Int32 dwFillAttribute;
                public Int32 dwFlags;
                public Int16 wShowWindow;
                public Int16 cbReserved2;
                public IntPtr lpReserved2;
                public IntPtr hStdInput;
                public IntPtr hStdOutput;
                public IntPtr hStdError;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            struct PROCESS_INFORMATION
            {
                public IntPtr hProcess;
                public IntPtr hThread;
                public int dwProcessId;
                public int dwThreadId;
            }

            /// <summary>
            /// 以当前登录的windows用户(角色权限)运行指定程序进程
            /// </summary>
            /// <param name="hToken"></param>
            /// <param name="lpApplicationName">指定程序(全路径)</param>
            /// <param name="lpCommandLine">参数</param>
            /// <param name="lpProcessAttributes">进程属性</param>
            /// <param name="lpThreadAttributes">线程属性</param>
            /// <param name="bInheritHandles"></param>
            /// <param name="dwCreationFlags"></param>
            /// <param name="lpEnvironment"></param>
            /// <param name="lpCurrentDirectory"></param>
            /// <param name="lpStartupInfo">程序启动属性</param>
            /// <param name="lpProcessInformation">最后返回的进程信息</param>
            /// <returns>是否调用成功</returns>
            [DllImport("ADVAPI32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
            static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes,
                                                          bool bInheritHandles, uint dwCreationFlags, string lpEnvironment, string lpCurrentDirectory,
                                                          ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

            [DllImport("KERNEL32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
            static extern bool CloseHandle(IntPtr hHandle);
            #endregion

            /// <summary>
            /// 以当前登录系统的用户角色权限启动指定的进程
            /// </summary>
            /// <param name="ChildProcName">指定的进程(全路径)</param>
            public static void CreateProcess(string ChildProcName)
            {
                IntPtr ppSessionInfo = IntPtr.Zero;
                UInt32 SessionCount = 0;
                if (WTSEnumerateSessions(
                                        (IntPtr)WTS_CURRENT_SERVER_HANDLE,  // Current RD Session Host Server handle would be zero. 
                                        0,  // This reserved parameter must be zero. 
                                        1,  // The version of the enumeration request must be 1. 
                                        ref ppSessionInfo, // This would point to an array of session info. 
                                        ref SessionCount  // This would indicate the length of the above array.
                                        ))
                {
                    for (int nCount = 0; nCount < SessionCount; nCount++)
                    {
                        WTS_SESSION_INFO tSessionInfo = (WTS_SESSION_INFO)Marshal.PtrToStructure(ppSessionInfo + nCount * Marshal.SizeOf(typeof(WTS_SESSION_INFO)), typeof(WTS_SESSION_INFO));
                        if (WTS_CONNECTSTATE_CLASS.WTSActive == tSessionInfo.State)
                        {
                            IntPtr hToken = IntPtr.Zero;
                            if (WTSQueryUserToken(tSessionInfo.SessionID, out hToken))
                            {
                                PROCESS_INFORMATION tProcessInfo;
                                STARTUPINFO tStartUpInfo = new STARTUPINFO();
                                tStartUpInfo.cb = Marshal.SizeOf(typeof(STARTUPINFO));
                                bool ChildProcStarted = CreateProcessAsUser(
                                                                            hToken,             // Token of the logged-on user. 
                                                                            ChildProcName,      // Name of the process to be started. 
                                                                            null,               // Any command line arguments to be passed. 
                                                                            IntPtr.Zero,        // Default Process' attributes. 
                                                                            IntPtr.Zero,        // Default Thread's attributes. 
                                                                            false,              // Does NOT inherit parent's handles. 
                                                                            0,                  // No any specific creation flag. 
                                                                            null,               // Default environment path. 
                                                                            null,               // Default current directory. 
                                                                            ref tStartUpInfo,   // Process Startup Info.  
                                                                            out tProcessInfo    // Process information to be returned. 
                                                         );
                                if (ChildProcStarted)
                                {
                                    CloseHandle(tProcessInfo.hThread);
                                    CloseHandle(tProcessInfo.hProcess);
                                }
                                else
                                {
                                    ShowServiceMessage("CreateProcessAsUser失败", "CreateProcess");
                                }
                                CloseHandle(hToken);
                                break;
                            }
                        }
                    }
                    WTSFreeMemory(ppSessionInfo);
                }
            }
        }

        public void WriteLogs(string path, string context)
        {
            var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            var sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine(context);

            sw.Flush();
            sw.Close();
            fs.Close();
        }

        protected override void OnCustomCommand(int command)
        {
            base.OnCustomCommand(command);

            switch (command)
            {
                case (int)SimpleServiceCustomCommands.Command1:

                    //Command1 Implementation
                    break;

                case (int)SimpleServiceCustomCommands.Command2:
                    //Command2 Implementation
                    break;

                case (int)SimpleServiceCustomCommands.Command3:
                    //Command3 Implementation
                    Thread.Sleep(2000);
                    IsMainAPPRuinning = false;
                    break;
                case (int)SimpleServiceCustomCommands.Command4:
                    KeepingRunningCounter = 0;
                    //Command4 Implementation
                    break;
                default:
                    break;

            }
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
