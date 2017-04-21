using System;

namespace Audio.CoreAudio
{
    /** <summary>
     * 
     *  Defines constants that indicate the general physical attributes of an audio endpoint device.
     *  
     *  </summary>
     *  <remarks>
     *  MSDN Reference: http://msdn.microsoft.com/en-us/library/dd370830.aspx
     *       https://msdn.microsoft.com/en-us/library/windows/desktop/dd370830(v=vs.85).aspx
     *  </remarks>
    */
    public enum EndpointFormFactor : UInt32
    {
        RemoteNetworkDevice,
        Speakers,
        LineLevel,
        Headphones,
        Microphone,
        Headset,
        Handset,
        UnknownDigitalPassthrough,
        SPDIF,
        DigitalAudioDisplayDevice,
        UnknownFormFactor,
        EndpointFormFactor_enum_count
    };
}

