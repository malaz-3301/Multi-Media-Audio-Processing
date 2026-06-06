using System;
using System.Collections.Generic;

public class AudioDecompressor
{
    
    public static float[] Inverse_NonLinear_Quantizer(sbyte[] bits,int quantLevel)
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

  
    public static float[] Inverse_DeltaModulation(float firstSample, byte[] bits, float stepSize = 0.01f)
    {
        float[] samples = new float[bits.Length];

        
        samples[0] = firstSample;

        for (int i = 1; i < bits.Length; i++)
        {
            
            bool isUp = bits[i - 1] == 1;

            
            float change = isUp ? stepSize : -stepSize;

            
            float reconstructedSample = samples[i - 1] + change;

            samples[i] = Math.Clamp(reconstructedSample, -1f, 1f);
        }

        return samples;
    }

    
    public static float[] Inverse_DPCM(float firstSample, float quantizationFactor, sbyte[] compressedSamples)
    {
        
        float[] samples = new float[compressedSamples.Length + 1];

       
        samples[0] = firstSample;

        for (int i = 0; i < compressedSamples.Length; i++)
        {
            
            float diff = compressedSamples[i] / (float)quantizationFactor;

            
            float reconstructedSample = samples[i] + diff;

            samples[i + 1] = Math.Clamp(reconstructedSample, -1f, 1f);
        }

        return samples;
    }
}