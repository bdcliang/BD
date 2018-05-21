namespace System.Data.SQLite
{
    using System;
    using System.Data;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SQLiteTypeNames
    {
        internal string typeName;
        internal DbType dataType;
        internal SQLiteTypeNames(string newtypeName, DbType newdataType)
        {
            this.typeName = newtypeName;
            this.dataType = newdataType;
        }
    }
}

