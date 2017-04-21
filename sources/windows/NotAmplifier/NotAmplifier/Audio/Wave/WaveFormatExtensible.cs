using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Audio.Wave;

namespace Audio.Wave
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
    public class WaveFormatExtensible : WaveFormat
    {
        private readonly int dwChannelMask;
        private readonly Guid subFormat;
        private short wValidBitsPerSample;

        public short ValidBitsPerSample => wValidBitsPerSample;

        public ESpeakerConfiguration ChannelMask => (ESpeakerConfiguration)dwChannelMask;

        public Guid SubFormat => subFormat;

        public override int SampleRate
        {
            get
            {
                return base.SampleRate;
            }
            set
            {
                base.SampleRate     = value;
                wValidBitsPerSample = (short)base.SampleRate;
            }
        }

        /// <summary>
        /// Parameterless constructor for marshalling
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        WaveFormatExtensible()
        {
        }

        /// <summary>
        /// Creates a new WaveFormatExtensible for PCM
        /// WMMEDIASUBTYPE_PCM("00000001-0000-0010-8000-00AA00389B71")
        /// </summary>
        public WaveFormatExtensible(ESampleRate rate, EBitDepth bits, ESpeakerConfiguration channelMask)
            : this(rate, bits, channelMask, new Guid("00000001-0000-0010-8000-00AA00389B71"))
        {
        }

        public WaveFormatExtensible(ESampleRate rate, EBitDepth bits, ESpeakerConfiguration channelMask, Guid subFormat)
            : base(rate, bits, channelMask, WaveFormatEncoding.Extensible, Marshal.SizeOf(typeof(WaveFormatExtensible)))
        {
            wValidBitsPerSample = (short)bits;
            dwChannelMask       = (int)channelMask;
            this.subFormat      = subFormat;
        }

        public override string ToString()
        {
            return
                $"{{ {base.ToString()} wBitsPerSample:{wValidBitsPerSample} dwChannelMask:{dwChannelMask} subFormat:{subFormat} extraSize:{ExtraSize} }}";
        }
    }
}
