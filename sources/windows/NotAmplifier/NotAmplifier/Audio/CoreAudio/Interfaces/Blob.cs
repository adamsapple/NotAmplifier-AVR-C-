using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audio.CoreAudio.Interfaces
{
    internal struct Blob
    {
        public int Length;
        public IntPtr Data;

        Blob(bool whoCares)
        {
            Length = 1;
            Data = IntPtr.Zero;
        }
    }
}
