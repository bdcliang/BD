namespace System.Data.SQLite
{
    using System;

    public enum SQLiteErrorCode
    {
        Abort = 4,
        Auth = 0x17,
        Busy = 5,
        CantOpen = 14,
        Constraint = 0x13,
        Corrupt = 11,
        Done = 0x65,
        Empty = 0x10,
        Error = 1,
        Format = 0x18,
        Full = 13,
        Internal = 2,
        Interrupt = 9,
        IOErr = 10,
        Locked = 6,
        Mismatch = 20,
        Misuse = 0x15,
        NOLFS = 0x16,
        NoMem = 7,
        NotADatabase = 0x1a,
        NotFound = 12,
        Ok = 0,
        Perm = 3,
        Protocol = 15,
        Range = 0x19,
        ReadOnly = 8,
        Row = 100,
        Schema = 0x11,
        TooBig = 0x12
    }
}

