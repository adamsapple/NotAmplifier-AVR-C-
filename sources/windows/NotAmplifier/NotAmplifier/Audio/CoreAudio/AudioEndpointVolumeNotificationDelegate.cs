using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audio.CoreAudio
{
    /// <summary>
    /// volume変更時に通知されるdelegate
    /// </summary>
    /// <param name="data">volumeやmuteなどの情報が格納された構造体</param>
    public delegate void AudioEndpointVolumeNotificationDelegate(AudioVolumeNotificationData data);
}
