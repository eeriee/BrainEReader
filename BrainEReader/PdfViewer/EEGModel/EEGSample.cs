using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EEGPdfViewer.EEGModel
{
    public class EEGSample
    {
        public string Label { get; set; }
        public string Time { get; set; }
        public double[] Raw_Values { get; set; }
        public byte Signal_Quality { get; set; }
        public double Attention { get; set; }
        public double Meditation { get; set; }
        public double[] EEG_Power { get; set; }

    }
}
