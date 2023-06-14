using AudioPlayer.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioPlayer.Handlers
{
    public class Mp3TrackHandler
    {
        public Mp3TrackHandler(byte[] trackData)
        {
            TrackData = trackData;
            HeaderData = new HeaderData(this);
            FrameData= new FrameData(this);
        }

        internal byte[] TrackData { get; set; }
        public HeaderData HeaderData { get; set; }
        public FrameData FrameData { get; set; }
    }

    public class HeaderData
    {
        public HeaderData(Mp3TrackHandler mp3TrackHandler)
        {
            Mp3TrackHandler = mp3TrackHandler;
        }
        private Mp3TrackHandler Mp3TrackHandler { get; set; }

        public string TagIdentifier()
        {
            var returnString = "";

            try
            {
                var selArray = Mp3TrackHandler.TrackData.Take(3).ToArray();
                if (selArray != null)
                {
                    returnString = Encoding.ASCII.GetString(selArray);
                }

            }
            catch { }
            

            return returnString;
        }

        public string TagVersion()
        {
            var returnString = "";

            var selArray = Mp3TrackHandler.TrackData.Skip(3).Take(2).ToArray();
            if (selArray != null)
            {
                returnString = selArray[0].ToString() + "." + selArray[1].ToString();
            }


            return returnString;
        }

        public int TagSize()
        {
            //Including Header i calulation which is always 10 byte long.
            var returnInteger = 10;

            var selArray = Mp3TrackHandler.TrackData.Skip(6).Take(4).ToArray();
            if (selArray != null)
            {
                returnInteger += selArray[3] + (selArray[2] << 7) + (selArray[1] << 14) + (selArray[0] << 7);
            }

            //Including Header i calulation.
            return returnInteger;
        }
    }

    public class FrameData
    {
        public FrameData(Mp3TrackHandler mp3TrackHandler)
        {
            Mp3TrackHandler = mp3TrackHandler;
        }

        private Mp3TrackHandler Mp3TrackHandler { get; set; }

        //private Dictionary<string, string> ReturnData { get; set; }


        public List<TrackFrameData> ExtracedFrameData(int tagsize)
        {
            var returnData= new List<TrackFrameData>();

            //A tracks first 10 bytes are always Header Data, so lets skip those (0 indexed value, therefore 9).
            int frameCounter = 9;
            while (true) 
            {
                try
                {
                    //Extract the frame header (10 bytes)
                    var selectedFrameHeader = Mp3TrackHandler.TrackData.Skip(frameCounter + 1).Take(10).ToArray();

                    //Byte 0 - 3 contains the 
                    var frameIdentifierData = selectedFrameHeader.Take(4).ToArray();

                    var frameLengthData = selectedFrameHeader.Skip(4).Take(4).ToArray();
                    var frameLength = 0;

                    if (frameLengthData != null)
                    {
                        frameLength += frameLengthData[3] + (frameLengthData[2] << 8) + (frameLengthData[1] << 8) + (frameLengthData[0] << 8);
                    }

                    //Extract the frame data (found after the header data 10 bytes)
                    var selectedFrameData = Mp3TrackHandler.TrackData.Skip(frameCounter + 1 + 10).Take(frameLength).ToArray();

                    var frameKey = Encoding.ASCII.GetString(frameIdentifierData);
                    var frameData = Encoding.ASCII.GetString(selectedFrameData);

                    frameCounter += 10 + frameLength;

                    returnData.Add(new TrackFrameData() { FrameIdentifier = frameKey, FrameData = frameData });

                    if (frameCounter > tagsize)
                    {
                        break;
                    }
                }
                catch { break; }
            }

            return returnData;
        }

    }
}
