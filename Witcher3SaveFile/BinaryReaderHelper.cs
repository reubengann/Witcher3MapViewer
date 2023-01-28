namespace SaveFile
{
    internal static class BinaryReaderHelper
    {
        public static string ReadString(this BinaryReader reader, int count)
        {
            if (count < 1)
            {
                throw new ArgumentException("Invalid number of bytes to read");
            }
            return new string(reader.ReadChars(count));
        }

        public static string PeekString(this BinaryReader reader, int count)
        {
            if (count < 1)
            {
                throw new ArgumentException("Invalid number of bytes to read");
            }
            string s = new string(reader.ReadChars(count));
            reader.SeekRelative(-count);
            return s;
        }

        public static float ReadFloat(this BinaryReader reader)
        {
            return reader.ReadSingle();
        }

        public static bool PeekIfString(this BinaryReader reader, int count)
        {
            for (int i = 1; i <= count; i++)
            {
                byte b = reader.ReadByte();
                if (b < 65 || b > 90)
                {
                    reader.BaseStream.Position -= i;
                    return false;
                }
            }
            reader.BaseStream.Position -= count;
            return true;
        }

        public static void Seek(this BinaryReader reader, int offset)
        {
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        }

        public static void SeekRelative(this BinaryReader reader, int number)
        {
            reader.BaseStream.Seek(number, SeekOrigin.Current);
        }

        public static void SeekFromEnd(this BinaryReader reader, int number)
        {
            reader.BaseStream.Seek(number, SeekOrigin.End);
        }

        public static int Tell(this BinaryReader reader)
        {
            return (int)reader.BaseStream.Position;
        }

        public static int PeekInt16(this BinaryReader reader)
        {
            int data = reader.ReadInt16();
            reader.SeekRelative(-2);
            return data;
        }

        public static long FindPosition(this BinaryReader reader, byte[] sequence)
        {
            if (sequence.Length > reader.BaseStream.Length)
                return -1;

            byte[] buffer = new byte[sequence.Length];

            int i;
            while ((i = reader.Read(buffer, 0, sequence.Length)) == sequence.Length)
            {
                if (sequence.SequenceEqual(buffer))
                    return reader.BaseStream.Position - sequence.Length;
                else
                    reader.BaseStream.Position -= sequence.Length - PadLeftSequence(buffer, sequence);
            }
            return -1;
        }

        private static int PadLeftSequence(byte[] bytes, byte[] seqBytes)
        {
            int i = 1;
            while (i < bytes.Length)
            {
                int n = bytes.Length - i;
                byte[] aux1 = new byte[n];
                byte[] aux2 = new byte[n];
                Array.Copy(bytes, i, aux1, 0, n);
                Array.Copy(seqBytes, aux2, n);
                if (aux1.SequenceEqual(aux2))
                    return i;
                i++;
            }
            return i;
        }
    }
}
