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
        writer.Write(GetOutputChannels(info));
        writer.Write(info.bitsPerSample);
        writer.Write(info.isMP3Numeric());
        writer.Write(info.bitRate);
        writer.Write(ToBytes(bytes));
    }

    public static void SaveDPCM(string filePath, float firstSample, sbyte[] compressedSamples, float quantizationFactor, int sampleRate, AudioFileInfo info)
    {
        using BinaryWriter writer = new BinaryWriter(File.Create(filePath));

        writer.Write(firstSample);
        writer.Write(quantizationFactor);
        writer.Write(compressedSamples.Length);
        writer.Write(sampleRate);
        writer.Write(GetOutputChannels(info));
        writer.Write(info.bitsPerSample);
        writer.Write(info.isMP3Numeric());
        writer.Write(info.bitRate);
        writer.Write(ToBytes(compressedSamples));
    }

    public static void SaveDeltaModulation(string filePath, byte[] bits, float firstSample, float stepSize, int totalSamples, int sampleRate, AudioFileInfo info)
    {
        using BinaryWriter writer = new BinaryWriter(File.Create(filePath));

        writer.Write(firstSample);
        writer.Write(stepSize);
        writer.Write(totalSamples);
        writer.Write(sampleRate);
        writer.Write(GetOutputChannels(info));
        writer.Write(info.bitsPerSample);
        writer.Write(info.isMP3Numeric());
        writer.Write(info.bitRate);
        writer.Write(bits.Length);
        writer.Write(bits);
    }

    private static int GetOutputChannels(AudioFileInfo info)
    {
        return info.channels > 1 ? 1 : Math.Max(1, info.channels);
    }

    private static byte[] ToBytes(sbyte[] values)
    {
        byte[] result = new byte[values.Length];

        for (int i = 0; i < values.Length; i++)
        {
            result[i] = unchecked((byte)values[i]);
        }

        return result;
    }
}