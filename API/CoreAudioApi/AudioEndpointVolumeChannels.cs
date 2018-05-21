namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class AudioEndpointVolumeChannels
    {
        private IAudioEndpointVolume _AudioEndPointVolume;
        private AudioEndpointVolumeChannel[] _Channels;

        internal AudioEndpointVolumeChannels(IAudioEndpointVolume parent)
        {
            this._AudioEndPointVolume = parent;
            int count = this.Count;
            this._Channels = new AudioEndpointVolumeChannel[count];
            for (int i = 0; i < count; i++)
            {
                this._Channels[i] = new AudioEndpointVolumeChannel(this._AudioEndPointVolume, i);
            }
        }

        public int Count
        {
            get
            {
                int num;
                Marshal.ThrowExceptionForHR(this._AudioEndPointVolume.GetChannelCount(out num));
                return num;
            }
        }

        public AudioEndpointVolumeChannel this[int index]
        {
            get
            {
                return this._Channels[index];
            }
        }
    }
}

