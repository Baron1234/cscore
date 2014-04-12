﻿using CSCore.DMO;
using CSCore.DMO.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CSCore.Streams.Effects
{
    /// <summary>
    /// Wrapper of the DirectX Chorus Effect.
    /// </summary>
    public sealed class DmoChorusEffect : DmoAggregator
    {
        private DmoChorusEffectObject _comObj;
        private DirectSoundFXChorus _effect;

        public DmoChorusEffect(IWaveSource source)
            : base(source)
        {
            Initialize();
        }

        protected override MediaObject CreateMediaObject(WaveFormat inputFormat, WaveFormat outputFormat)
        {
            _comObj = new DmoChorusEffectObject();
            var mediaObject = new MediaObject(Marshal.GetComInterfaceForObject(_comObj, typeof(IMediaObject)));
            _effect = mediaObject.QueryInterface<DirectSoundFXChorus>();

            return mediaObject;
        }

        protected override WaveFormat GetOutputFormat()
        {
            return GetInputFormat();
        }

        private DirectSoundFXChorus Effect
        {
            get { return _effect; }
        }

        [ComImport]
        [Guid("efe6629c-81f7-4281-bd91-c9d604a95af6")]
        private sealed class DmoChorusEffectObject
        {
        }

        #region properties
        /// <summary>
        /// Number of milliseconds the input is delayed before it is played back, in the range from 0 to 20. The default value is 16 ms.
        /// </summary>
        public float Delay
        {
            get { return Effect.Parameters.Delay; }
            set
            {
                if (value < DelayMin || value > DelayMax)
                    throw new ArgumentOutOfRangeException("value");
                SetValue("Delay", value);
            }
        }

        /// <summary>
        /// Percentage by which the delay time is modulated by the low-frequency oscillator, in hundredths of a percentage point. Must be in the range from 0 through 100. The default value is 10.
        /// </summary>
        public float Depth
        {
            get { return Effect.Parameters.Depth; }
            set
            {
                if (value < DepthMin || value > DepthMax)
                    throw new ArgumentOutOfRangeException("value");
                SetValue("Depth", value);
            }
        }

        /// <summary>
        /// Percentage of output signal to feed back into the effect's input, in the range from -99 to 99. The default value is 25.
        /// </summary>
        public float Feedback
        {
            get { return Effect.Parameters.Feedback; }
            set
            {
                if (value < FeedbackMin || value > FeedbackMax)
                    throw new ArgumentOutOfRangeException("value");
                SetValue("Feedback", value);
            }
        }

        /// <summary>
        /// Frequency of the LFO, in the range from 0 to 10. The default value is 1.1.
        /// </summary>
        public float Frequency
        {
            get { return Effect.Parameters.Frequency; }
            set
            {
                if (value < FrequencyMin || value > FrequencyMax)
                    throw new ArgumentOutOfRangeException("value");
                SetValue("Frequency", value);
            }
        }

        /// <summary>
        /// Waveform shape of the LFO. By default, the waveform is a sine.
        /// </summary>
        public Waveform Waveform
        {
            get { return (Waveform)Effect.Parameters.Waveform; }
            set
            {
                SetValue("Waveform", (int)value);
            }
        }

        /// <summary>
        /// Phase differential between left and right LFOs. The default value is Phase90.
        /// </summary>
        public Phase Phase
        {
            get { return (Phase)Effect.Parameters.Phase; }
            set
            {
                SetValue("Phase", (int)value);
            }
        }

        /// <summary>
        /// Ratio of wet (processed) signal to dry (unprocessed) signal. Must be in the range from 0 through 100 (all wet). The default value is 50.
        /// </summary>
        public float WetDryMix
        {
            get { return Effect.Parameters.WetDryMix; }
            set
            {
                if (value < WetDryMixMin || value > WetDryMixMax)
                    throw new ArgumentOutOfRangeException("value");
                SetValue("WetDryMix", value);
            }
        }

        private void SetValue<T>(string fieldname, T value) where T : struct
        {
            var p = Effect.Parameters;
            p.GetType().GetField(fieldname).SetValueForValueType(ref p, value);
            Effect.Parameters = p;
        }
        #endregion

        #region contants
        public const float DelayDefault = 16f;
        public const float DelayMax = 20f;
        public const float DelayMin = 0f;
        public const float DepthDefault = 10f;
        public const float DepthMax = 100f;
        public const float DepthMin = 0f;
        public const float FeedbackDefault = 25f;
        public const float FeedbackMax = 99f;
        public const float FeedbackMin = -99f;
        public const float FrequencyDefault = 1.1f;
        public const float FrequencyMax = 10f;
        public const float FrequencyMin = 0f;
        public const int Phase180 = 4;
        public const int Phase90 = 3;
        public const int PhaseDefault = 3;
        public const int PhaseMax = 4;
        public const int PhaseMin = 0;
        public const int PhaseNegative180 = 0;
        public const int PhaseNegative90 = 1;
        public const int PhaseZero = 2;
        public const int WaveformDefault = 1;
        public const int WaveformSin = 1;
        public const int WaveformTriangle = 0;
        public const float WetDryMixDefault = 50f;
        public const float WetDryMixMax = 100f;
        public const float WetDryMixMin = 0f;
        #endregion
    }

    /// <summary>
    /// Default value is Phase90.
    /// </summary>
    public enum Phase : int
    {
        Phase180 = 4,
        /// <summary>
        /// Default
        /// </summary>
        Phase90 = 3,
        PhaseZero = 2,
        PhaseNegative90 = 1,
        PhaseNegative180 = 0,
    }

    /// <summary>
    /// Default value is WaveformSin.
    /// </summary>
    public enum Waveform : int
    {
        /// <summary>
        /// Default
        /// </summary>
        WaveformSin = 1,
        WaveformTriangle = 0
    }
}
