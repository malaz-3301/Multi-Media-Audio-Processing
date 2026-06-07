
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using NAudio.Wave;
using WinFormsApp1;

public class CompressionManager {

    private static IProgress<int> progress;
    private static CancellationToken token;
    private static System.Threading.Timer timer;
    private static bool running = false;
    private static int recordedSeconds = 0;
    private static CompressionTypes currentType;
    private static AudioFileInfo audioInfo;
    
    public static void CompressAudioFile(AudioFileReader reader, CompressionSettings compSett,IProgress<int> prog,CancellationToken tkn,string origFilePath,AudioFileInfo audioFileInfo )
    {
        CompressionChartController.Reset();
        currentType = compSett.Type;
        recordedSeconds = 0;
        initalizeTimer();
        CompressionReporter.StartCompression();
        CompressionReporter.recordCompressionSettings(compSett);
        audioInfo = audioFileInfo;
        string outPath = GetOutputPath(compSett);
        CompressionReporter.recordCompedFile(outPath);
        CompressionReporter.recordOrigFile(origFilePath);

        progress = prog;
        token = tkn;
        
        switch (compSett.Type)
        {
            case CompressionTypes.DeltaModulation:
                handleDeltaCompress(reader, compSett,outPath);
                break;

            case CompressionTypes.NonlinearQuant:
                handleNonLinearQuant(reader, compSett, outPath);
                break;

            case CompressionTypes.DPCM:
                handleDPCM(reader, compSett,outPath);
                break;
        }

        CompressionReporter.EndCompression();

        running = false;       
    }


    static void Tick(object state)
    {
        if (!running)
        {
            timer?.Dispose();
            return;
        }

        int samples = AudioCompressor.getSamplesProcessed();
        recordedSeconds += 1;

        double spaceSavedPercentage = 0.0;
        switch (currentType)
        {
            case CompressionTypes.DeltaModulation:
                
                spaceSavedPercentage = ((audioInfo.bitsPerSample - 1.0) / 32.0) * 100.0;
                break;

            case CompressionTypes.NonlinearQuant:
            case CompressionTypes.DPCM:          
                spaceSavedPercentage = ((audioInfo.bitsPerSample - 8.0) / 32.0) * 100.0;
                break;
        }

        
        CompressionChartController.AddPoint(recordedSeconds, samples, spaceSavedPercentage);
    }
    private static void initalizeTimer() {
        running = true;
        timer = new System.Threading.Timer(Tick!,null,0,1000);
    }

    public static void ReportProgress(int percent)
    {
        progress?.Report(percent);
    }

    public static void CheckCancellation()
    {
        token.ThrowIfCancellationRequested();
    }

    public static string GetOutputPath(CompressionSettings settings) {
        string fileName= "";
        switch (settings.Type)
        {
            case CompressionTypes.DeltaModulation:
                fileName = "output_delta.dlt";
                break;

            case CompressionTypes.NonlinearQuant:
                fileName = "output_nlq.nlq";
                break;

            case CompressionTypes.DPCM:
                fileName = "output_dpcm.dpcm";
                break;
        }
        return Path.Combine(settings.SavePath, fileName);
    }

    static void handleDeltaCompress(AudioFileReader reader, CompressionSettings compSett,string outPath) {
        float[] samples = SamplesHelper.GetResampledSamples(reader,compSett.SampleRate);
        var compResult=AudioCompressor.DeltaModulation(samples);
        FileSaver.SaveDeltaModulation(outPath, compResult.packedBits,compResult.firstSample,compSett.StepSize,compResult.totalSamples,compSett.SampleRate, audioInfo);
    }

    static void handleNonLinearQuant(AudioFileReader reader, CompressionSettings compSett, string outPath) {
        float[] samples = SamplesHelper.GetResampledSamples(reader, compSett.SampleRate);
        sbyte[] compResult= AudioCompressor.NonLinear_Quantizer(samples,compSett.QuantizationLevels);
        FileSaver.SaveNonLinearQuant(outPath,compResult,compSett.QuantizationLevels, compSett.SampleRate, audioInfo);
    }

    static void handleDPCM(AudioFileReader reader, CompressionSettings compSett,string outPath)
    {
        float[] samples = SamplesHelper.GetResampledSamples(reader, compSett.SampleRate);
        float quantFactor = compSett.QuantizationFactor;
        var compResult = AudioCompressor.DPCM(samples,quantFactor);
        FileSaver.SaveDPCM(outPath,compResult.firstSample,compResult.compressedSamples,quantFactor, compSett.SampleRate, audioInfo);
    }


}