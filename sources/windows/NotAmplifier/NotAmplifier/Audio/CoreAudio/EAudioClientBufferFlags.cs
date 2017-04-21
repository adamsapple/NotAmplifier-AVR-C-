using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audio.CoreAudio
{
    /// <summary>
    /// Audio Client Buffer Flags
    /// </summary>
    [Flags]
    public enum  EAudioClientBufferFlags
    {
        None,
        /// <summary>
        /// AUDCLNT_BUFFERFLAGS_DATA_DISCONTINUITY
        /// </summary>
        DataDiscontinuity    = 0x1,
        /// <summary>
        /// AUDCLNT_BUFFERFLAGS_SILENT
        /// </summary>
        Silent               = 0x2,
        /// <summary>
        /// AUDCLNT_BUFFERFLAGS_TIMESTAMP_ERROR
        /// </summary>
        TimestampError       = 0x4
    }
}
