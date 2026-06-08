using System;
using System.IO;
using WinFormsApp1;

public class FileSaver
{
    public static void SaveNonLinearQuant(string filePath, sbyte[] bytes, int quantizationLevels, int sampleRate, AudioFileInfo info)
    {
        using BinaryWriter writer = new BinaryWriter(File.Create(filePath));

        writer.Write(quantizationLevels);
        writer.Write(bytes.Length);
        writer.Write(sampleRate);
        writer.Write(Math.Max(1, info.channels));
        writer.Write(info.bitsPerSample);
        writer.Write(info.isMP3Numeric());
        writer.Write(info.bitRate);
        writer.Write(ToBytes(bytes));
    }

    public static void SaveDPCM(string filePath, float[] firstSamples, sbyte[] compressedSamples, float quantizationFactor, int sampleRate, AudioFileInfo info)
    {
        using BinaryWriter writer = new BinaryWriter(File.Create(filePath));

        writer.Write(firstSamples.Length);
        foreach (float sample in firstSamples)
            writer.Write(sample);

        writer.Write(quantizationFactor);
        writer.Write(compressedSamples.Length);
        writer.Write(sampleRate);
        writer.Write(Math.Max(1, info.channels));
        writer.Write(info.bitsPerSample);
        writer.Write(info.isMP3Numeric());
        writer.Write(info.bitRate);
        writer.Write(ToBytes(compressedSamples));
    }

    public static void SaveDeltaModulation(string filePath, byte[] bits, float[] firstSamples, float stepSize, int totalSamples, int sampleRate, AudioFileInfo info)
    {
        using BinaryWriter writer = new BinaryWriter(File.Create(filePath));

        writer.Write(firstSamples.Length);
        foreach (float sample in firstSamples)
            writer.Write(sample);

        writer.Write(stepSize);
        writer.Write(totalSamples);
        writer.Write(sampleRate);
        writer.Write(Math.Max(1, info.channels));
        writer.Write(info.bitsPerSample);
        writer.Write(info.isMP3Numeric());
        writer.Write(info.bitRate);
        writer.Write(bits.Length);
        writer.Write(bits);
    }

    private static byte[] ToBytes(sbyte[] values)
    {
        byte[] result = new byte[values.Length];

        for (int i = 0; i < values.Length; i++)
            result[i] = unchecked((byte)values[i]);

        return result;
    }
}