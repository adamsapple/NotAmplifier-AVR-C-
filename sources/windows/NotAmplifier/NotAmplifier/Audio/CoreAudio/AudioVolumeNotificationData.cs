using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace Audio.CoreAudio
{

    public class AudioVolumeNotificationData
    {
        public Guid EventContext;

        //[MarshalAs(UnmanagedType.Bool)]
        public bool Muted;

        //[MarshalAs(UnmanagedType.R4)]
        public float MasterVolume;

        //[MarshalAs(UnmanagedType.U4)]
        public uint nChannels;

        //[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]

        public float[] ChannelVolumes;


        public void Initialize(IntPtr ptr)
        {
            var data = (AUDIO_VOLUME_NOTIFICATION_DATA)Marshal.PtrToStructure(ptr, typeof(AUDIO_VOLUME_NOTIFICATION_DATA));

            EventContext = data.EventContext;
            Muted = data.Muted;
            MasterVolume = data.MasterVolume;
            nChannels = data.nChannels;

            //Determine offset in structure of the first float
            var offset = Marshal.OffsetOf(typeof(AUDIO_VOLUME_NOTIFICATION_DATA), "ChannelVolumes");
            //Determine offset in memory of the first float
            var firstFloatPtr = (IntPtr)((long)ptr + (long)offset);
            float[] voldata = ChannelVolumes;

            if (voldata == null || data.nChannels > voldata.Length)
            {
                voldata = new float[data.nChannels];
            }

            //Read all floats from memory.
            for (var i = 0; i < data.nChannels; i++)
            {
                voldata[i] = (float)Marshal.PtrToStructure(firstFloatPtr, typeof(float));
            }

            ChannelVolumes = voldata;
        }

        public void Initialize(MMDevice device)
        {
            EventContext = Guid.Empty;
            MasterVolume = device.AudioEndpointVolume.MasterVolumeLevelScalar;
            Muted        = device.AudioEndpointVolume.Mute;
            nChannels    = (uint)device.AudioMeterInformation.MeteringChannelCount;

            float[] voldata = ChannelVolumes;
            if (voldata == null || nChannels > voldata.Length)
            {
                voldata = new float[nChannels];
            }

            //Read all floats from memory.
            for (var i = 0; i < nChannels; i++)
            {
                voldata[i] = 0;
            }

            ChannelVolumes = voldata;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AUDIO_VOLUME_NOTIFICATION_DATA
    {
        public Guid EventContext;

        //[MarshalAs(UnmanagedType.Bool)]
        public bool Muted;

        //[MarshalAs(UnmanagedType.R4)]
        public float MasterVolume;

        //[MarshalAs(UnmanagedType.U4)]
        public uint nChannels;

        //[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]
        public float ChannelVolumes;

        AUDIO_VOLUME_NOTIFICATION_DATA(bool whoCares=false)
        {
            EventContext = Guid.Empty;
            Muted = whoCares;
            MasterVolume = 0;
            nChannels = 0;
            ChannelVolumes = 0;
        }
    }
}
