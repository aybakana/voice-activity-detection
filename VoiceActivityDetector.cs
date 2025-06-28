using System;
using System.Numerics;

public class VoiceActivityDetector
{
    public float EnergyThreshold { get; set; } = 0.1f;
    public float SFMThreshold { get; set; } = 0.8f;

    public struct Features
    {
        public double Energy;
        public short F;
        public float SFM;
    }

    public Features ExtractFeatures(Complex[] spectrum)
    {
        Features features = new Features();
        features.Energy = CalculateEnergy(spectrum);
        features.F = CalculateDominantFrequency(spectrum);
        features.SFM = CalculateSFM(spectrum);
        return features;
    }

    private double CalculateEnergy(Complex[] spectrum)
    {
        double energy = 0;
        foreach (var complex in spectrum)
        {
            energy += complex.Magnitude * complex.Magnitude;
        }
        return energy;
    }

    private short CalculateDominantFrequency(Complex[] spectrum)
    {
        float maxMagnitude = 0;
        int maxIndex = 0;
        for (int i = 0; i < spectrum.Length / 2; i++)
        {
            float magnitude = (float)spectrum[i].Magnitude;
            if (magnitude > maxMagnitude)
            {
                maxMagnitude = magnitude;
                maxIndex = i;
            }
        }
        return (short)maxIndex;
    }

    private float CalculateSFM(Complex[] spectrum)
    {
        double arithmeticMean = 0;
        double geometricMean = 1;
        double epsilon = 1e-8; // Avoid division by zero

        for (int i = 0; i < spectrum.Length / 2; i++)
        {
            arithmeticMean += spectrum[i].Magnitude;
            geometricMean *= (spectrum[i].Magnitude + epsilon);
        }

        arithmeticMean /= (spectrum.Length / 2);
        geometricMean = Math.Pow(geometricMean, 1.0 / (spectrum.Length / 2));

        return (float)(geometricMean / (arithmeticMean + epsilon));
    }

    public bool IsVoiceDetected(Features features)
    {
        return features.Energy > EnergyThreshold && features.SFM < SFMThreshold;
    }
}