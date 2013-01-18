namespace VoiceEncoder
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// FourCC коды
    /// </summary>
    public enum FourCC
    {
        /// <summary>
        /// FCC.FourCC('W', 'A', 'V', 'E')
        /// </summary>
        Wave = 0x45564157,

        /// <summary>
        /// FCC.FourCC('f', 'm', 't', ' ')
        /// </summary>
        WavFmt = 0x20746d66,

        /// <summary>
        /// FCC.FourCC('D', 'A', 'T', 'A')
        /// </summary>
        WavData = 0x41544144,

        /// <summary>
        /// FCC.FourCC('d', 'a', 't', 'a') 
        /// </summary>
        Wavdata = 0x61746164,

        /// <summary>
        /// FCC.FourCC('R', 'I', 'F', 'F')
        /// </summary>
        Riff = 0x46464952,

        /// <summary>
        /// FCC.FourCC('L', 'I', 'S', 'T')
        /// </summary>
        List = 0x5453494c,

        /// <summary>
        /// FCC.FourCC('A', 'V', 'I', ' ')
        /// </summary>
        Avi = 0x20495641,
    }

    
    public struct RiffChunk
    {
    
        public FourCC FCC
        {
            get;
            set;
        }

    
        public uint Size
        {
            get;
            set;
        }

        public FourCC FCCList
        {
            get;
            set;
        }

        public bool IsList()
        {
            return this.FCC == FourCC.List;
        }
    }

    public class RiffParser : IDisposable
    {
        private const uint SizeOfRiffList = 12;

        private const uint SizeOfRiffChunk = 8;

        private RiffChunk chunk;

        private FourCC fccId;

        private FourCC fccType;

        private Stream stream;

        private BinaryReader br;
        
        private uint bytesRemaining;

        private uint containerSize;

        private long containerOffset;

        private long currentChunkOffset;

        public RiffParser(Stream stream, FourCC id, long startOfContainer)
        {
            try
            {
                if (stream == null)
                {
                }

                this.fccId = id;
                this.containerOffset = startOfContainer;

                this.stream = stream;
                this.br = new BinaryReader(stream);

                this.ReadRiffHeader();
            }
            catch (Exception) { }
        }

        public FourCC RiffId
        {
            get
            {
                return this.fccId;
            }
        }

        public FourCC RiffType
        {
            get
            {
                return this.fccType;
            }
        }

        public RiffChunk Chunk
        {
            get
            {
                return this.chunk;
            }
        }

        public long DataPosition
        {
            get
            {
                return this.currentChunkOffset + SizeOfRiffChunk;
            }
        }

        public uint BytesRemainingInChunk
        {
            get
            {
                return this.bytesRemaining;
            }
        }

        private long ChunkActualSize
        {
            get
            {
                return SizeOfRiffChunk + RiffRound(this.chunk.Size);
            }
        }

        public static bool IsAligned(int startIndex, int align)
        {
            return (startIndex % align) == 0;
        }

        public static bool IsAligned(long startIndex, int align)
        {
            return (startIndex % align) == 0;
        }

        public static int RiffRound(int count)
        {
            return count + (count & 1);
        }

        public static long RiffRound(long count)
        {
            return count + (count & 1);
        }

        public static string FormatFourCC(FourCC fcc)
        {

            uint code = (uint)fcc;
            return string.Format(
                "{0}{1}{2}{3}",
                (char)(code & 0xFF),
                (char)((code >> 8) & 0xFF),
                (char)((code >> 16) & 0xFF),
                (char)((code >> 24) & 0xFF));

        }

        
        public bool MoveToNextChunk()
        {
            try
            {
                
                Debug.Assert(this.currentChunkOffset > this.containerOffset, "The chunk cannot be out of bounds of the container");
                Debug.Assert(this.currentChunkOffset >= 0, "The chunk offset must be positive");
                Debug.Assert(this.containerOffset >= 0, "The container offset must be positive");

                long maxChunkSize;

                this.currentChunkOffset += this.ChunkActualSize;

                if (this.currentChunkOffset - this.containerOffset >= this.containerSize)
                {
                }

                this.stream.Position = this.currentChunkOffset;

                this.ReadChunkHeader();

                maxChunkSize = (long)this.containerSize - (this.currentChunkOffset - this.containerOffset);

                if (maxChunkSize < this.ChunkActualSize)
                {
                }

                this.bytesRemaining = this.chunk.Size;

                return true;
            }
            catch (Exception) { return false; }
        }

        public RiffParser EnumerateChunksInList()
        {
            if (!this.chunk.IsList())
            {
            }

            return new RiffParser(this.stream, FourCC.List, this.currentChunkOffset);
        }

        public void PrintChunkInformation(int indent)
        {
            try
            {
                while (true)
                {
                    for (int i = 0; i < indent; i++)
                    {
                        Console.Write("\t");
                    }

                    Console.WriteLine("{0} ({1} bytes) {2}", FormatFourCC(this.fccId), this.Chunk.Size, FormatFourCC(this.fccType));
                    if (this.chunk.IsList())
                    {
                        RiffParser listParser = this.EnumerateChunksInList();
                        listParser.PrintChunkInformation(indent + 1);
                    }

                   if( !MoveToNextChunk()) break;
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        
        public void MoveToStartOfChunk()
        {
            this.MoveToChunkOffset(0);
        }

        public void MoveToChunkOffset(uint offset)
        {
            try
            {
            if (offset > this.chunk.Size)
            {
            }

            this.stream.Position = this.currentChunkOffset + offset + SizeOfRiffChunk;
            this.bytesRemaining = this.chunk.Size - offset;
            }
            catch (Exception) { }
        }

        public byte[] ReadDataFromChunk(uint count)
        {
            if (count > this.bytesRemaining)
            {
            }

            this.stream.Position = this.currentChunkOffset + this.chunk.Size - this.bytesRemaining + RiffParser.SizeOfRiffChunk;

            byte[] data = this.br.ReadBytes((int)count);
            this.bytesRemaining -= (uint)data.Length;

            return data;
        }

        public uint ProcessDataFromChunk(uint count)
        {
            if (count > this.bytesRemaining)
            {
            }

            this.bytesRemaining -= count;

            return count;
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
                if (this.br != null)
                {
                    this.br.Close();
                    this.br = null;
                }
            }
        }

        private void ReadRiffHeader()
        {
            try
            {
                RiffChunk header = new RiffChunk();

                if (!IsAligned(this.containerOffset, 2))
                {
                }

                if (this.containerOffset < 0)
                {
                }

                this.stream.Position = this.containerOffset;

                header.FCC = (FourCC)this.br.ReadUInt32();
                header.Size = this.br.ReadUInt32();
                header.FCCList = (FourCC)this.br.ReadUInt32();

				if (header.FCC != this.fccId)
                {
                }

                this.containerSize = header.Size + SizeOfRiffChunk;
                this.fccType = header.FCCList;

                this.currentChunkOffset = this.containerOffset + SizeOfRiffList;

                this.ReadChunkHeader();
            }
            catch(Exception){}
        }

        private void ReadChunkHeader()
        {
            this.chunk.FCC = (FourCC)this.br.ReadUInt32();
            this.chunk.Size = this.br.ReadUInt32();
            this.bytesRemaining = this.chunk.Size;
        }
    }
}
