using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    internal class CompressionDialog : Form
    {
        private ComboBox typeBox;
        private Panel settingsPanel;
        private Button okBtn;
        private Button resetBtn;
        private NumericUpDown sampleRateInput;
        private TextBox savePathBox;
        private Button browseBtn;
        private ComboBox? quantLevelsBox;
        private NumericUpDown? quantFactorInput;
        private NumericUpDown? stepSizeInput;
        private Panel progressPanel;
        private ProgressBar progressBar;
        private Label statusLabel;
        private CancellationTokenSource? cts;
        private Button cancelBtn;
        private readonly int sampleRate;
        private CompressionChart compChart;

        public Func<CompressionSettings, IProgress<int>, CancellationToken, Task> CompressionTask { get; set; } = null!;
        public CompressionSettings Result { get; private set; } = new CompressionSettings();

        public CompressionDialog(int sampleRate)
        {
            Text = "Compression Settings";
            Width = 650;
            Height = 520;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            this.sampleRate = Math.Clamp(sampleRate, 8000, 96000);

            Label typeLabel = new Label();
            typeLabel.Text = "Compression Type:";
            typeLabel.SetBounds(20, 20, 120, 20);

            typeBox = new ComboBox();
            typeBox.DataSource = Enum.GetValues(typeof(CompressionTypes));
            typeBox.DropDownStyle = ComboBoxStyle.DropDownList;
            typeBox.SetBounds(150, 20, 220, 25);
            typeBox.SelectedIndexChanged += TypeBox_SelectedIndexChanged;

            Label sampleRateLabel = new Label();
            sampleRateLabel.Text = "Sample Rate:";
            sampleRateLabel.SetBounds(20, 60, 120, 20);

            sampleRateInput = new NumericUpDown();
            sampleRateInput.SetBounds(150, 60, 120, 20);
            sampleRateInput.Minimum = 8000;
            sampleRateInput.Maximum = 96000;
            sampleRateInput.Value = this.sampleRate;
            sampleRateInput.Increment = 1000;

            Label savePathLabel = new Label();
            savePathLabel.Text = "Save Path:";
            savePathLabel.SetBounds(20, 100, 120, 20);

            savePathBox = new TextBox();
            savePathBox.SetBounds(150, 100, 180, 20);
            savePathBox.ReadOnly = true;

            browseBtn = new Button();
            browseBtn.Text = "...";
            browseBtn.SetBounds(340, 98, 40, 24);
            browseBtn.Click += BrowseBtn_Click;

            settingsPanel = new Panel();
            settingsPanel.SetBounds(20, 140, 360, 100);

            okBtn = new Button();
            okBtn.Text = "OK";
            okBtn.SetBounds(20, 260, 80, 30);
            okBtn.Click += OkBtn_Click;

            resetBtn = new Button();
            resetBtn.Text = "Reset";
            resetBtn.SetBounds(110, 260, 80, 30);
            resetBtn.Click += ResetBtn_Click;

            BuildProgressPanel();

            Controls.Add(typeLabel);
            Controls.Add(typeBox);
            Controls.Add(sampleRateLabel);
            Controls.Add(sampleRateInput);
            Controls.Add(savePathLabel);
            Controls.Add(savePathBox);
            Controls.Add(browseBtn);
            Controls.Add(settingsPanel);
            Controls.Add(okBtn);
            Controls.Add(resetBtn);
            Controls.Add(progressPanel);

            Load += CompressionDialog_Load;
        }

        private void CompressionDialog_Load(object? sender, EventArgs e)
        {
            if (typeBox.Items.Count > 0)
                typeBox.SelectedIndex = 0;
        }

        private void BrowseBtn_Click(object? sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select output folder";

            if (dialog.ShowDialog() == DialogResult.OK)
                savePathBox.Text = dialog.SelectedPath;
        }

        private void TypeBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (typeBox.SelectedItem == null)
                return;

            BuildUI((CompressionTypes)typeBox.SelectedItem);
        }

        private void BuildUI(CompressionTypes type)
        {
            settingsPanel.Controls.Clear();
            quantLevelsBox = null;
            quantFactorInput = null;
            stepSizeInput = null;

            switch (type)
            {
                case CompressionTypes.NonlinearQuant:
                    BuildNonLinear();
                    break;
                case CompressionTypes.DPCM:
                    BuildDPCM();
                    break;
                case CompressionTypes.DeltaModulation:
                    BuildDelta();
                    break;
            }
        }

        private void BuildNonLinear()
        {
            Label lbl = new Label();
            lbl.Text = "Quantization Levels:";
            lbl.SetBounds(0, 10, 150, 20);

            quantLevelsBox = new ComboBox();
            quantLevelsBox.SetBounds(170, 10, 120, 25);
            quantLevelsBox.DropDownStyle = ComboBoxStyle.DropDownList;
            quantLevelsBox.Items.AddRange(new object[] { 2, 4, 8, 16, 32, 64, 128, 256 });
            quantLevelsBox.SelectedItem = 256;

            settingsPanel.Controls.Add(lbl);
            settingsPanel.Controls.Add(quantLevelsBox);
        }

        private void BuildDPCM()
        {
            Label lbl = new Label();
            lbl.Text = "Quantization Factor:";
            lbl.SetBounds(0, 10, 150, 20);

            quantFactorInput = new NumericUpDown();
            quantFactorInput.SetBounds(170, 10, 120, 20);
            quantFactorInput.Minimum = 1;
            quantFactorInput.Maximum = 512;
            quantFactorInput.Value = 128;

            settingsPanel.Controls.Add(lbl);
            settingsPanel.Controls.Add(quantFactorInput);
        }

        private void BuildDelta()
        {
            Label lbl = new Label();
            lbl.Text = "Step Size:";
            lbl.SetBounds(0, 10, 150, 20);

            stepSizeInput = new NumericUpDown();
            stepSizeInput.SetBounds(170, 10, 120, 20);
            stepSizeInput.Minimum = 0.001M;
            stepSizeInput.Maximum = 1;
            stepSizeInput.DecimalPlaces = 3;
            stepSizeInput.Increment = 0.001M;
            stepSizeInput.Value = 0.01M;

            settingsPanel.Controls.Add(lbl);
            settingsPanel.Controls.Add(stepSizeInput);
        }

        private void BuildProgressPanel()
        {
            progressPanel = new Panel();
            progressPanel.SetBounds(0, 0, Width, Height);
            progressPanel.Visible = false;

            statusLabel = new Label();
            statusLabel.Text = "Compressing file, please wait...";
            statusLabel.SetBounds(25, 20, 584, 25);
            statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            statusLabel.Font = new System.Drawing.Font(statusLabel.Font.FontFamily, 10f, System.Drawing.FontStyle.Bold);

            progressBar = new ProgressBar();
            progressBar.SetBounds(25, 50, 584, 30);
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.SetBounds(275, 95, 100, 32);
            cancelBtn.Click += CancelBtn_Click;

            compChart = new CompressionChart();
            compChart.formsPlot.SetBounds(20, 145, 594, 310);

            progressPanel.Controls.Add(statusLabel);
            progressPanel.Controls.Add(progressBar);
            progressPanel.Controls.Add(cancelBtn);
            progressPanel.Controls.Add(compChart.formsPlot);
        }

        private async void OkBtn_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(savePathBox.Text))
            {
                MessageBox.Show("Please select a save location.", "Missing Save Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Result = new CompressionSettings
            {
                Type = (CompressionTypes)typeBox.SelectedItem,
                SampleRate = (int)sampleRateInput.Value,
                SavePath = savePathBox.Text,
                QuantizationLevels = quantLevelsBox != null ? Convert.ToInt32(quantLevelsBox.SelectedItem) : 0,
                QuantizationFactor = quantFactorInput != null ? (float)quantFactorInput.Value : 0f,
                StepSize = stepSizeInput != null ? (float)stepSizeInput.Value : 0f
            };

            ShowProgressPanel();
            await RunCompression();
        }

        private void ShowProgressPanel()
        {
            foreach (Control ctrl in Controls)
            {
                if (ctrl != progressPanel)
                    ctrl.Visible = false;
            }

            progressPanel.Visible = true;
            Text = "Running Compression...";
        }

        private async Task RunCompression()
        {
            var progressHandler = new Progress<int>(percent =>
            {
                progressBar.Value = Math.Min(100, Math.Max(0, percent));
                statusLabel.Text = $"Compressing... {percent}% completed.";
            });

            cts = new CancellationTokenSource();

            try
            {
                if (CompressionTask != null)
                {
                    await Task.Run(async () => await CompressionTask(Result, progressHandler, cts.Token));
                    ShowCompressionSuccess();
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("No compression backend linked.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ResetUI();
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Compression operation was cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ResetUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Compression Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetUI();
            }
            finally
            {
                cts?.Dispose();
                cts = null;
            }
        }

        private void ShowCompressionSuccess()
        {
            DialogResult choice = MessageBox.Show("Compression completed successfully!\n\nWould you like to view the detailed compression report?", "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (choice == DialogResult.Yes)
            {
                string reportText = CompressionReporter.GenerateReport();
                MessageBox.Show(reportText, "Compression Summary Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ResetBtn_Click(object? sender, EventArgs e)
        {
            sampleRateInput.Value = sampleRate;

            if (typeBox.Items.Count > 0)
                typeBox.SelectedIndex = 0;

            BuildUI((CompressionTypes)typeBox.SelectedItem);
        }

        private void ResetUI()
        {
            progressPanel.Visible = false;
            progressBar.Value = 0;
            Text = "Compression Settings";

            foreach (Control ctrl in Controls)
            {
                if (ctrl != progressPanel)
                    ctrl.Visible = true;
            }
        }

        private void CancelBtn_Click(object? sender, EventArgs e)
        {
            cts?.Cancel();
        }
    }
}