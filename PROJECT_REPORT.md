# Audio Project Report

This project is a C# WinForms desktop application for audio file processing.

Implemented features:

- Load audio from the interface.
- Load audio using drag and drop.
- Show loaded file details automatically in the workspace.
- Play the audio before processing.
- Show file size, duration, sample rate, channel count, bit rate, bit depth, and encoding type.
- Process audio using Nonlinear Quantization, DPCM, and Delta Modulation.
- Let the user choose the sample rate and algorithm settings before running.
- Show a progress bar during execution.
- Show runtime charts for processing speed and saving percentage.
- Allow cancelling the running operation.
- Reset the loaded file values.
- Save processed files to disk.
- Reconstruct audio from the saved project files.
- Show a final report with before and after size, saving percentage, elapsed time, algorithm name, and selected settings.

Implementation notes:

- NAudio is used for audio reading, writing, and playback.
- ScottPlot is used for runtime charts.
- Custom binary files store the needed metadata for reconstruction.
- Stereo input is converted to mono before processing so the algorithms work on one clean signal.
