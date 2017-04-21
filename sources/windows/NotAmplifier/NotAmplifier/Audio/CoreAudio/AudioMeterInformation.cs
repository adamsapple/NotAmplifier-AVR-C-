using System;
using System.Runtime.InteropServices;

using Audio.CoreAudio.Interfaces;

namespace Audio.CoreAudio
{
    public class AudioMeterInformation
    {
        internal IAudioMeterInformation _AudioMeterInformation;

        internal AudioMeterInformation(IAudioMeterInformation meter)
        {
            _AudioMeterInformation = meter;
        }

        public void Dispose()
        {
            _AudioMeterInformation = null;
        }

        private float GetPeakValue()
        {
            float result;
            Marshal.ThrowExceptionForHR(_AudioMeterInformation.GetPeakValue(out result));
            return result;
        }

        private int GetMeteringChannelCount()
        {
            int result;
            Marshal.ThrowExceptionForHR(_AudioMeterInformation.GetMeteringChannelCount(out result));
            return result;
        }
        
        public float PeakValue
        {
            get
            {
                return GetPeakValue();
            }
        }

        public int MeteringChannelCount
        {
            get
            {
                return GetMeteringChannelCount();
            }
        }

        public int QueryHardwareSupport()
        {
            int result;
            Marshal.ThrowExceptionForHR(_AudioMeterInformation.QueryHardwareSupport(out result));
            return result;
        }

        
        public float[] GetChannelsPeakValues()
        {
            var result = new float[MeteringChannelCount];
            var Params = GCHandle.Alloc(result, GCHandleType.Pinned);
            Marshal.ThrowExceptionForHR(_AudioMeterInformation.GetChannelsPeakValues(result.Length, Params.AddrOfPinnedObject()));
            Params.Free();
            return result;
        }

        public virtual void Dispose(bool disposing)
        {
            //_AudioMeterInformation = null;
        }

        ~AudioMeterInformation()
        {
            Dispose(false);
        }

        /*
    [PreserveSig]
    int GetChannelsPeakValues(int u32ChannelCount, [In] IntPtr afPeakValues);
    [PreserveSig]
    int QueryHardwareSupport(out int pdwHardwareSupportMask);
    */
    }
}
