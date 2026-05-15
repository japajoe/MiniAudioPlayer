using System;

namespace MiniAudioPlayer.Utilities
{
    public class RingBuffer
    {
        private readonly float[] data;
        private int writeIndex;
        private int readIndex;
        private readonly int capacity;
        private readonly object lockObject = new object();

        public RingBuffer(int size)
        {
            capacity = size;
            data = new float[capacity];
            writeIndex = 0;
            readIndex = 0;
        }

        public void Write(ReadOnlySpan<float> samples)
        {
            lock (lockObject)
            {
                int samplesCount = samples.Length;
                int currentAvailable = (writeIndex - readIndex + capacity) % capacity;
                int spaceLeft = capacity - currentAvailable - 1;

                // If the incoming period is larger than the remaining space,
                // we advance the read index to "drop" the oldest samples.
                if (samplesCount > spaceLeft)
                {
                    int overflow = samplesCount - spaceLeft;
                    readIndex = (readIndex + overflow) % capacity;
                }

                for (int i = 0; i < samplesCount; i++)
                {
                    data[writeIndex] = samples[i];
                    writeIndex = (writeIndex + 1) % capacity;
                }
            }
        }

        public int Read(Span<float> destination)
        {
            lock (lockObject)
            {
                int available = (writeIndex - readIndex + capacity) % capacity;
                int count = Math.Min(available, destination.Length);

                for (int i = 0; i < count; i++)
                {
                    destination[i] = data[readIndex];
                    readIndex = (readIndex + 1) % capacity;
                }

                return count;
            }
        }

        public int GetAvailableCount()
        {
            lock (lockObject)
            {
                return (writeIndex - readIndex + capacity) % capacity;
            }
        }

        public void Clear()
        {
            lock (lockObject)
            {
                readIndex = 0;
                writeIndex = 0;
            }
        }
    }
}