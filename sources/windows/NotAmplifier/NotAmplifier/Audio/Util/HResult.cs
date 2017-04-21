using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audio.Util
{
    public enum HResult :int
    {
        S_OK = 0,
        S_FALSE = 1,
        E_INVALIDARG = unchecked((int)0x80000003),
    }

    public static class HResultUtil
    {
        public static uint MAKE_HRESULT(uint sev, uint fac, uint code)
        {
            return (uint)(((uint)sev) << 31 | ((uint)fac) << 16 | ((uint)code));
        }

        //public static int MAKE_SCODE(int sev, int fac, int code)
        //{
        //    return ((SCODE)(((unsigned long)(sev) << 31) | ((unsigned long)(fac) << 16) | ((unsigned long)(code))) )
        //}
    }
}
