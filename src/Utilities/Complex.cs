using System.Runtime.CompilerServices;

namespace MiniAudioPlayer.Utilities
{
    public struct Complex
    {
        public float Real;
        public float Imag;

        public Complex(float real, float imag)
        {
            Real = real;
            Imag = imag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Complex operator +(Complex a, Complex b) => new Complex(a.Real + b.Real, a.Imag + b.Imag);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Complex operator -(Complex a, Complex b) => new Complex(a.Real - b.Real, a.Imag - b.Imag);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Complex operator *(Complex a, Complex b) => new Complex(
            a.Real * b.Real - a.Imag * b.Imag,
            a.Real * b.Imag + a.Imag * b.Real
        );
    }
}