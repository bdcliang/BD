namespace System.Data.SQLite
{
    using System;
    using System.Runtime.InteropServices;

    internal abstract class SQLiteBase : SQLiteConvert, IDisposable
    {
        internal static object _lock = new object();

        internal SQLiteBase(SQLiteDateFormats fmt) : base(fmt)
        {
        }

        internal abstract IntPtr AggregateContext(IntPtr context);
        internal abstract int AggregateCount(IntPtr context);
        internal abstract void Bind_Blob(SQLiteStatement stmt, int index, byte[] blobData);
        internal abstract void Bind_DateTime(SQLiteStatement stmt, int index, DateTime dt);
        internal abstract void Bind_Double(SQLiteStatement stmt, int index, double value);
        internal abstract void Bind_Int32(SQLiteStatement stmt, int index, int value);
        internal abstract void Bind_Int64(SQLiteStatement stmt, int index, long value);
        internal abstract void Bind_Null(SQLiteStatement stmt, int index);
        internal abstract int Bind_ParamCount(SQLiteStatement stmt);
        internal abstract int Bind_ParamIndex(SQLiteStatement stmt, string paramName);
        internal abstract string Bind_ParamName(SQLiteStatement stmt, int index);
        internal abstract void Bind_Text(SQLiteStatement stmt, int index, string value);
        internal abstract void Cancel();
        internal abstract void ChangePassword(byte[] newPasswordBytes);
        internal abstract void ClearPool();
        internal abstract void Close();
        internal static void CloseConnection(SQLiteConnectionHandle db)
        {
            lock (_lock)
            {
                int errorCode = UnsafeNativeMethods.sqlite3_close_interop((IntPtr) db);
                if (errorCode > 0)
                {
                    throw new SQLiteException(errorCode, SQLiteLastError(db));
                }
            }
        }

        internal abstract TypeAffinity ColumnAffinity(SQLiteStatement stmt, int index);
        internal abstract int ColumnCount(SQLiteStatement stmt);
        internal abstract string ColumnDatabaseName(SQLiteStatement stmt, int index);
        internal abstract int ColumnIndex(SQLiteStatement stmt, string columnName);
        internal abstract void ColumnMetaData(string dataBase, string table, string column, out string dataType, out string collateSequence, out bool notNull, out bool primaryKey, out bool autoIncrement);
        internal abstract string ColumnName(SQLiteStatement stmt, int index);
        internal abstract string ColumnOriginalName(SQLiteStatement stmt, int index);
        internal abstract string ColumnTableName(SQLiteStatement stmt, int index);
        internal abstract string ColumnType(SQLiteStatement stmt, int index, out TypeAffinity nAffinity);
        internal abstract int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, string s1, string s2);
        internal abstract int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, char[] c1, char[] c2);
        internal abstract void CreateCollation(string strCollation, SQLiteCollation func, SQLiteCollation func16);
        internal abstract void CreateFunction(string strFunction, int nArgs, bool needCollSeq, SQLiteCallback func, SQLiteCallback funcstep, SQLiteFinalCallback funcfinal);
        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool bDisposing)
        {
        }

        internal static void FinalizeStatement(SQLiteStatementHandle stmt)
        {
            lock (_lock)
            {
                int errorCode = UnsafeNativeMethods.sqlite3_finalize_interop((IntPtr) stmt);
                if (errorCode > 0)
                {
                    throw new SQLiteException(errorCode, null);
                }
            }
        }

        internal abstract long GetBytes(SQLiteStatement stmt, int index, int nDataoffset, byte[] bDest, int nStart, int nLength);
        internal abstract long GetChars(SQLiteStatement stmt, int index, int nDataoffset, char[] bDest, int nStart, int nLength);
        internal abstract CollationSequence GetCollationSequence(SQLiteFunction func, IntPtr context);
        internal abstract int GetCursorForTable(SQLiteStatement stmt, int database, int rootPage);
        internal abstract DateTime GetDateTime(SQLiteStatement stmt, int index);
        internal abstract double GetDouble(SQLiteStatement stmt, int index);
        internal abstract void GetIndexColumnExtendedInfo(string database, string index, string column, out int sortMode, out int onError, out string collationSequence);
        internal abstract int GetInt32(SQLiteStatement stmt, int index);
        internal abstract long GetInt64(SQLiteStatement stmt, int index);
        internal abstract long GetParamValueBytes(IntPtr ptr, int nDataOffset, byte[] bDest, int nStart, int nLength);
        internal abstract double GetParamValueDouble(IntPtr ptr);
        internal abstract int GetParamValueInt32(IntPtr ptr);
        internal abstract long GetParamValueInt64(IntPtr ptr);
        internal abstract string GetParamValueText(IntPtr ptr);
        internal abstract TypeAffinity GetParamValueType(IntPtr ptr);
        internal abstract long GetRowIdForCursor(SQLiteStatement stmt, int cursor);
        internal abstract string GetText(SQLiteStatement stmt, int index);
        internal abstract object GetValue(SQLiteStatement stmt, int index, SQLiteType typ);
        internal abstract bool IsNull(SQLiteStatement stmt, int index);
        internal abstract void Open(string strFilename, SQLiteOpenFlagsEnum flags, int maxPoolSize, bool usePool);
        internal abstract SQLiteStatement Prepare(SQLiteConnection cnn, string strSql, SQLiteStatement previous, uint timeoutMS, out string strRemain);
        internal abstract int Reset(SQLiteStatement stmt);
        internal static void ResetConnection(SQLiteConnectionHandle db)
        {
            lock (_lock)
            {
                IntPtr zero = IntPtr.Zero;
                do
                {
                    zero = UnsafeNativeMethods.sqlite3_next_stmt((IntPtr) db, zero);
                    if (zero != IntPtr.Zero)
                    {
                        UnsafeNativeMethods.sqlite3_reset_interop(zero);
                    }
                }
                while (zero != IntPtr.Zero);
                UnsafeNativeMethods.sqlite3_exec((IntPtr) db, SQLiteConvert.ToUTF8("ROLLBACK"), IntPtr.Zero, IntPtr.Zero, out zero);
            }
        }

        internal abstract void ReturnBlob(IntPtr context, byte[] value);
        internal abstract void ReturnDouble(IntPtr context, double value);
        internal abstract void ReturnError(IntPtr context, string value);
        internal abstract void ReturnInt32(IntPtr context, int value);
        internal abstract void ReturnInt64(IntPtr context, long value);
        internal abstract void ReturnNull(IntPtr context);
        internal abstract void ReturnText(IntPtr context, string value);
        internal abstract void SetCommitHook(SQLiteCommitCallback func);
        internal abstract void SetPassword(byte[] passwordBytes);
        internal abstract void SetRollbackHook(SQLiteRollbackCallback func);
        internal abstract void SetTimeout(int nTimeoutMS);
        internal abstract void SetUpdateHook(SQLiteUpdateCallback func);
        internal abstract string SQLiteLastError();
        internal static string SQLiteLastError(SQLiteConnectionHandle db)
        {
            int num;
            return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_errmsg_interop((IntPtr) db, out num), num);
        }

        internal abstract bool Step(SQLiteStatement stmt);

        internal abstract int Changes { get; }

        internal abstract string Version { get; }
    }
}

