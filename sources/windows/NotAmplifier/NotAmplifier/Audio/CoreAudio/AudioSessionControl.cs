using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Audio.CoreAudio.Interfaces;

namespace Audio.CoreAudio
{
    public class AudioSessionControl
    {
        internal IAudioSessionControl2 _AudioSessionControl;

        internal AudioSessionControl(IAudioSessionControl2 realAudioSessionControl)
        {
            //IAudioMeterInformation _meters = realAudioSessionControl as IAudioMeterInformation;
            //ISimpleAudioVolume _volume = realAudioSessionControl as ISimpleAudioVolume;
            //if (_meters != null)
            //    _AudioMeterInformation = new CoreAudioApi.AudioMeterInformation(_meters);
            //if (_volume != null)
            //     _SimpleAudioVolume = new SimpleAudioVolume(_volume);
            _AudioSessionControl = realAudioSessionControl;

        }
    }
}
