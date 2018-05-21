namespace CoreAudioApi.Interfaces
{
    using CoreAudioApi;
    using System;
    using System.Runtime.InteropServices;

    [Guid("1BE09788-6894-4089-8586-9A2A6C265AC5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMEndpoint
    {
        [PreserveSig]
        int GetDataFlow(out EDataFlow pDataFlow);
    }
}

