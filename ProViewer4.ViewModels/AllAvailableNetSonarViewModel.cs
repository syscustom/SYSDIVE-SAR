// ProViewer4.ViewModels.AllAvailableNetSonarViewModel
using BlueView.Wpf.Commands;
using ProViewer4;
using ProViewer4.ViewModels;
using SML;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ProViewer4.ViewModels
{
    public class AllAvailableNetSonarViewModel : ObservableObject
    {
        private List<SonarInfo> _discovered;

        private List<string> _netSonarDisplayNames = new List<string>();

        private AvailableSonarViewModel _selectedNetSonar;

        public bool IsAtLeastOneNetSonarPresent => _discovered.Count > 0;

        public List<string> NetSonarDisplayNames
        {
            get
            {
                return _netSonarDisplayNames;
            }
            set
            {
                _netSonarDisplayNames = value;
                RaisePropertyChanged(() => NetSonarDisplayNames);
            }
        }

        public int IndexOfSelectedNetSonar
        {
            get;
            set;
        }

        public ICommand NetSonarChanged => new RelayCommand(NetSonarChangedExecute, NetSonarChangedCanExecute);

        public AvailableSonarViewModel SelectedNetSonar
        {
            get
            {
                return _selectedNetSonar;
            }
            set
            {
                _selectedNetSonar = value;
                RaisePropertyChanged(() => SelectedNetSonar);
            }
        }

        public AllAvailableNetSonarViewModel()
        {
            _discovered = new List<SonarInfo>();
            IndexOfSelectedNetSonar = 0;
        }

        private void NetSonarChangedExecute()
        {
            int sonarIndex = IndexOfSelectedNetSonar;
            if (sonarIndex >= 0 && sonarIndex < _discovered.Count)
            {
                SonarInfo selected = _discovered[sonarIndex];
                SelectedNetSonar = new AvailableSonarViewModel(selected);
            }
        }

        private bool NetSonarChangedCanExecute()
        {
            return _discovered.Count() > 0;
        }

        public void UpdateFrom(IEnumerable<SonarInfo> discovered)
        {
            _discovered = new List<SonarInfo>(discovered);
            RaisePropertyChanged(() => IsAtLeastOneNetSonarPresent);
            if (discovered.Count() != 0)
            {
                List<string> names = new List<string>();
                foreach (SonarInfo sonar in discovered)
                {
                    string s = string.IsNullOrEmpty(sonar.Name) ? (string.IsNullOrEmpty(sonar.SonarModelName) ? $"Unknown sonar at {sonar.IpAddress}" : sonar.SonarModelName) : $"{sonar.Name} ({sonar.SonarModelName})";
                    names.Add(s);
                }
                NetSonarDisplayNames = names;
                if (NetSonarDisplayNames != null && IndexOfSelectedNetSonar >= 0 && IndexOfSelectedNetSonar < NetSonarDisplayNames.Count)
                {
                    NetSonarChangedExecute();
                }
            }
        }
    }


}
