using System;
using NAudio.Wave;

namespace WinFormsApp1
{
    public class ArraySampleProvider : ISampleProvider
    {
        private readonly float[] samples;
        private int position;

        public ArraySampleProvider(float[] samples, int sampleRate, int channels)
        {
            this.samples = samples;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(Math.Max(8000, sampleRate), Math.Max(1, channels));
        }

        public WaveFormat WaveFormat { get; }

        public int Read(float[] buffer, int offset, int count)
        {
            int available = samples.Length - position;
            int read = Math.Min(available, count);

            for (int i = 0; i < read; i++)
            {
                buffer[offset + i] = Math.Clamp(samples[position + i], -1f, 1f);
            }

            position += read;
            return read;
        }
    }
}