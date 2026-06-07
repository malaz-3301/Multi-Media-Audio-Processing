using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    public class AudioFileInfo
    {
        public int sampleRate;
        public int bitsPerSample;
        public int channels;
        public bool isMP3;

        public AudioFileInfo(int sampleRate, int channels, int bitsPerSample,bool isMP3) {
            this.sampleRate=sampleRate;
            this.bitsPerSample=bitsPerSample;
            this.channels=channels;
            this.isMP3=isMP3;
        }

        public int isMP3Numeric() {
            if (isMP3) return 1;
            return 0;
        }
    }
}
