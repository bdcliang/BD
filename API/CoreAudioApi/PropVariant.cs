namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    [StructLayout(LayoutKind.Explicit)]
    public struct PropVariant
    {
        [FieldOffset(8)]
        private Blob blobVal;
        [FieldOffset(8)]
        private bool boolVal;
        [FieldOffset(8)]
        private byte bVal;
        [FieldOffset(8)]
        private sbyte cVal;
        [FieldOffset(8)]
        private DateTime date;
        [FieldOffset(8)]
        private double dblVal;
        [FieldOffset(8)]
        private IntPtr everything_else;
        [FieldOffset(8)]
        private System.Runtime.InteropServices.ComTypes.FILETIME filetime;
        [FieldOffset(8)]
        private float fltVal;
        [FieldOffset(8)]
        private long hVal;
        [FieldOffset(8)]
        private short iVal;
        [FieldOffset(8)]
        private int lVal;
        [FieldOffset(8)]
        private int scode;
        [FieldOffset(8)]
        private ulong uhVal;
        [FieldOffset(8)]
        private ushort uiVal;
        [FieldOffset(8)]
        private uint ulVal;
        [FieldOffset(0)]
        private short vt;
        [FieldOffset(2)]
        private short wReserved1;
        [FieldOffset(4)]
        private short wReserved2;
        [FieldOffset(6)]
        private short wReserved3;

        internal byte[] GetBlob()
        {
            byte[] buffer = new byte[this.blobVal.Length];
            for (int i = 0; i < this.blobVal.Length; i++)
            {
                buffer[i] = Marshal.ReadByte((IntPtr) (((long) this.blobVal.Data) + i));
            }
            return buffer;
        }

        public object Value
        {
            get
            {
                VarEnum vt = (VarEnum) this.vt;
                switch (vt)
                {
                    case VarEnum.VT_I2:
                        return this.iVal;

                    case VarEnum.VT_I4:
                        return this.lVal;

                    case VarEnum.VT_I1:
                        return this.bVal;

                    case VarEnum.VT_UI4:
                        return this.ulVal;

                    case VarEnum.VT_I8:
                        return this.hVal;

                    case VarEnum.VT_INT:
                        return this.iVal;

                    case VarEnum.VT_LPWSTR:
                        return Marshal.PtrToStringUni(this.everything_else);

                    case VarEnum.VT_BLOB:
                        return this.GetBlob();
                }
                return ("FIXME Type = " + vt.ToString());
            }
        }
    }
}

