namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class MMDeviceCollection
    {
        private IMMDeviceCollection _MMDeviceCollection;

        internal MMDeviceCollection(IMMDeviceCollection parent)
        {
            this._MMDeviceCollection = parent;
        }

        public int Count
        {
            get
            {
                uint num;
                Marshal.ThrowExceptionForHR(this._MMDeviceCollection.GetCount(out num));
                return (int) num;
            }
        }

        public MMDevice this[int index]
        {
            get
            {
                IMMDevice device;
                this._MMDeviceCollection.Item((uint) index, out device);
                return new MMDevice(device);
            }
        }
    }
}

