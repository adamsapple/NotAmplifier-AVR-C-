using System;
using System.Runtime.InteropServices;


namespace Audio.CoreAudio.Interfaces
{
    [Guid(ComIIds.IID_AUDIO_CAPTURE_CLIENT)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    interface IAudioCaptureClient
    {
        /*HRESULT GetBuffer(
            BYTE** ppData,
            UINT32* pNumFramesToRead,
            DWORD* pdwFlags,
            UINT64* pu64DevicePosition,
            UINT64* pu64QPCPosition
            );*/

        int GetBuffer(
            out IntPtr dataBuffer,
            out int    numFramesToRead,
            out EAudioClientBufferFlags bufferFlags,
            out long   devicePosition,
            out long   qpcPosition);

        int ReleaseBuffer(int numFramesRead);

        int GetNextPacketSize(out int numFramesInNextPacket);

    }
}
