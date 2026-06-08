using System.Collections.Generic;
using NAudio.Wave;

namespace WinFormsApp1
{
    public class SamplesHelper
    {
        public static float[] GetResampledSamples(AudioFileReader reader, int targetSampleRate)
        {
            if (reader.WaveFormat.SampleRate == targetSampleRate)
                return GetSamples(reader);

            reader.Position = 0;

            var targetFormat = WaveFormat.CreateIeeeFloatWaveFormat(targetSampleRate, reader.WaveFormat.Channels);

            using var resampler = new MediaFoundationResampler(reader, targetFormat);
            resampler.ResamplerQuality = 60;

            var sampleProvider = resampler.ToSampleProvider();
            List<float> samples = new();
            float[] buffer = new float[4096];
            int samplesRead;

            while ((samplesRead = sampleProvider.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < samplesRead; i++)
                    samples.Add(buffer[i]);
            }

            return samples.ToArray();
        }

        private static float[] GetSamples(AudioFileReader reader)
        {
            reader.Position = 0;

            List<float> samples = new();
            float[] buffer = new float[4096];
            int samplesRead;

            while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < samplesRead; i++)
                    samples.Add(buffer[i]);
            }

            return samples.ToArray();
        }
    }
}