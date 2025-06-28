using System;
using System.Numerics;
// using MathNet.Numerics.IntegralTransforms; // Alternative using MathNet

public class FFTProcessor
{
    public int Points { get; set; } = 256;

    public Complex[] Process(short[] samples)
    {
        if (samples == null || samples.Length != Points)
        {
            throw new ArgumentException($"Samples must be {Points} in length.");
        }

        Complex[] complexSamples = new Complex[Points];
        for (int i = 0; i < Points; i++)
        {
            complexSamples[i] = new Complex(samples[i], 0);
        }

        // Inplace FFT using Cooley-Tukey algorithm
        InplaceFFT(complexSamples);

        return complexSamples;
    }

    // Cooley-Tukey FFT algorithm (Radix-2)
    private void InplaceFFT(Complex[] data)
    {
        int n = data.Length;
        if ((n & (n - 1)) != 0)
        {
            throw new ArgumentException("Data length must be a power of 2.");
        }

        // Bit reversal permutation
        for (int i = 1, j = 0; i < n - 1; ++i)
        {
            for (int k = n >> 1; (j ^= k) < k; k >>= 1);
            if (i < j)
            {
                Complex temp = data[i];
                data[i] = data[j];
                data[j] = temp;
            }
        }

        // Cooley-Tukey iterative calculation
        for (int len = 2; len <= n; len <<= 1)
        {
            double angle = -2 * Math.PI / len;
            Complex wlen = new Complex(Math.Cos(angle), Math.Sin(angle));

            for (int i = 0; i < n; i += len)
            {
                Complex w = new Complex(1, 0);
                for (int j = 0; j < len / 2; ++j)
                {
                    Complex u = data[i + j];
                    Complex v = data[i + j + len / 2] * w;
                    data[i + j] = u + v;
                    data[i + j + len / 2] = u - v;
                    w *= wlen;
                }
            }
        }
    }

    public float[] GetMagnitudes(Complex[] spectrum)
    {
        float[] magnitudes = new float[spectrum.Length / 2];
        for (int i = 0; i < magnitudes.Length; i++)
        {
            magnitudes[i] = (float)spectrum[i].Magnitude;
        }
        return magnitudes;
    }
}