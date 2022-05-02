// ProViewer4.Interfaces.IProViewerExceptionViewModel
using System;
using System.Windows.Input;

namespace ProViewer4.Interfaces
{
    public interface IProViewerExceptionViewModel
    {
        Exception CurrentException
        {
            get;
            set;
        }

        string CustomerEmail
        {
            get;
            set;
        }

        string Description
        {
            get;
            set;
        }

        string Version
        {
            get;
            set;
        }

        ICommand SubmitCommand
        {
            get;
        }

        bool CanSubmit
        {
            get;
        }

        string ExtraInformation
        {
            get;
            set;
        }
    }
}