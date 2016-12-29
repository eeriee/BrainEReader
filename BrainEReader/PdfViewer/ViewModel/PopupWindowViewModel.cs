using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Forms;
using System.Reflection;
using System.Media;
using System.Windows.Threading;
using System.Data;

namespace EEGPdfViewer.ViewModel
{
    class PopupWindowViewModel: ViewModelBase
    {
        public PopupWindowViewModel()
        {
            _timertext = "";
            _confirmtext = "";
            _infoisopen = false;
            _confirmisopen = false;
            _confirmtitle = "";
        }

        public string ConfirmTitle
        {
            get
            {
                return _confirmtitle;
            }
            set
            {
                if (value != _confirmtitle)
                {
                    _confirmtitle = value;
                    OnPropertyChanged("ConfirmTitle");
                }
            }
        }

        public string ConfirmText
        {
            get
            {
                return _confirmtext;
            }
            set
            {
                if (value != _confirmtext)
                {
                    _confirmtext = value;
                    OnPropertyChanged("ConfirmText");
                }
            }
        }

        public string TimerText
        {
            get
            {
                return _timertext;
            }
            set
            {
                if (value != _timertext)
                {
                    _timertext = value;
                    OnPropertyChanged("TimerText");
                }
            }
        }

        public bool InfoIsOpen
        {
            get
            {
                return _infoisopen;
            }
            set
            {
                if (value != _infoisopen)
                {
                    _infoisopen = value;
                    OnPropertyChanged("InfoIsOpen");
                }
            }
        }

        public bool ConfirmIsOpen
        {
            get
            {
                return _confirmisopen;
            }
            set
            {
                if (value != _confirmisopen)
                {
                    _confirmisopen = value;
                    OnPropertyChanged("ConfirmIsOpen");
                }
            }
        }
        private string _confirmtitle, _confirmtext, _timertext;
        private bool _infoisopen, _confirmisopen;
        
    }
}
