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
    public class PcmToWave
    {

        public void SavePcmToWav(MemoryStream Data, Stream Output, int BitPerSample, int SamplePeerSecond, int Channels)
        {

            BinaryWriter _output = new BinaryWriter(Output);
            _output.Write("RIFF".ToCharArray());
            _output.Write((uint)(Data.Length));
            _output.Write("WAVE".ToCharArray());
            _output.Write("fmt ".ToCharArray());
            _output.Write((uint)0x10);
            _output.Write((ushort)0x1);
            _output.Write((ushort)Channels);
            _output.Write((uint)SamplePeerSecond);
            _output.Write((uint)(BitPerSample * SamplePeerSecond * Channels / 8));
            _output.Write((ushort)(BitPerSample * Channels / 8));
            _output.Write((ushort)BitPerSample);
            
            _output.Write("data".ToCharArray());
            _output.Write(Data.ToArray(), 0, (int)Data.Length);
        }
    }
}
