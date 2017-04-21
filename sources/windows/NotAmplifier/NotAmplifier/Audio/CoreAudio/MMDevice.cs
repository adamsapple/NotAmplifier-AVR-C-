using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Audio.CoreAudio.Interfaces;

namespace Audio.CoreAudio
{

    /// <summary>
    /// 以下のメソッドはメインスレッド以外でCallするとInvalidCastExceptionとかでる
    /// Activate
    /// OpenPropertyStore
    /// GetId
    /// GetState
    /// http://stackoverflow.com/questions/14020232/coreaudioapi-with-threading-invalidcastexception
    /// </summary>
    public class MMDevice
    {
        private IMMDevice _RealDevice;
        private PropertyStore _PropertyStore;
        private AudioMeterInformation   _AudioMeterInformation;
        private AudioEndpointVolume     _AudioEndpointVolume;
        private AudioClient             _AudioClient;
        //private AudioSessionManager _AudioSessionManager;


        private static Guid IID_IAudioMeterInformation = typeof(IAudioMeterInformation).GUID;
        private static Guid IID_IAudioEndpointVolume   = typeof(IAudioEndpointVolume).GUID;
        private static Guid IID_IAudioClient           = typeof(IAudioClient).GUID;

        private readonly string _Id;

        internal MMDevice(IMMDevice realDevice)
        {
            Marshal.ThrowExceptionForHR(realDevice.GetId(out _Id));


            _RealDevice = realDevice;
            GetProperty();
        }

        internal static string GetID(IMMDevice realDevice)
        {
            string result;
            Marshal.ThrowExceptionForHR(realDevice.GetId(out result));
            return result;
        }

        private void GetProperty()
        {
            if(_PropertyStore != null) {
                return;
            }
            IPropertyStore propstore;
            Marshal.ThrowExceptionForHR(_RealDevice.OpenPropertyStore(EStgmAccess.STGM_READ, out propstore));
            _PropertyStore = new PropertyStore(propstore);
            
        }

        public object GetProperty(PropertyKey key)
        {
            if (_PropertyStore == null)
            {
                GetProperty();
            }

            return _PropertyStore[key].Value;
        }

        private EDeviceState GetState()
        {
            EDeviceState result;
            Marshal.ThrowExceptionForHR(_RealDevice.GetState(out result));

            return result;
        }
        
        private void GetAudioMeterInformation()
        {
            object result;
            Marshal.ThrowExceptionForHR(_RealDevice.Activate(ref IID_IAudioMeterInformation, ClsCtx.InprocServer, IntPtr.Zero, out result));
            _AudioMeterInformation = new AudioMeterInformation(result as IAudioMeterInformation);
        }
        
        private void GetAudioEndpointVolume()
        {
            object result;
            Marshal.ThrowExceptionForHR(_RealDevice.Activate(ref IID_IAudioEndpointVolume, ClsCtx.All, IntPtr.Zero, out result));
            //_AudioEndpointVolume = new AudioEndpointVolume(result as IAudioEndpointVolume);
            _AudioEndpointVolume = new AudioEndpointVolume( result as IAudioEndpointVolume );
        }

        private void GetAudioClient()
        {
            object result;
            Marshal.ThrowExceptionForHR(_RealDevice.Activate(ref IID_IAudioClient, ClsCtx.All, IntPtr.Zero, out result));
            _AudioClient = new CoreAudio.AudioClient(result as IAudioClient);
        }

        public EDeviceState State => GetState();
        public string Id => _Id;

        public AudioEndpointVolume AudioEndpointVolume
        {
            get
            {
                if (_AudioEndpointVolume == null)
                {
                    GetAudioEndpointVolume();
                }

                return _AudioEndpointVolume;
            }
        }


        public AudioMeterInformation AudioMeterInformation
        {
            get
            {
                if (_AudioMeterInformation == null)
                    GetAudioMeterInformation();

                return _AudioMeterInformation;
            }
        }

        public AudioClient AudioClient
        {
            get
            {
                if (_AudioClient == null)
                    GetAudioClient();

                return _AudioClient;
            }
        }

        public string FriendlyName       => GetProperty(PropertyKeys.PKEY_DEVICE_INTERFACE_FRIENDLY_NAME) as string;
        public string DeviceFriendlyName => GetProperty(PropertyKeys.PKEY_DEVICE_FRIENDLY_NAME) as string;
        public string DeviceDescription  => GetProperty(PropertyKeys.PKEY_DEVICE_DESCRIPTION) as string;
        public string IconPath           => GetProperty(PropertyKeys.PKEY_DEVICE_ICON) as string;
        

        internal void FireOnDeviceStateChanged(string deviceId, EDeviceState newState)
        {
        }

        internal void FireOnDeviceAdded(string deviceId)
        {

        }

        internal void FireOnDeviceRemoved(string deviceId)
        {

        }

        internal void FireOnDefaultDeviceChanged(EDataFlow dataFlow, ERole deviceRole, string defaultDeviceId)
        {

        }

        internal void FireOnPropertyValueChanged(string deviceId, PropertyKey propertyKey)
        {

        }

        public virtual void Dispose(bool disposing)
        {
            /*
            if (_RealDevice != null)
            {
                _AudioEndpointVolume?.Dispose(disposing);
                _AudioMeterInformation?.Dispose(disposing);
                _AudioClient?.Dispose();
                _PropertyStore = null;
                _RealDevice    = null;
            }
            */
        }

        ~MMDevice()
        {
            //Todo:無駄な解放をやめなきゃならぬ
            //Dispose(false);
        }
    }
}
