using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProViewer4.Interfaces
{
    public interface IApplicationViewModel
    {
        ITrackerSettingsViewModel TrackerSettingsViewModel
        {
            get;
        }

        TrackerViewModel TrackerViewModel
        {
            get;
        }

        void LoadSettings();

        void SaveSettings();
    }
}
