namespace CoreAudioApi
{
    using System;

    public class AudioVolumeNotificationData
    {
        private int _Channels;
        private float[] _ChannelVolume;
        private Guid _EventContext;
        private float _MasterVolume;
        private bool _Muted;

        public AudioVolumeNotificationData(Guid eventContext, bool muted, float masterVolume, float[] channelVolume)
        {
            this._EventContext = eventContext;
            this._Muted = muted;
            this._MasterVolume = masterVolume;
            this._Channels = channelVolume.Length;
            this._ChannelVolume = channelVolume;
        }

        public int Channels
        {
            get
            {
                return this._Channels;
            }
        }

        public float[] ChannelVolume
        {
            get
            {
                return this._ChannelVolume;
            }
        }

        public Guid EventContext
        {
            get
            {
                return this._EventContext;
            }
        }

        public float MasterVolume
        {
            get
            {
                return this._MasterVolume;
            }
        }

        public bool Muted
        {
            get
            {
                return this._Muted;
            }
        }
    }
}

