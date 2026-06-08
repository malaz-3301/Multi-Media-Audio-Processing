using System;
using System.IO;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace WinFormsApp1
{
    public class FileWriter
    {
        public static void ExportFloatArrayToWav(string outputWavPath, float[] decompressedSamples, int sampleRate = 44100, int channels = 1, int bitsPerSample = 16)
        {
            var sampleProvider = new ArraySampleProvider(decompressedSamples, sampleRate, channels);
            WaveFileWriter.CreateWaveFile16(outputWavPath, sampleProvider);
        }

        public static void ExportFloatArrayToMP3(string outputMp3Path, float[] decompressedSamples, int sampleRate = 44100, int channels = 1, int bitRate = 192000)
        {
            bitRate = bitRate <= 0 ? 128000 : bitRate;
            bitRate = bitRate < 1000 ? bitRate * 1000 : bitRate;

            byte[] byteArray = new byte[decompressedSamples.Length * sizeof(float)];
            Buffer.BlockCopy(decompressedSamples, 0, byteArray, 0, byteArray.Length);
            WaveFormat floatFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);

            using (MemoryStream ms = new MemoryStream(byteArray))
            using (RawSourceWaveStream waveStream = new RawSourceWaveStream(ms, floatFormat))
            {
                var sampleProvider = waveStream.ToSampleProvider();
                var pcm16Provider = new SampleToWaveProvider16(sampleProvider);
                MediaFoundationEncoder.EncodeToMp3(pcm16Provider, outputMp3Path, bitRate);
            }
        }
    }
}