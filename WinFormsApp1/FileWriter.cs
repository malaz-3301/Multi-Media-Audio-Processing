using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace WinFormsApp1
{
    public class FileWriter
    {
        public static void ExportFloatArrayToWav(string outputWavPath, float[] decompressedSamples, int sampleRate = 44100, int channels = 1, int bitsPerSample = 16)
        {
            WaveFormat waveFormat;

            if (bitsPerSample == 32)
            {
                waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
            }
            else if (bitsPerSample != 16 && bitsPerSample != 24)
            {
                waveFormat = new WaveFormat(sampleRate, 16, channels);
            }
            else
            {
                waveFormat = new WaveFormat(sampleRate, bitsPerSample, channels);
            }

            using (WaveFileWriter writer = new WaveFileWriter(outputWavPath, waveFormat))
            {
                writer.WriteSamples(decompressedSamples, 0, decompressedSamples.Length);
            }
        }

        public static void ExportFloatArrayToMP3(string outputMp3Path, float[] decompressedSamples, int sampleRate = 44100, int channels = 1, int bitRate = 192000)
        {
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

