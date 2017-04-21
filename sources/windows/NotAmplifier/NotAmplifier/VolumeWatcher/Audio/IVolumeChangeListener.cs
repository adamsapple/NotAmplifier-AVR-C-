using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Audio.CoreAudio;

namespace VolumeWatcher.Audio
{
    public delegate void AudioVolumeChangedDelegate(float vol, bool mute);
    public delegate void AudioDeviceChangedDelegate(MMDevice device);

    public interface IVolumeChangeListener
    {
        void OnVolumeChanged( float vol, bool mute );
        void OnDeviceChanged(MMDevice device);
    }
}
