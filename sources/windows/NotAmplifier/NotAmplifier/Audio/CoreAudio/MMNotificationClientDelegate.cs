using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Audio.CoreAudio.Interfaces;

namespace Audio.CoreAudio
{
    public delegate void MMNotificationClientDeviceStateChangedDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
            [MarshalAs(UnmanagedType.U4)] EDeviceState newState);

    public delegate void MMNotificationClientDeviceAddedDelegate([MarshalAs(UnmanagedType.LPWStr)] string deviceId);

    public delegate void MMNotificationClientDeviceRemovedDelegate([MarshalAs(UnmanagedType.LPWStr)] string deviceId);

    public delegate void MMNotificationClientDefaultDeviceChangedDelegate(
        [MarshalAs(UnmanagedType.I4)] EDataFlow dataFlow,
        [MarshalAs(UnmanagedType.I4)] ERole deviceRole,
        [MarshalAs(UnmanagedType.LPWStr)] string defaultDeviceId);

    public delegate void MMNotificationClientPropertyValueChangedDelegate(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId, PropertyKey propertyKey);
}
