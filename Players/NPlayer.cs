using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Players.NPlayer
{
    public delegate void Stopped();
    public delegate void Paused();
    public delegate void Resumed();

    public class NPlayer
    {
        public enum PlaybackStopTypes
        {
            PlaybackStoppedByUser, PlaybackStoppedReachingEndOfFile
        }

        public PlaybackStopTypes PlaybackStopType { get; set; }
        public enum PlayerStatus
        {
            Stopped,
            Paused,
            Resumed
        }

        private AudioFileReader _audioFileReader;

        private DirectSoundOut _output;

        public event Stopped PlayerStopped;
        public event Paused PlayerPaused;
        public event Resumed PlayerResumed;


        public NPlayer(string filepath, float volume)
        {
            PlaybackStopType = PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;

            _audioFileReader = new AudioFileReader(filepath) { Volume = volume };

            _output = new DirectSoundOut(200);
            _output.PlaybackStopped += _output_PlaybackStopped;

            var wc = new WaveChannel32(_audioFileReader);
            wc.PadWithZeroes = false;

            _output.Init(wc);
        }

        public void Play(PlaybackState playbackState, double currentVolumeLevel)
        {
            if (playbackState == PlaybackState.Stopped || playbackState == PlaybackState.Paused)
            {
                _output.Play();
                OnProcessCompleted(PlayerStatus.Resumed);
            }

            _audioFileReader.Volume = (float)currentVolumeLevel;
        }

        private void _output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            OnProcessCompleted(PlayerStatus.Stopped);
        }

            public void Stop()
        {
            if (_output != null)
            {
                _output.Stop();
            }
        }

        public void Pause()
        {
            if (_output != null)
            {
                _output.Pause();
                OnProcessCompleted(PlayerStatus.Paused);
            }
        }

        public void TogglePlayPause(double currentVolumeLevel)
        {
            if (_output != null)
            {
                if (_output.PlaybackState == PlaybackState.Playing)
                {
                    Pause();
                }
                else
                {
                    Play(_output.PlaybackState, currentVolumeLevel);
                }
            }
            else
            {
                Play(PlaybackState.Stopped, currentVolumeLevel);
            }
        }

        public void Dispose()
        {
            if (_output != null)
            {
                if (_output.PlaybackState == PlaybackState.Playing)
                {
                    _output.Stop();
                }
                _output.Dispose();
                _output = null;
            }
            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }
        }

        public double GetLenghtInSeconds()
        {
            if (_audioFileReader != null)
            {
                return _audioFileReader.TotalTime.TotalSeconds;
            }
            else
            {
                return 0;
            }
        }

        public double GetPositionInSeconds()
        {
            return _audioFileReader != null ? _audioFileReader.CurrentTime.TotalSeconds : 0;
        }

        public float GetVolume()
        {
            if (_audioFileReader != null)
            {
                return _audioFileReader.Volume;
            }
            return 1;
        }

        public void SetPosition(double value)
        {
            if (_audioFileReader != null)
            {
                _audioFileReader.CurrentTime = TimeSpan.FromSeconds(value);
            }
        }

        public void SetVolume(float value)
        {
            if (_output != null)
            {
                _audioFileReader.Volume = value;
            }
        }

        protected virtual void OnProcessCompleted(PlayerStatus playerStatus) //protected virtual method
        {
            switch(playerStatus)
            {
                case PlayerStatus.Stopped:
                    PlayerStopped?.Invoke();
                    break;
                case PlayerStatus.Paused:
                    PlayerPaused?.Invoke();
                    break;
                case PlayerStatus.Resumed:
                    PlayerResumed?.Invoke();
                    break;
                default: 
                    break;
            }
        }
    }
}
