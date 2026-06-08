using WinFormsApp1;

public class CompressionSettings
{
    private float stepSize;

    public static float CurrentDeltaStepSize { get; private set; } = 0.01f;
    public CompressionTypes Type { get; set; }
    public int SampleRate { get; set; }
    public int QuantizationLevels { get; set; }
    public string SavePath { get; set; }
    public float QuantizationFactor { get; set; }

    public float StepSize
    {
        get => stepSize;
        set
        {
            stepSize = value <= 0 ? 0.01f : value;
            CurrentDeltaStepSize = stepSize;
        }
    }
}