// ProViewer4.Models.ProViewerTrace
#define TRACE
using ProViewer4.Interfaces;
using System;
using System.Diagnostics;

namespace ProViewer4.Models
{
    public class ProViewerTrace : TraceSource, IProViewerTrace
    {
        private int _Pid;

        public ProViewerTrace(int pid, string name = "ProViewerTrace")
            : base(name)
        {
            _Pid = pid;
            TraceData(TraceEventType.Start, 3, "                                                          ");
        }

        public void TraceInfo(string info)
        {
            TraceData(TraceEventType.Information, 3, $"{_Pid} {info}");
            Flush();
        }

        public void TraceError(string message)
        {
            TraceData(TraceEventType.Error, 1, $"{_Pid} {message}");
            Flush();
        }

        public void TraceException(Exception ex)
        {
            TraceData(TraceEventType.Error, 1, $"{_Pid} {ex.ToString()}");
            Flush();
        }

        void IProViewerTrace.Close()
        {
            Close();
        }

        void IProViewerTrace.Flush()
        {
            Flush();
        }

        SourceSwitch IProViewerTrace.get_Switch()
        {
            return base.Switch;
        }

        void IProViewerTrace.set_Switch(SourceSwitch P_0)
        {
            base.Switch = P_0;
        }
    }
}
