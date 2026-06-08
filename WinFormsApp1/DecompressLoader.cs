using System;
using System.IO;

namespace WinFormsApp1
{
    public class DecompressLoader
    {
        public static (int quantizationLevels, sbyte[] bytes, AudioFileInfo audioFileInfo) LoadNonLinearQuant(string filePath)
        {
            using BinaryReader reader = new BinaryReader(File.OpenRead(filePath));

            int quantizationLevels = reader.ReadInt32();
            int length = reader.ReadInt32();
            int sampleRate = reader.ReadInt32();
            int channels = reader.ReadInt32();
            int bitsPerSample = reader.ReadInt32();
            bool isMP3 = reader.ReadInt32() == 1;
            int bitRate = reader.ReadInt32();

            AudioFileInfo audioInfo = new AudioFileInfo(sampleRate, channels, bitsPerSample, bitRate, isMP3);
            byte[] rawBytes = reader.ReadBytes(length);

            return (quantizationLevels, ToSBytes(rawBytes), audioInfo);
        }

        public static (float firstSample, sbyte[] bytes, float quantizationFactor, AudioFileInfo audioFileInfo) LoadDPCM(string filePath)
        {
            using BinaryReader reader = new BinaryReader(File.OpenRead(filePath));

            float firstSample = reader.ReadSingle();
            float quantizationFactor = reader.ReadSingle();
            int length = reader.ReadInt32();
            int sampleRate = reader.ReadInt32();
            int channels = reader.ReadInt32();
            int bitsPerSample = reader.ReadInt32();
            bool isMP3 = reader.ReadInt32() == 1;
            int bitRate = reader.ReadInt32();

            AudioFileInfo audioInfo = new AudioFileInfo(sampleRate, channels, bitsPerSample, bitRate, isMP3);
            byte[] rawBytes = reader.ReadBytes(length);

            return (firstSample, ToSBytes(rawBytes), quantizationFactor, audioInfo);
        }

        public static (float firstSample, byte[] bytes, float stepSize, int totalSamples, AudioFileInfo audioFileInfo) LoadDelta(string filePath)
        {
            using BinaryReader reader = new BinaryReader(File.OpenRead(filePath));

            float firstSample = reader.ReadSingle();
            float stepSize = reader.ReadSingle();
            int totalSamples = reader.ReadInt32();
            int sampleRate = reader.ReadInt32();
            int channels = reader.ReadInt32();
            int bitsPerSample = reader.ReadInt32();
            bool isMP3 = reader.ReadInt32() == 1;
            int bitRate = reader.ReadInt32();
            int packedLength = reader.ReadInt32();

            AudioFileInfo audioInfo = new AudioFileInfo(sampleRate, channels, bitsPerSample, bitRate, isMP3);
            byte[] rawBytes = reader.ReadBytes(packedLength);

            return (firstSample, rawBytes, stepSize, totalSamples, audioInfo);
        }

        private static sbyte[] ToSBytes(byte[] values)
        {
            sbyte[] result = new sbyte[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                result[i] = unchecked((sbyte)values[i]);
            }

            return result;
        }
    }
}