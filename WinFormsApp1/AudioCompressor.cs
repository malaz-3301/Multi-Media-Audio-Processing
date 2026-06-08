using System;

public class AudioCompressor
{
    private static int lastPercent = -1;
    public static int samplesProcessed = 0;

    public static void ResetCounters()
    {
        lastPercent = -1;
        samplesProcessed = 0;
    }

    public static int GetSamplesProcessed()
    {
        int value = samplesProcessed;
        samplesProcessed = 0;
        return value;
    }

    public static int getSamplesProcessed()
    {
        return GetSamplesProcessed();
    }

    private static void UpdateProgress(int index, int total)
    {
        samplesProcessed++;
        int percent = total <= 0 ? 100 : (int)(((long)(index + 1) * 100) / total);

        if (percent > lastPercent)
        {
            lastPercent = percent;
            CompressionManager.ReportProgress(Math.Clamp(percent, 0, 100));

            if (percent >= 100)
                lastPercent = -1;
        }
    }

    public static sbyte[] NonLinear_Quantizer(float[] samples, int quantLevel)
    {
        quantLevel = Math.Max(2, quantLevel);
        sbyte[] result = new sbyte[samples.Length];
        const float logMuPlusOne = 5.54517f;
        float maxLevel = (quantLevel - 1) / 2f;

        for (int i = 0; i < samples.Length; i++)
        {
            CompressionManager.CheckCancellation();
            float sample = Math.Clamp(samples[i], -1f, 1f);
            float compressed = MathF.Log(1f + 255f * MathF.Abs(sample)) / logMuPlusOne;

            if (sample < 0)
                compressed *= -1;

            result[i] = (sbyte)Math.Clamp((int)MathF.Round(compressed * maxLevel), -128, 127);
            UpdateProgress(i, samples.Length);
        }

        return result;
    }

    public static (float firstSample, byte[] packedBits, int totalSamples) DeltaModulation(float[] samples)
    {
        return DeltaModulation(samples, 0.01f);
    }

    public static (float firstSample, byte[] packedBits, int totalSamples) DeltaModulation(float[] samples, float stepSize)
    {
        if (samples.Length == 0)
            return (0f, Array.Empty<byte>(), 0);

        stepSize = Math.Max(MathF.Abs(stepSize), 0.001f);
        float firstSample = Math.Clamp(samples[0], -1f, 1f);
        float reconstructed = firstSample;
        byte[] packedBits = new byte[(samples.Length - 1 + 7) / 8];

        for (int i = 1; i < samples.Length; i++)
        {
            CompressionManager.CheckCancellation();
            bool isUp = Math.Clamp(samples[i], -1f, 1f) >= reconstructed;

            if (isUp)
            {
                int byteIndex = (i - 1) / 8;
                int bitIndex = (i - 1) % 8;
                packedBits[byteIndex] |= (byte)(1 << (7 - bitIndex));
            }

            reconstructed += isUp ? stepSize : -stepSize;
            reconstructed = Math.Clamp(reconstructed, -1f, 1f);
            UpdateProgress(i, samples.Length);
        }

        return (firstSample, packedBits, samples.Length);
    }

    public static (float firstSample, float quantizeFactor, sbyte[] compressedSamples) DPCM(float[] samples, float quantizationFactor)
    {
        if (samples.Length == 0)
            return (0f, quantizationFactor, Array.Empty<sbyte>());

        quantizationFactor = Math.Max(MathF.Abs(quantizationFactor), 1f);
        float firstSample = Math.Clamp(samples[0], -1f, 1f);
        float reconstructed = firstSample;
        sbyte[] result = new sbyte[samples.Length - 1];

        for (int i = 1; i < samples.Length; i++)
        {
            CompressionManager.CheckCancellation();
            float diff = Math.Clamp(samples[i], -1f, 1f) - reconstructed;
            int quantized = Math.Clamp((int)MathF.Round(diff * quantizationFactor), -128, 127);
            result[i - 1] = (sbyte)quantized;
            reconstructed += quantized / quantizationFactor;
            reconstructed = Math.Clamp(reconstructed, -1f, 1f);
            UpdateProgress(i, samples.Length);
        }

        return (firstSample, quantizationFactor, result);
    }
}