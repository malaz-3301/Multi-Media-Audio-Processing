using System.IO;
using System.Threading;
using NAudio.Wave;
using WinFormsApp1;

public class CompressionManager
{
    private static IProgress<int> progress;
    private static CancellationToken token;
    private static System.Threading.Timer timer;
    private static bool running = false;
    private static int recordedSeconds = 0;
    private static int lastChartPercent = -1;
    private static CompressionTypes currentType;
    private static AudioFileInfo audioInfo;

    public static void CompressAudioFile(AudioFileReader reader, CompressionSettings compSett, IProgress<int> prog, CancellationToken tkn, string origFilePath, AudioFileInfo audioFileInfo)
    {
        CompressionChartController.Reset();
        AudioCompressor.ResetCounters();
        currentType = compSett.Type;
        recordedSeconds = 0;
        lastChartPercent = -1;
        audioInfo = audioFileInfo;
        progress = prog;
        token = tkn;

        initalizeTimer();
        CompressionReporter.StartCompression();
        CompressionReporter.recordCompressionSettings(compSett);
        string outPath = GetOutputPath(compSett);
        CompressionReporter.recordCompedFile(outPath);
        CompressionReporter.recordOrigFile(origFilePath);

        try
        {
            switch (compSett.Type)
            {
                case CompressionTypes.DeltaModulation:
                    handleDeltaCompress(reader, compSett, outPath);
                    break;
                case CompressionTypes.NonlinearQuant:
                    handleNonLinearQuant(reader, compSett, outPath);
                    break;
                case CompressionTypes.DPCM:
                    handleDPCM(reader, compSett, outPath);
                    break;
            }

            AddChartPoint(100);
            CompressionReporter.EndCompression();
        }
        finally
        {
            running = false;
            timer?.Dispose();
        }
    }

    static void Tick(object state)
    {
        if (!running)
            return;

        AddChartPoint(-1);
    }

    private static void initalizeTimer()
    {
        running = true;
        timer = new System.Threading.Timer(Tick, null, 0, 500);
    }

    public static void ReportProgress(int percent)
    {
        progress?.Report(percent);

        if (percent == 100 || percent - lastChartPercent >= 5)
        {
            lastChartPercent = percent;
            AddChartPoint(percent);
        }
    }

    public static void CheckCancellation()
    {
        token.ThrowIfCancellationRequested();
    }

    public static string GetOutputPath(CompressionSettings settings)
    {
        string fileName = "";
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

    static void handleDeltaCompress(AudioFileReader reader, CompressionSettings compSett, string outPath)
    {
        float[] samples = SamplesHelper.GetResampledSamples(reader, compSett.SampleRate);
        var compResult = AudioCompressor.DeltaModulation(samples, compSett.StepSize);
        FileSaver.SaveDeltaModulation(outPath, compResult.packedBits, compResult.firstSample, compSett.StepSize, compResult.totalSamples, compSett.SampleRate, audioInfo);
    }

    static void handleNonLinearQuant(AudioFileReader reader, CompressionSettings compSett, string outPath)
    {
        float[] samples = SamplesHelper.GetResampledSamples(reader, compSett.SampleRate);
        sbyte[] compResult = AudioCompressor.NonLinear_Quantizer(samples, compSett.QuantizationLevels);
        FileSaver.SaveNonLinearQuant(outPath, compResult, compSett.QuantizationLevels, compSett.SampleRate, audioInfo);
    }

    static void handleDPCM(AudioFileReader reader, CompressionSettings compSett, string outPath)
    {
        float[] samples = SamplesHelper.GetResampledSamples(reader, compSett.SampleRate);
        float quantFactor = compSett.QuantizationFactor;
        var compResult = AudioCompressor.DPCM(samples, quantFactor);
        FileSaver.SaveDPCM(outPath, compResult.firstSample, compResult.compressedSamples, quantFactor, compSett.SampleRate, audioInfo);
    }

    private static void AddChartPoint(int percent)
    {
        int samples = AudioCompressor.getSamplesProcessed();
        recordedSeconds += 1;
        double saving = GetSpaceSavedPercentage();
        CompressionChartController.AddPoint(recordedSeconds, samples, saving);
    }

    private static double GetSpaceSavedPercentage()
    {
        switch (currentType)
        {
            case CompressionTypes.DeltaModulation:
                return ((audioInfo.bitsPerSample - 1.0) / audioInfo.bitsPerSample) * 100.0;
            case CompressionTypes.NonlinearQuant:
            case CompressionTypes.DPCM:
                return ((audioInfo.bitsPerSample - 8.0) / audioInfo.bitsPerSample) * 100.0;
            default:
                return 0.0;
        }
    }
}