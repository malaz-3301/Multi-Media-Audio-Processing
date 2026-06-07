using System;
using System.Threading;

using NAudio.Wave;

public class AudioCompressor
{
    private static int _lastReportedPercent = -1;
    public static int samplesProcessed = 0;

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
    public static int getSamplesProcessed() {
        int temp = samplesProcessed;
        samplesProcessed = 0;
        return temp;
    
    }

    public static sbyte[] NonLinear_Quantizer(float[]samples,int quantLevel){
        
        sbyte[] bits = new sbyte[samples.Length];

        const float LogMuPlusOne = 5.54517f;

        float maxLevel = (quantLevel - 1) / 2f;

        for (int i = 0; i < samples.Length; i++)
        {
            CompressionManager.CheckCancellation();
            float currentVal = samples[i];
            float compressedVal = MathF.Log(1f + 255f * Math.Abs(samples[i])) / LogMuPlusOne;
            if (currentVal < 0) compressedVal *= -1;
            float quantized = (compressedVal * maxLevel);
            bits[i]=(sbyte)Math.Clamp(MathF.Round(quantized), -128, 127);
            updateProgress(i, samples.Length);
        }
        return bits;

    }

    public static (float firstSample, byte[] packedBits,int totalSamples) DeltaModulation(float[] samples)
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
    public static (float firstSample, float quantizeFactor,sbyte[] compressedSamples) DPCM(float[]samples,float quantizationFactor) {
        

        float firstSample = samples[0];             

        sbyte[] compressedSamples = new sbyte[samples.Length - 1];

        for (int i = 1; i < samples.Length; i++)
        {
            CompressionManager.CheckCancellation();
            float diff = samples[i] - samples[i - 1];

            compressedSamples[i - 1] = (sbyte)(Math.Clamp((diff * quantizationFactor), -128, 127));
            updateProgress(i, samples.Length);

        }
        return (firstSample,quantizationFactor ,compressedSamples);
    }

    
}
