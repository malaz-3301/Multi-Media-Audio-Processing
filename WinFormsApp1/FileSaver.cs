using System;
using System.IO;
using ScottPlot;
using WinFormsApp1;
using static Emgu.CV.ML.KNearest;


public class FileSaver
{

    public static void SaveNonLinearQuant(string filePath, sbyte[] bytes, int quantizationLevels, int sampleRate, AudioFileInfo info) {
        using BinaryWriter writer =
                new BinaryWriter(File.Create(filePath));
        writer.Write(quantizationLevels);
        writer.Write(bytes.Length);
        writer.Write(sampleRate);
        writer.Write(info.channels);
        writer.Write(info.bitsPerSample);
        writer.Write(info.isMP3Numeric());
        writer.Write(info.bitRate);


        byte[] rawBytes = (byte[])(object)bytes;
        writer.Write(rawBytes);
    }
    
    public static void SaveDPCM(string filePath,float firstSample,sbyte[] compressedSamples,float quantizationFactor,int sampleRate, AudioFileInfo info)
    {
        using BinaryWriter writer =
            new BinaryWriter(File.Create(filePath));

        writer.Write(firstSample);
        writer.Write(quantizationFactor);
        writer.Write(compressedSamples.Length);
        writer.Write(sampleRate);
        writer.Write(info.channels);
        writer.Write(info.bitsPerSample);
        writer.Write(info.isMP3Numeric());
        writer.Write(info.bitRate);




        byte[] rawBytes = (byte[])(object)compressedSamples;
        writer.Write(rawBytes);
        
    }
    public static void SaveDeltaModulation(string filePath, byte[]bits,float firstSample,float stepSize, int totalSamples,int sampleRate, AudioFileInfo info) {
        using BinaryWriter writer =
               new BinaryWriter(File.Create(filePath));

        writer.Write(firstSample);
        writer.Write(stepSize);
        writer.Write(totalSamples);
        writer.Write(sampleRate);
        writer.Write(info.channels);
        writer.Write(info.bitsPerSample);
        writer.Write(info.isMP3Numeric());
        writer.Write(info.bitRate);



        writer.Write(bits);
        
    }
}
