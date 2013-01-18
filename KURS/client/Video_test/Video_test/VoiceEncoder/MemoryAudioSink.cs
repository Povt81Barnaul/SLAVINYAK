using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;

namespace VoiceEncoder
{

    public class MemoryAudioSink : AudioSink
    {
        public event EventHandler OnBufferFulfill;
        public bool StartSending = false;

        protected override void OnCaptureStarted()
        {
        }

        protected override void OnCaptureStopped()
        {

        }

        protected override void OnFormatChange(AudioFormat Format)
        {
            
        }

        protected override void OnSamples(long sampleTime, long sampleDuration, byte[] sampleData)
        {
            if (StartSending)
            {
                
                if (OnBufferFulfill != null)
                {
                    OnBufferFulfill(sampleData, null);
                }
            }
        }

    }
}
