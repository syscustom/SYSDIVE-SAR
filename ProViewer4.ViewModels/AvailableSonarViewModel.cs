#define TRACE
using ProViewer4;
using ProViewer4.ViewModels;
using SML;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProViewer4.ViewModels
{
    public class AvailableSonarViewModel : ObservableObject
    {
        private List<AvailableHead> _availableHeads = new List<AvailableHead>();

        private SonarInfo _model;

        private AvailableHead _selectedHead;

        public List<AvailableHead> AvailableHeads
        {
            get
            {
                Trace.TraceWarning("get_Available heads = {0}", _availableHeads.Count);
                return _availableHeads;
            }
            set
            {
                _availableHeads = value;
                RaisePropertyChanged(() => AvailableHeads);
            }
        }

        public AvailableHead SelectedHead
        {
            get
            {
                return _selectedHead;
            }
            set
            {
                _selectedHead = value;
            }
        }

        public string IpAddress => _model.IpAddress;

        public string FirmwareVersion => _model.FirmwareVersion;

        public AvailableSonarViewModel()
        {
        }

        public AvailableSonarViewModel(SonarInfo sonar)
        {
            _model = sonar;
            List<AvailableHead> headDescriptions = new List<AvailableHead>();
            int index = 0;
            HeadInfo[] heads = sonar.Heads;
            foreach (HeadInfo head in heads)
            {
                string s = head.Name;
                if (string.IsNullOrEmpty(s))
                {
                    s += $"Head {head.Frequency}";
                }
                AvailableHead h = new AvailableHead(s, index++);
                headDescriptions.Add(h);
            }
            AvailableHeads = headDescriptions;
            _selectedHead = headDescriptions[0];
            RaisePropertyChanged(() => SelectedHead);
        }
    }
}
