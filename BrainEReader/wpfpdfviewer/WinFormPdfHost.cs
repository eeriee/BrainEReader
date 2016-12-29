using System.ComponentModel;
using System.Windows.Forms;

namespace WPFPdfViewer
{
    public partial class WinFormPdfHost : UserControl
    {
        public WinFormPdfHost()
        {
            InitializeComponent();
        }


        public void LoadFile(string path, int page)
        {          
            axAcroPDF1.LoadFile(path);        
            axAcroPDF1.src = path;
            axAcroPDF1.setViewScroll("FitV", 0);
           
            axAcroPDF1.setPageMode("none");
            //axAcroPDF1.setView("full");
            axAcroPDF1.setShowScrollbars(false);
            axAcroPDF1.setShowToolbar(false);
            axAcroPDF1.setCurrentPage(page);
            //axAcroPDF1.setZoom(100);
           

          /*  avDoc.Open(path,"");
            avDoc.SetViewMode(1);
            pdDoc = (CAcroPDDoc)avDoc.GetPDDoc();
            avPageView = (AcroAVPageView)avDoc.GetAVPageView();
            pdPage = (CAcroPDPage)avPageView.GetPage();            */
     
        }

        public void GoToPrev()
        {
            axAcroPDF1.gotoPreviousPage();
        }

        public void GoToNext()
        {
            axAcroPDF1.gotoNextPage();
        }

        public void SetPage(int page)
        {
            axAcroPDF1.setCurrentPage(page);
        }
      /* public int GetCurrentPage()
        {
            return pdPage.GetNumber();
        }

        public int GetTotalPage()
        {
            return avPageView.GetPageNum();
        }
        public void SetShowToolBar(bool on)
        {
            axAcroPDF1.setShowToolbar(on);
        } */
    }
}
