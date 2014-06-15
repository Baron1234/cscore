using System;
using CSCore.Win32;

namespace CSCore.XAudio2
{
    /// <summary>
    /// Represents the base class from which <see cref="XAudio2SourceVoice"/>, <see cref="XAudio2SubmixVoice"/> and <see cref="XAudio2MasteringVoice"/> are derived.
    /// </summary>
    public class XAudio2Voice : ComObject
    {
        private const string n = "IXAudio2Voice";

        internal XAudio2Voice()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XAudio2Voice"/> class.
        /// </summary>
        /// <param name="ptr">Native pointer of the <see cref="XAudio2Voice"/> object.</param>
        public XAudio2Voice(IntPtr ptr)
            : base(ptr)
        {
        }

        /// <summary>
        /// Gets the <see cref="CSCore.XAudio2.VoiceDetails"/> of the <see cref="XAudio2Voice"/>.
        /// These details include information about the number of input channels, the sample rate and the <see cref="CSCore.XAudio2.VoiceFlags"/>.
        /// </summary>
        public VoiceDetails VoiceDetails
        {
            get
            {
                VoiceDetails value;
                XAudio2Exception.Try(GetVoiceDetailsNative(out value), n, "GetVoiceDetails");
                return value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CSCore.XAudio2.FilterParameters"/> of the <see cref="XAudio2Voice"/>.
        /// </summary>
        public FilterParameters FilterParameters
        {
            get { return GetFilterParameters(); }
            set { SetFilterParameters(value, 0); }
        }

        /// <summary>
        /// Gets or sets the volume of the <see cref="XAudio2Voice"/>. The default value is 1.0.
        /// </summary>
        /// <remarks>
        /// The master volume level is applied at different times depending on the type of voice. 
        /// For submix and mastering voices the volume level is applied just before the voice's built in filter and effect chain is applied. 
        /// For source voices the master volume level is applied after the voice's filter and effect 
        /// chain is applied. Volume levels are expressed as floating-point amplitude multipliers 
        /// between -2^24 and 2^24, with a maximum 
        /// gain of 144.5 dB. A volume level of 1.0 means there is no attenuation or gain and 0 means silence. 
        /// Negative levels can be used to invert the audio's phase.
        /// </remarks>
        public float Volume
        {
            get { return GetVolume(); }
            set { SetVolume(value, 0); }
        }

        /// <summary>
        /// Returns information about the creation flags, input channels, and sample rate of a voice.
        /// </summary>
        /// <param name="voiceDetails"><see cref="CSCore.XAudio2.VoiceDetails"/> object containing information about the voice.</param>
        /// <returns>HRESULT</returns>
        public unsafe int GetVoiceDetailsNative(out VoiceDetails voiceDetails)
        {
            voiceDetails = default(VoiceDetails);
            fixed(void* ptr = &voiceDetails)
            {
                return InteropCalls.CallI(_basePtr, ptr, ((void**)(*(void**)_basePtr))[0]);
            }
        }

        /// <summary>
        /// Designates a new set of submix or mastering voices to receive the output of the voice.
        /// </summary>
        /// <param name="voiceSends">
        /// VoiceSends structure which contains Output voices. If <see cref="voiceSends"/> is null, the voice will send 
        /// its output to the current mastering voice. All of the voices in a send list must have the same input sample rate.
        /// </param>
        /// <returns>HRESULT</returns>
        public unsafe int SetOutputVoicesNative(VoiceSends? voiceSends)
        {
            VoiceSends value;
            if(voiceSends.HasValue)
                value = voiceSends.Value;

            return InteropCalls.CallI(_basePtr, voiceSends.HasValue ? &value : (void*)IntPtr.Zero, ((void**)(*(void**)_basePtr))[1]);
        }

        /// <summary>
        /// Designates a new set of submix or mastering voices to receive the output of the voice.
        /// </summary>
        /// <param name="voiceSendDescriptors">
        /// Array of <see cref="VoiceSendDescriptor"/>s. if <see cref="voiceSendDescriptors"/> is null, the voice will send its output to the current mastering voice.
        /// All voices in the <see cref="voiceSendDescriptors"/> must have the same input sample rate.
        /// </param>
        public unsafe void SetOutputVoices(VoiceSendDescriptor[] voiceSendDescriptors)
        {
            if (voiceSendDescriptors == null)
            {
                XAudio2Exception.Try(SetOutputVoicesNative(null), n, "SetOutputVoices");
            }
            else
            {
                fixed (void* ptr = &voiceSendDescriptors[0])
                {
                    var p = new VoiceSends()
                    {
                        SendCount = voiceSendDescriptors.Length,
                        SendsPtr = new IntPtr(ptr)
                    };
                    XAudio2Exception.Try(SetOutputVoicesNative(p), n, "SetOutputVoices");
                }
            }
        }

        /// <summary>
        /// Replaces the effect chain of the voice.
        /// </summary>
        /// <param name="effectChain">Describes the new effect chain to use. 
        /// If null is passed, the current effect chain is removed.</param>
        /// <returns>HRESULT</returns>
        public unsafe int SetEffectChainNative(EffectChain? effectChain)
        {
            void* ptr = (void*)IntPtr.Zero;
            //check whether null is passed -> if null is passed, the chain will be removed
            if (effectChain.HasValue)
            {
                var value = effectChain.Value;
                ptr = &value;
            }

            return InteropCalls.CallI(_basePtr, ptr, ((void**)(*(void**)_basePtr))[2]);
        }

        /// <summary>
        /// Replaces the effect chain of the voice.
        /// </summary>
        /// <param name="effectDescriptors">Describes the new effect chain to use. 
        /// If null is passed, the current effect chain is removed.</param>
        public void SetEffectChain(EffectDescriptor[] effectDescriptors)
        {
            if (effectDescriptors == null || effectDescriptors.Length == 0)
            {
                SetEffectChainNative(null);
            }
            else
            {
                unsafe
                {
                    fixed (void* p = &effectDescriptors[0])
                    {
                        var value = new EffectChain()
                        {
                            EffectCount = effectDescriptors.Length,
                            EffectDescriptorsPtr = new IntPtr(p)
                        };
                        
                        SetEffectChainNative(value);
                    }
                }
            }
        }

        /// <summary>
        /// Enables the effect at a given position in the effect chain of the voice.
        /// </summary>
        /// <param name="effectIndex">Zero-based index of an effect in the effect chain of the voice.</param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        /// <returns>HRESULT</returns>
        public unsafe int EnableEffectNative(int effectIndex, int operationSet)
        {
            return InteropCalls.CallI(_basePtr, effectIndex, operationSet, ((void**)(*(void**)_basePtr))[3]);
        }

        /// <summary>
        /// Enables the effect at a given position in the effect chain of the voice.
        /// </summary>
        /// <param name="effectIndex">Zero-based index of an effect in the effect chain of the voice.</param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        public void EnableEffect(int effectIndex, int operationSet)
        {
            XAudio2Exception.Try(EnableEffectNative(effectIndex, operationSet), n, "EnableEffect");
        }

        /// <summary>
        /// Enables the effect at a given position in the effect chain of the voice.
        /// </summary>
        /// <param name="effectIndex">Zero-based index of an effect in the effect chain of the voice.</param>
        public void EnableEffect(int effectIndex)
        {
            EnableEffect(effectIndex, 0);
        }

        /// <summary>
        /// Disables the effect at a given position in the effect chain of the voice.
        /// </summary>
        /// <param name="effectIndex">Zero-based index of an effect in the effect chain of the voice.</param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        /// <returns>HRESULT</returns>
        public unsafe int DisableEffectNative(int effectIndex, int operationSet)
        {
            return InteropCalls.CallI(_basePtr, effectIndex, operationSet, ((void**)(*(void**)_basePtr))[4]);
        }

        /// <summary>
        /// Disables the effect at a given position in the effect chain of the voice.
        /// </summary>
        /// <param name="effectIndex">Zero-based index of an effect in the effect chain of the voice.</param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        public void DisableEffect(int effectIndex, int operationSet)
        {
            XAudio2Exception.Try(DisableEffectNative(effectIndex, operationSet), n, "DisableEffect");
        }

        /// <summary>
        /// Disables the effect at a given position in the effect chain of the voice.
        /// </summary>
        /// <param name="effectIndex">Zero-based index of an effect in the effect chain of the voice.</param>
        public void DisableEffect(int effectIndex)
        {
            DisableEffect(effectIndex, 0);
        }

        /// <summary>
        /// Returns the running state of the effect at a specified position in the effect chain of the voice.
        /// </summary>
        /// <param name="effectIndex">Zero-based index of an effect in the effect chain of the voice.</param>
        /// <param name="enabled">Returns true if the effect is enabled. If the effect is disabled, returns false.</param>
        public unsafe void GetEffectStateNative(int effectIndex, out NativeBool enabled)
        {
            fixed(void* p = &enabled)
            {
                InteropCalls.CallI1(_basePtr, effectIndex, p, ((void**)(*(void**)_basePtr))[5]);
            }
        }

        /// <summary>
        /// Returns whether the effect at the specified position in the effect chain is enabled.
        /// </summary>
        /// <param name="effectIndex">Zero-based index of an effect in the effect chain of the voice.</param>
        /// <returns>Returns true if the effect is enabled. If the effect is disabled, returns false.</returns>
        public bool IsEffectEnabled(int effectIndex)
        {
            NativeBool value;
            GetEffectStateNative(effectIndex, out value);
            return value;
        }

        /// <summary>
        /// Sets parameters for a given effect in the voice's effect chain.
        /// </summary>
        /// <param name="effectIndex">Zero-based index of an effect within the voice's effect chain.</param>
        /// <param name="effectParameters"> New values of the effect-specific parameters. </param>
        /// <param name="parametersByteSize">Size of the <see cref="effectParameters"/> array in bytes.</param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        /// <returns>HRESULT</returns>
        public unsafe int SetEffectParametersNative(int effectIndex, IntPtr effectParameters, int parametersByteSize, int operationSet)
        {
            return InteropCalls.CallI(_basePtr, effectIndex, effectParameters, parametersByteSize, operationSet, ((void**)(*(void**)_basePtr))[6]);
        }

        /// <summary>
        /// Sets parameters for a given effect in the voice's effect chain.
        /// </summary>
        /// <typeparam name="T">Effect parameter.</typeparam>
        /// <param name="effectIndex">Zero-based index of an effect within the voice's effect chain.</param>
        /// <param name="effectParameters">New values of the effect-specific parameters.</param>
        public void SetEffectParameters<T>(int effectIndex, T effectParameters) where T : struct
        {
            SetEffectParameters<T>(effectIndex, effectParameters, 0);
        }

        /// <summary>
        /// Sets parameters for a given effect in the voice's effect chain.
        /// </summary>
        /// <typeparam name="T">Effect parameter.</typeparam>
        /// <param name="effectIndex">Zero-based index of an effect within the voice's effect chain.</param>
        /// <param name="effectParameters">New values of the effect-specific parameters.</param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        public unsafe void SetEffectParameters<T>(int effectIndex, T effectParameters, int operationSet) where T : struct
        {
            int parameterSize = Utils.Utils.SizeOf<T>();
            void* ptr = stackalloc byte[parameterSize];

            Utils.Utils.WriteToMemory(new IntPtr(ptr), ref effectParameters);
            XAudio2Exception.Try(SetEffectParametersNative(effectIndex, new IntPtr(ptr), parameterSize, operationSet),
                n, "SetEffectParameters");
        }

        /// <summary>
        /// Returns the current effect-specific parameters of a given effect in the voice's effect chain.
        /// </summary>
        /// <param name="effectIndex">Zero-based index of an effect within the voice's effect chain.</param>
        /// <param name="effectParameters">Returns the current values of the effect-specific parameters.</param>
        /// <param name="parametersByteSize">Size of the <see cref="effectParameters"/> array in bytes.</param>
        /// <returns>HRESULT</returns>
        public unsafe int GetEffectParametersNative(int effectIndex, IntPtr effectParameters, int parametersByteSize)
        {
            return InteropCalls.CallI(_basePtr, effectIndex, effectParameters.ToPointer(), parametersByteSize, ((void**) (*(void**) _basePtr))[7]);
        }

#warning not tested
        /// <summary>
        /// Returns the current effect-specific parameters of a given effect in the voice's effect chain.
        /// </summary>
        /// <typeparam name="T">Effect parameters.</typeparam>
        /// <param name="effectIndex">Zero-based index of an effect within the voice's effect chain.</param>
        /// <returns>Effect parameters value.</returns>
        public unsafe T GetEffectParameters<T>(int effectIndex) where T : struct
        {
            int parameterSize = Utils.Utils.SizeOf<T>();
            void* ptr = stackalloc byte[parameterSize];

            XAudio2Exception.Try(GetEffectParametersNative(effectIndex, (IntPtr) ptr, parameterSize),
                n, "GetEffectParameters");

            return Utils.Utils.Read<T>((IntPtr) ptr);
        }

        /// <summary>
        /// Sets the voice's filter parameters.
        /// </summary>
        /// <param name="filterParameters"><see cref="CSCore.XAudio2.FilterParameters"/> structure containing the filter information.</param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        /// <returns>HRESULT</returns>
        public unsafe int SetFilterParametersNative(FilterParameters filterParameters, int operationSet)
        {
            return InteropCalls.CallI(_basePtr, &filterParameters, operationSet, ((void**) (*(void**) _basePtr))[8]);
        }

        /// <summary>
        /// Sets the voice's filter parameters.
        /// </summary>
        /// <param name="filterParameters"><see cref="CSCore.XAudio2.FilterParameters"/> structure containing the filter information.</param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        public void SetFilterParameters(FilterParameters filterParameters, int operationSet)
        {
            XAudio2Exception.Try(SetFilterParametersNative(filterParameters, operationSet), n, "SetFilterParameters");
        }

        /// <summary>
        /// Gets the voice's filter parameters.
        /// </summary>
        /// <param name="filterParameters"><see cref="CSCore.XAudio2.FilterParameters"/> structure containing the filter information.</param>
        /// <returns>HRESULT</returns>
        public unsafe int GetFilterParametersNative(out FilterParameters filterParameters)
        {
            filterParameters = default(FilterParameters);
            fixed (void* p = &filterParameters)
            {
                return InteropCalls.CallI(_basePtr, p, ((void**) (*(void**) _basePtr))[9]);
            }
        }

        /// <summary>
        /// Gets the voice's filter parameters.
        /// </summary>
        /// <returns><see cref="CSCore.XAudio2.FilterParameters"/> structure containing the filter information.</returns>
        public FilterParameters GetFilterParameters()
        {
            FilterParameters r;
            XAudio2Exception.Try(GetFilterParametersNative(out r), n, "GetFilterParameters");
            return r;
        }

        /// <summary>
        /// Sets the filter parameters on one of this voice's sends.
        /// </summary>
        /// <param name="destinationVoice">The destination voice of the send whose filter parameters will be set.</param>
        /// <param name="filterParameters"><see cref="CSCore.XAudio2.FilterParameters"/> structure containing the filter information.</param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        /// <returns>HRESULT</returns>
        public unsafe int SetOutputFilterParametersNative(XAudio2Voice destinationVoice, FilterParameters filterParameters,
            int operationSet)
        {
            IntPtr pVoice = destinationVoice == null ? IntPtr.Zero : destinationVoice.BasePtr;
            return InteropCalls.CallI(_basePtr, (void*) pVoice, &filterParameters, operationSet,
                ((void**) (*(void**) _basePtr))[10]);
        }

        /// <summary>
        /// Sets the filter parameters on one of this voice's sends.
        /// </summary>
        /// <param name="destinationVoice">The destination voice of the send whose filter parameters will be set.</param>
        /// <param name="filterParameters"><see cref="CSCore.XAudio2.FilterParameters"/> structure containing the filter information.</param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        public void SetOutputFilterParameters(XAudio2Voice destinationVoice, FilterParameters filterParameters,
            int operationSet)
        {
            XAudio2Exception.Try(SetOutputFilterParametersNative(destinationVoice, filterParameters, operationSet), n, "SetOutputFilterParameters");
        }

        /// <summary>
        /// Sets the filter parameters on one of this voice's sends.
        /// </summary>
        /// <param name="destinationVoice">The destination voice of the send whose filter parameters will be set.</param>
        /// <param name="filterParameters"><see cref="CSCore.XAudio2.FilterParameters"/> structure containing the filter information.</param>
        public void SetOutputFilterParameters(XAudio2Voice destinationVoice, FilterParameters filterParameters)
        {
            SetOutputFilterParameters(destinationVoice, filterParameters, 0);
        }

        /// <summary>
        /// Returns the filter parameters from one of this voice's sends.
        /// </summary>
        /// <param name="destinationVoice">The destination voice of the send whose filter parameters will be read.</param>
        /// <param name="filterParameters"><see cref="CSCore.XAudio2.FilterParameters"/> structure containing the filter information.</param>
        /// <returns>HRESULT</returns>
        public unsafe int GetOutputFilterParametersNative(XAudio2Voice destinationVoice,
            out FilterParameters filterParameters)
        {
            filterParameters = default(FilterParameters);

            IntPtr pVoice = destinationVoice == null ? IntPtr.Zero : destinationVoice.BasePtr;
            fixed (void* p = &filterParameters)
            {
                return InteropCalls.CallI(_basePtr, (void*) pVoice, p, ((void**) (*(void**) _basePtr))[11]);
            }
        }

        /// <summary>
        /// Returns the filter parameters from one of this voice's sends.
        /// </summary>
        /// <param name="destinationVoice">The destination voice of the send whose filter parameters will be read.</param>
        /// <returns><see cref="CSCore.XAudio2.FilterParameters"/> structure containing the filter information.</returns>
        public FilterParameters GetOutputFilterParameters(XAudio2Voice destinationVoice)
        {
            FilterParameters value;
            XAudio2Exception.Try(GetOutputFilterParametersNative(destinationVoice, out value), n, "GetOutputFilterParameters");
            return value;
        }

        /// <summary>
        /// Sets the overall volume level for the voice.
        /// </summary>
        /// <param name="volume">Overall volume level to use. See Remarks for more information on volume levels.</param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        /// <returns>HRESULT</returns>
        /// <remarks>
        /// The master volume level is applied at different times depending on the type of voice. 
        /// For submix and mastering voices the volume level is applied just before the voice's built in filter and effect chain is applied. 
        /// For source voices the master volume level is applied after the voice's filter and effect 
        /// chain is applied. Volume levels are expressed as floating-point amplitude multipliers 
        /// between -2^24 and 2^24, with a maximum 
        /// gain of 144.5 dB. A volume level of 1.0 means there is no attenuation or gain and 0 means silence. 
        /// Negative levels can be used to invert the audio's phase.
        /// </remarks>
        public unsafe int SetVolumeNative(float volume, int operationSet)
        {
            return InteropCalls.CallI(_basePtr, volume, operationSet, ((void**) (*(void**) _basePtr))[12]);
        }

        /// <summary>
        /// Sets the overall volume level for the voice.
        /// </summary>
        /// <param name="volume">Overall volume level to use. See Remarks for more information on volume levels.</param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        /// <remarks>
        /// The master volume level is applied at different times depending on the type of voice. 
        /// For submix and mastering voices the volume level is applied just before the voice's built in filter and effect chain is applied. 
        /// For source voices the master volume level is applied after the voice's filter and effect 
        /// chain is applied. Volume levels are expressed as floating-point amplitude multipliers 
        /// between -2^24 and 2^24, with a maximum 
        /// gain of 144.5 dB. A volume level of 1.0 means there is no attenuation or gain and 0 means silence. 
        /// Negative levels can be used to invert the audio's phase.
        /// </remarks>
        public void SetVolume(float volume, int operationSet)
        {
            XAudio2Exception.Try(SetVolumeNative(volume, operationSet), n, "SetVolume");
        }

        /// <summary>
        /// Gets the current overall volume level of the voice.
        /// </summary>
        /// <param name="volume">Returns the current overall volume level of the voice. See Remarks for more information on volume levels.</param>
        /// <returns>HRESULT</returns>
        /// <remarks>
        /// The master volume level is applied at different times depending on the type of voice. 
        /// For submix and mastering voices the volume level is applied just before the voice's built in filter and effect chain is applied. 
        /// For source voices the master volume level is applied after the voice's filter and effect 
        /// chain is applied. Volume levels are expressed as floating-point amplitude multipliers 
        /// between -2^24 and 2^24, with a maximum 
        /// gain of 144.5 dB. A volume level of 1.0 means there is no attenuation or gain and 0 means silence. 
        /// Negative levels can be used to invert the audio's phase.
        /// </remarks>
        public unsafe int GetVolumeNative(out float volume)
        {
            fixed (void* p = &volume)
            {
                return InteropCalls.CallI(_basePtr, p, ((void**) (*(void**) _basePtr))[13]);
            }
        }

        /// <summary>
        /// Gets the current overall volume level of the voice.
        /// </summary>
        /// <returns>The current overall volume level of the voice. See Remarks for more information on volume levels.</returns>
        /// <remarks>
        /// The master volume level is applied at different times depending on the type of voice. 
        /// For submix and mastering voices the volume level is applied just before the voice's built in filter and effect chain is applied. 
        /// For source voices the master volume level is applied after the voice's filter and effect 
        /// chain is applied. Volume levels are expressed as floating-point amplitude multipliers 
        /// between -2^24 and 2^24, with a maximum 
        /// gain of 144.5 dB. A volume level of 1.0 means there is no attenuation or gain and 0 means silence. 
        /// Negative levels can be used to invert the audio's phase.
        /// </remarks>
        public float GetVolume()
        {
            float v;
            XAudio2Exception.Try(GetVolumeNative(out v), n, "GetVolume");
            return v;
        }

        /// <summary>
        /// Sets the volume levels for the voice, per channel. This method is valid only for source and submix voices, because mastering voices do not specify volume per channel.
        /// </summary>
        /// <param name="channelCount">Number of channels in the voice.</param>
        /// <param name="volumes">Array containing the new volumes of each channel in the voice. The array must have <see cref="channelCount"/> elements. See Remarks for more information on volume levels.</param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        /// <returns>HRESULT</returns>
        /// <remarks>
        /// The master volume level is applied at different times depending on the type of voice. 
        /// For submix and mastering voices the volume level is applied just before the voice's built in filter and effect chain is applied. 
        /// For source voices the master volume level is applied after the voice's filter and effect 
        /// chain is applied. Volume levels are expressed as floating-point amplitude multipliers 
        /// between -2^24 and 2^24, with a maximum 
        /// gain of 144.5 dB. A volume level of 1.0 means there is no attenuation or gain and 0 means silence. 
        /// Negative levels can be used to invert the audio's phase.
        /// </remarks>
        public unsafe int SetChannelVolumesNative(int channelCount, float[] volumes, int operationSet)
        {
            fixed (void* p = &volumes[0])
            {
                return InteropCalls.CallI(_basePtr, channelCount, p, operationSet, ((void**) (*(void**) _basePtr))[14]);
            }
        }

        /// <summary>
        /// Sets the volume levels for the voice, per channel. This method is valid only for source and submix voices, because mastering voices do not specify volume per channel.
        /// </summary>
        /// <param name="channelCount">Number of channels in the voice.</param>
        /// <param name="volumes">Array containing the new volumes of each channel in the voice. The array must have <see cref="channelCount"/> elements. See Remarks for more information on volume levels.</param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        /// <remarks>
        /// The master volume level is applied at different times depending on the type of voice. 
        /// For submix and mastering voices the volume level is applied just before the voice's built in filter and effect chain is applied. 
        /// For source voices the master volume level is applied after the voice's filter and effect 
        /// chain is applied. Volume levels are expressed as floating-point amplitude multipliers 
        /// between -2^24 and 2^24, with a maximum 
        /// gain of 144.5 dB. A volume level of 1.0 means there is no attenuation or gain and 0 means silence. 
        /// Negative levels can be used to invert the audio's phase.
        /// </remarks>

        public void SetChannelVolumes(int channelCount, float[] volumes, int operationSet)
        {
            if(volumes == null)
                throw new ArgumentNullException("volumes");
            if(volumes.Length != channelCount)
                throw new ArgumentException("The length of the volumes argument has to be equal to the channelCount argument.");
            XAudio2Exception.Try(SetChannelVolumesNative(volumes.Length, volumes, operationSet), n, "SetChannelVolumes");
        }

        /// <summary>
        /// Sets the volume levels for the voice, per channel. This method is valid only for source and submix voices, because mastering voices do not specify volume per channel.
        /// </summary>
        /// <param name="channelCount">Number of channels in the voice.</param>
        /// <param name="volumes">Array containing the new volumes of each channel in the voice. The array must have <see cref="channelCount"/> elements. See Remarks for more information on volume levels.</param>
        /// <remarks>
        /// The master volume level is applied at different times depending on the type of voice. 
        /// For submix and mastering voices the volume level is applied just before the voice's built in filter and effect chain is applied. 
        /// For source voices the master volume level is applied after the voice's filter and effect 
        /// chain is applied. Volume levels are expressed as floating-point amplitude multipliers 
        /// between -2^24 and 2^24, with a maximum 
        /// gain of 144.5 dB. A volume level of 1.0 means there is no attenuation or gain and 0 means silence. 
        /// Negative levels can be used to invert the audio's phase.
        /// </remarks>
        public void SetChannelVolumes(int channelCount, float[] volumes)
        {
            SetChannelVolumes(channelCount, volumes, 0);
        }


        /// <summary>
        /// Returns the volume levels for the voice, per channel. 
        /// These settings are applied after the effect chain is applied. 
        /// This method is valid only for source and submix voices, because mastering voices do not specify volume per channel.
        /// </summary>
        /// <param name="channelCount">Confirms the channel count of the voice.</param>
        /// <param name="volumes">Returns the current volume level of each channel in the voice. The array must have at least <see cref="channelCount"/> elements. 
        /// See remarks for more information on volume levels.</param>
        /// <returns>HRESULT</returns>
        /// <remarks>
        /// The master volume level is applied at different times depending on the type of voice. 
        /// For submix and mastering voices the volume level is applied just before the voice's built in filter and effect chain is applied. 
        /// For source voices the master volume level is applied after the voice's filter and effect 
        /// chain is applied. Volume levels are expressed as floating-point amplitude multipliers 
        /// between -2^24 and 2^24, with a maximum 
        /// gain of 144.5 dB. A volume level of 1.0 means there is no attenuation or gain and 0 means silence. 
        /// Negative levels can be used to invert the audio's phase.
        /// </remarks>
        public unsafe int GetChannelVolumesNative(int channelCount, float[] volumes)
        {
            fixed (void* p = &volumes[0])
            {
                return InteropCalls.CallI(_basePtr, channelCount, p, ((void**) (*(void**) _basePtr))[15]);
            }
        }

        /// <summary>
        /// Returns the volume levels for the voice, per channel. 
        /// These settings are applied after the effect chain is applied. 
        /// This method is valid only for source and submix voices, because mastering voices do not specify volume per channel.
        /// </summary>
        /// <param name="channelCount">Confirms the channel count of the voice.</param>
        /// <returns>Returns the current volume level of each channel in the voice. The has at least <see cref="channelCount"/> elements.</returns>
        /// <remarks>
        /// The master volume level is applied at different times depending on the type of voice. 
        /// For submix and mastering voices the volume level is applied just before the voice's built in filter and effect chain is applied. 
        /// For source voices the master volume level is applied after the voice's filter and effect 
        /// chain is applied. Volume levels are expressed as floating-point amplitude multipliers 
        /// between -2^24 and 2^24, with a maximum 
        /// gain of 144.5 dB. A volume level of 1.0 means there is no attenuation or gain and 0 means silence. 
        /// Negative levels can be used to invert the audio's phase.
        /// </remarks>
        public float[] GetChannelVolumes(int channelCount)
        {
            if(channelCount <= 0)
                throw new ArgumentOutOfRangeException("channelCount");
            float[] volumes = new float[channelCount];
            XAudio2Exception.Try(GetChannelVolumesNative(channelCount, volumes), n, "GetChannelVolumes");
            return volumes;
        }

        /// <summary>
        /// Sets the volume level of each channel of the final output for the voice. These channels are mapped to the input channels of a specified destination voice.
        /// </summary>
        /// <param name="destinationVoice">
        /// Destination <see cref="XAudio2Voice"/> for which to set volume levels.
        /// If the voice sends to a single target voice then specifying null will cause SetOutputMatrix to operate on that target voice.
        /// </param>
        /// <param name="sourceChannels">Confirms the output channel count of the voice. This is the number of channels that are produced by the last effect in the chain.</param>
        /// <param name="destinationChannels">Confirms the input channel count of the destination voice.</param>
        /// <param name="levelMatrix">
        /// Array of [SourceChannels � DestinationChannels] volume levels sent to the destination voice. 
        /// The level sent from source channel S to destination channel D is specified in the form levelMatrix[SourceChannels � D + S]. 
        /// For more details see http://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.ixaudio2voice.ixaudio2voice.setoutputmatrix(v=vs.85).aspx.
        /// </param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        /// <returns>HRESULT</returns>
        public unsafe int SetOutputMatrixNative(XAudio2Voice destinationVoice, int sourceChannels,
            int destinationChannels, float[] levelMatrix, int operationSet)
        {
            IntPtr pVoice = destinationVoice == null ? IntPtr.Zero : destinationVoice.BasePtr;
            fixed (void* p = &levelMatrix[0])
            {
                return InteropCalls.CallI(_basePtr, pVoice.ToPointer(), sourceChannels, destinationChannels, p,
                    operationSet, ((void**) (*(void**) _basePtr))[16]);
            }
        }

        /// <summary>
        /// Sets the volume level of each channel of the final output for the voice. These channels are mapped to the input channels of a specified destination voice.
        /// </summary>
        /// <param name="destinationVoice">
        /// Destination <see cref="XAudio2Voice"/> for which to set volume levels.
        /// If the voice sends to a single target voice then specifying null will cause SetOutputMatrix to operate on that target voice.
        /// </param>
        /// <param name="sourceChannels">Confirms the output channel count of the voice. This is the number of channels that are produced by the last effect in the chain.</param>
        /// <param name="destinationChannels">Confirms the input channel count of the destination voice.</param>
        /// <param name="levelMatrix">
        /// Array of [SourceChannels � DestinationChannels] volume levels sent to the destination voice. 
        /// The level sent from source channel S to destination channel D is specified in the form levelMatrix[SourceChannels � D + S]. 
        /// For more details see http://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.ixaudio2voice.ixaudio2voice.setoutputmatrix(v=vs.85).aspx.
        /// </param>
        /// <param name="operationSet">Identifies this call as part of a deferred batch. For more information see http://msdn.microsoft.com/en-us/library/windows/desktop/ee415807(v=vs.85).aspx. </param>
        public void SetOutputMatrix(XAudio2Voice destinationVoice, int sourceChannels,
            int destinationChannels, float[] levelMatrix, int operationSet)
        {
            int result = SetOutputMatrixNative(destinationVoice, sourceChannels, destinationChannels, levelMatrix,
                operationSet);
            XAudio2Exception.Try(result, n, "SetOutputMatrix");
        }

        /// <summary>
        /// Sets the volume level of each channel of the final output for the voice. These channels are mapped to the input channels of a specified destination voice.
        /// </summary>
        /// <param name="destinationVoice">
        /// Destination <see cref="XAudio2Voice"/> for which to set volume levels.
        /// If the voice sends to a single target voice then specifying null will cause SetOutputMatrix to operate on that target voice.
        /// </param>
        /// <param name="sourceChannels">Confirms the output channel count of the voice. This is the number of channels that are produced by the last effect in the chain.</param>
        /// <param name="destinationChannels">Confirms the input channel count of the destination voice.</param>
        /// <param name="levelMatrix">
        /// Array of [SourceChannels � DestinationChannels] volume levels sent to the destination voice. 
        /// The level sent from source channel S to destination channel D is specified in the form levelMatrix[SourceChannels � D + S]. 
        /// For more details see http://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.ixaudio2voice.ixaudio2voice.setoutputmatrix(v=vs.85).aspx.
        /// </param>
        public void SetOutputMatrix(XAudio2Voice destinationVoice, int sourceChannels,
            int destinationChannels, float[] levelMatrix)
        {
            SetOutputMatrix(destinationVoice, sourceChannels, destinationChannels, levelMatrix, 0);
        }

        /// <summary>
        /// Gets the volume level of each channel of the final output for the voice. These channels are mapped to the input channels of a specified destination voice.
        /// </summary>
        /// <param name="destinationVoice">The destination <see cref="XAudio2Voice"/> to retrieve the output matrix for.</param>
        /// <param name="sourceChannels">Confirms the output channel count of the voice. This is the number of channels that are produced by the last effect in the chain.</param>
        /// <param name="destinationChannels">Confirms the input channel count of the destination voice.</param>
        /// <param name="levelMatrix">
        /// Array of [SourceChannels � DestinationChannels] volume levels sent to the destination voice. 
        /// The level sent from source channel S to destination channel D is specified in the form levelMatrix[SourceChannels � D + S]. 
        /// For more details see http://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.ixaudio2voice.ixaudio2voice.getoutputmatrix(v=vs.85).aspx.
        /// </param>
        /// <returns>HRESULT</returns>
        public unsafe int GetOutputMatrixNative(XAudio2Voice destinationVoice, int sourceChannels,
            int destinationChannels, float[] levelMatrix)
        {
            IntPtr pVoice = destinationVoice == null ? IntPtr.Zero : destinationVoice.BasePtr;
            fixed (void* p = &levelMatrix[0])
            {
                return InteropCalls.CallI(_basePtr, (void*)pVoice, sourceChannels, destinationChannels, p,
                    ((void**) (*(void**) _basePtr))[17]);
            }
        }

        /// <summary>
        /// Gets the volume level of each channel of the final output for the voice. These channels are mapped to the input channels of a specified destination voice.
        /// </summary>
        /// <param name="destinationVoice">The destination <see cref="XAudio2Voice"/> to retrieve the output matrix for.</param>
        /// <param name="sourceChannels">Confirms the output channel count of the voice. This is the number of channels that are produced by the last effect in the chain.</param>
        /// <param name="destinationChannels">Confirms the input channel count of the destination voice.</param>
        /// <param name="levelMatrix">
        /// Array of [SourceChannels � DestinationChannels] volume levels sent to the destination voice. 
        /// The level sent from source channel S to destination channel D is specified in the form levelMatrix[SourceChannels � D + S]. 
        /// For more details see http://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.ixaudio2voice.ixaudio2voice.getoutputmatrix(v=vs.85).aspx.
        /// </param>

        public void GetOutputMatrix(XAudio2Voice destinationVoice, int sourceChannels,
            int destinationChannels, float[] levelMatrix)
        {
            int result = GetOutputMatrixNative(destinationVoice, sourceChannels, destinationChannels, levelMatrix);
            XAudio2Exception.Try(result, n, "GetOutputMatrix");
        }

        /// <summary>
        /// Destroys the voice. If necessary, stops the voice and removes it from the XAudio2 graph.
        /// </summary>
        public unsafe void DestroyVoice()
        {
            InteropCalls.CallI2(_basePtr, ((void**) (*(void**) _basePtr))[18]);
        }
    }
}