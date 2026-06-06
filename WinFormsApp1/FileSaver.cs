using System;
using System.IO;
using static Emgu.CV.ML.KNearest;


public class FileSaver
{

    public static void SaveNonLinearQuant(string filePath, sbyte[]bytes,int quantizationLevels) {
        using BinaryWriter writer =
                new BinaryWriter(File.Create(filePath));
        writer.Write(quantizationLevels);
        writer.Write(bytes.Length);

        byte[] rawBytes = (byte[])(object)bytes;
        writer.Write(rawBytes);
    }
    
    public static void SaveDPCM(string filePath,float firstSample,sbyte[] compressedSamples,float quantizationFactor)
    {
        using BinaryWriter writer =
            new BinaryWriter(File.Create(filePath));

        writer.Write(firstSample);
        writer.Write(quantizationFactor);
        writer.Write(compressedSamples.Length);
        
        byte[] rawBytes = (byte[])(object)compressedSamples;
        writer.Write(rawBytes);
        
    }
    public static void SaveDeltaModulation(String filePath, byte[]bits,float firstSample,float stepSize) {
        using BinaryWriter writer =
               new BinaryWriter(File.Create(filePath));

        writer.Write(firstSample);
        writer.Write(stepSize);
        writer.Write(bits.Length);

        writer.Write(bits);
        
    }
}
