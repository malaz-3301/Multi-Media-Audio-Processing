using ScottPlot;
using ScottPlot.WinForms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class CompressionChart
    {
        public FormsPlot formsPlot;

        public List<double> seconds = new List<double>();
        public List<double> samples = new List<double>();
        public List<double> compressionRatios = new List<double>();

        public CompressionChart()
        {
            formsPlot = new FormsPlot
            {
                Dock = DockStyle.None,
                Width = 600,
                Height = 250,
            };

            ConfigurePlotStyles();
            CompressionChartController.Initialize(this);
        }

        private void ConfigurePlotStyles()
        {
            formsPlot.Plot.Title("Compression Performance Analytics");
            formsPlot.Plot.XLabel("Seconds");

            formsPlot.Plot.Axes.Left.Label.Text = "Samples Processed / Sec";
            formsPlot.Plot.Axes.Left.Label.ForeColor = ScottPlot.Color.FromHex("#2196F3");

            formsPlot.Plot.Axes.Right.Label.Text = "Compression Rate(Space Saved %)";
            formsPlot.Plot.Axes.Right.Label.ForeColor = ScottPlot.Color.FromHex("#4CAF50");
        }

        public void AddPoint(int second, int samplesProcessed, double currentRatio)
        {
            seconds.Add(second);
            samples.Add(samplesProcessed);
            compressionRatios.Add(currentRatio);

            RefreshChart();
        }

        public void RefreshChart()
        {
            if (formsPlot.InvokeRequired)
            {
                formsPlot.BeginInvoke(new Action(RefreshChart));
                return;
            }

            formsPlot.Plot.Clear();
            ConfigurePlotStyles();

            if (seconds.Count == 0) return;

            var speedScatter = formsPlot.Plot.Add.Scatter(seconds.ToArray(), samples.ToArray());
            speedScatter.Color = ScottPlot.Color.FromHex("#2196F3");
            speedScatter.LineWidth = 2;
            speedScatter.Axes.YAxis = formsPlot.Plot.Axes.Left;

            var ratioScatter = formsPlot.Plot.Add.Scatter(seconds.ToArray(), compressionRatios.ToArray());
            ratioScatter.Color = ScottPlot.Color.FromHex("#4CAF50");
            ratioScatter.LineWidth = 2;
            ratioScatter.Axes.YAxis = formsPlot.Plot.Axes.Right;

            formsPlot.Plot.Axes.AutoScale();
            formsPlot.Refresh();
        }

        public void Clear()
        {
            seconds.Clear();
            samples.Clear();
            compressionRatios.Clear();
            RefreshChart();
        }
    }

    public class CompressionChartController
    {
        private static CompressionChart chart;

        public static void Initialize(CompressionChart c)
        {
            chart = c;
        }

        public static void AddPoint(int second, int samplesProcessed, double currentRatio)
        {
            chart?.AddPoint(second, samplesProcessed, currentRatio);
        }

        public static void Reset()
        {
            chart?.Clear();
        }
    }
}