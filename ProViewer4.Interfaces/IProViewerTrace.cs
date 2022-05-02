// ProViewer4.Interfaces.IProViewerTrace
using System;
using System.Diagnostics;

namespace ProViewer4.Interfaces
{
    public interface IProViewerTrace
    {
        SourceSwitch Switch
        {
            get;
            set;
        }

        void TraceInfo(string info);

        void TraceError(string message);

        void TraceException(Exception ex);

        void Close();

        void Flush();
    }
}