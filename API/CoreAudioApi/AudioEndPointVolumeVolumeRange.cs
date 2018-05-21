namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Runtime.InteropServices;

    public class AudioEndPointVolumeVolumeRange
    {
        private float _VolumeIncrementdB;
        private float _VolumeMaxdB;
        private float _VolumeMindB;

        internal AudioEndPointVolumeVolumeRange(IAudioEndpointVolume parent)
        {
            Marshal.ThrowExceptionForHR(parent.GetVolumeRange(out this._VolumeMindB, out this._VolumeMaxdB, out this._VolumeIncrementdB));
        }

        public float IncrementdB
        {
            get
            {
                return this._VolumeIncrementdB;
            }
        }

        public float MaxdB
        {
            get
            {
                return this._VolumeMaxdB;
            }
        }

        public float MindB
        {
            get
            {
                return this._VolumeMindB;
            }
        }
    }
}

