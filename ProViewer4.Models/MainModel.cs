#define TRACE
using BlueView.Sonar.Interfaces;
using BlueView.Sonar.Mock;
using BlueView.Sonar.SdkWrapper;
using BlueView.Sonar.SML;
using BlueView.Unit;
using BVTSDK;
using ProViewer4;
using ProViewer4.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace ProViewer4.Models
{
    [Serializable]
    [XmlType(TypeName = "ProViewerObservableObject")]
    public abstract class ObservableObject : BlueView.Wpf.Helpers.ObservableObject
    {
        //[NonSerialized]
        //private DependencyObject _dummyDependencyObject;

        /*
        public bool IsInDesignMode
        {
            get
            {
                if (Application.Current.Dispatcher.Thread != Thread.CurrentThread)
                {
                    return false;
                }
                if (_dummyDependencyObject == null)
                {
                    _dummyDependencyObject = new DependencyObject();
                }

                return System.ComponentModel.DesignerProperties.GetIsInDesignMode(_dummyDependencyObject);
            }
        }
        */
    }

    public sealed class MainModel : ObservableObject, IDisposable
    {
        private IModelFactory _ModelFactory;

        private SonarController _SonarRun;

        //private Tracker _tracker = new Tracker();

        private bool _IsShowingAdvancedSettings;

        public bool IsConnectedFile
        {
            get
            {
                if (Sonar == null)
                {
                    return false;
                }
                return Sonar.IsConnectedFile;
            }
        }

        public bool IsConnectedNetwork
        {
            get
            {
                if (Sonar == null)
                {
                    return false;
                }
                return Sonar.IsConnectedNetwork;
            }
        }

        public SonarModel Sonar
        {
            get;
            private set;
        }

        public UnitViewModel Units
        {
            get;
            private set;
        }

        public IColorMapper ColorMapper
        {
            get;
            private set;
        }

        public SonarController SonarRun
        {
            get
            {
                return _SonarRun;
            }
            private set
            {
                if (_SonarRun != null)
                {
                    _SonarRun.PingMarkerReached -= OnSonarPingMarkerReached;
                }
                _SonarRun = value;
                if (_SonarRun != null)
                {
                    _SonarRun.PingMarkerReached += OnSonarPingMarkerReached;
                }
                RaisePropertyChanged(() => SonarRun);
            }
        }

        public IModelFactory ModelFactory
        {
            get
            {
                return _ModelFactory;
            }
            set
            {
                _ModelFactory = value;
                RaisePropertyChanged(() => ModelFactory);
            }
        }

       //public Tracker Tracker => _tracker;

        public bool IsShowingAdvancedSettings
        {
            get
            {
                return _IsShowingAdvancedSettings;
            }
            set
            {
                _IsShowingAdvancedSettings = value;
                RaisePropertyChanged(() => IsShowingAdvancedSettings);
            }
        }

        public event EventHandler<EventArgs> SonarDisconnected;

        public event EventHandler<EventArgs> SonarConnected;

        public event EventHandler<PingEventArgs> SonarPingReceived;

        public event EventHandler<NavDataEventArgs> SonarNavDataReceived;

        public event EventHandler<NavDataEventArgs> NewNavDataReceived;

        public event EventHandler<EventArgs> SonarPingMarkerReached;

        public MainModel()
        {
            _ModelFactory = new BlueView.Sonar.Mock.ModelFactory();
            ISonar s = _ModelFactory.CreateSonar();
            ColorMapper = _ModelFactory.CreateColorMapper();
            SonarRun = new SonarController(s, 0);
            Sonar = new ProViewer4.Models.SonarModel(_SonarRun, ColorMapper);
            Units = UnitViewModel.ViewModel;
            Sonar.SonarConnected += OnSonarConnected;
            Sonar.SonarDisconnected += OnSonarDisconnected;
            Sonar.NavDataReceived += OnSonarNavDataReceived;
            Sonar.PingReceived += OnSonarPingReceived;
        }

        public void Init()
        {
        }

        public void Close()
        {
            if (SonarRun != null && SonarRun.IsRunning)
            {
                SonarRun.Stop();
            }
        }

        private void OnSonarConnected(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => IsConnectedFile);
            RaisePropertyChanged(() => IsConnectedNetwork);
            if (this.SonarConnected != null)
            {
                this.SonarConnected(sender, e);
            }
        }

        private void OnSonarDisconnected(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => IsConnectedFile);
            RaisePropertyChanged(() => IsConnectedNetwork);
            if (this.SonarDisconnected != null)
            {
                this.SonarDisconnected(sender, e);
            }
        }

        private void OnSonarPingReceived(object sender, PingEventArgs e)
        {
            if (this.SonarPingReceived != null)
            {
                this.SonarPingReceived(sender, e);
            }
        }

        private void OnSonarPingMarkerReached(object sender, EventArgs e)
        {
            if (this.SonarPingMarkerReached != null)
            {
                this.SonarPingMarkerReached(sender, e);
            }
        }

        private void OnSonarNavDataReceived(object sender, NavDataEventArgs e)
        {
            if (this.SonarNavDataReceived != null)
            {
                this.SonarNavDataReceived(sender, e);
            }
        }

        internal void PutNewNavData(string id, INavData navData)
        {
            if (this.NewNavDataReceived != null)
            {
                this.NewNavDataReceived(this, new NavDataEventArgs(id, navData));
            }
        }

        private bool CheckAndCreateRealSdkFactory()
        {
            if (_ModelFactory is BlueView.Sonar.SdkWrapper.ModelFactory)
            {
                return false;
            }
            IModelFactory factory = new BlueView.Sonar.SdkWrapper.ModelFactory();
            IColorMapper colorMapper = factory.CreateColorMapper();
            if (SonarRun != null)
            {
                SonarRun.Stop();
            }
            SonarRun = null;
            ModelFactory = factory;
            ColorMapper = colorMapper;
            RaisePropertyChanged(() => ColorMapper);
            RaisePropertyChanged(() => ModelFactory);
            return true;
        }

        public void DisconnectSonar()
        {
            if (SonarRun != null)
            {
                SonarRun.Stop();
            }
        }

        private bool InitializeControllerForNewSonar(string type, string location, int head = 0)
        {
            if (SonarRun != null)
            {
                SonarRun.Stop();
            }
            ISonar s = _ModelFactory.CreateSonar();
            if (s == null)
            {
                Trace.TraceError("No sonar");
                return false;
            }
            try
            {
                s.Open(type, location);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                return false;
            }
            if (s.HeadCount < head + 1)
            {
                return false;
            }
            SonarController sonarController = new SonarController(s, head);
            sonarController.IsDelayingLivePings = Sonar.IsDelayingLivePings;
            sonarController.RefreshRateMS = (int)(Sonar.RefreshRate * 1000.0);
            SonarController sonarRun = sonarController;
            SonarController oldRun = SonarRun;
            sonarRun.IsRealTime = true;
            SonarRun = sonarRun;
            oldRun?.Dispose();
            Sonar.UpdateSonar(ColorMapper, SonarRun);
            SonarRun.Start();
            return true;
        }

        public bool OpenSonarFile(string filename)
        {
            CheckAndCreateRealSdkFactory();
            if (!File.Exists(filename))
            {
                Trace.TraceError("no such file {0}", filename);
                return false;
            }
            if (!InitializeControllerForNewSonar("FILE", filename))
            {
                Trace.TraceError("Failed to open File");
                return false;
            }
            return true;
        }

        public bool OpenSonarOnNetwork(string ip, int head = 0)
        {
            CheckAndCreateRealSdkFactory();
            if (!InitializeControllerForNewSonar("NET", ip, head))
            {
                Trace.TraceError("Failed to open sonar on IP " + ip);
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            if (_SonarRun != null)
            {
                _SonarRun.Dispose();
            }
            //if (_tracker != null)
            //{
            //    _tracker.Dispose();
            //}
        }
    }


}
