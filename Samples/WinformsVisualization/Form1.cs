﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CSCore;
using CSCore.Codecs;
using CSCore.DSP;
using CSCore.SoundOut;
using CSCore.Streams;
using WinformsVisualization.Visualization;

namespace WinformsVisualization
{
    public partial class Form1 : Form
    {
        private ISoundOut _soundOut;
        private LineSpectrum _lineSpectrum;
        private VoicePrint3DSpectrum _voicePrint3DSpectrum;

        private readonly Bitmap _bitmap = new Bitmap(2000, 600);
        private int _xpos;

        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = CodecFactory.SupportedFilesFilterEn,
                Title = "Select a file..."
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stop();

                const FftSize fftSize = FftSize.Fft4096;

                IWaveSource source = CodecFactory.Instance.GetCodec(openFileDialog.FileName);
                var spectrumProvider = new BasicSpectrumProvider(source.WaveFormat.Channels,
                    source.WaveFormat.SampleRate, fftSize);
                _lineSpectrum = new LineSpectrum(fftSize)
                {
                    SpectrumProvider = spectrumProvider,
                    UseAverage = true,
                    BarCount = 50,
                    BarSpacing = 2,
                    IsXLogScale = true,
                    ScalingStrategy = ScalingStrategy.Sqrt
                };
                _voicePrint3DSpectrum = new VoicePrint3DSpectrum(fftSize)
                {
                    SpectrumProvider = spectrumProvider,
                    UseAverage = true,
                    PointCount = 200,
                    IsXLogScale = true,
                    ScalingStrategy = ScalingStrategy.Sqrt
                };

                var notificationSource = new SingleBlockNotificationStream(source);
                notificationSource.SingleBlockRead += (s, a) => spectrumProvider.Add(a.Left, a.Right);

                source = notificationSource.ToWaveSource(16);

                _soundOut = new WasapiOut();
                _soundOut.Initialize(new LoopStream(source));
                _soundOut.Play();

                timer1.Start();

                propertyGridTop.SelectedObject = _lineSpectrum;
                propertyGridBottom.SelectedObject = _voicePrint3DSpectrum;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Stop();
        }

        private void Stop()
        {
            timer1.Stop();

            if (_soundOut != null)
            {
                IWaveSource source = _soundOut.WaveSource;
                _soundOut.Stop();
                _soundOut.Dispose();

                source.Dispose();
                _soundOut = null;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            GenerateLineSpectrum();
            GenerateVoice3DPrintSpectrum();   
        }

        private void GenerateLineSpectrum()
        {
            Image image = pictureBoxTop.Image;
            pictureBoxTop.Image = _lineSpectrum.CreateSpectrumLine(pictureBoxTop.Size, Color.Green, Color.Red, Color.Black, true);
            if (image != null)
                image.Dispose();
        }

        private void GenerateVoice3DPrintSpectrum()
        {
            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                pictureBoxBottom.Image = null;
                _voicePrint3DSpectrum.CreateVoicePrint3D(g, new RectangleF(0, 0, _bitmap.Width, _bitmap.Height), _xpos, Color.Black, 3);
                pictureBoxBottom.Image = _bitmap;
                _xpos += 3;
                if (_xpos >= _bitmap.Width)
                    _xpos = 0;
            }
        }
    }
}
