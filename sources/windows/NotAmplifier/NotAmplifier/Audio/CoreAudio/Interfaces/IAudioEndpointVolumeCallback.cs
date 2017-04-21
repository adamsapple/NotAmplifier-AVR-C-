using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace Audio.CoreAudio.Interfaces
{
    [Guid(ComIIds.IID_AUDIO_ENDPOINT_VOLUME_CALLBACK)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolumeCallback
    {
        [PreserveSig]
        int OnNotify([In] IntPtr notificationData);  //PAUDIO_VOLUME_NOTIFICATION_DATA
    }
}
