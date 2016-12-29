using System.Drawing;
using System.Windows.Forms;

namespace WPFPdfViewer
{
    partial class WinFormPdfHost
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WinFormPdfHost));

            // 
            // axAcroPDF1
            // 
            this.axAcroPDF1 = new AxAcroPDFLib.AxAcroPDF();
            ((System.ComponentModel.ISupportInitialize)(this.axAcroPDF1)).BeginInit();
            this.SuspendLayout();
            this.axAcroPDF1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axAcroPDF1.Enabled = true;
            this.axAcroPDF1.Location = new System.Drawing.Point(0, 0);
            this.axAcroPDF1.Name = "axAcroPDF1";
            this.axAcroPDF1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axAcroPDF1.OcxState")));
            this.axAcroPDF1.Size = new System.Drawing.Size(150, 150);
            this.axAcroPDF1.TabIndex = 0;
            //  

            /*mApp = new AcroAppClass();
            mApp.Show();

            this.avDoc = new AcroAVDocClass();
            //((System.ComponentModel.ISupportInitialize)(this.avDoc)).BeginInit();
            this.SuspendLayout(); */


            // WinFormPdfHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.axAcroPDF1);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "WinFormPdfHost";
            this.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(this.axAcroPDF1)).EndInit();

        }

        #endregion

        public AxAcroPDFLib.AxAcroPDF axAcroPDF1;
    }
}
