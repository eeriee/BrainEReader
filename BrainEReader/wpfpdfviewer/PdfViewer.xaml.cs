using System.Windows.Controls;
using System.Threading;
using System.Windows.Media;
using System.Windows;
using System.IO;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;
using iTextSharp.text.xml;
using System.ComponentModel;
using System;

namespace WPFPdfViewer
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class PdfViewer : UserControl, INotifyPropertyChanged
    {
        public PdfViewer()
        {
            InitializeComponent();
            _winFormPdfHost = pdfHost.Child as WinFormPdfHost;
            _isDisplay = false;

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        public string currentPage
        {
            get
            {
                return _pdfPage.ToString();
            }
            set
            {
                if (value != _pdfPage.ToString())
                {
                    _pdfPage = Int32.Parse(value);
                    OnPropertyChanged("currentPage");
                }
            }
        }

        public string totalPage
        {
            get
            {
                return _totalPage.ToString();
            }
            set
            {
                if (value != _totalPage.ToString())
                {
                    _totalPage = Int32.Parse(value);
                    OnPropertyChanged("totalPage");
                }
            }
        }
        public string PdfPath
        {
            get { return _pdfPath; }
            set
            {
                _pdfPath = value;
                //_pdfPage = value;
                // LoadFile(_pdfPath);
            }
        }

        public bool IsDisplay()
        {
            return _isDisplay;
        }

        public void LoadFile(string path, int page)
        {
            _winFormPdfHost.LoadFile(path, page);
            _pdfPath = path;
            currentPage = page.ToString();
            _isDisplay = true;
            PdfReader pdfReader = new PdfReader(path);
            totalPage = pdfReader.NumberOfPages.ToString();
        }

        public void setIsDisplay(bool d)
        {
            _isDisplay = d;
        }

        public void GoToPrev()
        {
            _winFormPdfHost.GoToPrev();
            if (_pdfPage > 1)
            {
                currentPage = (_pdfPage - 1).ToString();
            }
        }

        public void GoToNext()
        {
            _winFormPdfHost.GoToNext();
            if(_pdfPage < _totalPage)
                currentPage = (_pdfPage + 1).ToString();
        }
        public void SetPage(int page)
        {
            _winFormPdfHost.SetPage(page);
            currentPage = page.ToString();
        }


        public string GetTotalPage()
        {
            return _totalPage.ToString();
        }

        private string _pdfPath;
        private bool _isDisplay;
        private int _pdfPage;
        private int _totalPage;
        private readonly WinFormPdfHost _winFormPdfHost;
    }
}
