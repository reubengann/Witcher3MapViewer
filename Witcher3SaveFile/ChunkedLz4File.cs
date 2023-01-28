using LZ4PCL;
using System.Diagnostics;
using System.Text;

namespace SaveFile
{
    public static class ChunkedLz4File
    {
        public static Stream Decompress(Stream input)
        {
            ChunkedLz4FileHeader header = ChunkedLz4FileHeader.Read(input);
            var table = ChunkedLz4FileTable.Read(input, header.ChunkCount);
            input.Position = header.HeaderSize;
            var data = new byte[header.HeaderSize + table.Chunks.Sum(c => c.DecompressedChunkSize)];
            var memoryStream = new MemoryStream(data) { Position = header.HeaderSize };
            foreach (var chunk in table.Chunks)
            {
                byte[] chunkData = chunk.Read(input);
                memoryStream.Write(chunkData, 0, chunkData.Length);
                Debug.Assert(input.Position == chunk.EndOfChunkOffset || chunk.EndOfChunkOffset == 0);
            }
            memoryStream.Position = header.HeaderSize;
            return memoryStream;
        }
    }

    public class ChunkedLz4FileHeader
    {
        public int ChunkCount { get; set; }
        public int HeaderSize { get; set; }

        public static ChunkedLz4FileHeader Read(Stream input)
        {
            using (var reader = new BinaryReader(input, Encoding.ASCII, true))
            {
                string saveFileHeader = reader.ReadString(4);

                if (saveFileHeader != "SNFH")
                {
                    throw new InvalidOperationException();
                }

                string chunkedLz4FileHeader = reader.ReadString(4);
                if (chunkedLz4FileHeader != "FZLC")
                {
                    throw new InvalidOperationException();
                }

                return new ChunkedLz4FileHeader
                {
                    ChunkCount = reader.ReadInt32(),
                    HeaderSize = reader.ReadInt32()
                };
            }
        }

        private static string ConvertString(byte[] bin)
        {
            return Convert.ToBase64String(bin);
        }
    }

    public class Lz4Chunk
    {
        public int CompressedChunkSize { get; set; }

        public int DecompressedChunkSize { get; set; }

        public int EndOfChunkOffset { get; set; }

        public byte[] Read(Stream inputStream)
        {
            byte[] inputData = new byte[CompressedChunkSize];
            byte[] outputData = new byte[DecompressedChunkSize];

            inputStream.Read(inputData, 0, CompressedChunkSize);
            unsafe
            {
                fixed (byte* input = inputData)
                fixed (byte* output = outputData)
                {
                    int bytesDecoded = LZ4Codec.Decode32(input, inputData.Length, output, outputData.Length, true);
                    Debug.Assert(bytesDecoded == DecompressedChunkSize);
                }

                Debug.Assert(inputStream.Position == EndOfChunkOffset || EndOfChunkOffset == 0);
            }

            return outputData;
        }
    }

    public class ChunkedLz4FileTable
    {
        public Lz4Chunk[] Chunks { get; set; }

        public static ChunkedLz4FileTable Read(Stream input, int chunkCount)
        {
            using (var reader = new BinaryReader(input, Encoding.ASCII, true))
            {
                Lz4Chunk[] chunks = new Lz4Chunk[chunkCount];
                for (int i = 0; i < chunkCount; i++)
                {
                    chunks[i] = new Lz4Chunk
                    {
                        CompressedChunkSize = reader.ReadInt32(),
                        DecompressedChunkSize = reader.ReadInt32(),
                        EndOfChunkOffset = reader.ReadInt32()
                    };
                }

                return new ChunkedLz4FileTable
                {
                    Chunks = chunks,
                };
            }
        }
    }
}
