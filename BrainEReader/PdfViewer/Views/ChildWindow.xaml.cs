using System.ComponentModel;
using System.Windows;
using Microsoft.Practices.Unity;
using EEGPdfViewer.Infrastructure;
using EEGPdfViewer.Services;
using EEGPdfViewer.ViewModel;
using System.Windows.Controls;

namespace EEGPdfViewer.Views
{
    /// <summary>
    /// Interaction logic for ChildWindow.xaml
    /// </summary>
    public partial class ChildWindow : Window, IChildWindow, INotifyPropertyChanged
    {
        private readonly IUnityContainer _container;
        private ChildWindowViewModel childWindowViewModel;
        private bool _popupIsChecked, _audioIsChecked, _curPopupChecked, _curAudioChecked, _modeIsChecked, _curModeChecked;
        private string _task, _class;

        public ChildWindow()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
            _container = UnityContainerResolver.Container;
            //var childWindowNested = _container.Resolve<IChildWindowNested>();   
            childWindowViewModel = new ChildWindowViewModel(this);
            Closing += ChildWindowClosing;
            _popupIsChecked = _audioIsChecked = _curPopupChecked = _curAudioChecked = _modeIsChecked = _curModeChecked = true;
            readModeSetting.DataContext = expModeSetting.DataContext = this;
            _task = "eye_states";
            _class = "1";
        }

        #region IChildWindow Members

        public void SetOwner(object window)
        {
            Owner = window as Window;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public bool ModeIsUnchecked
        {
            get
            {
                return !_modeIsChecked;
            }
            set
            {
                OnPropertyChanged("ModeIsUnchecked");
            }
        }

        public bool ModeIsExp
        {
            get
            {
                return !_curModeChecked;
            }
            set
            {
                OnPropertyChanged("ModeIsExp");
            }
        }

        public bool ModeIsRead
        {
            get
            {
                return _curModeChecked;
            }
            set
            {
                _curModeChecked = value;
                OnPropertyChanged("ModeIsRead");
            }
        }

        public bool IsDisplay()
        {
            if (this.Visibility == Visibility.Hidden)
                return false;
            return true;
        }
        #endregion


        public bool getPopupIsChecked()
        {
            return _popupIsChecked;
        }

        public bool getAudioIsChecked()
        {
            return _audioIsChecked;
        }

        public bool getModeIsChecked()
        {
            return _modeIsChecked;
        }
        public string getTask()
        {
            return _task;
        }

        public string getClass()
        {
            return _class;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
            //this.Close();
            checkPopup.IsChecked = _popupIsChecked;
            checkAudio.IsChecked = _audioIsChecked;
            readMode.IsChecked = _modeIsChecked;
            expMode.IsChecked = !_modeIsChecked;
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
            checkPopup.IsChecked = _popupIsChecked = _curPopupChecked;
            checkAudio.IsChecked = _audioIsChecked = _curAudioChecked;
            readMode.IsChecked = _modeIsChecked = _curModeChecked;
            expMode.IsChecked = !_modeIsChecked;
            ModeIsUnchecked = !_modeIsChecked;
        }
        private void ChildWindowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
            checkPopup.IsChecked = _popupIsChecked;
            checkAudio.IsChecked = _audioIsChecked;
            readMode.IsChecked = _modeIsChecked;
            expMode.IsChecked = !_modeIsChecked;
        }

        private void CheckPopupChanged(object sender, RoutedEventArgs e)
        {
            _curPopupChecked = (bool)(sender as CheckBox).IsChecked;

        }

        private void CheckAudioChanged(object sender, RoutedEventArgs e)
        {
            _curAudioChecked = (bool)(sender as CheckBox).IsChecked;
        }
        private void CheckModeChanged(object sender, RoutedEventArgs e)
        {
            ModeIsRead = (bool)(sender as RadioButton).IsChecked;
            ModeIsExp = !_curModeChecked;
        }

        private void Task_TextChanged(object sender, TextChangedEventArgs e)
        {
            string tmp = (sender as TextBox).Text;
            if (tmp != "" && tmp != null)
                _task = tmp;

        }

        private void Class_TextChanged(object sender, TextChangedEventArgs e)
        {
            string tmp = (sender as TextBox).Text;
            if (tmp != "" && tmp != null)
                _class= tmp;
        }
    }
}