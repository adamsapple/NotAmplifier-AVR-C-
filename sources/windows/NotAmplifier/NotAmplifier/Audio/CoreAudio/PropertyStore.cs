using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Audio.CoreAudio.Interfaces;

namespace Audio.CoreAudio
{
    public class PropertyStore
    {
        private IPropertyStore _property;

        internal PropertyStore(IPropertyStore property)
        {
            _property = property;
        }

        private int GetCount()
        {
            uint result;
            Marshal.ThrowExceptionForHR(_property.GetCount(out result));
            return (int)result;

        }

        private PropertyKey GetAt(uint index)
        {
            PropertyKey result;
            Marshal.ThrowExceptionForHR(_property.GetAt(index, out result));
            return result;
        }

        private PropVariant GetValue(PropertyKey propertyKey)
        {
            PropVariant result;
            Marshal.ThrowExceptionForHR(_property.GetValue(ref propertyKey, out result));
            return result;
        }

        private void SetValue(PropertyKey propertyKey, object value)
        {
            Marshal.ThrowExceptionForHR(_property.SetValue(ref propertyKey, ref value));
        }

        private void Commit()
        {
            Marshal.ThrowExceptionForHR(_property.Commit());
        }

        public bool Contains(PropertyKey compareKey)
        {
            for (int i=0,len=Count; i<len; ++i)
            {
                var key = GetAt((uint)i);
                if (key.FormatId == compareKey.FormatId && key.PropertyId == compareKey.PropertyId)
                {
                    return true;
                }
            }
            return false;
        }

        public PropVariant this[PropertyKey queryKey]
        {
            get
            {
                for (int i=0,len=Count; i<len; ++i)
                {
                    var key = GetAt((uint)i);
                    if (key.FormatId != queryKey.FormatId || key.PropertyId != queryKey.PropertyId)
                    {
                        continue;
                    }

                    PropVariant result;
                    Marshal.ThrowExceptionForHR(_property.GetValue(ref key, out result));
                    return result;
                }
                return new PropVariant();
            }
        }

        public int Count
        {
            get
            {
                return GetCount();
            }
        }
    }
}
