﻿using CSCore.SoundOut.MmInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace CSCore.SoundOut
{
    public class WaveOut : ISoundOut
    {
        public static int GetDeviceCount()
        {
            return MMInterops.waveOutGetNumDevs();
        }

        public static WaveOutCaps GetDevice(int device)
        {
            WaveOutCaps caps = new WaveOutCaps();
            MMInterops.waveOutGetDevCaps((uint)device, out caps, (uint)Marshal.SizeOf(caps));
            return caps;
        }

        public static WaveOutCaps[] GetDevices()
        {
            WaveOutCaps[] caps = new WaveOutCaps[WaveOut.GetDeviceCount()];
            for (int i = 0; i < caps.Length; i++)
                caps[i] = GetDevice(i);
            return caps;
        }

        public event EventHandler Stopped;

        int _device = 0;
        protected volatile IntPtr _hWaveOut;
        protected object _lockObj = new object();
        IWaveSource _source;
        int _latency = 150;

        PlaybackState _playbackState = PlaybackState.Stopped;
        List<WaveOutBuffer> _buffers;

        int _activeBuffers;

        internal object LockObj { get { return _lockObj; } }

        public float Volume
        {
            get { return MMInterops.GetVolume(_hWaveOut); }
            set { MMInterops.SetVolume(_hWaveOut, value, value); }
        }

        public IWaveSource WaveSource
        {
            get { return _source; }
        }

        public PlaybackState PlaybackState
        {
            get { return _playbackState; }
        }

        public int Device
        {
            get { return _device; }
            set { _device = value; }
        }

        public IntPtr WaveOutHandle
        {
            get { return _hWaveOut; }
        }

        public int Latency
        {
            get { return _latency; }
            set { _latency = value; }
        }

        public WaveOut()
        {
             callback = new MMInterops.WaveCallback(Callback);
        }

        public virtual void Initialize(IWaveSource source)
        {
            int bufferSize;
            lock (_lockObj)
            {
                _source = source;
                _hWaveOut = CreateWaveOut();
                bufferSize = (int)source.WaveFormat.MillisecondsToBytes(_latency);
                _buffers = new List<WaveOutBuffer>();
                for (int i = 0; i < 2; i++)
                {
                    _buffers.Add(new WaveOutBuffer(this, bufferSize));
                    _buffers.Last().Initialize();
                }
            }
        }

        public void Play()
        {
            if (_playbackState == SoundOut.PlaybackState.Stopped)
            {
                StartPlayback();
                _playbackState = SoundOut.PlaybackState.Playing;
            }
            else if (_playbackState == SoundOut.PlaybackState.Paused)
            {
                Resume();
                _playbackState = SoundOut.PlaybackState.Playing;
            }
        }

        public void Pause()
        {
            if (_playbackState == SoundOut.PlaybackState.Playing)
            {
                lock (_lockObj)
                {
                    Context.Current.Logger.MMResult(MMInterops.waveOutPause(_hWaveOut),
                        "waveOutPause", "WaveOut.Pause()");
                }
                _playbackState = SoundOut.PlaybackState.Paused;
            }
        }

        public void Resume()
        {
            if (_playbackState == SoundOut.PlaybackState.Paused)
            {
                lock (_lockObj)
                {
                    Context.Current.Logger.MMResult(MMInterops.waveOutRestart(_hWaveOut),
                        "waveOutRestart", "WaveOut.Resume()");
                }
                _playbackState = SoundOut.PlaybackState.Playing;
            }
        }

        public void Stop()
        {
            if (_playbackState != SoundOut.PlaybackState.Stopped)
            {
                _playbackState = SoundOut.PlaybackState.Stopped;
                lock (_lockObj)
                {
                    var result = MMInterops.waveOutReset(_hWaveOut);
                    Context.Current.Logger.MMResult(
                        result,
                        "waveOutReset",
                        "WaveOut.Stop()");
                }

                RaiseStopped();
            }
        }

        private MMInterops.WaveCallback callback;

        protected virtual IntPtr CreateWaveOut()
        {
            IntPtr handle;
            Context.Current.Logger.MMResult(MMInterops.waveOutOpen(out handle, (IntPtr)_device, _source.WaveFormat, callback, IntPtr.Zero,
                                            MMInterops.WaveInOutOpenFlags.CALLBACK_FUNCTION), "waveOutOpen", "WaveOut.CreateWaveOut()");

            return handle;
        }

        protected virtual void Callback(IntPtr handle, WaveMsg msg, UIntPtr user, WaveHeader header, UIntPtr reserved)
        {
            if (_hWaveOut != handle) return; //message does not belong to this waveout instance
            if (msg == WaveMsg.WOM_DONE)
            {
                GCHandle hBuffer = (GCHandle)header.userData;
                WaveOutBuffer buffer = hBuffer.Target as WaveOutBuffer;
                System.Threading.Interlocked.Decrement(ref _activeBuffers);

                if (buffer == null) return;
                if (_playbackState != SoundOut.PlaybackState.Stopped)
                {
                    lock (_lockObj)
                    {
                        if (buffer.WriteData())
                            System.Threading.Interlocked.Increment(ref _activeBuffers);
                    }
                }

                if (_activeBuffers == 0)
                {
                    _playbackState = SoundOut.PlaybackState.Stopped;
                    RaiseStopped();
                }
            }
            else if (msg == WaveMsg.WOM_CLOSE)
            {
                var state = _playbackState;
                _playbackState = SoundOut.PlaybackState.Stopped;
                if (state != SoundOut.PlaybackState.Stopped)
                    RaiseStopped();
                Context.Current.Logger.Info("Closing WaveOut", "WaveOut.CallBack");
            }
        }

        private void StartPlayback()
        {
            foreach (var buffer in _buffers)
            {
                if (!buffer.IsInQueue)
                {
                    if (buffer.WriteData())
                    {
                        System.Threading.Interlocked.Increment(ref _activeBuffers);
                    }
                    else
                    {
                        _playbackState = SoundOut.PlaybackState.Stopped;
                        RaiseStopped();
                        break;
                    }
                }
            }
        }

        protected void RaiseStopped()
        {
            if (Stopped != null)
            {
                Stopped(this, new EventArgs());
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (_hWaveOut == IntPtr.Zero)
                return;

            Stop();
            lock (_lockObj)
            {
                if (_buffers != null)
                    _buffers.ForEach(x => x.Dispose());
                Context.Current.Logger.MMResult(MMInterops.waveOutClose(_hWaveOut), "waveOutClose", "WaveOut.Dispose()");
                _hWaveOut = IntPtr.Zero;
            }

            Context.Current.Logger.Info("WaveOut disposed");
        }

        ~WaveOut()
        {
            Dispose(false);
        }
    }
}
