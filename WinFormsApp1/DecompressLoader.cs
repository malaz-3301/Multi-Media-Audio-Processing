using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using ScottPlot;

namespace WinFormsApp1
{
    public class DecompressLoader
    {
        public static (int quantizationLevels, sbyte[] bytes, AudioFileInfo audioFileInfo) LoadNonLinearQuant(string filePath)
        {
            using BinaryReader reader = new BinaryReader(File.OpenRead(filePath));

            int quantizationLevels = reader.ReadInt32();

            int length = reader.ReadInt32();
            int sampleRate= reader.ReadInt32();
            int channels = reader.ReadInt32();
            int bitsPerSample = reader.ReadInt32();
            bool isMP3 = reader.ReadInt32()==1;
            int bitRate = reader.ReadInt32();

            AudioFileInfo adI = new AudioFileInfo(sampleRate,channels,bitsPerSample,bitRate,isMP3);

            byte[] rawBytes = reader.ReadBytes(length);

            sbyte[] result = new sbyte[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = unchecked((sbyte)rawBytes[i]);
            }

            return (quantizationLevels: quantizationLevels,bytes: result,audioFileInfo: adI);
        }

        public static (float firstSample, sbyte[] bytes, float quantizationFactor, AudioFileInfo audioFileInfo) LoadDPCM(string filePath)
        {
            using BinaryReader reader = new BinaryReader(File.OpenRead(filePath));

            float firstSample= (float)reader.ReadSingle();
            float quantizeFactor= (float)reader.ReadSingle();
    
            int length = reader.ReadInt32();
            int sampleRate= reader.ReadInt32();
            int channels = reader.ReadInt32();
            int bitsPerSample = reader.ReadInt32();
            bool isMP3 = reader.ReadInt32() == 1;
            int bitRate = reader.ReadInt32();


            AudioFileInfo adI = new AudioFileInfo(sampleRate, channels, bitsPerSample, bitRate, isMP3);


            byte[] rawBytes = reader.ReadBytes(length);

            sbyte[] result = new sbyte[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = unchecked((sbyte)rawBytes[i]);
            }

            return (firstSample: firstSample, bytes:result,quantizationFactor:quantizeFactor, audioFileInfo: adI);
        }

        public static (float firstSample, byte[] bytes, float stepSize,int totalSamples, AudioFileInfo audioFileInfo) LoadDelta(string filePath)
        {
            using BinaryReader reader = new BinaryReader(File.OpenRead(filePath));

            float firstSample = (float)reader.ReadSingle();
            float stepSize = (float)reader.ReadSingle();

            int length = reader.ReadInt32();
            int sampleRate= reader.ReadInt32();
            int channels = reader.ReadInt32();
            int bitsPerSample = reader.ReadInt32();
            bool isMP3 = reader.ReadInt32() == 1;
            int bitRate = reader.ReadInt32();


            AudioFileInfo adI = new AudioFileInfo(sampleRate, channels, bitsPerSample, bitRate, isMP3);


            byte[] rawBytes = reader.ReadBytes(length);

            return (firstSample: firstSample, bytes: rawBytes, stepSize: stepSize,totalSamples:length,audioFileInfo:adI);
        }

    }
}
