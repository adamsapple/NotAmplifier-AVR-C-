using System;
using System.Runtime.InteropServices;

namespace Audio.CoreAudio.Interfaces
{
    [Guid(ComIIds.IID_AUDIO_RENDER_CLIENT)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    interface IAudioRenderClient
    {
        int GetBuffer(int numFramesRequested, out IntPtr dataBufferPointer);
        int ReleaseBuffer(int numFramesWritten, EAudioClientBufferFlags bufferFlags);
    }


}
