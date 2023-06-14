using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
