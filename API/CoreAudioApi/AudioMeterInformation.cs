namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Runtime.InteropServices;

    public class AudioMeterInformation
    {
        private IAudioMeterInformation _AudioMeterInformation;
        private AudioMeterInformationChannels _Channels;
        private EEndpointHardwareSupport _HardwareSupport;

        internal AudioMeterInformation(IAudioMeterInformation realInterface)
        {
            int num;
            this._AudioMeterInformation = realInterface;
            Marshal.ThrowExceptionForHR(this._AudioMeterInformation.QueryHardwareSupport(out num));
            this._HardwareSupport = (EEndpointHardwareSupport) num;
            this._Channels = new AudioMeterInformationChannels(this._AudioMeterInformation);
        }

        public EEndpointHardwareSupport HardwareSupport
        {
            get
            {
                return this._HardwareSupport;
            }
        }

        public float MasterPeakValue
        {
            get
            {
                float num;
                Marshal.ThrowExceptionForHR(this._AudioMeterInformation.GetPeakValue(out num));
                return num;
            }
        }

        public AudioMeterInformationChannels PeakValues
        {
            get
            {
                return this._Channels;
            }
        }
    }
}

