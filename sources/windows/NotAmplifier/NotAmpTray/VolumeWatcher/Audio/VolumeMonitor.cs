using System;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using Audio.CoreAudio;
using Audio.CoreAudio.Interfaces;

//
// https://msdn.microsoft.com/en-us/library/windows/desktop/dd316602.aspx?f=255&MSPPError=-2147217396
// https://github.com/xenolightning/AudioSwitcher/blob/master/AudioSwitcher.AudioApi.CoreAudio/Internal/Interfaces/ComIIds.cs
// https://github.com/xenolightning/AudioSwitcher/blob/master/AudioSwitcher.AudioApi.CoreAudio/Internal/Interfaces/IAudioSessionControl2.cs
// http://www.pinvoke.net/default.aspx/Enums/CLSCTX.html
// https://github.com/maindefine/volumecontrol/blob/558f23121127dd11d590b9cdf3f192ac0f66fa4d/C%23/CoreAudioApi/MMDeviceEnumerator.cs
// https://github.com/xenolightning/AudioSwitcher/blob/master/AudioSwitcher.AudioApi.CoreAudio/Internal/AudioEndpointVolumeCallback.cs
// https://www.ipentec.com/document/document.aspx?page=csharp-sizeable-form-minimum-size-ignore
// 


namespace VolumeWatcher.Audio
{
    /**
     * <summary>Windowsのデフォルト再生デバイスのVolumeを監視する。</summary>
     */
    public class VolumeMonitor
    {
        private MMDeviceEnumerator deviceEnumerator = null;     // [CoreAudioApi関係]
        private MMDevice device;                                // [CoreAudioApi関係]
        //private MMDeviceCollection devices;                     // [CoreAudioApi関係]
        private AudioEndpointVolume volume;                     // [CoreAudioApi関係]
        private IVolumeChangeListener listener = null;          // volume変更の通知先
        private InnerListener innerListener;                    // WASAPIの通知を受け取るクラス

        private readonly EDataFlow DataFlow;
        private readonly ERole Role;


        AudioVolumeNotificationData avndata;

        /**
         * <summary>コンストラクタ</summary>
         */
        public VolumeMonitor(EDataFlow flow, ERole role) {

            DataFlow      = flow;
            Role          = role;
            innerListener = new InnerListener(this);
        }

        /**
         * <summary>WASAPI関係の初期化処理</summary>
         */
        public void initDevice()
        {
            if (deviceEnumerator != null)
            {
                return;
            }
            // https://www.ipentec.com/document/document.aspx?page=csharp-shell-namespace-get-icon-from-file-path

            // get the speakers (1st render + multimedia) device
            deviceEnumerator = MMDeviceEnumerator.GetInstance();

            // set device state notification.
            deviceEnumerator.OnDefaultDeviceChanged += innerListener.OnDefaultDeviceChanged;
            
            // get default device.
            device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow, Role);
            if(device == null)
            {
                return;
            }
            volume = device.AudioEndpointVolume;

            // set volume state notification.
            volume.OnVolumeNotification += innerListener.OnVolumeNotify;
        }

        /**
         * <summary>デバイス更新処理。主にデフォルトデバイスの更新があった際に呼ばれる。
         *      対象デバイスにListenerの設定を行う</summary>
         * <remarks><paramref name="deviceId"/>がnull場合、処理を終了する。</remarks>
         * <param name="deviceId">deviceIdから再生デバイスの参照を取得し、内部クラス<see cref="InnerListener"/>への通知設定を行う。</param>
         * <returns>void</returns>
         */
        void updateDevice(string deviceId)
        {
            // イレギュラー時は終了
            if (deviceEnumerator == null || deviceId == null)
            {
                return;
            }

            // defaultでなくなったDeviceの参照を切る
            if(volume != null)
            {
                volume.OnVolumeNotification -= innerListener.OnVolumeNotify;
            }
            // 今までDefaultだったデバイスを開放
            //volume?.Dispose(true);
            //device?.Dispose(true);
            volume = null;
            device = null;

            // 新しくdefaultになったDeviceの参照を得る
            device = deviceEnumerator.GetDevice(deviceId);
            if (device != null)
            {
                volume    = device.AudioEndpointVolume;
                var meter = device.AudioMeterInformation;
                volume.OnVolumeNotification += innerListener.OnVolumeNotify;    // volumeに対して変更通知のListenerを設定
            }
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        public void releaseDevice()
        {
            if (deviceEnumerator == null)
            {
                return;
            }

            //todo:
            if (volume != null)
            {
                volume.OnVolumeNotification -= innerListener.OnVolumeNotify;
            }

            volume = null;
            device = null;

            deviceEnumerator.OnDefaultDeviceChanged -= innerListener.OnDefaultDeviceChanged;

            //Marshal.ReleaseComObject(volume);             // 解放しない方が良いっぽい(解放した時点でPGカウンタが飛んだ)
            //Marshal.ReleaseComObject(device);             // 解放しない方が良いっぽい(２回目の取得が出来なくなってしまった)
            //Marshal.ReleaseComObject(deviceEnumerator);
            deviceEnumerator = null;
        }

        /// <summary>
        /// volume変更の通知先を設定する
        /// </summary>
        /// <param name="vcl"></param>
        public void setVolumeChangeListener([In] IVolumeChangeListener vcl){
            listener = vcl;
            //listener.OnDeviceChanged();//
        }

        public AudioVolumeNotificationData GetAudioVolumeNotificationData
        {
            get
            {
                if (avndata == null)
                {
                    avndata = new AudioVolumeNotificationData();
                }
                var data = avndata;
                data.Initialize(device);

                return data;
            }
        }
        
        public MMDevice AudioDevice
        {
            get
            {
                return device;
            }
        }

        public AudioEndpointVolume AudioVolume
        {
            get
            {
                return volume;
            }
        }

        public event AudioVolumeChangedDelegate OnVolumeNotification
        {
            add
            {
                innerListener._OnAudioVolumeChanged += value;
            }
            remove
            {
                innerListener._OnAudioVolumeChanged -= value;
            }
        }

        public event AudioDeviceChangedDelegate OnDefaultDeviceChanged
        {
            add
            {
                innerListener._OnAudioDeviceChanged += value;
            }
            remove
            {
                innerListener._OnAudioDeviceChanged -= value;
            }
        }

        /// <summary>
        /// WASAPIの通知を受け取る内部クラス
        /// </summary>
        class InnerListener
        {
            private VolumeMonitor monitor;
            //public event AudioEndpointVolumeNotificationDelegate _OnVolumeNotification;
            //public event MMNotificationClientDefaultDeviceChangedDelegate _OnDefaultDeviceChanged;
            public event AudioVolumeChangedDelegate _OnAudioVolumeChanged;
            public event AudioDeviceChangedDelegate _OnAudioDeviceChanged;

            public InnerListener(VolumeMonitor _monitor)
            {
                monitor = _monitor;
            }

            /*/
            /// <summary>
            /// IMMNotificationClient : OnDeviceStateChanged
            /// </summary>
            /// <param name="deviceId"></param>
            /// <param name="newState"></param>
            public void OnDeviceStateChanged(
                        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
                        [MarshalAs(UnmanagedType.U4)] EDeviceState newState)
            {
                // Console.WriteLine("OnDeviceStateChanged: deviceId="+ deviceId +", state=" + newState);

                if (newState == EDeviceState.Active)
                {
                    //Console.WriteLine("Active");
                }
                else if (newState == EDeviceState.NotPresent)
                {
                    // Console.WriteLine("NotPresent");
                }
                //releaseDevice();
                //initDevice();
            }

            /// <summary>
            /// IMMNotificationClient : OnDeviceAdded
            /// </summary>
            /// <param name="deviceId"></param>
            public void OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)] string deviceId)
            {
                //Console.WriteLine("OnDeviceAdded:" + deviceId);
            }

            /// <summary>
            /// IMMNotificationClient : OnDeviceRemoved
            /// </summary>
            /// <param name="deviceId"></param>
            public void OnDeviceRemoved(
                [MarshalAs(UnmanagedType.LPWStr)] string deviceId)
            {
                //Console.WriteLine("OnDeviceRemoved:" + deviceId);
            }
            /// <summary>
            /// IMMNotificationClient : OnPropertyValueChanged
            /// </summary>
            /// <param name="deviceId"></param>
            /// <param name="propertyKey"></param>
            public void OnPropertyValueChanged(
                [MarshalAs(UnmanagedType.LPWStr)] string deviceId, PropertyKey propertyKey)
            {
                //Console.WriteLine("OnPropertyValueChanged:" + deviceId);
            }
            //*/

            /// <summary>
            /// IMMNotificationClient : OnDefaultDeviceChanged
            /// </summary>
            /// <param name="dataFlow"></param>
            /// <param name="deviceRole"></param>
            /// <param name="defaultDeviceId"></param>
            public void OnDefaultDeviceChanged(
                [MarshalAs(UnmanagedType.I4)] EDataFlow dataFlow,
                [MarshalAs(UnmanagedType.I4)] ERole deviceRole,
                [MarshalAs(UnmanagedType.LPWStr)] string defaultDeviceId)
            {
                //Console.WriteLine("OnDefaultDeviceChanged:  DeviceId:[" + defaultDeviceId + "] EDataFlow:[" + dataFlow+ "] ERole:[" + deviceRole+"]");
                if (dataFlow != monitor.DataFlow || deviceRole != monitor.Role)
                {
                    return;
                }


                var dispatcher = Application.Current.Dispatcher;
                //dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate () {
                dispatcher.Invoke((Action)delegate ()
                {


                    monitor.updateDevice(defaultDeviceId);

                    // if (monitor.listener == null)
                    // {
                    //    return;
                    //}
                    //
                    //monitor.listener.OnDeviceChanged(monitor.device);

                    Debug.WriteLine($"OnDefaultDeviceChanged flow={dataFlow} role={deviceRole} device={defaultDeviceId} list={(_OnAudioDeviceChanged?.GetInvocationList().Length ?? 0)}");
                    _OnAudioDeviceChanged?.Invoke(monitor.device);
                });
            }

            /// <summary>
            /// volume状態変更時にCallされる
            /// </summary>
            /// <param name="data"></param>
            public void OnVolumeNotify(AudioVolumeNotificationData data)
            {
                //if (monitor.listener == null) return;

                // 更新されたボリューム値をListenerに通知
                //monitor.listener.OnVolumeChanged(data.MasterVolume, data.Muted);

                /*
                if (_OnAudioVolumeChanged == null || _OnAudioVolumeChanged.GetInvocationList().Length == 0)
                {
                    return;
                }
                _OnAudioVolumeChanged(data.MasterVolume, data.Muted);
                */

                _OnAudioVolumeChanged?.Invoke(data.MasterVolume, data.Muted);
            }
        }
    }
}
