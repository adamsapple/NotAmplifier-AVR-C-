using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Audio.Wave;
using Audio.CoreAudio.Interfaces;

namespace Audio.CoreAudio
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// https://github.com/SjB/NAudio/tree/master/NAudio/CoreAudioApi
    /// </remarks>
    public class AudioClient :IDisposable
    {
        private IAudioClient _realAudioClient;

        private AudioRenderClient  _AudioRenderClient;
        private AudioCaptureClient _AudioCaptureClient;
        private WaveFormat mixFormat;

        public int BufferSize       => GetBufferSize();
        public WaveFormat MixFormat => mixFormat;

        public AudioRenderClient AudioRenderClient
        {
            get
            {
                if(_AudioRenderClient == null) GetAudioRenderClient();
                return _AudioRenderClient;
            }
        }

        public AudioCaptureClient AudioCaptureClient
        {
            get
            {
                if (_AudioCaptureClient == null) GetAudioCaptureClient();
                return _AudioCaptureClient;
            }
        }

        internal AudioClient(IAudioClient realAudioClient)
        {
            _realAudioClient = realAudioClient;
            GetMixFormat();
        }

        public void  Initialize(EAudioClientShareMode shareMode,
            EAudioClientStreamFlags streamFlags,
            long bufferDuration,
            long periodicity,
            WaveFormat format,
            Guid audioSessionGuid)
        {
            var hresult = _realAudioClient.Initialize(shareMode, streamFlags, bufferDuration, periodicity, format, ref audioSessionGuid);
            Marshal.ThrowExceptionForHR(hresult);
            // may have changed the mix format so reset it
            mixFormat = null;

        }
        
        private void GetMixFormat()
        {
            Marshal.ThrowExceptionForHR( _realAudioClient.GetMixFormat(out mixFormat) );
        }

        /// <summary>
        /// Determines whether if the specified output format is supported
        /// </summary>
        /// <param name="shareMode">The share mode.</param>
        /// <param name="desiredFormat">The desired format.</param>
        /// <returns>
        /// 	<c>true</c> if [is format supported] [the specified share mode]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFormatSupported(EAudioClientShareMode shareMode,
            WaveFormat desiredFormat)
        {
            WaveFormatExtensible closestMatchFormat;
            return IsFormatSupported(shareMode, desiredFormat, out closestMatchFormat);
        }

        /// <summary>
        /// Determines if the specified output format is supported in shared mode
        /// </summary>
        /// <param name="shareMode">Share Mode</param>
        /// <param name="desiredFormat">Desired Format</param>
        /// <param name="closestMatchFormat">Output The closest match format.</param>
        /// <returns>
        /// 	<c>true</c> if [is format supported] [the specified share mode]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFormatSupported(EAudioClientShareMode shareMode, WaveFormat desiredFormat, out WaveFormatExtensible closestMatchFormat)
        {
            int hresult = _realAudioClient.IsFormatSupported(shareMode, desiredFormat, out closestMatchFormat);
            // S_OK is 0, S_FALSE = 1
            if (hresult == 0)
            {
                // directly supported
                return true;
            }
            if (hresult == 1)
            {
                return false;
            }
            //else if (hresult == (int)AudioClientErrors.UnsupportedFormat)
            //{
            //    return false;
            //}
            else
            {
                Marshal.ThrowExceptionForHR(hresult);
            }
            // shouldn't get here
            throw new NotSupportedException("Unknown hresult " + hresult.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shareMode"></param>
        /// <param name="desiredFormat"></param>
        /// <returns></returns>
        public WaveFormatExtensible CheckSupportFormat(EAudioClientShareMode shareMode, WaveFormat desiredFormat)
        {
            WaveFormatExtensible closestMatchFormat;
            int hresult = _realAudioClient.IsFormatSupported(shareMode, desiredFormat, out closestMatchFormat);
            return closestMatchFormat;
        }

        /// <summary>
        /// Set the Event Handle for buffer synchro.
        /// </summary>
        /// <param name="eventWaitHandle">The Wait Handle to setup</param>
        public void SetEventHandle(EventWaitHandle eventWaitHandle)
        {
            _realAudioClient.SetEventHandle(eventWaitHandle.SafeWaitHandle.DangerousGetHandle());
        }

        private int GetBufferSize()
        {
            uint result;
            Marshal.ThrowExceptionForHR(_realAudioClient.GetBufferSize(out result));

            return (int)result;
        }

        private void GetAudioRenderClient()
        {
            object result;
            var IID_AUDIO_RENDER_CLIENT = typeof(IAudioRenderClient).GUID;
            Marshal.ThrowExceptionForHR(_realAudioClient.GetService(IID_AUDIO_RENDER_CLIENT, out result));
            _AudioRenderClient = new AudioRenderClient(result as IAudioRenderClient);
        }

        private void GetAudioCaptureClient()
        {
            object result;
            var IID_AUDIO_CAPTURE_CLIENT = typeof(IAudioCaptureClient).GUID;
            Marshal.ThrowExceptionForHR(_realAudioClient.GetService(IID_AUDIO_CAPTURE_CLIENT, out result));
            _AudioCaptureClient = new AudioCaptureClient(result as IAudioCaptureClient);
        }

        /// <summary>
        /// Gets the stream latency (must initialize first)
        /// </summary>
        public long StreamLatency
        {
            get
            {
                long result;
                Marshal.ThrowExceptionForHR(_realAudioClient.GetStreamLatency(out result));
                return result;
            }
        }

        /// <summary>
        /// Gets the current padding (must initialize first)
        /// </summary>
        public int CurrentPadding
        {
            get
            {
                int result;
                Marshal.ThrowExceptionForHR(_realAudioClient.GetCurrentPadding(out result));
                return result;
            }
        }

        /// <summary>
        /// Gets the default device period (can be called before initialize)
        /// </summary>
        public long DefaultDevicePeriod
        {
            get
            {
                long defaultDevicePeriod;
                long minimumDevicePeriod;
                Marshal.ThrowExceptionForHR(_realAudioClient.GetDevicePeriod(out defaultDevicePeriod, out minimumDevicePeriod));
                return defaultDevicePeriod;
            }
        }

        /// <summary>
        /// Gets the minimum device period (can be called before initialize)
        /// </summary>
        public long MinimumDevicePeriod
        {
            get
            {
                long defaultDevicePeriod;
                long minimumDevicePeriod;
                Marshal.ThrowExceptionForHR(_realAudioClient.GetDevicePeriod(out defaultDevicePeriod, out minimumDevicePeriod));
                return minimumDevicePeriod;
            }
        }

        /*
        [PreserveSig]
        int GetMixFormat([Out] out WaveFormatExtensible format);
        */

        public void Start()
        {
            _realAudioClient.Start();
        }

        public void Stop()
        {
            _realAudioClient.Stop();
        }

        public void Reset()
        {
            _realAudioClient.Reset();
        }

        public override string ToString()
        {
            return $"{{ Latency: {StreamLatency}, Padding: {CurrentPadding}, MDPeriod: {MinimumDevicePeriod}nsec, DDPeriod: {DefaultDevicePeriod}nsec }}";
        }

        #region Dispose Members.

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (_realAudioClient == null)
            {
                return;
            }
            /*
            _AudioRenderClient?.Dispose();
            _AudioCaptureClient?.Dispose();
            */
            mixFormat = null;
            _AudioRenderClient = null;
            _AudioCaptureClient = null;
            Marshal.ReleaseComObject(_realAudioClient);
            _realAudioClient = null;
            GC.SuppressFinalize(this);
        }

        #endregion

        ~AudioClient()
        {
            Dispose();
        }
    }
}
