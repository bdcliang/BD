namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class AudioMeterInformationChannels
    {
        private IAudioMeterInformation _AudioMeterInformation;

        internal AudioMeterInformationChannels(IAudioMeterInformation parent)
        {
            this._AudioMeterInformation = parent;
        }

        public int Count
        {
            get
            {
                int num;
                Marshal.ThrowExceptionForHR(this._AudioMeterInformation.GetMeteringChannelCount(out num));
                return num;
            }
        }

        public float this[int index]
        {
            get
            {
                float[] numArray = new float[this.Count];
                GCHandle handle = GCHandle.Alloc(numArray, GCHandleType.Pinned);
                Marshal.ThrowExceptionForHR(this._AudioMeterInformation.GetChannelsPeakValues(numArray.Length, handle.AddrOfPinnedObject()));
                handle.Free();
                return numArray[index];
            }
        }
    }
}

