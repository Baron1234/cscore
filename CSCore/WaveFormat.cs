﻿using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CSCore
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class WaveFormat
    {
        protected AudioEncoding _encoding;
        protected short _channels;
        protected int _sampleRate;
        protected int _bytesPerSecond;

        protected short blockAlign;
        protected short bitsPerSample;
        protected short extraSize;

        public short Channels
        {
            get { return _channels; }
        }

        public int SampleRate
        {
            get { return _sampleRate; }
        }

        public int BytesPerSecond
        {
            get { return _bytesPerSecond; }
        }

        /// <summary>
        /// Frame-Size = [channels>] * (( [bits/sample]+7) / 8)
        /// </summary>
        public int BlockAlign
        {
            get { return blockAlign; }
        }

        public short BitsPerSample
        {
            get { return bitsPerSample; }
        }

        public int ExtraSize
        {
            get { return extraSize; }
            internal set { extraSize = (short)value; }
        }

        public int BytesPerSample
        {
            get { return BitsPerSample / 8; }
        }

        public AudioEncoding WaveFormatTag { get { return _encoding; } }

        /// <summary>
        /// 44100Hz, 16bps, 2 channels, pcm
        /// </summary>
        public WaveFormat()
            : this(44100, 16, 2)
        {
        }

        public WaveFormat(WaveFormat waveFormat, int sampleRate)
            : this(sampleRate, waveFormat.BitsPerSample, waveFormat.Channels, waveFormat._encoding)
        {
        }

        public WaveFormat(int sampleRate, int bits, int channels)
            : this(sampleRate, bits, channels, AudioEncoding.Pcm)
        {
        }

        public WaveFormat(int sampleRate, int bits, int channels, AudioEncoding encoding)
            : this(sampleRate, bits, channels, encoding, 0)
        {
        }

        public WaveFormat(int sampleRate, int bits, int channels, AudioEncoding encoding, int extraSize)
        {
            if (sampleRate < 1)
                throw new ArgumentOutOfRangeException("sampleRate");
            if (bits < 0)
                throw new ArgumentOutOfRangeException("bits");
            if (channels < 1)
                throw new ArgumentOutOfRangeException("Channels must be > 0");

            this._sampleRate = sampleRate;
            this.bitsPerSample = (short)bits;
            this._channels = (short)channels;
            this._encoding = encoding;
            this.blockAlign = (short)(channels * (bits / 8));
            this._bytesPerSecond = (sampleRate * blockAlign);
            this.ExtraSize = (short)extraSize;
        }

        public long MillisecondsToBytes(long milliseconds)
        {
            long result = (BytesPerSecond / 1000) * milliseconds;
            result -= result % BlockAlign;
            return result;
        }

        public long BytesToMilliseconds(long bytes)
        {
            bytes -= bytes % BlockAlign;
            long result = (bytes / BytesPerSecond) * 1000;
            return result;
        }

        public override string ToString()
        {
            return GetInformation().ToString();
        }

        protected StringBuilder GetInformation()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("ChannelsAvailable: " + Channels);
            builder.Append("|SampleRate: " + SampleRate);
            builder.Append("|Bps: " + BytesPerSecond);
            builder.Append("|BlockAlign: " + BlockAlign);
            builder.Append("|BitsPerSample: " + BitsPerSample);
            builder.Append("|Encoding: " + _encoding);

            return builder;
        }
    }

    public enum AudioEncoding : ushort
    {
        Unknown = 0x0000,
        Pcm = 0x0001,
        Adpcm = 0x0002,
        IeeeFloat = 0x0003,
        Vselp = 0x0004,
        IbmCvsd = 0x0005,
        ALaw = 0x0006,
        MuLaw = 0x0007,
        Dts = 0x0008,
        Drm = 0x0009,
        OkiAdpcm = 0x0010,
        DviAdpcm = 0x0011,
        ImaAdpcm = DviAdpcm,
        MediaspaceAdpcm = 0x0012,
        SierraAdpcm = 0x0013,
        G723Adpcm = 0x0014,
        DigiStd = 0x0015,
        DigiFix = 0x0016,
        DialogicOkiAdpcm = 0x0017,
        MediaVisionAdpcm = 0x0018,
        CUCodec = 0x0019,
        YamahaAdpcm = 0x0020,
        SonarC = 0x0021,
        DspGroupTrueSpeech = 0x0022,
        EchoSpeechCorporation1 = 0x0023,
        AudioFileAf36 = 0x0024,
        Aptx = 0x0025,
        AudioFileAf10 = 0x0026,
        Prosody1612 = 0x0027,
        Lrc = 0x0028,
        DolbyAc2 = 0x0030,
        Gsm610 = 0x0031,
        MsnAudio = 0x0032,
        AntexAdpcme = 0x0033,
        ControlResVqlpc = 0x0034,
        DigiReal = 0x0035,
        DigiAdpcm = 0x0036,
        ControlResCr10 = 0x0037,
        WAVE_FORMAT_NMS_VBXADPCM = 0x0038, // Natural MicroSystems
        WAVE_FORMAT_CS_IMAADPCM = 0x0039, // Crystal Semiconductor IMA ADPCM
        WAVE_FORMAT_ECHOSC3 = 0x003A, // Echo Speech Corporation
        WAVE_FORMAT_ROCKWELL_ADPCM = 0x003B, // Rockwell International
        WAVE_FORMAT_ROCKWELL_DIGITALK = 0x003C, // Rockwell International
        WAVE_FORMAT_XEBEC = 0x003D, // Xebec Multimedia Solutions Limited
        WAVE_FORMAT_G721_ADPCM = 0x0040, // Antex Electronics Corporation
        WAVE_FORMAT_G728_CELP = 0x0041, // Antex Electronics Corporation
        WAVE_FORMAT_MSG723 = 0x0042, // Microsoft Corporation
        Mpeg = 0x0050, // WAVE_FORMAT_MPEG, Microsoft Corporation
        WAVE_FORMAT_RT24 = 0x0052, // InSoft, Inc.
        WAVE_FORMAT_PAC = 0x0053, // InSoft, Inc.
        MpegLayer3 = 0x0055, // WAVE_FORMAT_MPEGLAYER3, ISO/MPEG Layer3 Format Tag
        WAVE_FORMAT_LUCENT_G723 = 0x0059, // Lucent Technologies
        WAVE_FORMAT_CIRRUS = 0x0060, // Cirrus Logic
        WAVE_FORMAT_ESPCM = 0x0061, // ESS Technology
        WAVE_FORMAT_VOXWARE = 0x0062, // Voxware Inc
        WAVE_FORMAT_CANOPUS_ATRAC = 0x0063, // Canopus, co., Ltd.
        WAVE_FORMAT_G726_ADPCM = 0x0064, // APICOM
        WAVE_FORMAT_G722_ADPCM = 0x0065, // APICOM
        WAVE_FORMAT_DSAT_DISPLAY = 0x0067, // Microsoft Corporation
        WAVE_FORMAT_VOXWARE_BYTE_ALIGNED = 0x0069, // Voxware Inc
        WAVE_FORMAT_VOXWARE_AC8 = 0x0070, // Voxware Inc
        WAVE_FORMAT_VOXWARE_AC10 = 0x0071, // Voxware Inc
        WAVE_FORMAT_VOXWARE_AC16 = 0x0072, // Voxware Inc
        WAVE_FORMAT_VOXWARE_AC20 = 0x0073, // Voxware Inc
        WAVE_FORMAT_VOXWARE_RT24 = 0x0074, // Voxware Inc
        WAVE_FORMAT_VOXWARE_RT29 = 0x0075, // Voxware Inc
        WAVE_FORMAT_VOXWARE_RT29HW = 0x0076, // Voxware Inc
        WAVE_FORMAT_VOXWARE_VR12 = 0x0077, // Voxware Inc
        WAVE_FORMAT_VOXWARE_VR18 = 0x0078, // Voxware Inc
        WAVE_FORMAT_VOXWARE_TQ40 = 0x0079, // Voxware Inc
        WAVE_FORMAT_SOFTSOUND = 0x0080, // Softsound, Ltd.
        WAVE_FORMAT_VOXWARE_TQ60 = 0x0081, // Voxware Inc
        WAVE_FORMAT_MSRT24 = 0x0082, // Microsoft Corporation
        WAVE_FORMAT_G729A = 0x0083, // AT&T Labs, Inc.
        WAVE_FORMAT_MVI_MVI2 = 0x0084, // Motion Pixels
        WAVE_FORMAT_DF_G726 = 0x0085, // DataFusion Systems (Pty) (Ltd)
        WAVE_FORMAT_DF_GSM610 = 0x0086, // DataFusion Systems (Pty) (Ltd)
        WAVE_FORMAT_ISIAUDIO = 0x0088, // Iterated Systems, Inc.
        WAVE_FORMAT_ONLIVE = 0x0089, // OnLive! Technologies, Inc.
        WAVE_FORMAT_SBC24 = 0x0091, // Siemens Business Communications Sys
        WAVE_FORMAT_DOLBY_AC3_SPDIF = 0x0092, // Sonic Foundry
        WAVE_FORMAT_MEDIASONIC_G723 = 0x0093, // MediaSonic
        WAVE_FORMAT_PROSODY_8KBPS = 0x0094, // Aculab plc
        WAVE_FORMAT_ZYXEL_ADPCM = 0x0097, // ZyXEL Communications, Inc.
        WAVE_FORMAT_PHILIPS_LPCBB = 0x0098, // Philips Speech Processing
        WAVE_FORMAT_PACKED = 0x0099, // Studer Professional Audio AG
        WAVE_FORMAT_MALDEN_PHONYTALK = 0x00A0, // Malden Electronics Ltd.
        Gsm = 0x00A1,
        G729 = 0x00A2,
        G723 = 0x00A3,
        Acelp = 0x00A4,
        WAVE_FORMAT_RHETOREX_ADPCM = 0x0100, // Rhetorex Inc.
        WAVE_FORMAT_IRAT = 0x0101, // BeCubed Software Inc.
        WAVE_FORMAT_VIVO_G723 = 0x0111, // Vivo Software
        WAVE_FORMAT_VIVO_SIREN = 0x0112, // Vivo Software
        WAVE_FORMAT_DIGITAL_G723 = 0x0123, // Digital Equipment Corporation
        WAVE_FORMAT_SANYO_LD_ADPCM = 0x0125, // Sanyo Electric Co., Ltd.
        WAVE_FORMAT_SIPROLAB_ACEPLNET = 0x0130, // Sipro Lab Telecom Inc.
        WAVE_FORMAT_SIPROLAB_ACELP4800 = 0x0131, // Sipro Lab Telecom Inc.
        WAVE_FORMAT_SIPROLAB_ACELP8V3 = 0x0132, // Sipro Lab Telecom Inc.
        WAVE_FORMAT_SIPROLAB_G729 = 0x0133, // Sipro Lab Telecom Inc.
        WAVE_FORMAT_SIPROLAB_G729A = 0x0134, // Sipro Lab Telecom Inc.
        WAVE_FORMAT_SIPROLAB_KELVIN = 0x0135, // Sipro Lab Telecom Inc.
        WAVE_FORMAT_G726ADPCM = 0x0140, // Dictaphone Corporation
        WAVE_FORMAT_QUALCOMM_PUREVOICE = 0x0150, // Qualcomm, Inc.
        WAVE_FORMAT_QUALCOMM_HALFRATE = 0x0151, // Qualcomm, Inc.
        WAVE_FORMAT_TUBGSM = 0x0155, // Ring Zero Systems, Inc.
        WAVE_FORMAT_MSAUDIO1 = 0x0160, // Microsoft Corporation
        WAVE_FORMAT_UNISYS_NAP_ADPCM = 0x0170, // Unisys Corp.
        WAVE_FORMAT_UNISYS_NAP_ULAW = 0x0171, // Unisys Corp.
        WAVE_FORMAT_UNISYS_NAP_ALAW = 0x0172, // Unisys Corp.
        WAVE_FORMAT_UNISYS_NAP_16K = 0x0173, // Unisys Corp.
        WAVE_FORMAT_CREATIVE_ADPCM = 0x0200, // Creative Labs, Inc
        WAVE_FORMAT_CREATIVE_FASTSPEECH8 = 0x0202, // Creative Labs, Inc
        WAVE_FORMAT_CREATIVE_FASTSPEECH10 = 0x0203, // Creative Labs, Inc
        WAVE_FORMAT_UHER_ADPCM = 0x0210, // UHER informatic GmbH
        WAVE_FORMAT_QUARTERDECK = 0x0220, // Quarterdeck Corporation
        WAVE_FORMAT_ILINK_VC = 0x0230, // I-link Worldwide
        WAVE_FORMAT_RAW_SPORT = 0x0240, // Aureal Semiconductor
        WAVE_FORMAT_ESST_AC3 = 0x0241, // ESS Technology, Inc.
        WAVE_FORMAT_IPI_HSX = 0x0250, // Interactive Products, Inc.
        WAVE_FORMAT_IPI_RPELP = 0x0251, // Interactive Products, Inc.
        WAVE_FORMAT_CS2 = 0x0260, // Consistent Software
        WAVE_FORMAT_SONY_SCX = 0x0270, // Sony Corp.
        WAVE_FORMAT_FM_TOWNS_SND = 0x0300, // Fujitsu Corp.
        WAVE_FORMAT_BTV_DIGITAL = 0x0400, // Brooktree Corporation
        WAVE_FORMAT_QDESIGN_MUSIC = 0x0450, // QDesign Corporation
        WAVE_FORMAT_VME_VMPCM = 0x0680, // AT&T Labs, Inc.
        WAVE_FORMAT_TPC = 0x0681, // AT&T Labs, Inc.
        WAVE_FORMAT_OLIGSM = 0x1000, // Ing C. Olivetti & C., S.p.A.
        WAVE_FORMAT_OLIADPCM = 0x1001, // Ing C. Olivetti & C., S.p.A.
        WAVE_FORMAT_OLICELP = 0x1002, // Ing C. Olivetti & C., S.p.A.
        WAVE_FORMAT_OLISBC = 0x1003, // Ing C. Olivetti & C., S.p.A.
        WAVE_FORMAT_OLIOPR = 0x1004, // Ing C. Olivetti & C., S.p.A.
        WAVE_FORMAT_LH_CODEC = 0x1100, // Lernout & Hauspie
        WAVE_FORMAT_NORRIS = 0x1400, // Norris Communications, Inc.
        WAVE_FORMAT_SOUNDSPACE_MUSICOMPRESS = 0x1500, // AT&T Labs, Inc.
        WAVE_FORMAT_DVM = 0x2000, // FAST Multimedia AG
        WAVE_FORMAT_RAW_AAC1 = 0x00FF, // Advanced Audio Coding (AAC).
        WAVE_FORMAT_MPEG_HEAAC = 0x1610, // Advanced Audio Coding (AAC).
        WAVE_FORMAT_WMAVOICE9 = 0x000A, 
        WAVE_FORMAT_WMASPDIF = 0x0164,
        WAVE_FORMAT_WMAUDIO_LOSSLESS = 0x0163,
        WAVE_FORMAT_WMAUDIO2 = 0x0161,
        WAVE_FORMAT_WMAUDIO3 = 0x0162,
        Extensible = 0xFFFE, // Microsoft
        WAVE_FORMAT_DEVELOPMENT = 0xFFFF
    }
}