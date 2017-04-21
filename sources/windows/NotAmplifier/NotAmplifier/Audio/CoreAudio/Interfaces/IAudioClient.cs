using System;
using System.Runtime.InteropServices;

using Audio.Wave;


namespace Audio.CoreAudio.Interfaces
{
    //[Guid(ComIIds.IID_AUDIO_CLIENT)]
    [Guid(ComIIds.IID_AUDIO_CLIENT2)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioClient
    {
        [PreserveSig]
        int Initialize(EAudioClientShareMode shareMode,
            [In] [MarshalAs(UnmanagedType.U4)] EAudioClientStreamFlags streamFlags,
            [In] [MarshalAs(UnmanagedType.I8)] long hnsBufferDuration,
            [In] [MarshalAs(UnmanagedType.I8)] long hnsPeriodicity,
            [In] WaveFormat pFormat,
            [In] ref Guid audioSessionGuid);

        [PreserveSig]
        int GetBufferSize([Out] [MarshalAs(UnmanagedType.U4)] out uint bufferSize);

        [PreserveSig]
        int GetStreamLatency([Out] [MarshalAs(UnmanagedType.I8)] out long bufferSize);

        [PreserveSig]
        int GetCurrentPadding(out int currentPadding);

        [PreserveSig]
        int IsFormatSupported(
            EAudioClientShareMode shareMode,
            [In] WaveFormat pFormat,
            [Out] out WaveFormatExtensible closestMatchFormat);

        [PreserveSig]
        int GetMixFormat([Out] out WaveFormat format);

        [PreserveSig]
        int GetDevicePeriod(
            [Out] [MarshalAs(UnmanagedType.I8)] out long defaultDevicePeriod,
            [Out] [MarshalAs(UnmanagedType.I8)]out long minimumDevicePeriod);

        [PreserveSig]
        int Start();

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int Reset();

        [PreserveSig]
        int SetEventHandle(IntPtr eventHandle);

        [PreserveSig]
        int GetService(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceId,
            [Out, MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);
    }
}
