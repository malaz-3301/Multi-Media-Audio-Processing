using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinFormsApp1;

namespace WinFormsApp1
{
    public class DecompressionManager
    {
        public static void HandleFileDecompression(string filePath,string outputPath) {
            float[] samples=[];
            
            if (filePath.EndsWith(".nlq"))  samples = HandleNonLinearQuant(filePath);
            if (filePath.EndsWith(".dlt"))  samples = HandleDelta(filePath);
            if (filePath.EndsWith(".dpcm")) samples = HandleDPCM(filePath);
            
            if (samples.Length == 0) return;
            
            FileWriter.ExportFloatArrayToWav(outputPath,samples);

        }
        static float[] HandleNonLinearQuant(string filePath) {
            var decompResult = DecompressLoader.LoadNonLinearQuant(filePath);
            float[] decompressedSamples = AudioDecompressor.Inverse_NonLinear_Quantizer(decompResult.bytes,decompResult.quantizationLevels);
            return decompressedSamples;
        }
        static float[] HandleDelta(string filePath)
        {
            var decompResult = DecompressLoader.LoadDelta(filePath);
            float[] decompressedSamples = AudioDecompressor.Inverse_DeltaModulation(decompResult.firstSample,decompResult.bytes,stepSize:decompResult.stepSize);
            return decompressedSamples;
        }
        static float[] HandleDPCM(string filePath)
        {
            var decompResult = DecompressLoader.LoadDPCM(filePath);
            float[] decompressedSamples = AudioDecompressor.Inverse_DPCM(decompResult.firstSample, decompResult.quantizationFactor, decompResult.bytes);
            return decompressedSamples;
        }
    }
}
