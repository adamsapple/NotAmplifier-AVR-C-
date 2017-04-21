using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Audio.CoreAudio.Interfaces
{
    [Guid(ComIIds.IID_PROPERTY_STORE), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStore
    {
        [PreserveSig]
        int GetCount(
            [Out] [MarshalAs(UnmanagedType.U4)] out uint propertyCount);

        [PreserveSig]
        int GetAt(
            [In] [MarshalAs(UnmanagedType.U4)] uint propertyIndex,
            [Out] out PropertyKey propertyKey);

        [PreserveSig]
        int GetValue(
            [In]  ref PropertyKey propertyKey,
            [Out] out PropVariant value);

        [PreserveSig]
        int SetValue(
            [In] ref PropertyKey propertyKey,
            [In] ref object value);

        [PreserveSig]
        int Commit();
    }
}
