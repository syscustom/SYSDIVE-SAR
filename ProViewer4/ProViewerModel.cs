#define TRACE
using BlueView.SML;
using BlueView.Sonar.Interfaces;
using BlueView.Sonar.Model;
using BlueView.Unit.Attributes;
using BVTSDK;
using ProViewer4;
using ProViewer4.Controls;
using ProViewer4.ViewModels;
using SML;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ProViewer4
{
    [UnitContainer]
    public sealed class ProViewerModel : ObservableObject, IDisposable
    {
        public class HeadWithImage
        {
            public IHead Head
            {
                get;
                set;
            }

            public IImage Image
            {
                get;
                set;
            }
        }

        private enum SonarLocationType
        {
            File,
            Net
        }

        private const int _MaxPingsInFlight = 1;

        private readonly ProViewerViewModel _appVmHack;

        private readonly NetworkInterface _networkInterface;

        private ISonar _Sonar;

        private string _fileName;

        private int _indexOfSelectedHead;

        private NavigationManager _navManager;

        private FileManager _fileManager;

        private PanTiltManager _panTiltManager;

        private readonly object _isProcessing = new object();

        private bool _isImageFrozen;

        private bool _isSavingNavAsync;

        private bool _Recording;

        public bool Recording => _Recording;

        public string CurrentRecordFile
        {
            get
            {
                if (_fileManager != null)
                {
                    return _fileManager.CurrentFileName;
                }
                return "";
            }
        }

        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
            }
        }

        public bool FreezeImage
        {
            get
            {
                return _isImageFrozen;
            }
            set
            {
                if (_isImageFrozen != value)
                {
                    _isImageFrozen = value;
                }
                RaisePropertyChanged(() => FreezeImage);
                _appVmHack.MainModel.Sonar.FreezeImage = value;
            }
        }

        public NavigationManager GPSMgr => _navManager;

        public bool IsSavingNavAsync
        {
            get
            {
                return _isSavingNavAsync;
            }
            set
            {
                if (_fileManager != null)
                {
                    _fileManager.NavIsAsync = value;
                }
                _isSavingNavAsync = value;
            }
        }

        public double MaxRecordFileSizeMb
        {
            get
            {
                if (_fileManager != null)
                {
                    return _fileManager.MaxFileSizeMb;
                }
                return 0.0;
            }
            set
            {
                if (_fileManager != null)
                {
                    _fileManager.MaxFileSizeMb = value;
                }
            }
        }

        public INavData CurrentNav
        {
            get;
            private set;
        }

        public double NavStaleTimeout
        {
            get
            {
                if (_fileManager != null)
                {
                    return _fileManager.NavStaleTimeout;
                }
                return -1.0;
            }
            set
            {
                if (_fileManager != null)
                {
                    _fileManager.NavStaleTimeout = value;
                }
            }
        }

        public PanTiltManager PTMgr
        {
            get
            {
                return _panTiltManager;
            }
            private set
            {
                if (_panTiltManager != null)
                {
                    _panTiltManager.Dispose();
                }
                _panTiltManager = value;
            }
        }

        public double RecordFileSize
        {
            get
            {
                if (_fileManager != null)
                {
                    return _fileManager.FileSizeMb;
                }
                return 0.0;
            }
        }

        public int SonarDiscoveryTimeout
        {
            get;
            set;
        }

        public List<SonarInfo> SonarInfo
        {
            get;
            private set;
        }

        public event EventHandler<RoutedEventArgs> SonarInfoUpdated;

        public event EventHandler<NavDataEventArgs> NewNavDataReceived;

        public event EventHandler<EventArgs> NavStatusChanged;

        public ProViewerModel(ProViewerViewModel appVm, NetworkInterface networkInterface)
        {
            _networkInterface = networkInterface;
            _appVmHack = appVm;
            appVm.MainModel.Sonar.SonarChanged += Sonar_SonarChanged;
            appVm.MainModel.Sonar.AllSonarDataCalculated += Sonar_AllSonarDataCalculated;
            appVm.MainModel.PropertyChanged += MainModel_PropertyChanged;
            _Sonar = appVm.MainModel.Sonar.Sonar;
            App.GlobalApplicationViewModel.TrackerViewModel.PingProcessed += TrackerViewModel_PingProcessed;
            _panTiltManager = new PanTiltManager();
            _panTiltManager.Connected += _appVmHack.MainModel.Sonar.OnPanTiltStatusChanged;
            _panTiltManager.Disconnected += _appVmHack.MainModel.Sonar.OnPanTiltStatusChanged;
            _panTiltManager.PanOrTiltAngleChanged += _appVmHack.MainModel.Sonar.OnNewPanTiltPosition;
            SonarDiscoveryTimeout = 1;
            _indexOfSelectedHead = 0;
            FileName = "";
        }

        private void MainModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string propertyName;
            if ((propertyName = e.PropertyName) != null && propertyName == "ModelFactory")
            {
                if (_navManager != null)
                {
                    stopNav();
                    _navManager.NewData -= OnNavDataReceived;
                    _navManager.StatusChanged -= OnNavStatusChanged;
                    _navManager.Dispose();
                }
                _navManager = new NavigationManager(_appVmHack.MainModel.ModelFactory);
                _navManager.NewData += OnNavDataReceived;
                _navManager.StatusChanged += OnNavStatusChanged;
            }
        }

        private void OnNavStatusChanged(object sender, EventArgs e)
        {
            if (this.NavStatusChanged != null)
            {
                this.NavStatusChanged(sender, e);
            }
        }

        private void TrackerViewModel_PingProcessed(object sender, TrackEventArgs e)
        {
            StreamDataProducts(e.Ping, _Sonar, null, null, null, e);
        }

        private void Sonar_AllSonarDataCalculated(object sender, AllSonarDataCalculatedEventArgs e)
        {
            INavData nav = e.Ping.HasNavData ? e.Ping.NavDataCopy : CurrentNav;
            StreamDataProducts(e.Ping, _Sonar, e.ImageXy, e.ImageRTheta, nav, null);
        }

        private void Sonar_SonarChanged(object sender, SonarEventArgs e)
        {
            _Sonar = e.Sonar;
        }

        public void FindSonars()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate
            {
                ProViewerModel proViewerModel = this;
                SonarDiscovery sonarDiscovery = new SonarDiscovery
                {
                    Timeout = SonarDiscoveryTimeout
                };
                SonarInfo[] temp = null;
                try
                {
                    temp = sonarDiscovery.FindSonar();
                }
                catch (Exception exception)
                {
                    App.The.Interlocution.NotifyUserOfError(exception);
                    return;
                }
                Dispatcher dispatcher = Application.Current.Dispatcher;
                dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate
                {
                    proViewerModel.SonarInfo = new List<SonarInfo>(temp);
                    proViewerModel.SonarInfo.Sort((SonarInfo left, SonarInfo right) => left.IpAddress.CompareTo(right.IpAddress));
                    if (proViewerModel.SonarInfoUpdated != null)
                    {
                        proViewerModel.SonarInfoUpdated(proViewerModel, new RoutedEventArgs());
                    }
                });
            };
            worker.RunWorkerAsync();
        }

        internal void TryConnectToNetworkSonarAt(string fixedIpAddress)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate
            {
                using (Sonar sonar = new Sonar())
                {
                    SonarInfo = new List<SonarInfo>();
                    try
                    {
                        sonar.Open("NET", fixedIpAddress);
                        List<HeadInfo> list = new List<HeadInfo>();
                        for (int i = 0; i < sonar.HeadCount; i++)
                        {
                            Head head = sonar.GetHead(i);
                            list.Add(new HeadInfo(head.HeadName, head.CenterFreq.ToString()));
                        }
                        SonarInfo item = new SonarInfo(fixedIpAddress, sonar.SonarModelName, list.ToArray(), sonar.FirmwareRevision, sonar.SonarName);
                        SonarInfo.Add(item);
                    }
                    catch (SdkException)
                    {
                        Trace.TraceInformation("Failed to connect to sonar at {0}", fixedIpAddress);
                    }
                }
                Dispatcher dispatcher = Application.Current.Dispatcher;
                dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate
                {
                    if (this.SonarInfoUpdated != null)
                    {
                        this.SonarInfoUpdated(this, new RoutedEventArgs());
                    }
                });
            };
            worker.RunWorkerAsync();
        }

        public void startNav()
        {
            _navManager.Start();
            if (_fileManager != null)
            {
                _navManager.NewData += _fileManager.OnNewNavDataReceived;
            }
        }

        public void stopNav()
        {
            _navManager.Stop(true);
            CurrentNav = null;
            if (_fileManager != null)
            {
                _navManager.NewData -= _fileManager.OnNewNavDataReceived;
            }
        }

        private void OnNavDataReceived(object sender, NavDataEventArgs e)
        {
            if (CurrentNav == null)
            {
                CurrentNav = e.Data.Clone();
            }
            else
            {
                CurrentNav.Merge(e.Data);
            }
            double time = (_appVmHack.Wedge == null) ? 0.0 : ((double)_appVmHack.Wedge.TimeStamp);
            double dt = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(time)).TotalSeconds;
            CurrentNav.ClearStaleFields(dt + NavStaleTimeout);
            if (this.NewNavDataReceived != null)
            {
                this.NewNavDataReceived(sender, new NavDataEventArgs(e.ID, CurrentNav));
            }
            _appVmHack.MainModel.PutNewNavData(e.ID, CurrentNav);
            StreamDataProducts(null, _Sonar, null, null, CurrentNav, null);
        }

        private void PromptUserForHeadSelectionIfNecessary(ISonar s)
        {
            if (!s.IsFile || s.HeadCount < 2)
            {
                return;
            }
            int numHeadsWithPings = 0;
            for (int j = 0; j < s.HeadCount; j++)
            {
                IHead h = s.GetHead(j);
                if (h.PingCount > 1)
                {
                    numHeadsWithPings++;
                }
            }
            if (numHeadsWithPings < 2)
            {
                return;
            }
            TaskScheduler @default = TaskScheduler.Default;
            List<HeadWithImage> headsWithPings = new List<HeadWithImage>();
            for (int i = 0; i < s.HeadCount; i++)
            {
                IHead head = s.GetHead(i);
                if (head.PingCount > 0)
                {
                    headsWithPings.Add(new HeadWithImage
                    {
                        Head = head,
                        Image = null
                    });
                }
            }
            HeadWithImage[] sorted = new HeadWithImage[headsWithPings.Count];
            headsWithPings.CopyTo(sorted, 0);
            Array.Sort(sorted, (HeadWithImage left, HeadWithImage right) => left.Head.HeadID.CompareTo(right.Head.HeadID));
            headsWithPings = new List<HeadWithImage>(sorted);
            if (headsWithPings.Count > 1)
            {
                Window w = new Window();
                w.MaxHeight = App.The.MainWindow.Height;
                w.WindowStyle = WindowStyle.None;
                w.SizeToContent = SizeToContent.WidthAndHeight;
                w.Owner = App.The.MainWindow;
                w.Margin = App.The.MainWindow.Margin;
                w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                HeadSelectionControl headPicker = new HeadSelectionControl();
                headPicker.headList.ItemsSource = headsWithPings;
                w.Content = headPicker;
                headPicker.headList.SelectionChanged += delegate
                {
                    if (headPicker.headList.SelectedItem != null)
                    {
                        HeadWithImage headWithImage = (HeadWithImage)headPicker.headList.SelectedItem;
                        _indexOfSelectedHead = headWithImage.Head.HeadID;
                    }
                };
                headPicker.ContinueButton.Click += delegate
                {
                    w.Close();
                };
                headPicker.headList.MouseDoubleClick += delegate
                {
                    w.Close();
                };
                w.ShowDialog();
            }
        }

        private void OnTestRunEnded(object sender, EventArgs e)
        {
            Trace.TraceInformation("Test run is finished, exiting program.");
            if (App.The.CommandLineOptions.IsExitSpecified)
            {
                Application.Current.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            }
        }

        private void StreamDataProducts(IPing ping, ISonar sonar, IImage xy, IImage rtheta, INavData nav, TrackEventArgs tracks)
        {
            if (_networkInterface.Running && _networkInterface.ConnectedClients >= 1)
            {
                IImage image = null;
                if (xy != null)
                {
                    image = xy;
                }
                else if (rtheta != null)
                {
                    image = rtheta;
                }
                else if (tracks != null)
                {
                    image = tracks.TrackedImage;
                }
                if (image != null)
                {
                    StreamingSuperPing ssp = new StreamingSuperPing(ping, sonar, (float)image.FOVMinAngle, (float)image.FOVMaxAngle);
                    byte[] bytes = ssp.MakeProViewerFrame(_appVmHack.StreamXyImage ? xy : null, _appVmHack.StreamRthetaImage ? rtheta : null, _appVmHack.StreamNavdata ? nav : null, _appVmHack.StreamTargets ? tracks : null, _appVmHack.StreamPanTiltdata);
                    _networkInterface.PushMessageToSend(bytes);
                }
            }
        }

        public void saveSONStart(string filename)
        {
            if (!_Recording)
            {
                if (_fileManager != null)
                {
                    _fileManager.Dispose();
                }
                _fileManager = new FileManager(filename, _appVmHack.Wedge.SonarRun.Sonar, _appVmHack.MainModel.ModelFactory);
                if (_navManager.Running)
                {
                    _navManager.NewData += _fileManager.OnNewNavDataReceived;
                }
                _appVmHack.Wedge.SonarRun.PingReceived += _fileManager.OnNewPing;
                _fileManager.NavIsAsync = IsSavingNavAsync;
                _fileManager.Start();
                _Recording = true;
            }
        }

        public string[] SaveSonarStop()
        {
            string[] ret = _fileManager.Stop();
            _appVmHack.Wedge.SonarRun.PingReceived -= _fileManager.OnNewPing;
            _navManager.NewData -= _fileManager.OnNewNavDataReceived;
            if (_Recording)
            {
                _Recording = false;
            }
            return ret;
        }

        public void PutEventMark(string key, string text)
        {
            if (_fileManager != null)
            {
                _fileManager.PutEventMark(key, text);
            }
        }

        public void Dispose()
        {
            if (_navManager != null)
            {
                _navManager.Dispose();
                _navManager = null;
            }
            if (_panTiltManager != null)
            {
                _panTiltManager.Dispose();
                _panTiltManager = null;
            }
            if (_fileManager != null)
            {
                _fileManager.Dispose();
                _fileManager = null;
            }
        }
    }
}
