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
        public static void ExportFloatArrayToWav(string outputWavPath, float[] decompressedSamples, int sampleRate = 44100, int channels = 1)
        {

            WaveFormat waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
            using (WaveFileWriter writer = new WaveFileWriter(outputWavPath, waveFormat))
            {
                writer.WriteSamples(decompressedSamples, 0, decompressedSamples.Length);
            }
        }
    }
}
