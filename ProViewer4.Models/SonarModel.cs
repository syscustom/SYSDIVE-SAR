using BlueView.SML;
using BlueView.Sonar.Interfaces;
using BlueView.Sonar.Model;
using BlueView.Sonar.SdkWrapper;
using BlueView.Sonar.SML;
using ProViewer4;
using ProViewer4.Models;
using System;

namespace ProViewer4.Models
{

    public class PingImageEventArgs : EventArgs
    {
        public IPing Ping
        {
            get;
            private set;
        }

        public IImage TrackedImage
        {
            get;
            private set;
        }

        public PingImageEventArgs(IPing ping, IImage image)
        {
            Ping = ping;
            TrackedImage = image;
        }
    }

    public class SonarModel : BlueView.Sonar.Model.SonarModel
    {
        private ImageGeneratorThreadProxy _imageGenerator = new ImageGeneratorThreadProxy();

        private IModelFactory _factory;

        private IPing _LastShownPing;

        private object _ptQueryLock = new object();

        private double _panAngle = double.NaN;

        private double _tiltAngle = double.NaN;

        private bool _isPtConnected;

        public bool IsTrackerActive
        {
            get;
            set;
        }

        public bool IsConnectedFile
        {
            get
            {
                if (base.Sonar == null)
                {
                    return false;
                }
                return base.Sonar.IsFile;
            }
        }

        public bool IsConnectedNetwork
        {
            get
            {
                if (base.Sonar == null)
                {
                    return false;
                }
                if (base.Sonar.IsFile)
                {
                    return false;
                }
                if (!base.IsSonarConnected)
                {
                    return false;
                }
                return base.Sonar.IsConnected;
            }
        }

        public bool FreezeImage
        {
            get;
            set;
        }

        public event EventHandler<PingImageEventArgs> TrackDataReceived;

        public SonarModel(SonarController sonarRun, IColorMapper colorMapper)
            : base(sonarRun, colorMapper)
        {
            _factory = new ModelFactory();
        }

        protected override void RegisterSonarThreads(SonarController SonarRun)
        {
            base.RegisterSonarThreads(SonarRun);
            SonarRun.RegisterSonarCalculationThread<TrackThreadProperties>("SonarModel.TrackCalculation", TrackDataCalculation, TrackDataExpose);
            SonarRun.RegisterSonarCalculationThread<int>("SonarModel.ModifyPingWithPanTiltPositions", ModifyPingWithPanTiltPositions, null);
        }

        public override void RedrawImage(bool forceNewPing)
        {
            if (_LastShownPing != null && _SonarRun != null && (_SonarRun.IsPaused || FreezeImage))
            {
                InternalRedrawImage(_LastShownPing, forceNewPing);
            }
        }

        private void TrackDataCalculation(ref TrackThreadProperties properties, IPing ping, bool external)
        {
            if (!IsTrackerActive)
            {
                return;
            }
            if (properties._lastHead != ping.Head || properties._lastStartRange != ping.StartRange || properties._lastStopRange != ping.StopRange)
            {
                properties._lastStartRange = ping.StartRange;
                properties._lastStopRange = ping.StopRange;
                properties._lastHead = ping.Head;
                _imageGenerator.SetImageFilterFlags(ImageFilter.None);
                float rangeDiff = ping.StopRange - ping.StartRange;
                float rangeRes = rangeDiff / 500f;
                float degreesPerRangeResUnit = (float)(2.0 * Math.Atan(rangeRes / (2f * ping.StopRange)) * (180.0 / Math.PI));
                float minAngleInDegrees;
                float maxAngleInDegrees;
                ping.GetFOV(out minAngleInDegrees, out maxAngleInDegrees);
                float angularSpan = maxAngleInDegrees - minAngleInDegrees;
                int acrossTrackPixCount = (int)Math.Round(angularSpan / degreesPerRangeResUnit);
                if (acrossTrackPixCount > 500)
                {
                    int newAlongTrackPixelCount = (int)Math.Round(250000f / (float)acrossTrackPixCount);
                    rangeRes = rangeDiff / (float)newAlongTrackPixelCount;
                    acrossTrackPixCount = 500;
                }
                _imageGenerator.SetRangeResolution(rangeRes);
                _imageGenerator.SetRThetaImageWidth(acrossTrackPixCount);
            }
            properties._Image = _imageGenerator.GetImageRTheta(ping, null);
        }

        private void TrackDataExpose(ref TrackThreadProperties properties, IPing ping, bool external)
        {
            if (IsTrackerActive && this.TrackDataReceived != null)
            {
                this.TrackDataReceived(this, new PingImageEventArgs(ping, properties._Image));
            }
            if (!FreezeImage)
            {
                _LastShownPing = ping;
            }
        }

        internal void OnNewPanTiltPosition(object sender, PanTiltEventArgs e)
        {
            lock (_ptQueryLock)
            {
                _panAngle = e.PanAngle;
                _tiltAngle = e.TiltAngle;
            }
        }

        internal void OnPanTiltStatusChanged(object sender, PanTiltEventArgs e)
        {
            lock (_ptQueryLock)
            {
                if (e.State == PanTiltState.Disconnected)
                {
                    _isPtConnected = false;
                }
                else
                {
                    _isPtConnected = true;
                }
            }
        }

        private void ModifyPingWithPanTiltPositions(ref int discard, IPing ping, bool external)
        {
            double pan;
            double tilt;
            bool connected;
            lock (_ptQueryLock)
            {
                pan = _panAngle;
                tilt = _tiltAngle;
                connected = _isPtConnected;
            }
            if (!double.IsNaN(pan) && !double.IsNaN(tilt) && connected)
            {
                IOrientation o = _factory.CreateOrientation();
                o.Data = new OrientationData
                {
                    source = OrientationObject.Positioner,
                    target = OrientationObject.Head,
                    X_axis_degrees = pan,
                    Y_axis_degrees = tilt
                };
                ping.SetPositionerOrientation(o);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

}
