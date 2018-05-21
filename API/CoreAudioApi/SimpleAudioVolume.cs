namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Runtime.InteropServices;

    public class SimpleAudioVolume
    {
        private ISimpleAudioVolume _SimpleAudioVolume;

        internal SimpleAudioVolume(ISimpleAudioVolume realSimpleVolume)
        {
            this._SimpleAudioVolume = realSimpleVolume;
        }

        public float MasterVolume
        {
            get
            {
                float num;
                Marshal.ThrowExceptionForHR(this._SimpleAudioVolume.GetMasterVolume(out num));
                return num;
            }
            set
            {
                Guid empty = Guid.Empty;
                Marshal.ThrowExceptionForHR(this._SimpleAudioVolume.SetMasterVolume(value, ref empty));
            }
        }

        public bool Mute
        {
            get
            {
                bool flag;
                Marshal.ThrowExceptionForHR(this._SimpleAudioVolume.GetMute(out flag));
                return flag;
            }
            set
            {
                Guid empty = Guid.Empty;
                Marshal.ThrowExceptionForHR(this._SimpleAudioVolume.SetMute(value, ref empty));
            }
        }
    }
}

