using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace WinFormsApp1
{
    public class SamplesHelper
    {
        public static float[] GetResampledSamples(AudioFileReader reader, int targetSampleRate)
        {
            if (reader.WaveFormat.SampleRate == targetSampleRate)
            {
                return ConvertToMono(GetSamples(reader), reader.WaveFormat.Channels);
            }

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
                {
                    samples.Add(buffer[i]);
                }
            }

            return ConvertToMono(samples.ToArray(), reader.WaveFormat.Channels);
        }

        public static float[] ConvertToMono(float[] samples, int channels)
        {
            if (channels <= 1)
                return samples;

            int frames = samples.Length / channels;
            float[] mono = new float[frames];

            for (int frame = 0; frame < frames; frame++)
            {
                float sum = 0f;

                for (int channel = 0; channel < channels; channel++)
                {
                    sum += samples[(frame * channels) + channel];
                }

                mono[frame] = sum / channels;
            }

            return mono;
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
                {
                    samples.Add(buffer[i]);
                }
            }

            return samples.ToArray();
        }
    }
}