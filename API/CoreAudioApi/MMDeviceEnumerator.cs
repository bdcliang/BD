namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Runtime.InteropServices;

    public class MMDeviceEnumerator
    {
        private IMMDeviceEnumerator _realEnumerator = (new _MMDeviceEnumerator() as IMMDeviceEnumerator);

        public MMDeviceEnumerator()
        {
            if (Environment.OSVersion.Version.Major < 6)
            {
                throw new NotSupportedException("This functionality is only supported on Windows Vista or newer.");
            }
        }

        public MMDeviceCollection EnumerateAudioEndPoints(EDataFlow dataFlow, EDeviceState dwStateMask)
        {
            IMMDeviceCollection devices;
            Marshal.ThrowExceptionForHR(this._realEnumerator.EnumAudioEndpoints(dataFlow, dwStateMask, out devices));
            return new MMDeviceCollection(devices);
        }

        public MMDevice GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role)
        {
            IMMDevice ppEndpoint = null;
            Marshal.ThrowExceptionForHR(this._realEnumerator.GetDefaultAudioEndpoint(dataFlow, role, out ppEndpoint));
            return new MMDevice(ppEndpoint);
        }

        public MMDevice GetDevice(string ID)
        {
            IMMDevice ppDevice = null;
            Marshal.ThrowExceptionForHR(this._realEnumerator.GetDevice(ID, out ppDevice));
            return new MMDevice(ppDevice);
        }
    }
}

