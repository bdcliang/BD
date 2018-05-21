namespace System.Data.SQLite
{
    using System;
    using System.Runtime.InteropServices;

    internal class SQLite3_UTF16 : SQLite3
    {
        internal SQLite3_UTF16(SQLiteDateFormats fmt) : base(fmt)
        {
        }

        internal override void Bind_DateTime(SQLiteStatement stmt, int index, DateTime dt)
        {
            this.Bind_Text(stmt, index, base.ToString(dt));
        }

        internal override void Bind_Text(SQLiteStatement stmt, int index, string value)
        {
            int errorCode = UnsafeNativeMethods.sqlite3_bind_text16((IntPtr) stmt._sqlite_stmt, index, value, value.Length * 2, (IntPtr) (-1));
            if (errorCode > 0)
            {
                throw new SQLiteException(errorCode, this.SQLiteLastError());
            }
        }

        internal override string ColumnDatabaseName(SQLiteStatement stmt, int index)
        {
            int num;
            return UTF16ToString(UnsafeNativeMethods.sqlite3_column_database_name16_interop((IntPtr) stmt._sqlite_stmt, index, out num), num);
        }

        internal override string ColumnName(SQLiteStatement stmt, int index)
        {
            int num;
            return UTF16ToString(UnsafeNativeMethods.sqlite3_column_name16_interop((IntPtr) stmt._sqlite_stmt, index, out num), num);
        }

        internal override string ColumnOriginalName(SQLiteStatement stmt, int index)
        {
            int num;
            return UTF16ToString(UnsafeNativeMethods.sqlite3_column_origin_name16_interop((IntPtr) stmt._sqlite_stmt, index, out num), num);
        }

        internal override string ColumnTableName(SQLiteStatement stmt, int index)
        {
            int num;
            return UTF16ToString(UnsafeNativeMethods.sqlite3_column_table_name16_interop((IntPtr) stmt._sqlite_stmt, index, out num), num);
        }

        internal override DateTime GetDateTime(SQLiteStatement stmt, int index)
        {
            return base.ToDateTime(this.GetText(stmt, index));
        }

        internal override string GetParamValueText(IntPtr ptr)
        {
            int num;
            return UTF16ToString(UnsafeNativeMethods.sqlite3_value_text16_interop(ptr, out num), num);
        }

        internal override string GetText(SQLiteStatement stmt, int index)
        {
            int num;
            return UTF16ToString(UnsafeNativeMethods.sqlite3_column_text16_interop((IntPtr) stmt._sqlite_stmt, index, out num), num);
        }

        internal override void Open(string strFilename, SQLiteOpenFlagsEnum flags, int maxPoolSize, bool usePool)
        {
            if (base._sql == null)
            {
                base._usePool = usePool;
                if (usePool)
                {
                    base._fileName = strFilename;
                    base._sql = SQLiteConnectionPool.Remove(strFilename, maxPoolSize, out this._poolVersion);
                }
                if (base._sql == null)
                {
                    IntPtr ptr;
                    int errorCode = UnsafeNativeMethods.sqlite3_open16_interop(SQLiteConvert.ToUTF8(strFilename), (int) flags, out ptr);
                    if (errorCode > 0)
                    {
                        throw new SQLiteException(errorCode, null);
                    }
                    base._sql = ptr;
                }
                base._functionsArray = SQLiteFunction.BindFunctions(this);
            }
        }

        internal override void ReturnError(IntPtr context, string value)
        {
            UnsafeNativeMethods.sqlite3_result_error16(context, value, value.Length * 2);
        }

        internal override void ReturnText(IntPtr context, string value)
        {
            UnsafeNativeMethods.sqlite3_result_text16(context, value, value.Length * 2, (IntPtr) (-1));
        }

        public override string ToString(IntPtr b, int nbytelen)
        {
            return UTF16ToString(b, nbytelen);
        }

        public static string UTF16ToString(IntPtr b, int nbytelen)
        {
            if ((nbytelen == 0) || (b == IntPtr.Zero))
            {
                return "";
            }
            if (nbytelen == -1)
            {
                return Marshal.PtrToStringUni(b);
            }
            return Marshal.PtrToStringUni(b, nbytelen / 2);
        }
    }
}

