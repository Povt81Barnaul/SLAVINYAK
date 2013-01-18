namespace VoiceEncoder
{
    using System;
    using System.Diagnostics;
    using System.IO;
    
    public class WavParser : RiffParser
    {
        #region Constants
        private const uint MinFormatSize = WAVEFORMATEX.SizeOf - sizeof(short);
        #endregion

        #region Data
        private WAVEFORMATEX waveFormat;

        private long duration;
        #endregion

        public WavParser(Stream stream)
            : base(stream, FourCC.Riff, 0)
        {

            if (RiffType != FourCC.Wave)
            {
            }
        }

        #region Properties
        public WAVEFORMATEX WaveFormatEx
        {
            get
            {
                return this.waveFormat;
            }
        }

        public long Duration
        {
            get
            {
                return this.duration;
            }
        }
        #endregion

        public void ParseWaveHeader()
        {
            bool foundData = false;

            try
            {
                while (!foundData)
                {
                    if (Chunk.FCC == FourCC.WavFmt)
                    {
                        this.ReadFormatBlock();
                    }
                    else if (Chunk.FCC == FourCC.WavData || Chunk.FCC == FourCC.Wavdata)
                    {
                        foundData = true;
                        break;
                    }

                    if (!MoveToNextChunk()) break;
                }


            this.duration = this.waveFormat.AudioDurationFromBufferSize(Chunk.Size);
            }
            catch (Exception)
            {
                if (this.waveFormat == null || !foundData)
                {
                }
            }


        }

        private void ReadFormatBlock()
        {
            try
            {
                Debug.Assert(Chunk.FCC == FourCC.WavFmt, "This is not a WavFmt chunk");
                Debug.Assert(this.waveFormat == null, "The waveformat structure should have been set before");
                uint formatSize = 0;
                if (Chunk.Size < MinFormatSize)
                {
                }
                formatSize = Chunk.Size;

                this.waveFormat = new WAVEFORMATEX();

                byte[] data = ReadDataFromChunk(formatSize);

                this.waveFormat.SetFromByteArray(data);
            }
            catch (Exception)
            {
                this.waveFormat = null;
                throw;
            }
        }
    }
}
