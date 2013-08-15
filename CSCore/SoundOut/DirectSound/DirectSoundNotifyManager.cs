﻿using System;
using System.Threading;

namespace CSCore.SoundOut.DirectSound
{
    public class DirectSoundNotifyManager : IDisposable
    {
        public event EventHandler<DirectSoundNotifyEventArgs> NotifyAnyRaised;

        public event EventHandler Stopped;

        private DirectSoundSecondaryBuffer _buffer;
        private WaveFormat _waveFormat;
        private int _bufferSize;
        private DSBPositionNotify[] _positionNotifies;
        private WaitHandle[] _waitHandles;
        private bool _disposing;
        private Func<object, bool> _hasToStop;
        private int _latency;

        private Thread _thread;

        private DirectSoundNotify _notify;

        /// <summary>
        /// Was the notifymanager ever started?
        /// </summary>
        public bool GotStarted { get; private set; }

        public DirectSoundNotifyManager(DirectSoundSecondaryBuffer buffer, WaveFormat waveFormat, int bufferSize)
            : this(buffer, waveFormat, bufferSize, null)
        {
        }

        public DirectSoundNotifyManager(DirectSoundSecondaryBuffer buffer, WaveFormat waveFormat, int bufferSize, Func<object, bool> stopEvaluation = null)
        {
            if (stopEvaluation == null) _hasToStop = new Func<object, bool>((sender) => _disposing);
            else
            {
                _hasToStop = new Func<object, bool>((s) =>
                {
                    return stopEvaluation(this) || _disposing;
                });
            }

            _buffer = buffer;
            _waveFormat = waveFormat;
            _bufferSize = bufferSize;

            var notify = buffer.QueryInterface<DirectSoundNotify>();

            var waitHandleNull = new EventWaitHandle(false, EventResetMode.AutoReset);
            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            var waitHandleEnd = new EventWaitHandle(false, EventResetMode.AutoReset);

            DSBPositionNotify[] positionNotifies = new DSBPositionNotify[3];
            positionNotifies[0] = new DSBPositionNotify(0, waitHandleNull.SafeWaitHandle.DangerousGetHandle());
            positionNotifies[1] = new DSBPositionNotify((uint)bufferSize, waitHandle.SafeWaitHandle.DangerousGetHandle());
            positionNotifies[2] = new DSBPositionNotify(0xFFFFFFFF, waitHandleEnd.SafeWaitHandle.DangerousGetHandle());

            var result = notify.SetNotificationPositions(positionNotifies);
            DirectSoundException.Try(result, "IDirectSoundNotify", "SetNotificationPositions");

            _positionNotifies = positionNotifies;
            _waitHandles = new WaitHandle[] { waitHandleNull, waitHandle, waitHandleEnd };

            _latency = (int)(_bufferSize / (float)_waveFormat.BytesPerSecond * 1000);

            _notify = notify;

            Context.Current.Logger.Debug("DirectSoundNotifyManager initialized", "DirectSoundNotifyManager.ctor(DirectSoundSecondaryBuffer, WaveFormat, int, Func<object, bool>)");
        }

        public void Start()
        {
            if (_thread != null) return;

            _thread = new Thread(NotifyProc);
            _thread.Name = "DirectSoundNotifyManager Thread: ID = 0x" + _notify.BasePtr.ToInt64().ToString("x");
            _thread.Priority = ThreadPriority.AboveNormal;
            //_thread.IsBackground = true;
            _thread.Start();
            Context.Current.Logger.Debug("DirectSoundNotifyManager started", "DirectSoundNotifyManager.Start()");
        }

        private void NotifyProc()
        {
            try
            {
                GotStarted = true;
                while (true)
                {
                    if (_hasToStop(this))
                        break;

                    int handleIndex = WaitHandle.WaitAny(_waitHandles, _waitHandles.Length * _latency, true);

                    if (!RaiseNotifyAnyRaised(handleIndex))
                        break;
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                RaiseStopped();
                _thread = null;
            }
        }

        public bool Stop()
        {
            return Stop(_waitHandles.Length * _latency);
        }

        public bool Stop(int timeout)
        {
            _disposing = true;
            if (_thread != null)
            {
                if (!_thread.Join(timeout))
                {
                    Context.Current.Logger.Error(String.Format("DirectSoundNotifyManager stop failed: timeout after {0} ms", timeout), "DirectSoundNotifyManager.Stop()");
                    return false;
                }
                _thread = null;
            }
            Context.Current.Logger.Debug("DirectSoundNotifyManager stopped successfully", "DirectSoundNotifyManager.Stop(int)");
            return true;
        }

        public void Abort()
        {
            if (_thread != null)
            {
                _disposing = true;
                _thread.Abort();
                _thread = null;
            }
        }

        protected bool RaiseNotifyAnyRaised(int handleIndex)
        {
            if (NotifyAnyRaised != null)
            {
                var e = new DirectSoundNotifyEventArgs(handleIndex, _bufferSize);
                NotifyAnyRaised(this, e);
                return !e.StopPlayback;
            }
            return false;
        }

        protected void RaiseStopped()
        {
            if (Stopped != null)
                Stopped(this, new EventArgs());
        }

        public void WaitForStopped()
        {
            try
            {
                if (_thread != null && _thread.IsAlive)
                    _thread.Join();
            }
            catch (Exception)
            {
            }
        }

        public bool WaitForStopped(int timeout)
        {
            try
            {
                if (_thread != null && _thread.IsAlive)
                    return _thread.Join(timeout);
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsNotifyThread(Thread thread)
        {
            if (thread == null)
                throw new ArgumentNullException("thread");
            return thread.ManagedThreadId == thread.ManagedThreadId;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            Stop();
            if (_notify != null)
            {
                _notify.Dispose();
                _notify = null;
            }
        }

        ~DirectSoundNotifyManager()
        {
            Dispose(false);
        }
    }
}