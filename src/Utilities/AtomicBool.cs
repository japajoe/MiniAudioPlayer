using System.Threading;

namespace MiniAudioPlayer.Utilities
{
    public class AtomicBool
    {
        private int value;

        public AtomicBool()
        {
            value = 0;
        }

        public AtomicBool(bool initialValue)
        {
            value = initialValue ? 1 : 0;
        }

        public void Store(bool newValue)
        {
            Interlocked.Exchange(ref value, newValue ? 1 : 0);
        }

        public bool Load()
        {
            return Volatile.Read(ref value) == 1;
        }

        public bool Exchange(bool newValue)
        {
            return Interlocked.Exchange(ref value, newValue ? 1 : 0) == 1;
        }

        public bool CompareAndSwap(bool expected, bool desired)
        {
            int expectedInt = expected ? 1 : 0;
            int desiredInt = desired ? 1 : 0;

            // Returns the original value. If it matches expected, the swap happened.
            return Interlocked.CompareExchange(ref value, desiredInt, expectedInt) == expectedInt;
        }
    }
}