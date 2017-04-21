using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace Audio.CoreAudio
{
    [Flags]
    public enum EDeviceState
    {
        Active     = 0x00000001,
        Disabled   = 0x00000002,
        NotPresent = 0x00000004,
        Unplugged  = 0x00000008,
        All        = Active | Disabled| NotPresent| Unplugged,
    }
}
