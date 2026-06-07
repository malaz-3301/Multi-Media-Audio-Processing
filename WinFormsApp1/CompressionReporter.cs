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

        // --- NEW REPORT GENERATION FUNCTION ---
        public static string GenerateReport()
        {
            StringBuilder sb = new StringBuilder();
            TimeSpan duration = compEndTime - compStartTime;

            sb.AppendLine("========================================");
            sb.AppendLine("          COMPRESSION REPORT            ");
            sb.AppendLine("========================================");

            // 1. Time / Duration Information
            sb.AppendLine($"Start Time:  {compStartTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"End Time:    {compEndTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Duration:    {duration.TotalSeconds:F2} seconds");
            sb.AppendLine("----------------------------------------");

            // 2. Settings Configuration (Handles null fallback safely)
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
                sb.AppendLine("  - [No settings recorded]");
            }
            sb.AppendLine("----------------------------------------");

            // 3. File Metrics & Size Reduction Analysis
            sb.AppendLine("File Metrics:");

            long origSize = (origFile != null && origFile.Exists) ? origFile.Length : 0;
            long compSize = (compedFile != null && compedFile.Exists) ? compedFile.Length : 0;

            sb.AppendLine($"  - Original File:        {origFile?.Name ?? "Unknown"} ({FormatBytes(origSize)})");
            sb.AppendLine($"  - Compressed File:      {compedFile?.Name ?? "Unknown"} ({FormatBytes(compSize)})");

            // Reduction ratio math
            if (origSize > 0)
            {
                double reductionPercent = ((double)(origSize - compSize) / origSize) * 100.0;

                // Represent compression or expansion appropriately
                if (reductionPercent >= 0)
                {
                    sb.AppendLine($"  - Size Reduction:       {reductionPercent:F2}% smaller");
                }
                else
                {
                    sb.AppendLine($"  - Size Expansion:       {Math.Abs(reductionPercent):F2}% larger");
                }
            }
            else
            {
                sb.AppendLine("  - Size Reduction:       0.00% (Original file size missing or empty)");
            }

            sb.AppendLine("========================================");

            return sb.ToString();
        }

        // Helper method to make sizes human-readable (KB / MB instead of just raw bytes)
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