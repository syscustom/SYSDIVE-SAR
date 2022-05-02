using ProViewer4.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProViewer4.Interfaces
{
    public interface ITrackerSettingsViewModel
    {
        bool IsTrackerEnabled
        {
            get;
            set;
        }

        bool IsTrackerMaskShaded
        {
            get;
            set;
        }

        bool IsTrackerContextCurrent
        {
            get;
            set;
        }

        string DefaultTrackerConfiguration
        {
            get;
            set;
        }

        bool IsShowingOnlySelectedTargets
        {
            get;
            set;
        }

        bool IsShowingQuality
        {
            get;
            set;
        }

        bool IsShowingRangeAndBearing
        {
            get;
            set;
        }

        bool IsShowingLabels
        {
            get;
            set;
        }

        bool IsShowingVelocity
        {
            get;
            set;
        }

        ObservableCollection<TrackerConfigurationViewModel> TargetingTypes
        {
            get;
        }

        string TrackerStatusText
        {
            get;
        }

        ICommand ResetTrackerCommand
        {
            get;
        }

        ICommand ToggleLockCommand
        {
            get;
        }

        bool IsTargetLockRegionError
        {
            get;
            set;
        }

        bool IsTargetRegionLocked
        {
            get;
            set;
        }

        TrackerConfiguratorViewModel TrackerConfiguratorViewModel
        {
            get;
        }

        bool IsTrackerControlVisible
        {
            get;
            set;
        }

        ICommand ConfigureTrackerCommand
        {
            get;
        }

        event EventHandler<EventArgs> TargetBoxOptionsChanged;

        event EventHandler<EventArgs> TrackerStarted;

        event EventHandler<EventArgs> TrackerStopped;

        void SetLockRegionPathFigures(bool imageIsPolar, double theta1, double theta2, Point origin, Point pointRadial1Near, Point pointRadial1Far, Point pointRadial2Near, Point pointRadial2Far);

        void ResetTracker();

        void UpdateTrackerLockEnable();
    }

}
