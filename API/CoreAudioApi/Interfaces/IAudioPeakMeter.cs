namespace CoreAudioApi.Interfaces
{
    using System;
    using System.Runtime.InteropServices;

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("DD79923C-0599-45e0-B8B6-C8DF7DB6E796")]
    internal interface IAudioPeakMeter
    {
        int GetChannelCount(out int pcChannels);
        int GetLevel(int Channel, out float level);
    }
}

