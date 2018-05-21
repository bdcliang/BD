namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class AudioEndpointVolume : IDisposable
    {
        private IAudioEndpointVolume _AudioEndPointVolume;
        private AudioEndpointVolumeCallback _CallBack;
        private AudioEndpointVolumeChannels _Channels;
        private EEndpointHardwareSupport _HardwareSupport;
        private AudioEndpointVolumeStepInformation _StepInformation;
        private AudioEndPointVolumeVolumeRange _VolumeRange;

        public event AudioEndpointVolumeNotificationDelegate OnVolumeNotification;

        internal AudioEndpointVolume(IAudioEndpointVolume realEndpointVolume)
        {
            uint num;
            this._AudioEndPointVolume = realEndpointVolume;
            this._Channels = new AudioEndpointVolumeChannels(this._AudioEndPointVolume);
            this._StepInformation = new AudioEndpointVolumeStepInformation(this._AudioEndPointVolume);
            Marshal.ThrowExceptionForHR(this._AudioEndPointVolume.QueryHardwareSupport(out num));
            this._HardwareSupport = (EEndpointHardwareSupport) num;
            this._VolumeRange = new AudioEndPointVolumeVolumeRange(this._AudioEndPointVolume);
            this._CallBack = new AudioEndpointVolumeCallback(this);
            Marshal.ThrowExceptionForHR(this._AudioEndPointVolume.RegisterControlChangeNotify(this._CallBack));
        }

        public void Dispose()
        {
            if (this._CallBack != null)
            {
                Marshal.ThrowExceptionForHR(this._AudioEndPointVolume.UnregisterControlChangeNotify(this._CallBack));
                this._CallBack = null;
            }
        }

        ~AudioEndpointVolume()
        {
            this.Dispose();
        }

        internal void FireNotification(AudioVolumeNotificationData NotificationData)
        {
            AudioEndpointVolumeNotificationDelegate onVolumeNotification = this.OnVolumeNotification;
            if (onVolumeNotification != null)
            {
                onVolumeNotification(NotificationData);
            }
        }

        public void VolumeStepDown()
        {
            Marshal.ThrowExceptionForHR(this._AudioEndPointVolume.VolumeStepDown(Guid.Empty));
        }

        public void VolumeStepUp()
        {
            Marshal.ThrowExceptionForHR(this._AudioEndPointVolume.VolumeStepUp(Guid.Empty));
        }

        public AudioEndpointVolumeChannels Channels
        {
            get
            {
                return this._Channels;
            }
        }

        public EEndpointHardwareSupport HardwareSupport
        {
            get
            {
                return this._HardwareSupport;
            }
        }

        public float MasterVolumeLevel
        {
            get
            {
                float num;
                Marshal.ThrowExceptionForHR(this._AudioEndPointVolume.GetMasterVolumeLevel(out num));
                return num;
            }
            set
            {
                Marshal.ThrowExceptionForHR(this._AudioEndPointVolume.SetMasterVolumeLevel(value, Guid.Empty));
            }
        }

        public float MasterVolumeLevelScalar
        {
            get
            {
                float num;
                Marshal.ThrowExceptionForHR(this._AudioEndPointVolume.GetMasterVolumeLevelScalar(out num));
                return num;
            }
            set
            {
                Marshal.ThrowExceptionForHR(this._AudioEndPointVolume.SetMasterVolumeLevelScalar(value, Guid.Empty));
            }
        }

        public bool Mute
        {
            get
            {
                bool flag;
                Marshal.ThrowExceptionForHR(this._AudioEndPointVolume.GetMute(out flag));
                return flag;
            }
            set
            {
                Marshal.ThrowExceptionForHR(this._AudioEndPointVolume.SetMute(value, Guid.Empty));
            }
        }

        public AudioEndpointVolumeStepInformation StepInformation
        {
            get
            {
                return this._StepInformation;
            }
        }

        public AudioEndPointVolumeVolumeRange VolumeRange
        {
            get
            {
                return this._VolumeRange;
            }
        }
    }
}

