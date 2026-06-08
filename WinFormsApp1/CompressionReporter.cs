using System;
using System.IO;
using System.Text;

namespace WinFormsApp1
{
    public class CompressionReporter
    {
        private static DateTime compStartTime;
        private static DateTime compEndTime;
        private static CompressionSettings compSettings;
        private static FileInfo origFile;
        private static FileInfo compedFile;

        public static void StartCompression()
        {
            compStartTime = DateTime.Now;
        }

        public static void EndCompression()
        {
            compEndTime = DateTime.Now;
        }

        public static void recordCompressionSettings(CompressionSettings compSett)
        {
            compSettings = compSett;
        }

        public static void recordOrigFile(string filePath)
        {
            origFile = new FileInfo(filePath);
        }

        public static void recordCompedFile(string filePath)
        {
            compedFile = new FileInfo(filePath);
        }

        public static string GenerateReport()
        {
            origFile?.Refresh();
            compedFile?.Refresh();

            StringBuilder sb = new StringBuilder();
            TimeSpan duration = compEndTime - compStartTime;
            long origSize = origFile != null && origFile.Exists ? origFile.Length : 0;
            long compSize = compedFile != null && compedFile.Exists ? compedFile.Length : 0;

            sb.AppendLine("========================================");
            sb.AppendLine("          COMPRESSION REPORT            ");
            sb.AppendLine("========================================");
            sb.AppendLine($"Start Time:  {compStartTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"End Time:    {compEndTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Duration:    {duration.TotalSeconds:F2} seconds");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("Configuration Settings:");

            if (compSettings != null)
            {
                sb.AppendLine($"  - Algorithm Type:       {compSettings.Type}");
                sb.AppendLine($"  - Target Sample Rate:   {compSettings.SampleRate} Hz");
                sb.AppendLine($"  - Quantization Levels:  {compSettings.QuantizationLevels}");
                sb.AppendLine($"  - Quantization Factor:  {compSettings.QuantizationFactor}");
                sb.AppendLine($"  - Step Size:            {compSettings.StepSize}");
            }
            else
            {
                sb.AppendLine("  - No settings recorded");
            }

            sb.AppendLine("----------------------------------------");
            sb.AppendLine("File Metrics:");
            sb.AppendLine($"  - Original File:        {origFile?.Name ?? "Unknown"} ({FormatBytes(origSize)})");
            sb.AppendLine($"  - Compressed File:      {compedFile?.Name ?? "Unknown"} ({FormatBytes(compSize)})");

            if (origSize > 0 && compSize > 0)
            {
                double savingPercent = ((double)(origSize - compSize) / origSize) * 100.0;
                double ratio = (double)origSize / compSize;
                sb.AppendLine($"  - Compression Ratio:    {ratio:F2}:1");

                if (savingPercent >= 0)
                    sb.AppendLine($"  - Size Saving:          {savingPercent:F2}%");
                else
                    sb.AppendLine($"  - Size Expansion:       {Math.Abs(savingPercent):F2}%");
            }
            else
            {
                sb.AppendLine("  - Compression Ratio:    Not available");
                sb.AppendLine("  - Size Saving:          Not available");
            }

            sb.AppendLine("========================================");
            return sb.ToString();
        }

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "Bytes", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < suffixes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:F2} {suffixes[order]}";
        }
    }
}