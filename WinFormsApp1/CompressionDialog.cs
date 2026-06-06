using System;
using System.Windows.Forms;

namespace WinFormsApp1
{
    internal class CompressionDialog : Form
    {
        private ComboBox typeBox;
        private Panel settingsPanel;
        private Button okBtn;

        // Global settings
        private NumericUpDown sampleRateInput;

        // Save path
        private TextBox savePathBox;
        private Button browseBtn;

        // Algorithm settings
        private ComboBox quantLevelsBox;
        private NumericUpDown quantFactorInput;
        private NumericUpDown stepSizeInput;

        public CompressionSettings Result { get; private set; }

        public CompressionDialog()
        {
            Text = "Compression Settings";
            Width = 420;
            Height = 340;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            // ---------------- TYPE ----------------

            Label typeLabel = new Label();
            typeLabel.Text = "Compression Type:";
            typeLabel.SetBounds(20, 20, 120, 20);

            typeBox = new ComboBox();
            typeBox.DataSource = Enum.GetValues(typeof(CompressionTypes));
            typeBox.DropDownStyle = ComboBoxStyle.DropDownList;
            typeBox.SetBounds(150, 20, 220, 25);
            typeBox.SelectedIndexChanged += TypeBox_SelectedIndexChanged;

            // ---------------- SAMPLE RATE ----------------

            Label sampleRateLabel = new Label();
            sampleRateLabel.Text = "Sample Rate:";
            sampleRateLabel.SetBounds(20, 60, 120, 20);

            sampleRateInput = new NumericUpDown();
            sampleRateInput.SetBounds(150, 60, 120, 20);
            sampleRateInput.Minimum = 8000;
            sampleRateInput.Maximum = 96000;
            sampleRateInput.Value = 44100;
            sampleRateInput.Increment = 1000;

            // ---------------- SAVE PATH ----------------

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

            // ---------------- DYNAMIC SETTINGS ----------------

            settingsPanel = new Panel();
            settingsPanel.SetBounds(20, 140, 360, 100);

            // ---------------- OK BUTTON ----------------

            okBtn = new Button();
            okBtn.Text = "OK";
            okBtn.SetBounds(20, 260, 80, 30);
            okBtn.Click += OkBtn_Click;

            // ---------------- ADD CONTROLS ----------------

            Controls.Add(typeLabel);
            Controls.Add(typeBox);

            Controls.Add(sampleRateLabel);
            Controls.Add(sampleRateInput);

            Controls.Add(savePathLabel);
            Controls.Add(savePathBox);
            Controls.Add(browseBtn);

            Controls.Add(settingsPanel);
            Controls.Add(okBtn);

            Load += CompressionDialog_Load;
        }

        private void CompressionDialog_Load(object? sender, EventArgs e)
        {
            if (typeBox.Items.Count > 0)
            {
                typeBox.SelectedIndex = 0;
            }
        }

        private void BrowseBtn_Click(object? sender, EventArgs e)
        {
            using FolderBrowserDialog dialog =
                new FolderBrowserDialog();

            dialog.Description = "Select output folder";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                savePathBox.Text = dialog.SelectedPath;
            }
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

        // ---------------- NONLINEAR QUANTIZATION ----------------

        private void BuildNonLinear()
        {
            Label lbl = new Label();
            lbl.Text = "Quantization Levels:";
            lbl.SetBounds(0, 10, 150, 20);

            quantLevelsBox = new ComboBox();
            quantLevelsBox.SetBounds(170, 10, 120, 25);
            quantLevelsBox.DropDownStyle = ComboBoxStyle.DropDownList;

            quantLevelsBox.Items.AddRange(new object[]
            {
                2,
                4,
                8,
                16,
                32,
                64,
                128,
                256
            });

            quantLevelsBox.SelectedItem = 256;

            settingsPanel.Controls.Add(lbl);
            settingsPanel.Controls.Add(quantLevelsBox);
        }

        // ---------------- DPCM ----------------

        private void BuildDPCM()
        {
            Label lbl = new Label();
            lbl.Text = "Quantization Factor:";
            lbl.SetBounds(0, 10, 150, 20);

            quantFactorInput = new NumericUpDown();
            quantFactorInput.SetBounds(170, 10, 120, 20);
            quantFactorInput.Minimum = 1;
            quantFactorInput.Maximum = 256;
            quantFactorInput.Value = 8;

            settingsPanel.Controls.Add(lbl);
            settingsPanel.Controls.Add(quantFactorInput);
        }

        // ---------------- DELTA MODULATION ----------------

        private void BuildDelta()
        {
            Label lbl = new Label();
            lbl.Text = "Step Size:";
            lbl.SetBounds(0, 10, 150, 20);

            stepSizeInput = new NumericUpDown();
            stepSizeInput.SetBounds(170, 10, 120, 20);
            stepSizeInput.Minimum = 1;
            stepSizeInput.Maximum = 256;
            stepSizeInput.DecimalPlaces = 2;
            stepSizeInput.Increment = 0.1M;
            stepSizeInput.Value = 1;

            settingsPanel.Controls.Add(lbl);
            settingsPanel.Controls.Add(stepSizeInput);
        }

        // ---------------- OK ----------------

        private void OkBtn_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(savePathBox.Text))
            {
                MessageBox.Show(
                    "Please select a save location.",
                    "Missing Save Path",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            Result = new CompressionSettings
            {
                Type = (CompressionTypes)typeBox.SelectedItem,

                SampleRate = (int)sampleRateInput.Value,

                SavePath = savePathBox.Text,

                QuantizationLevels =quantLevelsBox != null? Convert.ToInt32(quantLevelsBox.SelectedItem) : 0,

                QuantizationFactor = quantFactorInput != null? (float)quantFactorInput.Value : 0f,

                StepSize = stepSizeInput != null ? (float)stepSizeInput.Value : 0f
            };

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}