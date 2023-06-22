using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AudioPlayer.Classes
{
    public class DataGridObject
    {
        public DataGridObject() : base()
        {
            RowValues = new Dictionary<string, string>();
        }

        public Dictionary<string, string> RowValues { get; set; }
    }

    public class TrackFrameData
    {
        public string FrameIdentifier { get; set; }
        public string FrameData { get; set; }
    }

    public class ApplicationColorScheme
    {
        public SolidColorBrush? ApplicationBanner { get; set; }
        public SolidColorBrush? ApplicationLogo { get; set; }
        public SolidColorBrush? ApplicationBackground { get;set; }
        public SolidColorBrush? ApplicationButtonText { get; set; }
        public SolidColorBrush? ApplicationDatagridText { get; set; }
    }
}
