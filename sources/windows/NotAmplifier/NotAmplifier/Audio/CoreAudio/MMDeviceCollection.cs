using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Audio.CoreAudio.Interfaces;

namespace Audio.CoreAudio
{
    public class MMDeviceCollection
    {
        private IMMDeviceCollection _realDeviceCollection;

        internal MMDeviceCollection(IMMDeviceCollection deviceCollection)
        {
            _realDeviceCollection = deviceCollection;
        }

        private int GetCount()
        {
            int result;
            Marshal.ThrowExceptionForHR(_realDeviceCollection.GetCount(out result));
            return result;
        }

        private MMDevice GetItem(uint index)
        {
            IMMDevice idevice;
            Marshal.ThrowExceptionForHR(_realDeviceCollection.Item(index, out idevice));
            //return new MMDevice(device);
            return MMDeviceEnumerator.GetInstance().CreateMMDevice(idevice);
        }

        public int Count
        {
            get
            {
                return GetCount();
            }
        }

        public MMDevice this[int index]
        {
            get
            {
                return GetItem((uint)index);
            }
        }
    }
}
