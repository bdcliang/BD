namespace CoreAudioApi.Interfaces
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Blob
    {
        public int Length;
        public IntPtr Data;
        private void FixCS0649()
        {
            this.Length = 0;
            this.Data = IntPtr.Zero;
        }
    }
}

