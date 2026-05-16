using System.IO;
using System.Text;

namespace MiniAudioPlayer.Utilities
{
    public class DirectoryStorage
    {
        private string lastShaderDirectory;
        private string lastAudioDirectory;

        public string LastShaderDirectory
        {
            get
            {
                return lastShaderDirectory;
            }
            set
            {
                if(string.IsNullOrEmpty(value))
                    return;
                lastShaderDirectory = value;
                if(!lastShaderDirectory.EndsWith("/"))
                    lastShaderDirectory += "/";
            }
        }

        public string LastAudioDirectory
        {
            get
            {
                return lastAudioDirectory;
            }
            set
            {
                if(string.IsNullOrEmpty(value))
                    return;
                lastAudioDirectory = value;
                if(!lastAudioDirectory.EndsWith("/"))
                    lastAudioDirectory += "/";
            }
        }

        public bool Serialize(string filePath)
        {
            if(string.IsNullOrEmpty(lastShaderDirectory))
                return false;
            if(string.IsNullOrEmpty(lastAudioDirectory))
                return false;

            int lengthPath1 = Encoding.UTF8.GetByteCount(lastShaderDirectory);
            int lengthPath2 = Encoding.UTF8.GetByteCount(lastAudioDirectory);

            int headerSize = 4;
            int dataSize = sizeof(int) + sizeof(int) + lengthPath1 + lengthPath2;
            int totalSize = headerSize + dataSize;

            byte[] data = new byte[totalSize];

            BinaryStream stream = new BinaryStream(data);
            stream.Write((byte)'.');
            stream.Write((byte)'m');
            stream.Write((byte)'d');
            stream.Write((byte)'s');
            stream.Write(lengthPath1);
            stream.Write(lastShaderDirectory);
            stream.Write(lengthPath2);
            stream.Write(lastAudioDirectory);

            try
            {
                File.WriteAllBytes(filePath, data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Deserialize(string filePath)
        {
            if(!File.Exists(filePath))
                return false;

            byte[] data = null;

            try
            {
                data = File.ReadAllBytes(filePath);

                BinaryStream stream = new BinaryStream(data, data.Length);

                if(stream.ReadByte() != (byte)'.')
                    return false;
                if(stream.ReadByte() != (byte)'m')
                    return false;
                if(stream.ReadByte() != (byte)'d')
                    return false;
                if(stream.ReadByte() != (byte)'s')
                    return false;

                int length1 = stream.ReadInt32();
                lastShaderDirectory = stream.ReadString(length1);
                int length2 = stream.ReadInt32();
                lastAudioDirectory = stream.ReadString(length2);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}