namespace CoreAudioApi
{
    using System;

    [Flags]
    public enum EDeviceState : uint
    {
        DEVICE_STATE_ACTIVE = 1,
        DEVICE_STATE_NOTPRESENT = 4,
        DEVICE_STATE_UNPLUGGED = 2,
        DEVICE_STATEMASK_ALL = 7
    }
}

