using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Audio.CoreAudio.Interfaces;


namespace Audio.CoreAudio
{
    /// <summary>
    /// Audio Render Client
    /// </summary>
    public class AudioRenderClient : IDisposable
    {
        IAudioRenderClient _realAudioRenderClient;

        internal AudioRenderClient(IAudioRenderClient audioRenderClientInterface)
        {
            this._realAudioRenderClient = audioRenderClientInterface;
        }

        /// <summary>
        /// Gets a pointer to the buffer
        /// </summary>
        /// <param name="numFramesRequested">Number of frames requested</param>
        /// <returns>Pointer to the buffer</returns>
        public IntPtr GetBuffer(int numFramesRequested)
        {
            IntPtr bufferPointer;
            Marshal.ThrowExceptionForHR(_realAudioRenderClient.GetBuffer(numFramesRequested, out bufferPointer));
            return bufferPointer;
        }

        /// <summary>
        /// Release buffer
        /// </summary>
        /// <param name="numFramesWritten">Number of frames written</param>
        /// <param name="bufferFlags">Buffer flags</param>
        public void ReleaseBuffer(int numFramesWritten, EAudioClientBufferFlags bufferFlags)
        {
            Marshal.ThrowExceptionForHR(_realAudioRenderClient.ReleaseBuffer(numFramesWritten, bufferFlags));
        }


        #region Dispose Members.

        /// <summary>
        /// Release the COM object
        /// </summary>
        public void Dispose()
        {
            if (_realAudioRenderClient != null)
            {
                // althugh GC would do this for us, we want it done now
                // to let us reopen WASAPI
                Marshal.ReleaseComObject(_realAudioRenderClient);
                _realAudioRenderClient = null;
                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
