using System;
using System.Runtime.CompilerServices;

namespace MiniAudioPlayer.Utilities
{
    public static class FFT
    {
        private static float[] window;

        public static void Compute(Complex[] data)
        {
            InitializeWindow(data.Length);

            int n = data.Length;

            // Apply Windowing
            for (int i = 0; i < n; i++)
            {
                data[i].Real *= window[i];
                data[i].Imag *= window[i];
            }

            // Bit-reversal permutation
            int j = 0;
            for (int i = 0; i < n; i++)
            {
                if (i < j)
                {
                    var temp = data[i];
                    data[i] = data[j];
                    data[j] = temp;
                }
                int m = n >> 1;
                while (m >= 1 && j >= m)
                {
                    j -= m;
                    m >>= 1;
                }
                j += m;
            }

            // Cooley-Tukey Butterfly iterations
            for (int len = 2; len <= n; len <<= 1)
            {
                float angle = -2.0f * MathF.PI / len;
                Complex wlen = new Complex(MathF.Cos(angle), MathF.Sin(angle));

                for (int i = 0; i < n; i += len)
                {
                    Complex w = new Complex(1, 0);
                    for (int k = 0; k < len / 2; k++)
                    {
                        Complex u = data[i + k];
                        Complex v = data[i + k + len / 2] * w;

                        data[i + k] = u + v;
                        data[i + k + len / 2] = u - v;
                        w *= wlen;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InitializeWindow(int size)
        {
            if (window == null || window?.Length != size)
            {
                window = new float[size];
                float denom = size - 1;
                for (int i = 0; i < size; i++)
                {
                    // Hann Window formula: 0.5 * (1 - cos(2 * PI * i / (N - 1)))
                    window[i] = 0.5f * (1.0f - MathF.Cos(2.0f * MathF.PI * i / denom));
                }
            }
        }
    }
}