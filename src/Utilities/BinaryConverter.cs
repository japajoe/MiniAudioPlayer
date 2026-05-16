using System;
using System.Text;

namespace MiniAudioPlayer.Utilities
{
    public enum ByteOrder
    {
        LittleEndian,
        BigEndian
    }

    public static unsafe class BinaryConverter
    {
        private static TextEncoder encoder = new TextEncoder();

        private static void memcpy(void* dst, void* src, ulong n)
        {
            Buffer.MemoryCopy(src, dst, n, n);
        }

        private static void MemCopy(byte[] destination, int destinationOffset, byte* source, int sourceOffset, uint length)
        {
            fixed(byte* dest = &destination[destinationOffset])
            {
                memcpy(dest, &source[sourceOffset], (ulong)length);
            }
        }

        public static void GetBytes(Int64 value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(Int64), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(Int64));
        }

        public static void GetBytes(UInt64 value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(UInt64), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(UInt64));
        }

        public static void GetBytes(Int32 value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(Int32), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(Int32));
        }        

        public static void GetBytes(UInt32 value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(UInt32), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(UInt32));
        }

        public static void GetBytes(Int16 value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(Int16), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(Int16));
        }

        public static void GetBytes(UInt16 value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(UInt16), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(UInt16));
        }

        public static void GetBytes(double value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(double), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(double));
        }

        public static void GetBytes(float value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(float), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(float));
        }

        public static int GetBytes(string value, int charIndex, int charCount, byte[] buffer, int offset, TextEncoding encoding)
        {
            return encoder.GetBytes(value, charIndex, charCount, buffer, offset, encoding);
        }

        public static int GetBytes(string value, int charCount, byte[] buffer, int offset, TextEncoding encoding)
        {
            return GetBytes(value, 0, charCount, buffer, offset, encoding);
        }

        public static int GetBytes(string value, byte[] buffer, int offset, TextEncoding encoding)
        {
            return GetBytes(value, 0, value.Length, buffer, offset, encoding);
        }

        public static Int64 ToInt64(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(Int64), byteOrder);
                return *(Int64*)value;
            }
        }

        public static UInt64 ToUInt64(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(UInt64), byteOrder);
                return *(UInt64*)value;
            }
        }

        public static Int32 ToInt32(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(Int32), byteOrder);
                return *(Int32*)value;
            }
        }

        public static UInt32 ToUInt32(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(UInt32), byteOrder);
                return *(UInt32*)value;
            }
        }

        public static Int16 ToInt16(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(Int16), byteOrder);
                return *(Int16*)value;
            }
        }

        public static UInt16 ToUInt16(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(UInt16), byteOrder);
                return *(UInt16*)value;
            }
        }

        public static float ToSingle(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(float), byteOrder);
                return *(float*)value;
            }
        }

        public static double ToDouble(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(double), byteOrder);
                return *(double*)value;
            }
        }

        public static string ToString(byte[] buffer, int offset, int length, TextEncoding encoding)
        {
            return encoder.ToString(buffer, offset, length, encoding);
        }

        public static void FlipBytes(byte[] bytes, int offset, int length)
        {
            fixed(byte* ptr = &bytes[offset])
            {
                FlipBytes(ptr, length);
            }
        }

        private static void FlipBytes(byte* bytes, int length)
        {
            for (int i = 0; i < length / 2; ++i) 
            {
                byte t = bytes[i];
                bytes[i] = bytes[length - i - 1];
                bytes[length - i - 1] = t;
            }
        }

        private static void ConvertToByteOrder(byte *bytes, int length, ByteOrder byteOrder)
        {
            if(byteOrder == ByteOrder.LittleEndian)
            {
                if(!BitConverter.IsLittleEndian)
                    FlipBytes(bytes, length);
            }
            else
            {
                if(BitConverter.IsLittleEndian)
                    FlipBytes(bytes, length);
            }
        }

        public static int GetByteCount(string value, TextEncoding encoding)
        {
            return GetByteCount(value, 0, value.Length, encoding);
        }

        public static int GetByteCount(string value, int charCount, TextEncoding encoding)
        {
            return GetByteCount(value, 0, charCount, encoding);
        }

        public static int GetByteCount(string value, int charIndex, int charCount, TextEncoding encoding)
        {
            return encoder.GetByteCount(value, charIndex, charCount, encoding);
        }
    }

    public enum TextEncoding
    {
        UTF8,
        UTF32,
        Unicode,
        ASCII
    }

    public sealed class TextEncoder
    {
        private UTF8Encoding utf8 = new UTF8Encoding();
        private UTF32Encoding utf32 = new UTF32Encoding();
        private UnicodeEncoding unicode = new UnicodeEncoding();
        private ASCIIEncoding ascii = new ASCIIEncoding();

        public TextEncoder()
        {
            utf8 = new UTF8Encoding();
            utf32 = new UTF32Encoding();
            unicode = new UnicodeEncoding();
            ascii = new ASCIIEncoding();
        }

        public int GetBytes(string value, int charIndex, int charCount, byte[] buffer, int offset, TextEncoding encoding)
        {
            int numBytes = 0;

            switch(encoding)
            {
                case TextEncoding.UTF8:
                    numBytes = utf8.GetBytes(value, charIndex, charCount, buffer, offset);
                    break;
                case TextEncoding.UTF32:
                    numBytes = utf32.GetBytes(value, charIndex, charCount, buffer, offset);
                    break;
                case TextEncoding.Unicode:
                    numBytes = unicode.GetBytes(value, charIndex, charCount, buffer, offset);
                    break;
                case TextEncoding.ASCII:
                    numBytes = ascii.GetBytes(value, charIndex, charCount, buffer, offset);
                    break;
                default:
                    return 0;
            }

            return numBytes;  
        }

        public string ToString(byte[] buffer, int offset, int length, TextEncoding encoding)
        {
            switch(encoding)
            {
                case TextEncoding.UTF8:
                    return utf8.GetString(buffer, offset, length);
                case TextEncoding.UTF32:
                    return utf32.GetString(buffer, offset, length);
                case TextEncoding.Unicode:
                    return unicode.GetString(buffer, offset, length);
                case TextEncoding.ASCII:
                    return ascii.GetString(buffer, offset, length);
                default:
                    return string.Empty;
            }
        }

        public unsafe int GetByteCount(string value, int charIndex, int charCount, TextEncoding encoding)
        {
            int numBytes = 0;

            fixed(char *ptr = value)
            {
                char *chars = &ptr[charIndex];

                switch(encoding)
                {
                    case TextEncoding.UTF8:
                        numBytes = utf8.GetByteCount(chars, charCount);
                        break;
                    case TextEncoding.UTF32:
                        numBytes = utf32.GetByteCount(chars, charCount);
                        break;
                    case TextEncoding.Unicode:
                        numBytes = unicode.GetByteCount(chars, charCount);
                        break;
                    case TextEncoding.ASCII:
                        numBytes = ascii.GetByteCount(chars, charCount);
                        break;
                    default:
                        return 0;
                }
            }

            return numBytes;    
        }
    }
}