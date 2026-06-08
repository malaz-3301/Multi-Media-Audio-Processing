using System;

public class AudioDecompressor
{
    public static float[] Inverse_NonLinear_Quantizer(sbyte[] bits, int quantLevel)
    {
        float[] samples = new float[bits.Length];
        const float mu = 255f;
        float maxLevel = (quantLevel - 1) / 2f;

        for (int i = 0; i < bits.Length; i++)
        {
            float compressedVal = bits[i] / maxLevel;
            float sign = MathF.Sign(compressedVal);
            float absCompressed = MathF.Abs(compressedVal);
            float originalSample = sign * ((MathF.Pow(1f + mu, absCompressed) - 1f) / mu);
            samples[i] = Math.Clamp(originalSample, -1f, 1f);
        }

        return samples;
    }

    public static float[] Inverse_DeltaModulation(float[] firstSamples, byte[] packedBits, int totalSamples, float stepSize = 0.01f, int channels = 1)
    {
        if (totalSamples <= 0)
            return Array.Empty<float>();

        channels = Math.Max(1, channels);
        float[] samples = new float[totalSamples];
        float[] reconstructed = new float[channels];
        int firstCount = Math.Min(firstSamples.Length, totalSamples);

        for (int i = 0; i < firstCount; i++)
        {
            samples[i] = Math.Clamp(firstSamples[i], -1f, 1f);
            reconstructed[i % channels] = samples[i];
        }

        for (int i = firstCount; i < totalSamples; i++)
        {
            int packedIndex = i - firstCount;
            int byteIndex = packedIndex / 8;
            int bitIndex = packedIndex % 8;
            int channel = i % channels;

            bool isUp = ((packedBits[byteIndex] >> (7 - bitIndex)) & 1) == 1;
            float change = isUp ? stepSize : -stepSize;
            reconstructed[channel] = Math.Clamp(reconstructed[channel] + change, -1f, 1f);
            samples[i] = reconstructed[channel];
        }

        return samples;
    }

    public static float[] Inverse_DPCM(float[] firstSamples, float quantizationFactor, sbyte[] compressedSamples, int channels = 1)
    {
        channels = Math.Max(1, channels);
        float[] samples = new float[firstSamples.Length + compressedSamples.Length];
        float[] reconstructed = new float[channels];

        for (int i = 0; i < firstSamples.Length; i++)
        {
            samples[i] = Math.Clamp(firstSamples[i], -1f, 1f);
            reconstructed[i % channels] = samples[i];
        }

        for (int i = 0; i < compressedSamples.Length; i++)
        {
            int sampleIndex = firstSamples.Length + i;
            int channel = sampleIndex % channels;
            float diff = compressedSamples[i] / quantizationFactor;
            reconstructed[channel] = Math.Clamp(reconstructed[channel] + diff, -1f, 1f);
            samples[sampleIndex] = reconstructed[channel];
        }

        return samples;
    }
}