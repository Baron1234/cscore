﻿using System;
using System.IO;
using System.Text;

namespace CSCore.Codecs.WAV
{
    public class WaveWriter : IDisposable
    {
        Stream _stream;
        BinaryWriter _writer;

        WaveFormat _waveFormat;

        long _waveStartPosition;
        int _dataLength;

        public WaveWriter(string fileName, WaveFormat waveFormat)
            : this(File.OpenWrite(fileName), waveFormat)
        {
        }

        public WaveWriter(Stream stream, WaveFormat waveFormat)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (!stream.CanWrite) throw new ArgumentException("stream not writeable");
            if (!stream.CanSeek) throw new ArgumentException("stream not seekable");

            _stream = stream;
            _waveStartPosition = stream.Position;
            _writer = new BinaryWriter(stream);
            for (int i = 0; i < 44; i++)
            {
                _writer.Write((byte)0);
            }
            _waveFormat = waveFormat;


            WriteHeader();
        }

        public void WriteSample(float sample)
        {
            if (_waveFormat.WaveFormatTag == AudioEncoding.Pcm)
            {
                switch (_waveFormat.BitsPerSample)
                {
                    case 8:
                        Write((byte)(byte.MaxValue * sample)); break;
                    case 16:
                        Write((short)sample); break;
                    case 24:
                        byte[] buffer = BitConverter.GetBytes((int)(int.MaxValue * sample));
                        Write(new byte[] { buffer[0], buffer[1], buffer[2] }, 0, 3);
                        break;
                    default:
                        throw new InvalidOperationException("Invalid Waveformat", new InvalidOperationException("Invalid BitsPerSample while using PCM encoding."));
                }
            }
            else if (_waveFormat.WaveFormatTag == AudioEncoding.Extensible && _waveFormat.BitsPerSample == 32)
            {
                Write(UInt16.MaxValue * (int)sample);
            }
            else if (_waveFormat.WaveFormatTag == AudioEncoding.IeeeFloat)
            {
                Write(sample);
            }
            else
            {
                throw new InvalidOperationException("Invalid Waveformat: Waveformat has to be PCM[8, 16, 24, 32];IeeeFloat[32]");
            }
        }

        public void WriteSamples(float[] samples, int offset, int count)
        {
            for (int i = offset; i < offset + count; i++)
                WriteSample(samples[i]);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
            _dataLength += count;
        }

        public void Write(byte value)
        {
            _writer.Write(value);
            _dataLength++;
        }

        public void Write(short value)
        {
            _writer.Write(value);
            _dataLength += 2;
        }

        public void Write(int value)
        {
            _writer.Write(value);
            _dataLength += 4;
        }

        public void Write(float value)
        {
            _writer.Write(value);
            _dataLength += 4;
        }

        private void WriteHeader()
        {
            _writer.Flush();

            long currentPosition = _stream.Position;
            _stream.Position = _waveStartPosition;

            WriteRiffHeader();
            WriteFMTChunk();
            WriteDataChunk();

            _stream.Position = currentPosition;
        }

        private void WriteRiffHeader()
        {
            _writer.Write(Encoding.UTF8.GetBytes("RIFF"));
            _writer.Write((int)(_stream.Length - 8));
            _writer.Write(Encoding.UTF8.GetBytes("WAVE"));
        }

        private void WriteFMTChunk()
        {
            _writer.Write(Encoding.UTF8.GetBytes("fmt "));
            _writer.Write(16);
            _writer.Write((short)_waveFormat.WaveFormatTag);
            _writer.Write(_waveFormat.Channels);
            _writer.Write(_waveFormat.SampleRate);
            _writer.Write(_waveFormat.BytesPerSecond);
            _writer.Write((short)_waveFormat.BlockAlign);
            _writer.Write(_waveFormat.BitsPerSample);
        }

        private void WriteDataChunk()
        {
            _writer.Write(Encoding.UTF8.GetBytes("data"));
            _writer.Write(_dataLength);
        }

        /*private int CalculateFileLength()
        {
            int length = 0;
            length += 4; //WAVE
            length += 16 + 4 + 4; //fmt
            length += 8; //Datachunk
            length += _dataLength;

            return _dataLength;
        }*/

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _stream != null && _writer != null)
            {
                try
                {
                    WriteHeader();
                }
                catch (Exception ex) 
                {
                    Context.Current.Logger.Fatal(ex, "WaveWriter.Dispose(bool)");
                }
                finally
                {
                    _writer.Dispose();
                    _writer = null;
                    _stream = null;
                }
            }
        }

        ~WaveWriter()
        {
            Dispose(false);
        }
    }
}
