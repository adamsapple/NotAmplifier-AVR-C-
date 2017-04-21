using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Audio.CoreAudio.Interfaces;

namespace Audio.CoreAudio
{
    public class AudioEndpointVolume
    {
        private IAudioEndpointVolume _realAudioEndpointVolume;
        private AudioEndpointVolumeCallback _callback;
        private event AudioEndpointVolumeNotificationDelegate _OnVolumeNotification;

        internal AudioEndpointVolume(IAudioEndpointVolume realAudioEndpointVolume)
        {
            _realAudioEndpointVolume = realAudioEndpointVolume;
            _callback = new AudioEndpointVolumeCallback(this);

            _OnVolumeNotification += (n) => { };
        }

        public void Dispose(bool disposing)
        {
            
            if (_OnVolumeNotification != null) {
                foreach(var d in _OnVolumeNotification.GetInvocationList())
                {
                    OnVolumeNotification -= (AudioEndpointVolumeNotificationDelegate)d;
                }
            }
            _callback                = null;
            _realAudioEndpointVolume = null;
        }

        internal void FireNotification(AudioVolumeNotificationData NotificationData)
        {
            if (_OnVolumeNotification != null)
                _OnVolumeNotification(NotificationData);
        }

        public bool Mute
        {
            get
            {
                bool result;
                Marshal.ThrowExceptionForHR(_realAudioEndpointVolume.GetMute(out result));
                return result;
            }
            set
            {
                Marshal.ThrowExceptionForHR(_realAudioEndpointVolume.SetMute(value, Guid.Empty));
            }
        }

        public float MasterVolumeLevel
        {
            get
            {
                float result;
                Marshal.ThrowExceptionForHR(_realAudioEndpointVolume.GetMasterVolumeLevel(out result));
                return result;
            }
            set
            {
                Marshal.ThrowExceptionForHR(_realAudioEndpointVolume.SetMasterVolumeLevel(value, Guid.Empty));
            }
        }

        public float MasterVolumeLevelScalar
        {
            get
            {
                float result;
                Marshal.ThrowExceptionForHR(_realAudioEndpointVolume.GetMasterVolumeLevelScalar(out result));
                return result;
            }
            set
            {
                Marshal.ThrowExceptionForHR(_realAudioEndpointVolume.SetMasterVolumeLevelScalar(Math.Max(0.0f,Math.Min(value,1.0f)), Guid.Empty));
            }
        }

        public event AudioEndpointVolumeNotificationDelegate OnVolumeNotification
        {
            add
            {
                _OnVolumeNotification += value;

                // delegateに関数が登録されていたら、CoreAudioへのCallback登録を行う
                if (_OnVolumeNotification.GetInvocationList().Length > 0)
                {
                    _realAudioEndpointVolume.RegisterControlChangeNotify(_callback);
                }
            }
            remove
            {
                _OnVolumeNotification -= value;

                // delegateに登録された関数が無ければ、CoreAudioへのcallback登録を解除する
                if(_OnVolumeNotification == null || _OnVolumeNotification.GetInvocationList().Length <= 0)
                {
                    _realAudioEndpointVolume.UnregisterControlChangeNotify(_callback);
                }

            }
        }
    }

    class AudioEndpointVolumeCallback : IAudioEndpointVolumeCallback
    {
        private readonly AudioEndpointVolume parent;
        public AudioVolumeNotificationData avndata;

        internal AudioEndpointVolumeCallback(AudioEndpointVolume _parent)
        {
            parent = _parent;
        }

        [PreserveSig]
        public int OnNotify(IntPtr notificationData)
        {
            avndata  = (avndata != null) ? avndata : new AudioVolumeNotificationData();
            var data = avndata;

            data.Initialize(notificationData);

            parent.FireNotification(data);
            return 0; //S_OK
        }
    }
}
