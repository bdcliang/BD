namespace System.Data.SQLite
{
    using System;

    [Flags]
    internal enum SQLiteOpenFlagsEnum
    {
        Create = 4,
        Default = 6,
        None = 0,
        ReadOnly = 1,
        ReadWrite = 2,
        SharedCache = 0x1000000
    }
}

