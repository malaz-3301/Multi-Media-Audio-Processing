using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    public class DecompressLoader
    {
        public static (int quantizationLevels, sbyte[] bytes) LoadNonLinearQuant(string filePath)
        {
            using BinaryReader reader = new BinaryReader(File.OpenRead(filePath));

            int quantizationLevels = reader.ReadInt32();

            int length = reader.ReadInt32();

            byte[] rawBytes = reader.ReadBytes(length);

            sbyte[] result = new sbyte[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = unchecked((sbyte)rawBytes[i]);
            }

            return (quantizationLevels: quantizationLevels,bytes: result );
        }

        public static (float firstSample, sbyte[] bytes, float quantizationFactor) LoadDPCM(string filePath)
        {
            using BinaryReader reader = new BinaryReader(File.OpenRead(filePath));

            float firstSample= (float)reader.ReadSingle();
            float quantizeFactor= (float)reader.ReadSingle();
    
            int length = reader.ReadInt32();

            byte[] rawBytes = reader.ReadBytes(length);

            sbyte[] result = new sbyte[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = unchecked((sbyte)rawBytes[i]);
            }

            return (firstSample: firstSample, bytes:result,quantizationFactor:quantizeFactor);
        }

        public static (float firstSample, byte[] bytes, float stepSize) LoadDelta(string filePath)
        {
            using BinaryReader reader = new BinaryReader(File.OpenRead(filePath));

            float firstSample = (float)reader.ReadSingle();
            float stepSize = (float)reader.ReadSingle();

            int length = reader.ReadInt32();

            byte[] rawBytes = reader.ReadBytes(length);

            return (firstSample: firstSample, bytes: rawBytes, stepSize: stepSize);
        }

    }
}
