using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JustMVVM;

namespace WpfApp1
{
    public class frmSonarDisplayViewModel : MVVMBase
    {
        private bool _fixAirspace;
        public bool FixAirspace
        {
            get { return _fixAirspace; }
            set
            {
                _fixAirspace = value;
                OnPropertyChanged();
            }
        }
    }
}
