using System;
using System.Drawing;
using System.IO;
using NAudio.Wave;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private WaveOutEvent? outputDev;
        private AudioFileReader? reader;
        private string? file;
        private Panel dropPanel;
        private Label fileStatusLabel;
        private TextBox workspaceInfoBox;
        private AudioFileInfo audioInfo = new AudioFileInfo();

        public Form1()
        {
            InitializeComponent();
            Text = "Audio Compression";
            ClientSize = new Size(800, 480);

            Button playBtn = CreateButton("Play", 25, 25, PlayAudio);
            Button loadBtn = CreateButton("Load", 25, 70, LoadAudio);
            Button stopBtn = CreateButton("Stop", 25, 115, StopAudio);
            Button removeBtn = CreateButton("Remove", 25, 160, RemoveAudio);
            Button infoBtn = CreateButton("Info", 25, 205, LoadAudioInfo);
            Button compressBtn = CreateButton("Compress", 25, 250, ShowCompressionDialog);
            Button decompressBtn = CreateButton("Decompress", 25, 295, DecompressFile, 90);
            Button resetBtn = CreateButton("Reset", 25, 340, ResetOriginalValues);

            Controls.Add(playBtn);
            Controls.Add(loadBtn);
            Controls.Add(stopBtn);
            Controls.Add(removeBtn);
            Controls.Add(infoBtn);
            Controls.Add(compressBtn);
            Controls.Add(decompressBtn);
            Controls.Add(resetBtn);

            dropPanel = new Panel();
            dropPanel.SetBounds(130, 25, 350, 120);
            dropPanel.BorderStyle = BorderStyle.FixedSingle;
            dropPanel.AllowDrop = true;

            Label dropText = new Label();
            dropText.Text = "Drag & Drop Audio File Here";
            dropText.Dock = DockStyle.Fill;
            dropText.TextAlign = ContentAlignment.MiddleCenter;

            dropPanel.Controls.Add(dropText);
            dropPanel.DragEnter += DropPanel_DragEnter;
            dropPanel.DragDrop += DropPanel_DragDrop;
            Controls.Add(dropPanel);

            fileStatusLabel = new Label();
            fileStatusLabel.Text = "No file loaded!";
            fileStatusLabel.AutoSize = true;
            fileStatusLabel.SetBounds(130, 160, 640, 25);
            Controls.Add(fileStatusLabel);

            workspaceInfoBox = new TextBox();
            workspaceInfoBox.SetBounds(130, 190, 640, 250);
            workspaceInfoBox.Multiline = true;
            workspaceInfoBox.ReadOnly = true;
            workspaceInfoBox.ScrollBars = ScrollBars.Vertical;
            workspaceInfoBox.Text = "No file loaded.";
            Controls.Add(workspaceInfoBox);
        }

        private Button CreateButton(string text, int x, int y, Action action, int width = 75)
        {
            Button button = new Button();
            button.Text = text;
            button.SetBounds(x, y, width, 30);
            button.Click += (s, e) => action();
            return button;
        }

        private void PlayAudio()
        {
            if (!ValidatePlayableFile())
                return;

            outputDev?.Stop();
            outputDev?.Dispose();

            reader!.Position = 0;
            outputDev = new WaveOutEvent();
            outputDev.Init(reader);
            outputDev.Play();
        }

        private void StopAudio()
        {
            outputDev?.Stop();
        }

        private void LoadAudio()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Audio Files|*.wav;*.mp3;*.aac;*.wma;*.flac|Compressed Files|*.nlq;*.dpcm;*.dlt|All Files|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                file = dialog.FileName;
                LoadReader(file);
                fileStatusLabel.Text = $"Loaded: {Path.GetFileName(file)}";
                UpdateWorkspaceInfo();
            }
        }

        private void LoadReader(string? selectedFile)
        {
            outputDev?.Stop();
            outputDev?.Dispose();
            outputDev = null;
            reader?.Dispose();
            reader = null;
            audioInfo = new AudioFileInfo();

            if (string.IsNullOrWhiteSpace(selectedFile) || IsCompressedFile(selectedFile))
                return;

            reader = new AudioFileReader(selectedFile);
            FileInfo fileInfo = new FileInfo(selectedFile);
            int sampleRate = reader.WaveFormat.SampleRate;
            int channels = reader.WaveFormat.Channels;
            int bitsPerSample = reader.WaveFormat.BitsPerSample > 0 ? reader.WaveFormat.BitsPerSample : 16;
            int bitRate = CalculateBitRate(fileInfo, reader);
            bool isMP3 = Path.GetExtension(selectedFile).Equals(".mp3", StringComparison.OrdinalIgnoreCase);

            audioInfo = new AudioFileInfo(sampleRate, channels, bitsPerSample, bitRate, isMP3);
        }

        private int CalculateBitRate(FileInfo fileInfo, AudioFileReader audioReader)
        {
            if (audioReader.TotalTime.TotalSeconds > 0)
                return (int)Math.Round((fileInfo.Length * 8.0) / audioReader.TotalTime.TotalSeconds);

            return audioReader.WaveFormat.AverageBytesPerSecond * 8;
        }

        private void LoadAudioInfo()
        {
            if (!ValidateFileLoaded())
                return;

            UpdateWorkspaceInfo();
            ShowDialogMessage(workspaceInfoBox.Text, "Audio File Properties", MessageBoxIcon.Information);
        }

        private void DropPanel_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
        }

        private void DropPanel_DragDrop(object? sender, DragEventArgs e)
        {
            string[]? files = (string[]?)e.Data?.GetData(DataFormats.FileDrop);

            if (files == null || files.Length == 0)
                return;

            file = files[0];
            LoadReader(file);
            fileStatusLabel.Text = $"Loaded: {Path.GetFileName(file)}";
            UpdateWorkspaceInfo();
            ShowDialogMessage("File loaded successfully.", "Success", MessageBoxIcon.Information);
        }

        private bool ValidateFileLoaded()
        {
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
            {
                ShowDialogMessage("Please load a file first.", "No File Selected", MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool ValidatePlayableFile()
        {
            if (!ValidateFileLoaded())
                return false;

            if (reader == null || IsCompressedFile(file))
            {
                ShowDialogMessage("This file is compressed. Please decompress it before playing or compressing it again.", "Compressed File", MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void ShowDialogMessage(string message, string title, MessageBoxIcon icon)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void RemoveAudio()
        {
            if (!ValidateFileLoaded())
                return;

            outputDev?.Stop();
            reader?.Dispose();
            outputDev?.Dispose();
            reader = null;
            outputDev = null;
            file = null;
            audioInfo = new AudioFileInfo();
            fileStatusLabel.Text = "No file loaded!";
            workspaceInfoBox.Text = "No file loaded.";
        }

        private void ResetOriginalValues()
        {
            if (!ValidateFileLoaded())
                return;

            LoadReader(file);
            fileStatusLabel.Text = $"Loaded: {Path.GetFileName(file)}";
            UpdateWorkspaceInfo();
            ShowDialogMessage("Original file values were restored.", "Reset", MessageBoxIcon.Information);
        }

        private void ShowCompressionDialog()
        {
            if (!ValidatePlayableFile())
                return;

            using var dlg = new CompressionDialog(audioInfo.sampleRate);

            dlg.CompressionTask = async (settings, progress, ct) =>
            {
                using var compressionReader = new AudioFileReader(file!);
                CompressionManager.CompressAudioFile(compressionReader, settings, progress, ct, file!, audioInfo);
            };

            dlg.ShowDialog();
        }

        private bool ValidateFileIsOfCompressType(bool showDialog = true)
        {
            if (IsCompressedFile(file))
                return true;

            if (showDialog)
                ShowDialogMessage("Please load a file with one of these extensions: .dpcm, .nlq, .dlt", "File Extension Error", MessageBoxIcon.Warning);

            return false;
        }

        private bool IsCompressedFile(string? selectedFile)
        {
            string extension = Path.GetExtension(selectedFile ?? string.Empty);
            return extension.Equals(".nlq", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".dpcm", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".dlt", StringComparison.OrdinalIgnoreCase);
        }

        private void DecompressFile()
        {
            if (!ValidateFileLoaded())
                return;

            if (!ValidateFileIsOfCompressType())
                return;

            using SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Choose Output File";
            saveDialog.Filter = "All Files (*.*)|*.*";
            saveDialog.AddExtension = false;
            saveDialog.DefaultExt = "";
            saveDialog.FileName = "reconstructed";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                DecompressionManager.HandleFileDecompression(file!, saveDialog.FileName);
                ShowDialogMessage("File decompressed successfully.", "Success", MessageBoxIcon.Information);
            }
        }

        private void UpdateWorkspaceInfo()
        {
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
            {
                workspaceInfoBox.Text = "No file loaded.";
                return;
            }

            FileInfo fileInfo = new FileInfo(file);

            if (IsCompressedFile(file))
            {
                workspaceInfoBox.Text =
                    $"Compressed File Loaded{Environment.NewLine}" +
                    $"File Name: {fileInfo.Name}{Environment.NewLine}" +
                    $"File Path: {fileInfo.FullName}{Environment.NewLine}" +
                    $"File Size: {FormatSize(fileInfo.Length)}{Environment.NewLine}" +
                    $"File Type: {Path.GetExtension(fileInfo.Name)}{Environment.NewLine}" +
                    "Use Decompress to reconstruct the audio file.";
                return;
            }

            if (reader == null)
            {
                workspaceInfoBox.Text = "The selected file could not be read as an audio file.";
                return;
            }

            workspaceInfoBox.Text =
                $"File Name: {fileInfo.Name}{Environment.NewLine}" +
                $"File Path: {fileInfo.FullName}{Environment.NewLine}" +
                $"File Size: {FormatSize(fileInfo.Length)}{Environment.NewLine}" +
                $"Duration: {reader.TotalTime:mm\\:ss}{Environment.NewLine}" +
                $"Sample Rate: {audioInfo.sampleRate} Hz{Environment.NewLine}" +
                $"Channels: {audioInfo.channels}{Environment.NewLine}" +
                $"Bit Rate: {audioInfo.bitRate / 1000.0:F0} kbps{Environment.NewLine}" +
                $"Bit Per Sample: {audioInfo.bitsPerSample} bits{Environment.NewLine}" +
                $"Encoding Type: {reader.WaveFormat.Encoding}";
        }

        private string FormatSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} Bytes";

            if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:F2} KB";

            return $"{bytes / 1024.0 / 1024.0:F2} MB";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            outputDev?.Stop();
            outputDev?.Dispose();
            reader?.Dispose();
            base.OnFormClosing(e);
        }
    }
}