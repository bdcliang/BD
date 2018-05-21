namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Runtime.InteropServices;

    public class MMDevice
    {
        private CoreAudioApi.AudioEndpointVolume _AudioEndpointVolume;
        private CoreAudioApi.AudioMeterInformation _AudioMeterInformation;
        private CoreAudioApi.AudioSessionManager _AudioSessionManager;
        private PropertyStore _PropertyStore;
        private IMMDevice _RealDevice;
        private static Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
        private static Guid IID_IAudioMeterInformation = typeof(IAudioMeterInformation).GUID;
        private static Guid IID_IAudioSessionManager = typeof(IAudioSessionManager2).GUID;

        internal MMDevice(IMMDevice realDevice)
        {
            this._RealDevice = realDevice;
        }

        private void GetAudioEndpointVolume()
        {
            object obj2;
            Marshal.ThrowExceptionForHR(this._RealDevice.Activate(ref IID_IAudioEndpointVolume, CLSCTX.ALL, IntPtr.Zero, out obj2));
            this._AudioEndpointVolume = new CoreAudioApi.AudioEndpointVolume(obj2 as IAudioEndpointVolume);
        }

        private void GetAudioMeterInformation()
        {
            object obj2;
            Marshal.ThrowExceptionForHR(this._RealDevice.Activate(ref IID_IAudioMeterInformation, CLSCTX.ALL, IntPtr.Zero, out obj2));
            this._AudioMeterInformation = new CoreAudioApi.AudioMeterInformation(obj2 as IAudioMeterInformation);
        }

        private void GetAudioSessionManager()
        {
            object obj2;
            Marshal.ThrowExceptionForHR(this._RealDevice.Activate(ref IID_IAudioSessionManager, CLSCTX.ALL, IntPtr.Zero, out obj2));
            this._AudioSessionManager = new CoreAudioApi.AudioSessionManager(obj2 as IAudioSessionManager2);
        }

        private void GetPropertyInformation()
        {
            IPropertyStore store;
            Marshal.ThrowExceptionForHR(this._RealDevice.OpenPropertyStore(EStgmAccess.STGM_READ, out store));
            this._PropertyStore = new PropertyStore(store);
        }

        public CoreAudioApi.AudioEndpointVolume AudioEndpointVolume
        {
            get
            {
                if (this._AudioEndpointVolume == null)
                {
                    this.GetAudioEndpointVolume();
                }
                return this._AudioEndpointVolume;
            }
        }

        public CoreAudioApi.AudioMeterInformation AudioMeterInformation
        {
            get
            {
                if (this._AudioMeterInformation == null)
                {
                    this.GetAudioMeterInformation();
                }
                return this._AudioMeterInformation;
            }
        }

        public CoreAudioApi.AudioSessionManager AudioSessionManager
        {
            get
            {
                if (this._AudioSessionManager == null)
                {
                    this.GetAudioSessionManager();
                }
                return this._AudioSessionManager;
            }
        }

        public EDataFlow DataFlow
        {
            get
            {
                EDataFlow flow;
                (this._RealDevice as IMMEndpoint).GetDataFlow(out flow);
                return flow;
            }
        }

        public string FriendlyName
        {
            get
            {
                if (this._PropertyStore == null)
                {
                    this.GetPropertyInformation();
                }
                if (this._PropertyStore.Contains(PKEY.PKEY_DeviceInterface_FriendlyName))
                {
                    return (string) this._PropertyStore[PKEY.PKEY_DeviceInterface_FriendlyName].Value;
                }
                return "Unknown";
            }
        }

        public string ID
        {
            get
            {
                string str;
                Marshal.ThrowExceptionForHR(this._RealDevice.GetId(out str));
                return str;
            }
        }

        public PropertyStore Properties
        {
            get
            {
                if (this._PropertyStore == null)
                {
                    this.GetPropertyInformation();
                }
                return this._PropertyStore;
            }
        }

        public EDeviceState State
        {
            get
            {
                EDeviceState state;
                Marshal.ThrowExceptionForHR(this._RealDevice.GetState(out state));
                return state;
            }
        }
    }
}

