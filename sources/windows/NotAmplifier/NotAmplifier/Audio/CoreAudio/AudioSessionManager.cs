using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Audio.CoreAudio.Interfaces;

namespace Audio.CoreAudio
{
    class AudioSessionManager
    {
        private IAudioSessionManager2 _AudioSessionManager;

        internal AudioSessionManager(IAudioSessionManager2 realAudioSessionManager)
        {
            _AudioSessionManager = realAudioSessionManager;

            //IAudioSessionEnumerator _SessionEnum;
            //Marshal.ThrowExceptionForHR(_AudioSessionManager.GetSessionEnumerator(out _SessionEnum));

            //_Sessions = new SessionCollection(_SessionEnum);
        }

        /*
        public SessionCollection Sessions
        {
            get
            {
                return _Sessions;
            }
        }
        */

    }
}
