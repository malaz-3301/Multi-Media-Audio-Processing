using System;

public class AudioCompressor
{
    private static int _lastReportedPercent = -1;
    public static int samplesProcessed = 0;

    public static void ResetCounters()
    {
        _lastReportedPercent = -1;
        samplesProcessed = 0;
    }

    private static void updateProgress(int sampleIndex, int numOfSamples)
    {
        if (sampleIndex == numOfSamples - 1)
        {
            CompressionManager.ReportProgress(100);
            _lastReportedPercent = -1;
            return;
        }

        samplesProcessed++;
        int currentPercent = (int)(((long)sampleIndex * 100) / numOfSamples);

        if (currentPercent > _lastReportedPercent)
        {
            _lastReportedPercent = currentPercent;
            CompressionManager.ReportProgress(currentPercent);
            System.Threading.Thread.Sleep(150);
        }
    }

    public static int getSamplesProcessed()
    {
        int temp = samplesProcessed;
        samplesProcessed = 0;
        return temp;
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
            updateProgress(i, samples.Length);
        }

        return result;
    }

    public static (float firstSample, byte[] packedBits, int totalSamples) DeltaModulation(float[] samples)
    {
        float firstSample = samples[0];
        int totalSamplesToCompress = samples.Length - 1;
        int packedLength = (totalSamplesToCompress + 7) / 8;
        byte[] packedBits = new byte[packedLength];

        for (int i = 1; i < samples.Length; i++)
        {
            CompressionManager.CheckCancellation();

            bool isUp = samples[i] > samples[i - 1];

            if (isUp)
            {
                int byteIndex = (i - 1) / 8;
                int bitIndex = (i - 1) % 8;
                packedBits[byteIndex] |= (byte)(1 << (7 - bitIndex));
            }

            updateProgress(i, samples.Length);
        }

        return (firstSample, packedBits, samples.Length);
    }

    public static (float[] firstSamples, float quantizeFactor, sbyte[] compressedSamples) DPCM(float[] samples, float quantizationFactor, int channels = 1)
    {
        if (samples.Length == 0)
            return (Array.Empty<float>(), quantizationFactor, Array.Empty<sbyte>());

        channels = Math.Max(1, channels);
        quantizationFactor = Math.Max(MathF.Abs(quantizationFactor), 1f);

        int firstCount = Math.Min(channels, samples.Length);
        float[] firstSamples = new float[firstCount];
        float[] reconstructed = new float[channels];

        for (int i = 0; i < firstCount; i++)
        {
            firstSamples[i] = Math.Clamp(samples[i], -1f, 1f);
            reconstructed[i] = firstSamples[i];
        }

        sbyte[] result = new sbyte[samples.Length - firstCount];

        for (int i = firstCount; i < samples.Length; i++)
        {
            CompressionManager.CheckCancellation();

            int channel = i % channels;
            float diff = Math.Clamp(samples[i], -1f, 1f) - reconstructed[channel];
            int quantized = Math.Clamp((int)MathF.Round(diff * quantizationFactor), -128, 127);
            result[i - firstCount] = (sbyte)quantized;
            reconstructed[channel] += quantized / quantizationFactor;
            reconstructed[channel] = Math.Clamp(reconstructed[channel], -1f, 1f);
            updateProgress(i, samples.Length);
        }

        return (firstSamples, quantizationFactor, result);
    }
}