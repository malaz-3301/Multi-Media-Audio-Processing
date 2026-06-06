using System;
using NAudio.Wave;

public class AudioCompressor
{

    public static sbyte[] NonLinear_Quantizer(float[]samples,int quantLevel){
        
        sbyte[] bits = new sbyte[samples.Length];

        const float LogMuPlusOne = 5.54517f;

        float maxLevel = (quantLevel - 1) / 2f;

        for (int i = 0; i < samples.Length; i++)
        {
            float currentVal = samples[i];
            float compressedVal = MathF.Log(1f + 255f * Math.Abs(samples[i])) / LogMuPlusOne;
            if (currentVal < 0) compressedVal *= -1;
            float quantized = (compressedVal * maxLevel);
            bits[i]=(sbyte)Math.Clamp(MathF.Round(quantized), -128, 127);
        }

        return bits;

    }

    public static (float firstSample, byte[] bits) DeltaModulation(float[]samples) {

        float firstSample = samples[0];
        byte[] bits=new byte[samples.Length];

        for (int i = 1; i < samples.Length; i++)
        {
            bool isUp = samples[i] > samples[i - 1];
            bits[i - 1] = (byte)(isUp?1:0);
        }

        return (firstSample:firstSample,bits:bits);

    }
    public static (float firstSample, float quantizeFactor,sbyte[] compressedSamples) DPCM(float[]samples,float quantizationFactor) {
        

        float firstSample = samples[0];
        
        

        sbyte[] compressedSamples = new sbyte[samples.Length - 1];

        for (int i = 1; i < samples.Length; i++)
        {
            float diff = samples[i] - samples[i - 1];

            compressedSamples[i - 1] = (sbyte)(Math.Clamp((diff * quantizationFactor), -128, 127));
        }

        return (firstSample,quantizationFactor ,compressedSamples);
    }

    
}
