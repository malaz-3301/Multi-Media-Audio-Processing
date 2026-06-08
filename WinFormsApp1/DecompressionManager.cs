using System;
using WinFormsApp1;

namespace WinFormsApp1
{
    public class DecompressionManager
    {
        public static void HandleFileDecompression(string filePath, string outputPath)
        {
            (AudioFileInfo audioFileInfo, float[] decompressedSamples) InvResults = (audioFileInfo: new AudioFileInfo(), decompressedSamples: []);
            string extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();

            if (extension == ".nlq") InvResults = HandleNonLinearQuant(filePath);
            if (extension == ".dlt") InvResults = HandleDelta(filePath);
            if (extension == ".dpcm") InvResults = HandleDPCM(filePath);

            if (InvResults.decompressedSamples.Length == 0) return;

            if (!InvResults.audioFileInfo.isMP3)
            {
                outputPath += ".wav";
                FileWriter.ExportFloatArrayToWav(outputPath, InvResults.decompressedSamples,
                    sampleRate: InvResults.audioFileInfo.sampleRate,
                    channels: InvResults.audioFileInfo.channels,
                    bitsPerSample: InvResults.audioFileInfo.bitsPerSample);
            }
            else
            {
                outputPath += ".mp3";
                FileWriter.ExportFloatArrayToMP3(outputPath, InvResults.decompressedSamples,
                    sampleRate: InvResults.audioFileInfo.sampleRate,
                    channels: InvResults.audioFileInfo.channels,
                    bitRate: InvResults.audioFileInfo.bitRate);
            }

            static (AudioFileInfo audioFileInfo, float[] decompressedSamples) HandleNonLinearQuant(string filePath)
            {
                var decompResult = DecompressLoader.LoadNonLinearQuant(filePath);
                float[] decompressedSamples = AudioDecompressor.Inverse_NonLinear_Quantizer(decompResult.bytes, decompResult.quantizationLevels);
                return (decompResult.audioFileInfo, decompressedSamples);
            }

            static (AudioFileInfo audioFileInfo, float[] decompressedSamples) HandleDelta(string filePath)
            {
                var decompResult = DecompressLoader.LoadDelta(filePath);
                float[] decompressedSamples = AudioDecompressor.Inverse_DeltaModulation(decompResult.firstSample, decompResult.bytes, decompResult.totalSamples, decompResult.stepSize);
                return (decompResult.audioFileInfo, decompressedSamples);
            }

            static (AudioFileInfo audioFileInfo, float[] decompressedSamples) HandleDPCM(string filePath)
            {
                var decompResult = DecompressLoader.LoadDPCM(filePath);
                float[] decompressedSamples = AudioDecompressor.Inverse_DPCM(decompResult.firstSamples, decompResult.quantizationFactor, decompResult.bytes, decompResult.audioFileInfo.channels);
                return (decompResult.audioFileInfo, decompressedSamples);
            }
        }
    }
}