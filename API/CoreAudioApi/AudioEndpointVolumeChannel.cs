namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Runtime.InteropServices;

    public class AudioEndpointVolumeChannel
    {
        private IAudioEndpointVolume _AudioEndpointVolume;
        private uint _Channel;

        internal AudioEndpointVolumeChannel(IAudioEndpointVolume parent, int channel)
        {
            this._Channel = (uint) channel;
            this._AudioEndpointVolume = parent;
        }

        public float VolumeLevel
        {
            get
            {
                float num;
                Marshal.ThrowExceptionForHR(this._AudioEndpointVolume.GetChannelVolumeLevel(this._Channel, out num));
                return num;
            }
            set
            {
                Marshal.ThrowExceptionForHR(this._AudioEndpointVolume.SetChannelVolumeLevel(this._Channel, value, Guid.Empty));
            }
        }

        public float VolumeLevelScalar
        {
            get
            {
                float num;
                Marshal.ThrowExceptionForHR(this._AudioEndpointVolume.GetChannelVolumeLevelScalar(this._Channel, out num));
                return num;
            }
            set
            {
                Marshal.ThrowExceptionForHR(this._AudioEndpointVolume.SetChannelVolumeLevelScalar(this._Channel, value, Guid.Empty));
            }
        }
    }
}

