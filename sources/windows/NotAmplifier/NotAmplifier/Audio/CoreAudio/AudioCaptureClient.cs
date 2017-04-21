using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using Audio.CoreAudio.Interfaces;

namespace Audio.CoreAudio
{


    /// <summary>
    /// Audio Capture Client
    /// </summary>
    public class AudioCaptureClient
    {
        IAudioCaptureClient _realAudioCaptureClient;

        internal AudioCaptureClient(IAudioCaptureClient audipCaptureClientInterface)
        {
            this._realAudioCaptureClient = audipCaptureClientInterface;
        }

        
        /// <summary>
        /// Gets a pointer to the buffer
        /// </summary>
        /// <returns>Pointer to the buffer</returns>
        public IntPtr GetBuffer(
            out int numFramesToRead,
            out EAudioClientBufferFlags bufferFlags,
            out long devicePosition,
            out long qpcPosition)
        {
            IntPtr bufferPointer;
            Marshal.ThrowExceptionForHR(_realAudioCaptureClient.GetBuffer(out bufferPointer, out numFramesToRead, out bufferFlags, out devicePosition, out qpcPosition));
            return bufferPointer;
        }

        /// <summary>
        /// Gets a pointer to the buffer
        /// </summary>
        /// <param name="numFramesToRead">Number of frames to read</param>
        /// <param name="bufferFlags">Buffer flags</param>
        /// <returns>Pointer to the buffer</returns>
        public IntPtr GetBuffer(
            out int numFramesToRead,
            out EAudioClientBufferFlags bufferFlags)
        {
            IntPtr bufferPointer;
            long devicePosition;
            long qpcPosition;
            Marshal.ThrowExceptionForHR(_realAudioCaptureClient.GetBuffer(out bufferPointer, out numFramesToRead, out bufferFlags, out devicePosition, out qpcPosition));
            return bufferPointer;
        }

        /// <summary>
        /// Gets the size of the next packet
        /// </summary>
        public int GetNextPacketSize()
        {
            int numFramesInNextPacket;
            Marshal.ThrowExceptionForHR(_realAudioCaptureClient.GetNextPacketSize(out numFramesInNextPacket));
            return numFramesInNextPacket;
        }

        /// <summary>
        /// Release buffer
        /// </summary>
        /// <param name="numFramesWritten">Number of frames written</param>
        public void ReleaseBuffer(int numFramesWritten)
        {
            Marshal.ThrowExceptionForHR(_realAudioCaptureClient.ReleaseBuffer(numFramesWritten));
        }

        #region Dispose Members.

        /// <summary>
        /// Release the COM object
        /// </summary>
        public void Dispose()
        {
            if (_realAudioCaptureClient != null)
            {
                // althugh GC would do this for us, we want it done now
                // to let us reopen WASAPI
                Marshal.ReleaseComObject(_realAudioCaptureClient);
                _realAudioCaptureClient = null;
                GC.SuppressFinalize(this);
            }
        }

        ~AudioCaptureClient()
        {
            Dispose();
        }

        #endregion
    }
}
