namespace System.Data.SQLite
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void SQLiteCallback(IntPtr context, int nArgs, IntPtr argsptr);
}

