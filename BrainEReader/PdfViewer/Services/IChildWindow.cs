
namespace EEGPdfViewer.Services
{
    public interface IChildWindow
    {
        void Close();
        bool? ShowDialog();
        void SetOwner(object window);
        bool? DialogResult { get; set; }
        bool IsDisplay();
        bool getPopupIsChecked();
        bool getAudioIsChecked();
        bool getModeIsChecked();
        string getTask();
        string getClass();
    }
}
