using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace WinFormsApp1
{
    public class FileWriter
    {
        public static void ExportFloatArrayToWav(string outputWavPath, float[] decompressedSamples, int sampleRate = 44100, int channels = 1, int bitsPerSample = 16)
        {

            WaveFormat waveFormat = new WaveFormat(sampleRate, bitsPerSample, channels);
            using (WaveFileWriter writer = new WaveFileWriter(outputWavPath, waveFormat))
            {
                writer.WriteSamples(decompressedSamples, 0, decompressedSamples.Length);
            }
        }

        public static void ExportFloatArrayToMp3(string outputMp3Path, float[] decompressedSamples, int sampleRate = 44100, int channels = 1, int bitRate = 192000)
        {

            byte[] byteArray = new byte[decompressedSamples.Length * sizeof(float)];
            Buffer.BlockCopy(decompressedSamples, 0, byteArray, 0, byteArray.Length);
            WaveFormat floatFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);

            using (MemoryStream ms = new MemoryStream(byteArray))
            using (RawSourceWaveStream waveStream = new RawSourceWaveStream(ms, floatFormat))
            {
                MediaFoundationEncoder.EncodeToMp3(waveStream, outputMp3Path, bitRate);
            }
        }
    }
}

