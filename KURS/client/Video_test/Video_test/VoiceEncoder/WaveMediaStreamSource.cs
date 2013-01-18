namespace VoiceEncoder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Media;

    public class WaveMediaStreamSource : MediaStreamSource, IDisposable
    {
        private Stream stream;

        private WavParser wavParser;
        
        private MediaStreamDescription audioDesc;

        private long currentPosition;

        private long startPosition;

        private long currentTimeStamp;

        private Dictionary<MediaSampleAttributeKeys, string> emptySampleDict = new Dictionary<MediaSampleAttributeKeys, string>();

        public WaveMediaStreamSource(Stream stream)
        {
            this.stream = stream;
        }

        #region IDisposable Members
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.wavParser != null)
                {
                    this.wavParser.Dispose();
                    this.wavParser = null;
                }
            }
        }

        protected override void OpenMediaAsync()
        {
            try
            {
                this.wavParser = new WavParser(this.stream);

                this.wavParser.ParseWaveHeader();

                this.wavParser.WaveFormatEx.ValidateWaveFormat();

                this.startPosition = this.currentPosition = this.wavParser.DataPosition;

                Dictionary<MediaStreamAttributeKeys, string> streamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
                Dictionary<MediaSourceAttributesKeys, string> sourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
                List<MediaStreamDescription> availableStreams = new List<MediaStreamDescription>();

                streamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = this.wavParser.WaveFormatEx.ToHexString();
                MediaStreamDescription msd = new MediaStreamDescription(MediaStreamType.Audio, streamAttributes);

                this.audioDesc = msd;
                availableStreams.Add(this.audioDesc);

                sourceAttributes[MediaSourceAttributesKeys.Duration] = this.wavParser.Duration.ToString();
                ReportOpenMediaCompleted(sourceAttributes, availableStreams);
            }
            catch(Exception){}
        }

        protected override void CloseMedia()
        {
            this.startPosition = this.currentPosition = 0;
            this.wavParser = null;
            this.audioDesc = null;
        }

        protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
        {
            throw new NotImplementedException();
        }

        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            uint bufferSize = (uint)AlignUp(
                this.wavParser.WaveFormatEx.AvgBytesPerSec,
                this.wavParser.WaveFormatEx.BlockAlign);

            bufferSize = Math.Min(bufferSize, (uint)this.wavParser.BytesRemainingInChunk);
            if (bufferSize > 0)
            {
                this.wavParser.ProcessDataFromChunk(bufferSize);

                MediaStreamSample sample = new MediaStreamSample(
                    this.audioDesc,
                    this.stream,
                    this.currentPosition,
                    bufferSize,
                    this.currentTimeStamp,
                    this.emptySampleDict);

                this.currentTimeStamp += this.wavParser.WaveFormatEx.AudioDurationFromBufferSize(bufferSize);
                this.currentPosition += bufferSize;


                ReportGetSampleCompleted(sample);
            }
            else
            {
                ReportGetSampleCompleted(new MediaStreamSample(this.audioDesc, null, 0, 0, 0, this.emptySampleDict));
            }
        }
        
        protected override void SeekAsync(long seekToTime)
        {
            if (seekToTime > this.wavParser.Duration)
            {
                throw new InvalidOperationException("The seek position is beyond the length of the stream");
            }

            this.currentPosition = this.wavParser.WaveFormatEx.BufferSizeFromAudioDuration(seekToTime) + this.startPosition;
            this.currentTimeStamp = seekToTime;
            ReportSeekCompleted(seekToTime);
        }

        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
            throw new NotImplementedException();
        }

        private static int AlignUp(int a, int b)
        {
            int tmp = a + b - 1;
            return tmp - (tmp % b);
        }
    }
}
