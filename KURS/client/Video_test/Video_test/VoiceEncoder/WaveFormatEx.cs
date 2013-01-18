namespace VoiceEncoder
{
    using System;

    public class WAVEFORMATEX
    {
        #region Data
        public const uint SizeOf = 18;

        private const short FormatPCM = 1;

        public short FormatTag
        {
            get;
            set;
        }

        public short Channels
        {
            get;
            set;
        }

        public int SamplesPerSec
        {
            get;
            set;
        }

        public int AvgBytesPerSec
        {
            get;
            set;
        }

        public short BlockAlign
        {
            get;
            set;
        }

        public short BitsPerSample
        {
            get;
            set;
        }

        public short Size
        {
            get;
            set;
        }

        public byte[] Ext
        {
            get;
            set;
        }
        #endregion Data

        public static string ToLittleEndianString(string bigEndianString)
        {
            if (bigEndianString == null)
            {
                return string.Empty;
            }

            char[] bigEndianChars = bigEndianString.ToCharArray();

            if (bigEndianChars.Length % 2 != 0)
            {
                return string.Empty;
            }

            int i, ai, bi, ci, di;
            char a, b, c, d;
            for (i = 0; i < bigEndianChars.Length / 2; i += 2)
            {
                ai = i;
                bi = i + 1;

                ci = bigEndianChars.Length - 2 - i;
                di = bigEndianChars.Length - 1 - i;

                a = bigEndianChars[ai];
                b = bigEndianChars[bi];
                c = bigEndianChars[ci];
                d = bigEndianChars[di];

                bigEndianChars[ci] = a;
                bigEndianChars[di] = b;
                bigEndianChars[ai] = c;
                bigEndianChars[bi] = d;
            }

            return new string(bigEndianChars);
        }

        public string ToHexString()
        {
            string s = string.Empty;

            s += ToLittleEndianString(string.Format("{0:X4}", this.FormatTag));
            s += ToLittleEndianString(string.Format("{0:X4}", this.Channels));
            s += ToLittleEndianString(string.Format("{0:X8}", this.SamplesPerSec));
            s += ToLittleEndianString(string.Format("{0:X8}", this.AvgBytesPerSec));
            s += ToLittleEndianString(string.Format("{0:X4}", this.BlockAlign));
            s += ToLittleEndianString(string.Format("{0:X4}", this.BitsPerSample));
            s += ToLittleEndianString(string.Format("{0:X4}", this.Size));

            return s;
        }

        public void SetFromByteArray(byte[] byteArray)
        {
            if ((byteArray.Length + 2) < SizeOf)
            {
                throw new ArgumentException("Byte array is too small");
            }

            this.FormatTag = BitConverter.ToInt16(byteArray, 0);
            this.Channels = BitConverter.ToInt16(byteArray, 2);
            this.SamplesPerSec = BitConverter.ToInt32(byteArray, 4);
            this.AvgBytesPerSec = BitConverter.ToInt32(byteArray, 8);
            this.BlockAlign = BitConverter.ToInt16(byteArray, 12);
            this.BitsPerSample = BitConverter.ToInt16(byteArray, 14);
            if (byteArray.Length >= SizeOf)
            {
                this.Size = BitConverter.ToInt16(byteArray, 16);
            }
            else
            {
                this.Size = 0;
            }

            if (byteArray.Length > WAVEFORMATEX.SizeOf)
            {
                this.Ext = new byte[byteArray.Length - WAVEFORMATEX.SizeOf];
                Array.Copy(byteArray, (int)WAVEFORMATEX.SizeOf, this.Ext, 0, this.Ext.Length);
            }
            else
            {
                this.Ext = null;
            }
        }

        public override string ToString()
        {
            char[] rawData = new char[18];
            BitConverter.GetBytes(this.FormatTag).CopyTo(rawData, 0);
            BitConverter.GetBytes(this.Channels).CopyTo(rawData, 2);
            BitConverter.GetBytes(this.SamplesPerSec).CopyTo(rawData, 4);
            BitConverter.GetBytes(this.AvgBytesPerSec).CopyTo(rawData, 8);
            BitConverter.GetBytes(this.BlockAlign).CopyTo(rawData, 12);
            BitConverter.GetBytes(this.BitsPerSample).CopyTo(rawData, 14);
            BitConverter.GetBytes(this.Size).CopyTo(rawData, 16);
            return new string(rawData);
        }

        public long AudioDurationFromBufferSize(uint audioDataSize)
        {
            if (this.AvgBytesPerSec == 0)
            {
                return 0;
            }

            return (long)audioDataSize * 10000000 / this.AvgBytesPerSec;
        }

        public long BufferSizeFromAudioDuration(long duration)
        {
            long size = duration * this.AvgBytesPerSec / 10000000;
            uint remainder = (uint)(size % this.BlockAlign);
            if (remainder != 0)
            {
                size += this.BlockAlign - remainder;
            }

            return size;
        }

        public void ValidateWaveFormat()
        {
            if (this.FormatTag != FormatPCM)
            {
                throw new InvalidOperationException("Only PCM format is supported");
            }

            if (this.Channels != 1 && this.Channels != 2)
            {
                throw new InvalidOperationException("Only 1 or 2 channels are supported");
            }

            if (this.BitsPerSample != 8 && this.BitsPerSample != 16)
            {
                throw new InvalidOperationException("Only 8 or 16 bit samples are supported");
            }

            if (this.Size != 0)
            {
                throw new InvalidOperationException("Size must be 0");
            }

            if (this.BlockAlign != this.Channels * (this.BitsPerSample / 8))
            {
                throw new InvalidOperationException("Block Alignment is incorrect");
            }

            if (this.SamplesPerSec > (uint.MaxValue / this.BlockAlign))
            {
                throw new InvalidOperationException("SamplesPerSec overflows");
            }

            if (this.AvgBytesPerSec != this.SamplesPerSec * this.BlockAlign)
            {
                throw new InvalidOperationException("AvgBytesPerSec is wrong");
            }
        }
    }
}
