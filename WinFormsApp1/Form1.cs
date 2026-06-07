using System.Threading.Channels;
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

        private AudioFileInfo audioInfo;

        public Form1()
        {
            InitializeComponent();

            // Play Button
            Button playBtn = new Button();
            playBtn.Text = "Play";
            playBtn.SetBounds(25, 25, 75, 30);
            playBtn.Click += (s, e) => PlayAudio();

            // Load Button
            Button loadBtn = new Button();
            loadBtn.Text = "Load";
            loadBtn.SetBounds(25, 70, 75, 30);
            loadBtn.Click += (s, e) => LoadAudio();

            // Stop Button
            Button stopBtn = new Button();
            stopBtn.Text = "Stop";
            stopBtn.SetBounds(25, 115, 75, 30);
            stopBtn.Click += (s, e) => StopAudio();

            Button removeBtn = new Button();
            removeBtn.Text = "Remove";
            removeBtn.SetBounds(25, 160, 75, 30);
            removeBtn.Click += (s, e) => RemoveAudio();

            Button infoBtn = new Button();
            infoBtn.Text = "Info";
            infoBtn.SetBounds(25, 205, 75, 30);
            infoBtn.Click += (s, e) => LoadAudioInfo();

            Button compressBtn = new Button();
            compressBtn.Text = "Compress";
            compressBtn.SetBounds(25, 250, 75, 30);
            compressBtn.Click += (s, e) => showCompressionDialog();

            Button decompressBtn = new Button();
            decompressBtn.Text = "Decompress";
            decompressBtn.SetBounds(25, 295, 80, 30);
            decompressBtn.Click += (s, e) => decompressFile();

            Controls.Add(removeBtn);
            Controls.Add(infoBtn);

            Controls.Add(playBtn);
            Controls.Add(loadBtn);
            Controls.Add(stopBtn);

            Controls.Add(compressBtn);
            Controls.Add(decompressBtn);

            // Drag & Drop Panel
            dropPanel = new Panel();
            dropPanel.SetBounds(130, 25, 350, 120);
            dropPanel.BorderStyle = BorderStyle.FixedSingle;
            dropPanel.AllowDrop = true;

            System.Windows.Forms.Label dropText = new System.Windows.Forms.Label();
            dropText.Text = "Drag & Drop Audio File Here";
            dropText.Dock = DockStyle.Fill;
            dropText.TextAlign = ContentAlignment.MiddleCenter;

            dropPanel.Controls.Add(dropText);

            dropPanel.DragEnter += DropPanel_DragEnter;
            dropPanel.DragDrop += DropPanel_DragDrop;

            Controls.Add(dropPanel);

            // Status Label
            fileStatusLabel = new Label();
            fileStatusLabel.Text = "No file loaded!";
            fileStatusLabel.AutoSize = true;
            fileStatusLabel.SetBounds(130, 160, 350, 30);

            Controls.Add(fileStatusLabel);
        }

        private void PlayAudio()
        {
            if (!ValidateFileLoaded())
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
            if (!ValidateFileLoaded())
                return;

            outputDev?.Stop();
        }

        private void LoadAudio()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter =
                "Audio Files|*.wav;*.mp3;*.aac;*.wma;*.flac|All Files|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                file = dialog.FileName;
                LoadReader(file);  

                fileStatusLabel.Text =
                    $"Audio File Loaded: {Path.GetFileName(file)}";
            }
        }

        private void LoadReader(string? file)
        {
            if (file == null)
                return;

            if (ValidateFileIsOfCompressType(false))
            {
                return;
            }

            outputDev?.Stop();
            outputDev?.Dispose();
            outputDev = null;

            reader?.Dispose();
            reader = new AudioFileReader(file);
            FileInfo fileInfo = new FileInfo(file!);
            int sampleRate = reader.WaveFormat.SampleRate;
            int channels = reader.WaveFormat.Channels;
            int bitsPerSample = reader.WaveFormat.BitsPerSample;
            int bitRate = (reader.WaveFormat.AverageBytesPerSecond * 8) / 1000;
            bool isMP3 = file!.ToLower().EndsWith(".mp3");
            audioInfo = new AudioFileInfo(sampleRate,channels,bitsPerSample,bitRate,isMP3);
            
        }

        private void LoadAudioInfo()
        {
            if (!ValidateFileLoaded())
                return;

            using (var audioReader = new AudioFileReader(file!))
            {
                FileInfo fileInfo = new FileInfo(file!);
                string message =
                    $"File Name: {fileInfo.Name}\n" +
                    $"File Size: {fileInfo.Length / 1024.0:F2} KB\n" +
                    $"Duration: {audioReader.TotalTime:mm\\:ss}\n" +
                    $"Sample Rate: {audioInfo.sampleRate} Hz\n" +
                    $"Channels: {audioInfo.channels}\n" +
                    $"Bit Rate: {audioInfo.bitRate} kbps\n" +
                    $"Encoding Type: {audioReader.WaveFormat.Encoding}";

                ShowDialogMessage(
                    message,
                    "Audio File Properties",
                    MessageBoxIcon.Information);
            }
        }

        private void DropPanel_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void DropPanel_DragDrop(object? sender, DragEventArgs e)
        {
            string[]? files =
                (string[]?)e.Data?.GetData(DataFormats.FileDrop);

            if (files == null || files.Length == 0)
                return;

            file = files[0];
            LoadReader(file);
            
           
            fileStatusLabel.Text =
                $"Loaded: {Path.GetFileName(file)}";

            ShowDialogMessage(
                "file loaded successfully.",
                "Success",
                MessageBoxIcon.Information);
        }

        private bool ValidateFileLoaded()
        {
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
            {
                ShowDialogMessage(
                    "Please load a file first.",
                    "No File Selected",
                    MessageBoxIcon.Warning);

                return false;
            }
            return true;
        }

        private void ShowDialogMessage(
            string message,
            string title,
            MessageBoxIcon icon)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                icon);
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

            fileStatusLabel.Text = "No file loaded!";

        }
        private void showCompressionDialog()
        {
            if (!ValidateFileLoaded())
                return;

            using var dlg = new CompressionDialog(audioInfo.sampleRate);

            dlg.CompressionTask = async (
            settings,
            progress,
            token) =>
            {    
              using var compressionReader =  new AudioFileReader(file!);
              CompressionManager.CompressAudioFile(
                compressionReader,
                settings,
                progress,
                token,
                file!,
                audioInfo
                );
            };

            dlg.ShowDialog();
        }
        private bool ValidateFileIsOfCompressType(bool showDialog=true) {
            string selectedFile = file ?? "";
            if (selectedFile.EndsWith("nlq")||selectedFile.EndsWith("dpcm")||selectedFile.EndsWith("dlt")) return true;
            
            if(showDialog)
            ShowDialogMessage(
                   "Please load a file of the following extensions:\"dpcm\",\"nlq\",\"dlt\"",
                   "File extension Error",
                   MessageBoxIcon.Warning);
            return false;
        }

        private void decompressFile() {
            if (!ValidateFileLoaded())
                return;
            if (!ValidateFileIsOfCompressType())
                return;
            string outputPath = @"C:\Users\Ward\Desktop\reconstructed.wav";
            DecompressionManager.HandleFileDecompression(file!,outputPath);
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