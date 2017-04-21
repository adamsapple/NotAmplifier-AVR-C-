using System;
using System.Runtime.InteropServices;

namespace Audio.CoreAudio.Interfaces
{
    [Guid(ComIIds.IID_AUDIO_METER_INFORMATION), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioMeterInformation
    {
        [PreserveSig]
        int GetPeakValue([Out] out float pfPeak);
        [PreserveSig]
        int GetMeteringChannelCount([Out] out int pnChannelCount);
        [PreserveSig]
        int GetChannelsPeakValues([In] int u32ChannelCount, [Out] IntPtr afPeakValues);
        [PreserveSig]
        int QueryHardwareSupport([Out] out int pdwHardwareSupportMask);
    };
}