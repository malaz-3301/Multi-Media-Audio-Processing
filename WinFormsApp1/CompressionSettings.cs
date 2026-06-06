using WinFormsApp1;

public  class CompressionSettings
{
    public CompressionTypes Type { get; set; }

   
    public int SampleRate { get; set; }
    public int QuantizationLevels { get; set; }
    public string SavePath { get; set; }

    public float QuantizationFactor { get; set; }

    
    public float StepSize { get; set; }
}