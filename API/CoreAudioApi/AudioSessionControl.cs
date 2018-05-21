namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Runtime.InteropServices;

    public class AudioSessionControl
    {
        internal CoreAudioApi.AudioMeterInformation _AudioMeterInformation;
        internal IAudioSessionControl2 _AudioSessionControl;
        internal CoreAudioApi.SimpleAudioVolume _SimpleAudioVolume;

        internal AudioSessionControl(IAudioSessionControl2 realAudioSessionControl)
        {
            IAudioMeterInformation realInterface = realAudioSessionControl as IAudioMeterInformation;
            ISimpleAudioVolume realSimpleVolume = realAudioSessionControl as ISimpleAudioVolume;
            if (realInterface != null)
            {
                this._AudioMeterInformation = new CoreAudioApi.AudioMeterInformation(realInterface);
            }
            if (realSimpleVolume != null)
            {
                this._SimpleAudioVolume = new CoreAudioApi.SimpleAudioVolume(realSimpleVolume);
            }
            this._AudioSessionControl = realAudioSessionControl;
        }

        public void RegisterAudioSessionNotification(IAudioSessionEvents eventConsumer)
        {
            Marshal.ThrowExceptionForHR(this._AudioSessionControl.RegisterAudioSessionNotification(eventConsumer));
        }

        public void UnregisterAudioSessionNotification(IAudioSessionEvents eventConsumer)
        {
            Marshal.ThrowExceptionForHR(this._AudioSessionControl.UnregisterAudioSessionNotification(eventConsumer));
        }

        public CoreAudioApi.AudioMeterInformation AudioMeterInformation
        {
            get
            {
                return this._AudioMeterInformation;
            }
        }

        public string DisplayName
        {
            get
            {
                IntPtr ptr;
                Marshal.ThrowExceptionForHR(this._AudioSessionControl.GetDisplayName(out ptr));
                string str = Marshal.PtrToStringAuto(ptr);
                Marshal.FreeCoTaskMem(ptr);
                return str;
            }
        }

        public string IconPath
        {
            get
            {
                IntPtr ptr;
                Marshal.ThrowExceptionForHR(this._AudioSessionControl.GetIconPath(out ptr));
                string str = Marshal.PtrToStringAuto(ptr);
                Marshal.FreeCoTaskMem(ptr);
                return str;
            }
        }

        public bool IsSystemIsSystemSoundsSession
        {
            get
            {
                return (this._AudioSessionControl.IsSystemSoundsSession() == 0);
            }
        }

        public uint ProcessID
        {
            get
            {
                uint num;
                Marshal.ThrowExceptionForHR(this._AudioSessionControl.GetProcessId(out num));
                return num;
            }
        }

        public string SessionIdentifier
        {
            get
            {
                IntPtr ptr;
                Marshal.ThrowExceptionForHR(this._AudioSessionControl.GetSessionIdentifier(out ptr));
                string str = Marshal.PtrToStringAuto(ptr);
                Marshal.FreeCoTaskMem(ptr);
                return str;
            }
        }

        public string SessionInstanceIdentifier
        {
            get
            {
                IntPtr ptr;
                Marshal.ThrowExceptionForHR(this._AudioSessionControl.GetSessionInstanceIdentifier(out ptr));
                string str = Marshal.PtrToStringAuto(ptr);
                Marshal.FreeCoTaskMem(ptr);
                return str;
            }
        }

        public CoreAudioApi.SimpleAudioVolume SimpleAudioVolume
        {
            get
            {
                return this._SimpleAudioVolume;
            }
        }

        public AudioSessionState State
        {
            get
            {
                AudioSessionState state;
                Marshal.ThrowExceptionForHR(this._AudioSessionControl.GetState(out state));
                return state;
            }
        }
    }
}

