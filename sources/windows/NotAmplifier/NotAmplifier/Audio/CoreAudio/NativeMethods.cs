using System;
using System.Runtime.InteropServices;

namespace Audio.CoreAudio
{
    // http://www.codeproject.com/Articles/460145/Recording-and-playing-PCM-audio-on-Windows-VB?msg=4373191#xx4373191xx

    public static class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = false, PreserveSig = true, SetLastError = true)]
        public static extern IntPtr CreateEventEx(IntPtr lpEventAttributes, IntPtr lpName, int dwFlags, EEventAccess dwDesiredAccess);

        [DllImport("kernel32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        public static extern int WaitForSingleObjectEx(IntPtr hEvent, int milliseconds, bool bAlertable);

        [DllImport("kernel32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        //[DllImport("Mmdevapi.dll", ExactSpelling = true, PreserveSig = false)]
        //public static extern void ActivateAudioInterfaceAsync([MarshalAs(UnmanagedType.LPWStr)] string deviceInterfacePath, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, IntPtr activationParams, IActivateAudioInterfaceCompletionHandler completionHandler, ref IActivateAudioInterfaceAsyncOperation activationOperation);
    }

    public enum EEventAccess
    {
        StandardRightsRequired = 0x000F0000,                                    // STANDARD_RIGHTS_REQUIRED
        Synchronize            = 0x00100000,                                    // SYNCHRONIZE
        EventAllAccess         = (StandardRightsRequired | Synchronize | 0x3),  // EVENT_ALL_ACCESS
        EventModifyState       = 0x0002,                                        // EVENT_MODIFY_STATE
    }
}
