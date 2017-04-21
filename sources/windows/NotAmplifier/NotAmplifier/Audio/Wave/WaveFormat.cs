using System;
using System.Runtime.InteropServices;

using Audio.Wave;

namespace Audio.Wave
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
    public class WaveFormat
    {
        protected readonly WaveFormatEncoding formatTag;
        protected readonly short  channels;
        protected int             samplesPerSec;
        protected int             avgBytesPerSec;
        protected short           blockAlign;
        protected short           bitsPerSample;
        protected short           cbSize;
        

        public WaveFormatEncoding FormatTag => formatTag;
        public int Channels => channels;
        public int AverageBytesPerSecond => avgBytesPerSec;
        public virtual short BlockAlign => blockAlign;
        public int ExtraSize => cbSize;

        public virtual int SampleRate
        {
            get
            {
                return samplesPerSec;
            }
            set
            {
                samplesPerSec  = value;
                avgBytesPerSec = samplesPerSec * blockAlign;
            }
        }

        public virtual short BitsPerSample
        {
            get
            {
                return bitsPerSample;
            }
            set
            {
                bitsPerSample  = value;
                blockAlign     = (short)(channels * (BitsPerSample / 8));
                avgBytesPerSec = samplesPerSec * blockAlign;
            }
        }

        /// <summary>
        /// Returns the encoding type used
        /// </summary>
        public WaveFormatEncoding Encoding => formatTag;

        /// <summary>
        /// Helper function to retrieve a WaveFormat structure from a pointer
        /// </summary>
        /// <param name="pointer">WaveFormat structure</param>
        /// <returns></returns>
        public static WaveFormat MarshalFromPtr(IntPtr pointer)
        {
            WaveFormat waveFormat = (WaveFormat)Marshal.PtrToStructure(pointer, typeof(WaveFormat));
            switch (waveFormat.Encoding)
            {
                case WaveFormatEncoding.Pcm:
                    // can't rely on extra size even being there for PCM so blank it to avoid reading
                    // corrupt data
                    waveFormat.cbSize = 0;
                    break;
                case WaveFormatEncoding.Extensible:
                    waveFormat = (WaveFormatExtensible)Marshal.PtrToStructure(pointer, typeof(WaveFormatExtensible));
                    break;
                default:
                    break;
            }
            return waveFormat;
        }

        /// <summary>
        /// Helper function to marshal WaveFormat to an IntPtr
        /// </summary>
        /// <param name="format">WaveFormat</param>
        /// <returns>IntPtr to WaveFormat structure (needs to be freed by callee)</returns>
        public static IntPtr MarshalToPtr(WaveFormat format)
        {
            int formatSize = Marshal.SizeOf(format);
            IntPtr formatPointer = Marshal.AllocHGlobal(formatSize);
            Marshal.StructureToPtr(format, formatPointer, false);
            return formatPointer;
        }

        protected WaveFormat()
        {

        }

        public WaveFormat(ESampleRate rate, EBitDepth bits, ESpeakerConfiguration channelMask)
            : this(rate, bits, channelMask, WaveFormatEncoding.Pcm, Marshal.SizeOf(typeof(WaveFormat)))
        {

        }

        protected WaveFormat(ESampleRate rate, EBitDepth bits, ESpeakerConfiguration channelMask, WaveFormatEncoding formatTag, int totalSize)
        {
            channels = ChannelsFromMask((int)channelMask);

            if (channels < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(channelMask), "Channels must be 1 or greater");
            }

            SampleRate    = (int)rate;
            BitsPerSample = (short)bits;
            cbSize        = 0;
            //blockAlign     = (short)(channels * (bitsPerSample / 8));
            //avgBytesPerSec = samplesPerSec * blockAlign;

            this.formatTag = formatTag;
            cbSize = (short)(totalSize - Marshal.SizeOf(typeof(WaveFormat)));
        }

        private short ChannelsFromMask(int channelMask)
        {
            short count = 0;

            // until all bits are zero
            while (channelMask > 0)
            {
                // check lower bit
                if ((channelMask & 1) == 1)
                    count++;

                // shift bits, removing lower bit
                channelMask >>= 1;
            }

            return count;
        }

        public override string ToString()
        {
            switch (formatTag)
            {
                case WaveFormatEncoding.Pcm:
                case WaveFormatEncoding.Extensible:
                    // formatTag just has some extra bits after the PCM header
                    return $"{{ {bitsPerSample} bit PCM: {samplesPerSec / 1000}kHz {channels} channels }}";
                default:
                    return formatTag.ToString();
            }
        }
    }
}
