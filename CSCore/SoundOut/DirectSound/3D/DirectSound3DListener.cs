﻿using CSCore.Win32;
using System;
using System.Runtime.InteropServices;

namespace CSCore.SoundOut.DirectSound
{
    [Guid("279AFA84-4981-11CE-A521-0020AF0BE560")]
    public unsafe class DirectSound3DListener : ComObject
    {
        const string c = "IDirectSound3DListerner";

        public static DirectSound3DListener FromBuffer(DirectSoundPrimaryBuffer buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            return buffer.QueryInterface<DirectSound3DListener>();
        }

        public DirectSound3DListener(IntPtr basePtr)
            : base(basePtr)
        {
        }

        public DSListener3DSettings AllParameters
        {
            get
            {
                DSListener3DSettings settings;
                DirectSoundException.Try(GetAllParameters(out settings), c, "GetAllParameters");
                return settings;
            }
            set
            {
                DirectSoundException.Try(SetAllParameters(value), c, "SetAllParameters");
            }
        }

        public float DistanceFactor
        {
            get
            {
                float r;
                DirectSoundException.Try(GetDistanceFactor(out r), c, "GetDistanceFactor");
                return r;
            }
            set
            {
                DirectSoundException.Try(SetDistanceFactor(value), c, "SetDistanceFactor");
            }
        }

        //http://msdn.microsoft.com/de-de/library/windows/desktop/ee418003(v=vs.85).aspx
        public DSResult GetAllParameters(out DSListener3DSettings dsListener3DSettings)
        {
            fixed (void* psettings = &dsListener3DSettings)
            {
                return InteropCalls.CalliMethodPtr(UnsafeBasePtr, psettings, ((void**)(*(void**)UnsafeBasePtr))[3]);
            }
        }

        public DSResult GetDistanceFactor(out float distanceFactor)
        {
            fixed (void* pdistanceFactor = &distanceFactor)
            {
                return InteropCalls.CalliMethodPtr(UnsafeBasePtr, pdistanceFactor, ((void**)(*(void**)UnsafeBasePtr))[4]);
            }
        }

        public DSResult GetDopplerFactor(out float dopplerFactor)
        {
            fixed (void* pdopplerFactor = &dopplerFactor)
            {
                return InteropCalls.CalliMethodPtr(UnsafeBasePtr, pdopplerFactor, ((void**)(*(void**)UnsafeBasePtr))[5]);
            }
        }

        public DSResult GetOrientation(out D3DVector orientFrontRef, out D3DVector orientTopRef)
        {
            fixed (void* porientfront = &orientFrontRef, porienttop = &orientTopRef)
            {
                return InteropCalls.CalliMethodPtr(UnsafeBasePtr, porientfront, porienttop, ((void**)(*(void**)UnsafeBasePtr))[6]);
            }
        }

        public DSResult GetPosition(out D3DVector position)
        {
            fixed (void* pposition = &position)
            {
                return InteropCalls.CalliMethodPtr(UnsafeBasePtr, pposition, ((void**)(*(void**)UnsafeBasePtr))[7]);
            }
        }

        public DSResult GetRolloffFactor(out float rolloffFactor)
        {
            fixed (void* prolloffFactor = &rolloffFactor)
            {
                return InteropCalls.CalliMethodPtr(UnsafeBasePtr, prolloffFactor, ((void**)(*(void**)UnsafeBasePtr))[8]);
            }
        }

        public DSResult GetVelocity(out D3DVector velocity)
        {
            fixed (void* pvelocity = &velocity)
            {
                return InteropCalls.CalliMethodPtr(UnsafeBasePtr, pvelocity, ((void**)(*(void**)UnsafeBasePtr))[9]);
            }
        }

        public DSResult SetAllParameters(DSListener3DSettings settings, DS3DApplyMode applyMode = DS3DApplyMode.Immediate)
        {
            return InteropCalls.CalliMethodPtr(UnsafeBasePtr, &settings, unchecked((int)applyMode), ((void**)(*(void**)UnsafeBasePtr))[10]);
        }

        public DSResult SetDistanceFactor(float distanceFactor, DS3DApplyMode applyMode = DS3DApplyMode.Immediate)
        {
            return InteropCalls.CalliMethodPtr(UnsafeBasePtr, distanceFactor, unchecked((int)applyMode), ((void**)(*(void**)UnsafeBasePtr))[11]);
        }

        public DSResult SetDopplerFactor(float dopplerFactor, DS3DApplyMode applyMode = DS3DApplyMode.Immediate)
        {
            return InteropCalls.CalliMethodPtr(UnsafeBasePtr, dopplerFactor, unchecked((int)applyMode), ((void**)(*(void**)UnsafeBasePtr))[12]);
        }

        public DSResult SetOrientation(float xFront, float yFront, float zFront, float xTop, float yTop, float zTop, DS3DApplyMode applyMode = DS3DApplyMode.Immediate)
        {
            return InteropCalls.CalliMethodPtr(UnsafeBasePtr, xFront, yFront, zFront, xTop, yTop, zTop, unchecked((int)applyMode), ((void**)(*(void**)UnsafeBasePtr))[13]);
        }

        public DSResult SetOrientation(D3DVector front, D3DVector top, DS3DApplyMode applyMode = DS3DApplyMode.Immediate)
        {
            return SetOrientation(front.X, front.Y, front.Z, top.X, top.Y, top.Z, applyMode);
        }

        public DSResult SetPosition(D3DVector position, DS3DApplyMode applyMode = DS3DApplyMode.Immediate)
        {
            return SetPosition(position.X, position.Y, position.Z, applyMode);
        }

        public DSResult SetPosition(float x, float y, float z, DS3DApplyMode applyMode = DS3DApplyMode.Immediate)
        {
            return InteropCalls.CalliMethodPtr(UnsafeBasePtr, x, y, z, unchecked((int)applyMode), ((void**)(*(void**)UnsafeBasePtr))[14]);
        }

        public DSResult SetRolloffFactor(float rolloffFactor, DS3DApplyMode applyMode = DS3DApplyMode.Immediate)
        {
            return InteropCalls.CalliMethodPtr(UnsafeBasePtr, rolloffFactor, unchecked((int)applyMode), ((void**)(*(void**)UnsafeBasePtr))[15]);
        }

        public DSResult SetVelocity(D3DVector velocity, DS3DApplyMode applyMode = DS3DApplyMode.Immediate)
        {
            return SetVelocity(velocity.X, velocity.Y, velocity.Z, applyMode);
        }

        public DSResult SetVelocity(float x, float y, float z, DS3DApplyMode applyMode = DS3DApplyMode.Immediate)
        {
            return InteropCalls.CalliMethodPtr(UnsafeBasePtr, x, y, z, unchecked((int)applyMode), ((void**)(*(void**)UnsafeBasePtr))[16]);
        }

        public DSResult CommitDeferredSettings()
        {
            return InteropCalls.CalliMethodPtr(UnsafeBasePtr, ((void**)(*(void**)UnsafeBasePtr))[17]);
        }
    }
}