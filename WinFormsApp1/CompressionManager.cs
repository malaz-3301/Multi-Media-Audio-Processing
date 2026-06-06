
using System.IO;
using NAudio.Wave;
using WinFormsApp1;

public class CompressionManager {

    public static void CompressAudioFile(AudioFileReader reader, CompressionSettings compSett)
    {
        switch (compSett.Type)
        {
            case CompressionTypes.DeltaModulation:
                handleDeltaCompress(reader, compSett);
                break;

            case CompressionTypes.NonlinearQuant:
                handleNonLinearQuant(reader, compSett);
                break;

            case CompressionTypes.DPCM:
                handleDPCM(reader, compSett);
                break;
        }
    }

    static void handleDeltaCompress(AudioFileReader reader, CompressionSettings compSett) {
        float[] samples = SamplesHelper.GetResampledSamples(reader,compSett.SampleRate);
        var compResult=AudioCompressor.DeltaModulation(samples);
        string fileName = $"output_delta.dlt";
        string path = Path.Combine(compSett.SavePath, fileName);
        FileSaver.SaveDeltaModulation(path, compResult.bits,compResult.firstSample,compSett.StepSize);

    }

    static void handleNonLinearQuant(AudioFileReader reader, CompressionSettings compSett) {
        float[] samples = SamplesHelper.GetResampledSamples(reader, compSett.SampleRate);
        sbyte[] compResult= AudioCompressor.NonLinear_Quantizer(samples,compSett.QuantizationLevels);
        string fileName = $"output_nlq.nlq";
        string path = Path.Combine(compSett.SavePath, fileName);
        FileSaver.SaveNonLinearQuant(path,compResult,compSett.QuantizationLevels);
    }

    static void handleDPCM(AudioFileReader reader, CompressionSettings compSett)
    {
        float[] samples = SamplesHelper.GetResampledSamples(reader, compSett.SampleRate);
        float quantFactor = compSett.QuantizationFactor;
        var compResult = AudioCompressor.DPCM(samples,quantFactor);
        string fileName = $"output_dpcm.dpcm";
        string path = Path.Combine(compSett.SavePath, fileName);
        FileSaver.SaveDPCM(path,compResult.firstSample,compResult.compressedSamples,quantFactor);

    }


}