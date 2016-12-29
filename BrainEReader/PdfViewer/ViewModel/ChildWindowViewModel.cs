using System.Windows.Input;
using EEGPdfViewer.Infrastructure;
using EEGPdfViewer.Services;

namespace EEGPdfViewer.ViewModel
{
	public class ChildWindowViewModel: ViewModelBase
	{
		private readonly IChildWindow _childWindow;
		private readonly IChildWindowNested _childWindowNested;
		private ICommand _cancelCommand;
		private ICommand _okCommand;
		private ICommand _openCommand;
        private bool _modeIsChecked;
		public ChildWindowViewModel(IChildWindow childWindow)
					//, IChildWindowNested childWindowNested)
		{
			_childWindow = childWindow;
			//_childWindowNested = childWindowNested;
            _modeIsChecked = false;
		}

		public ICommand OpenCommand
		{
			get
			{
				return _openCommand ?? (_openCommand =
					new DelegateCommand(OpenClick));
			}
		}

		public ICommand CancelCommand
		{
			get
			{
				return _cancelCommand ?? (_cancelCommand =
					new DelegateCommand(CancelClick));
			}
		}

		public ICommand OkCommand
		{
			get
			{
				return _okCommand ?? (_okCommand =
					new DelegateCommand(OkClick));
			}
		}

        public bool ModeIsChecked
        {
            get
            {
                return _modeIsChecked;
            }
            set
            {
                _modeIsChecked = value;
                OnPropertyChanged("ModeIsChecked");
            }
        }
		private void CancelClick()
		{
            _childWindow.DialogResult = false;
			_childWindow.Close();
		}

		private void OpenClick()
		{
			_childWindowNested.SetOwner(_childWindow);
			_childWindowNested.ShowDialog();
		}

		private void OkClick()
		{
			_childWindow.DialogResult = true;
			_childWindow.Close();
		}


	}
}