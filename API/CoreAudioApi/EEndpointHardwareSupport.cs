namespace CoreAudioApi
{
    using System;

    [Flags]
    public enum EEndpointHardwareSupport
    {
        Meter = 4,
        Mute = 2,
        Volume = 1
    }
}

