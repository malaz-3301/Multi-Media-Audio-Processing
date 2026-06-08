using System;

namespace WinFormsApp1
{
    public class AudioFileInfo
    {
        public int sampleRate;
        public int bitsPerSample;
        public int channels;
        public int bitRate;
        public bool isMP3;

        public AudioFileInfo() { }

        public AudioFileInfo(int sampleRate, int channels, int bitsPerSample, int bitRate, bool isMP3)
        {
            this.sampleRate = sampleRate;
            this.bitsPerSample = bitsPerSample;
            this.channels = channels;
            this.bitRate = bitRate;
            this.isMP3 = isMP3;
        }

        public int isMP3Numeric()
        {
            return isMP3 ? 1 : 0;
        }

        public int GetEncoderBitRate()
        {
            if (bitRate <= 0)
                return 128000;

            return bitRate < 1000 ? bitRate * 1000 : bitRate;
        }
    }
}