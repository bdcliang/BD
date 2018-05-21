namespace CoreAudioApi
{
    using System;

    public enum AudioSessionDisconnectReason
    {
        DisconnectReasonDeviceRemoval,
        DisconnectReasonServerShutdown,
        DisconnectReasonFormatChanged,
        DisconnectReasonSessionLogoff,
        DisconnectReasonSessionDisconnected,
        DisconnectReasonExclusiveModeOverride
    }
}

