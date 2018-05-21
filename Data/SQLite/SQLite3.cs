namespace System.Data.SQLite
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal class SQLite3 : SQLiteBase
    {
        private bool _buildingSchema;
        protected string _fileName;
        protected SQLiteFunction[] _functionsArray;
        protected int _poolVersion;
        protected SQLiteConnectionHandle _sql;
        protected bool _usePool;

        internal SQLite3(SQLiteDateFormats fmt) : base(fmt)
        {
        }

        internal override IntPtr AggregateContext(IntPtr context)
        {
            return UnsafeNativeMethods.sqlite3_aggregate_context(context, 1);
        }

        internal override int AggregateCount(IntPtr context)
        {
            return UnsafeNativeMethods.sqlite3_aggregate_count(context);
        }

        internal override void Bind_Blob(SQLiteStatement stmt, int index, byte[] blobData)
        {
            int errorCode = UnsafeNativeMethods.sqlite3_bind_blob((IntPtr) stmt._sqlite_stmt, index, blobData, blobData.Length, (IntPtr) (-1));
            if (errorCode > 0)
            {
                throw new SQLiteException(errorCode, this.SQLiteLastError());
            }
        }

        internal override void Bind_DateTime(SQLiteStatement stmt, int index, DateTime dt)
        {
            byte[] buffer = base.ToUTF8(dt);
            int errorCode = UnsafeNativeMethods.sqlite3_bind_text((IntPtr) stmt._sqlite_stmt, index, buffer, buffer.Length - 1, (IntPtr) (-1));
            if (errorCode > 0)
            {
                throw new SQLiteException(errorCode, this.SQLiteLastError());
            }
        }

        internal override void Bind_Double(SQLiteStatement stmt, int index, double value)
        {
            int errorCode = UnsafeNativeMethods.sqlite3_bind_double((IntPtr) stmt._sqlite_stmt, index, value);
            if (errorCode > 0)
            {
                throw new SQLiteException(errorCode, this.SQLiteLastError());
            }
        }

        internal override void Bind_Int32(SQLiteStatement stmt, int index, int value)
        {
            int errorCode = UnsafeNativeMethods.sqlite3_bind_int((IntPtr) stmt._sqlite_stmt, index, value);
            if (errorCode > 0)
            {
                throw new SQLiteException(errorCode, this.SQLiteLastError());
            }
        }

        internal override void Bind_Int64(SQLiteStatement stmt, int index, long value)
        {
            int errorCode = UnsafeNativeMethods.sqlite3_bind_int64((IntPtr) stmt._sqlite_stmt, index, value);
            if (errorCode > 0)
            {
                throw new SQLiteException(errorCode, this.SQLiteLastError());
            }
        }

        internal override void Bind_Null(SQLiteStatement stmt, int index)
        {
            int errorCode = UnsafeNativeMethods.sqlite3_bind_null((IntPtr) stmt._sqlite_stmt, index);
            if (errorCode > 0)
            {
                throw new SQLiteException(errorCode, this.SQLiteLastError());
            }
        }

        internal override int Bind_ParamCount(SQLiteStatement stmt)
        {
            return UnsafeNativeMethods.sqlite3_bind_parameter_count((IntPtr) stmt._sqlite_stmt);
        }

        internal override int Bind_ParamIndex(SQLiteStatement stmt, string paramName)
        {
            return UnsafeNativeMethods.sqlite3_bind_parameter_index((IntPtr) stmt._sqlite_stmt, SQLiteConvert.ToUTF8(paramName));
        }

        internal override string Bind_ParamName(SQLiteStatement stmt, int index)
        {
            int num;
            return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_bind_parameter_name_interop((IntPtr) stmt._sqlite_stmt, index, out num), num);
        }

        internal override void Bind_Text(SQLiteStatement stmt, int index, string value)
        {
            byte[] buffer = SQLiteConvert.ToUTF8(value);
            int errorCode = UnsafeNativeMethods.sqlite3_bind_text((IntPtr) stmt._sqlite_stmt, index, buffer, buffer.Length - 1, (IntPtr) (-1));
            if (errorCode > 0)
            {
                throw new SQLiteException(errorCode, this.SQLiteLastError());
            }
        }

        internal override void Cancel()
        {
            UnsafeNativeMethods.sqlite3_interrupt((IntPtr) this._sql);
        }

        internal override void ChangePassword(byte[] newPasswordBytes)
        {
            int errorCode = UnsafeNativeMethods.sqlite3_rekey((IntPtr) this._sql, newPasswordBytes, (newPasswordBytes == null) ? 0 : newPasswordBytes.Length);
            if (errorCode > 0)
            {
                throw new SQLiteException(errorCode, this.SQLiteLastError());
            }
        }

        internal override void ClearPool()
        {
            SQLiteConnectionPool.ClearPool(this._fileName);
        }

        internal override void Close()
        {
            if (this._sql != null)
            {
                if (this._usePool)
                {
                    SQLiteBase.ResetConnection(this._sql);
                    SQLiteConnectionPool.Add(this._fileName, this._sql, this._poolVersion);
                }
                else
                {
                    this._sql.Dispose();
                }
            }
            this._sql = null;
        }

        internal override TypeAffinity ColumnAffinity(SQLiteStatement stmt, int index)
        {
            return UnsafeNativeMethods.sqlite3_column_type((IntPtr) stmt._sqlite_stmt, index);
        }

        internal override int ColumnCount(SQLiteStatement stmt)
        {
            return UnsafeNativeMethods.sqlite3_column_count((IntPtr) stmt._sqlite_stmt);
        }

        internal override string ColumnDatabaseName(SQLiteStatement stmt, int index)
        {
            int num;
            return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_column_database_name_interop((IntPtr) stmt._sqlite_stmt, index, out num), num);
        }

        internal override int ColumnIndex(SQLiteStatement stmt, string columnName)
        {
            int num = this.ColumnCount(stmt);
            for (int i = 0; i < num; i++)
            {
                if (string.Compare(columnName, this.ColumnName(stmt, i), true, CultureInfo.InvariantCulture) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        internal override void ColumnMetaData(string dataBase, string table, string column, out string dataType, out string collateSequence, out bool notNull, out bool primaryKey, out bool autoIncrement)
        {
            IntPtr ptr;
            IntPtr ptr2;
            int num;
            int num2;
            int num3;
            int num5;
            int num6;
            int errorCode = UnsafeNativeMethods.sqlite3_table_column_metadata_interop((IntPtr) this._sql, SQLiteConvert.ToUTF8(dataBase), SQLiteConvert.ToUTF8(table), SQLiteConvert.ToUTF8(column), out ptr, out ptr2, out num, out num2, out num3, out num5, out num6);
            if (errorCode > 0)
            {
                throw new SQLiteException(errorCode, this.SQLiteLastError());
            }
            dataType = SQLiteConvert.UTF8ToString(ptr, num5);
            collateSequence = SQLiteConvert.UTF8ToString(ptr2, num6);
            notNull = num == 1;
            primaryKey = num2 == 1;
            autoIncrement = num3 == 1;
        }

        internal override string ColumnName(SQLiteStatement stmt, int index)
        {
            int num;
            return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_column_name_interop((IntPtr) stmt._sqlite_stmt, index, out num), num);
        }

        internal override string ColumnOriginalName(SQLiteStatement stmt, int index)
        {
            int num;
            return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_column_origin_name_interop((IntPtr) stmt._sqlite_stmt, index, out num), num);
        }

        internal override string ColumnTableName(SQLiteStatement stmt, int index)
        {
            int num;
            return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_column_table_name_interop((IntPtr) stmt._sqlite_stmt, index, out num), num);
        }

        internal override string ColumnType(SQLiteStatement stmt, int index, out TypeAffinity nAffinity)
        {
            int num;
            IntPtr nativestring = UnsafeNativeMethods.sqlite3_column_decltype_interop((IntPtr) stmt._sqlite_stmt, index, out num);
            nAffinity = this.ColumnAffinity(stmt, index);
            if (nativestring != IntPtr.Zero)
            {
                return SQLiteConvert.UTF8ToString(nativestring, num);
            }
            string[] typeDefinitions = stmt.TypeDefinitions;
            if (((typeDefinitions != null) && (index < typeDefinitions.Length)) && (typeDefinitions[index] != null))
            {
                return typeDefinitions[index];
            }
            return string.Empty;
        }

        internal override int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, string s1, string s2)
        {
            Encoding unicode = null;
            switch (enc)
            {
                case CollationEncodingEnum.UTF8:
                    unicode = Encoding.UTF8;
                    break;

                case CollationEncodingEnum.UTF16LE:
                    unicode = Encoding.Unicode;
                    break;

                case CollationEncodingEnum.UTF16BE:
                    unicode = Encoding.BigEndianUnicode;
                    break;
            }
            byte[] bytes = unicode.GetBytes(s1);
            byte[] buffer2 = unicode.GetBytes(s2);
            return UnsafeNativeMethods.sqlite3_context_collcompare(context, bytes, bytes.Length, buffer2, buffer2.Length);
        }

        internal override int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, char[] c1, char[] c2)
        {
            Encoding unicode = null;
            switch (enc)
            {
                case CollationEncodingEnum.UTF8:
                    unicode = Encoding.UTF8;
                    break;

                case CollationEncodingEnum.UTF16LE:
                    unicode = Encoding.Unicode;
                    break;

                case CollationEncodingEnum.UTF16BE:
                    unicode = Encoding.BigEndianUnicode;
                    break;
            }
            byte[] bytes = unicode.GetBytes(c1);
            byte[] buffer2 = unicode.GetBytes(c2);
            return UnsafeNativeMethods.sqlite3_context_collcompare(context, bytes, bytes.Length, buffer2, buffer2.Length);
        }

        internal override void CreateCollation(string strCollation, SQLiteCollation func, SQLiteCollation func16)
        {
            int errorCode = UnsafeNativeMethods.sqlite3_create_collation((IntPtr) this._sql, SQLiteConvert.ToUTF8(strCollation), 2, IntPtr.Zero, func16);
            if (errorCode == 0)
            {
                UnsafeNativeMethods.sqlite3_create_collation((IntPtr) this._sql, SQLiteConvert.ToUTF8(strCollation), 1, IntPtr.Zero, func);
            }
            if (errorCode > 0)
            {
                throw new SQLiteException(errorCode, this.SQLiteLastError());
            }
        }

        internal override void CreateFunction(string strFunction, int nArgs, bool needCollSeq, SQLiteCallback func, SQLiteCallback funcstep, SQLiteFinalCallback funcfinal)
        {
            int errorCode = UnsafeNativeMethods.sqlite3_create_function_interop((IntPtr) this._sql, SQLiteConvert.ToUTF8(strFunction), nArgs, 4, IntPtr.Zero, func, funcstep, funcfinal, needCollSeq ? 1 : 0);
            if (errorCode == 0)
            {
                errorCode = UnsafeNativeMethods.sqlite3_create_function_interop((IntPtr) this._sql, SQLiteConvert.ToUTF8(strFunction), nArgs, 1, IntPtr.Zero, func, funcstep, funcfinal, needCollSeq ? 1 : 0);
            }
            if (errorCode > 0)
            {
                throw new SQLiteException(errorCode, this.SQLiteLastError());
            }
        }

        protected override void Dispose(bool bDisposing)
        {
            if (bDisposing)
            {
                this.Close();
            }
        }

        internal override long GetBytes(SQLiteStatement stmt, int index, int nDataOffset, byte[] bDest, int nStart, int nLength)
        {
            int length = nLength;
            int num = UnsafeNativeMethods.sqlite3_column_bytes((IntPtr) stmt._sqlite_stmt, index);
            IntPtr ptr = UnsafeNativeMethods.sqlite3_column_blob((IntPtr) stmt._sqlite_stmt, index);
            if (bDest == null)
            {
                return (long) num;
            }
            if ((length + nStart) > bDest.Length)
            {
                length = bDest.Length - nStart;
            }
            if ((length + nDataOffset) > num)
            {
                length = num - nDataOffset;
            }
            if (length > 0)
            {
                Marshal.Copy((IntPtr) (ptr.ToInt64() + nDataOffset), bDest, nStart, length);
            }
            else
            {
                length = 0;
            }
            return (long) length;
        }

        internal override long GetChars(SQLiteStatement stmt, int index, int nDataOffset, char[] bDest, int nStart, int nLength)
        {
            int count = nLength;
            string text = this.GetText(stmt, index);
            int length = text.Length;
            if (bDest == null)
            {
                return (long) length;
            }
            if ((count + nStart) > bDest.Length)
            {
                count = bDest.Length - nStart;
            }
            if ((count + nDataOffset) > length)
            {
                count = length - nDataOffset;
            }
            if (count > 0)
            {
                text.CopyTo(nDataOffset, bDest, nStart, count);
            }
            else
            {
                count = 0;
            }
            return (long) count;
        }

        internal override CollationSequence GetCollationSequence(SQLiteFunction func, IntPtr context)
        {
            int num;
            int num2;
            int num3;
            CollationSequence sequence = new CollationSequence();
            IntPtr nativestring = UnsafeNativeMethods.sqlite3_context_collseq(context, out num2, out num3, out num);
            sequence.Name = SQLiteConvert.UTF8ToString(nativestring, num);
            sequence.Type = (CollationTypeEnum) num2;
            sequence._func = func;
            sequence.Encoding = (CollationEncodingEnum) num3;
            return sequence;
        }

        internal override int GetCursorForTable(SQLiteStatement stmt, int db, int rootPage)
        {
            return UnsafeNativeMethods.sqlite3_table_cursor((IntPtr) stmt._sqlite_stmt, db, rootPage);
        }

        internal override DateTime GetDateTime(SQLiteStatement stmt, int index)
        {
            int num;
            return base.ToDateTime(UnsafeNativeMethods.sqlite3_column_text_interop((IntPtr) stmt._sqlite_stmt, index, out num), num);
        }

        internal override double GetDouble(SQLiteStatement stmt, int index)
        {
            return UnsafeNativeMethods.sqlite3_column_double((IntPtr) stmt._sqlite_stmt, index);
        }

        internal override void GetIndexColumnExtendedInfo(string database, string index, string column, out int sortMode, out int onError, out string collationSequence)
        {
            IntPtr ptr;
            int num;
            int errorCode = UnsafeNativeMethods.sqlite3_index_column_info_interop((IntPtr) this._sql, SQLiteConvert.ToUTF8(database), SQLiteConvert.ToUTF8(index), SQLiteConvert.ToUTF8(column), out sortMode, out onError, out ptr, out num);
            if (errorCode != 0)
            {
                throw new SQLiteException(errorCode, "");
            }
            collationSequence = SQLiteConvert.UTF8ToString(ptr, num);
        }

        internal override int GetInt32(SQLiteStatement stmt, int index)
        {
            return UnsafeNativeMethods.sqlite3_column_int((IntPtr) stmt._sqlite_stmt, index);
        }

        internal override long GetInt64(SQLiteStatement stmt, int index)
        {
            return UnsafeNativeMethods.sqlite3_column_int64((IntPtr) stmt._sqlite_stmt, index);
        }

        internal override long GetParamValueBytes(IntPtr p, int nDataOffset, byte[] bDest, int nStart, int nLength)
        {
            int length = nLength;
            int num = UnsafeNativeMethods.sqlite3_value_bytes(p);
            IntPtr ptr = UnsafeNativeMethods.sqlite3_value_blob(p);
            if (bDest == null)
            {
                return (long) num;
            }
            if ((length + nStart) > bDest.Length)
            {
                length = bDest.Length - nStart;
            }
            if ((length + nDataOffset) > num)
            {
                length = num - nDataOffset;
            }
            if (length > 0)
            {
                Marshal.Copy((IntPtr) (ptr.ToInt32() + nDataOffset), bDest, nStart, length);
            }
            else
            {
                length = 0;
            }
            return (long) length;
        }

        internal override double GetParamValueDouble(IntPtr ptr)
        {
            return UnsafeNativeMethods.sqlite3_value_double(ptr);
        }

        internal override int GetParamValueInt32(IntPtr ptr)
        {
            return UnsafeNativeMethods.sqlite3_value_int(ptr);
        }

        internal override long GetParamValueInt64(IntPtr ptr)
        {
            return UnsafeNativeMethods.sqlite3_value_int64(ptr);
        }

        internal override string GetParamValueText(IntPtr ptr)
        {
            int num;
            return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_value_text_interop(ptr, out num), num);
        }

        internal override TypeAffinity GetParamValueType(IntPtr ptr)
        {
            return UnsafeNativeMethods.sqlite3_value_type(ptr);
        }

        internal override long GetRowIdForCursor(SQLiteStatement stmt, int cursor)
        {
            long num;
            if (UnsafeNativeMethods.sqlite3_cursor_rowid((IntPtr) stmt._sqlite_stmt, cursor, out num) == 0)
            {
                return num;
            }
            return 0L;
        }

        internal override string GetText(SQLiteStatement stmt, int index)
        {
            int num;
            return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_column_text_interop((IntPtr) stmt._sqlite_stmt, index, out num), num);
        }

        internal override object GetValue(SQLiteStatement stmt, int index, SQLiteType typ)
        {
            if (this.IsNull(stmt, index))
            {
                return DBNull.Value;
            }
            TypeAffinity affinity = typ.Affinity;
            Type type = null;
            if (typ.Type != DbType.Object)
            {
                type = SQLiteConvert.SQLiteTypeToType(typ);
                affinity = SQLiteConvert.TypeToAffinity(type);
            }
            switch (affinity)
            {
                case TypeAffinity.Int64:
                    if (type != null)
                    {
                        return Convert.ChangeType(this.GetInt64(stmt, index), type, null);
                    }
                    return this.GetInt64(stmt, index);

                case TypeAffinity.Double:
                    if (type != null)
                    {
                        return Convert.ChangeType(this.GetDouble(stmt, index), type, null);
                    }
                    return this.GetDouble(stmt, index);

                case TypeAffinity.Blob:
                    if ((typ.Type != DbType.Guid) || (typ.Affinity != TypeAffinity.Text))
                    {
                        int nLength = (int) this.GetBytes(stmt, index, 0, null, 0, 0);
                        byte[] bDest = new byte[nLength];
                        this.GetBytes(stmt, index, 0, bDest, 0, nLength);
                        if ((typ.Type == DbType.Guid) && (nLength == 0x10))
                        {
                            return new Guid(bDest);
                        }
                        return bDest;
                    }
                    return new Guid(this.GetText(stmt, index));

                case TypeAffinity.DateTime:
                    return this.GetDateTime(stmt, index);
            }
            return this.GetText(stmt, index);
        }

        internal override bool IsNull(SQLiteStatement stmt, int index)
        {
            return (this.ColumnAffinity(stmt, index) == TypeAffinity.Null);
        }

        internal override void Open(string strFilename, SQLiteOpenFlagsEnum flags, int maxPoolSize, bool usePool)
        {
            if (this._sql == null)
            {
                this._usePool = usePool;
                if (usePool)
                {
                    this._fileName = strFilename;
                    this._sql = SQLiteConnectionPool.Remove(strFilename, maxPoolSize, out this._poolVersion);
                }
                if (this._sql == null)
                {
                    IntPtr ptr;
                    int errorCode = UnsafeNativeMethods.sqlite3_open_interop(SQLiteConvert.ToUTF8(strFilename), (int) flags, out ptr);
                    if (errorCode > 0)
                    {
                        throw new SQLiteException(errorCode, null);
                    }
                    this._sql = ptr;
                }
                this._functionsArray = SQLiteFunction.BindFunctions(this);
                this.SetTimeout(0);
            }
        }

        internal override SQLiteStatement Prepare(SQLiteConnection cnn, string strSql, SQLiteStatement previous, uint timeoutMS, out string strRemain)
        {
            SQLiteStatement statement2;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptrRemain = IntPtr.Zero;
            int nRemain = 0;
            int errorCode = 0x11;
            int num3 = 0;
            byte[] buffer = SQLiteConvert.ToUTF8(strSql);
            string typedefs = null;
            SQLiteStatement statement = null;
            Random random = null;
            uint tickCount = (uint) Environment.TickCount;
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pSql = handle.AddrOfPinnedObject();
            try
            {
                while ((((errorCode == 0x11) || (errorCode == 6)) || (errorCode == 5)) && (num3 < 3))
                {
                    errorCode = UnsafeNativeMethods.sqlite3_prepare_interop((IntPtr) this._sql, pSql, buffer.Length - 1, out zero, out ptrRemain, out nRemain);
                    if (errorCode == 0x11)
                    {
                        num3++;
                    }
                    else
                    {
                        if (errorCode == 1)
                        {
                            if (string.Compare(this.SQLiteLastError(), "near \"TYPES\": syntax error", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                int index = strSql.IndexOf(';');
                                if (index == -1)
                                {
                                    index = strSql.Length - 1;
                                }
                                typedefs = strSql.Substring(0, index + 1);
                                strSql = strSql.Substring(index + 1);
                                strRemain = "";
                                while ((statement == null) && (strSql.Length > 0))
                                {
                                    statement = this.Prepare(cnn, strSql, previous, timeoutMS, out strRemain);
                                    strSql = strRemain;
                                }
                                if (statement != null)
                                {
                                    statement.SetTypes(typedefs);
                                }
                                return statement;
                            }
                            if (this._buildingSchema || (string.Compare(this.SQLiteLastError(), 0, "no such table: TEMP.SCHEMA", 0, 0x1a, StringComparison.OrdinalIgnoreCase) != 0))
                            {
                                continue;
                            }
                            strRemain = "";
                            this._buildingSchema = true;
                            try
                            {
                                ISQLiteSchemaExtensions service = ((IServiceProvider) SQLiteFactory.Instance).GetService(typeof(ISQLiteSchemaExtensions)) as ISQLiteSchemaExtensions;
                                if (service != null)
                                {
                                    service.BuildTempSchema(cnn);
                                }
                                while ((statement == null) && (strSql.Length > 0))
                                {
                                    statement = this.Prepare(cnn, strSql, previous, timeoutMS, out strRemain);
                                    strSql = strRemain;
                                }
                                return statement;
                            }
                            finally
                            {
                                this._buildingSchema = false;
                            }
                        }
                        switch (errorCode)
                        {
                            case 6:
                            case 5:
                                if (random == null)
                                {
                                    random = new Random();
                                }
                                if ((Environment.TickCount - tickCount) > timeoutMS)
                                {
                                    throw new SQLiteException(errorCode, this.SQLiteLastError());
                                }
                                Thread.Sleep(random.Next(1, 150));
                                break;
                        }
                    }
                }
                if (errorCode > 0)
                {
                    throw new SQLiteException(errorCode, this.SQLiteLastError());
                }
                strRemain = SQLiteConvert.UTF8ToString(ptrRemain, nRemain);
                if (zero != IntPtr.Zero)
                {
                    statement = new SQLiteStatement(this, zero, strSql.Substring(0, strSql.Length - strRemain.Length), previous);
                }
                statement2 = statement;
            }
            finally
            {
                handle.Free();
            }
            return statement2;
        }

        internal override int Reset(SQLiteStatement stmt)
        {
            int errorCode = UnsafeNativeMethods.sqlite3_reset_interop((IntPtr) stmt._sqlite_stmt);
            switch (errorCode)
            {
                case 0x11:
                    string str;
                    using (SQLiteStatement statement = this.Prepare(null, stmt._sqlStatement, null, (uint) (stmt._command._commandTimeout * 0x3e8), out str))
                    {
                        stmt._sqlite_stmt.Dispose();
                        stmt._sqlite_stmt = statement._sqlite_stmt;
                        statement._sqlite_stmt = null;
                        stmt.BindParameters();
                    }
                    return -1;

                case 6:
                case 5:
                    return errorCode;
            }
            if (errorCode > 0)
            {
                throw new SQLiteException(errorCode, this.SQLiteLastError());
            }
            return 0;
        }

        internal override void ReturnBlob(IntPtr context, byte[] value)
        {
            UnsafeNativeMethods.sqlite3_result_blob(context, value, value.Length, (IntPtr) (-1));
        }

        internal override void ReturnDouble(IntPtr context, double value)
        {
            UnsafeNativeMethods.sqlite3_result_double(context, value);
        }

        internal override void ReturnError(IntPtr context, string value)
        {
            UnsafeNativeMethods.sqlite3_result_error(context, SQLiteConvert.ToUTF8(value), value.Length);
        }

        internal override void ReturnInt32(IntPtr context, int value)
        {
            UnsafeNativeMethods.sqlite3_result_int(context, value);
        }

        internal override void ReturnInt64(IntPtr context, long value)
        {
            UnsafeNativeMethods.sqlite3_result_int64(context, value);
        }

        internal override void ReturnNull(IntPtr context)
        {
            UnsafeNativeMethods.sqlite3_result_null(context);
        }

        internal override void ReturnText(IntPtr context, string value)
        {
            byte[] buffer = SQLiteConvert.ToUTF8(value);
            UnsafeNativeMethods.sqlite3_result_text(context, SQLiteConvert.ToUTF8(value), buffer.Length - 1, (IntPtr) (-1));
        }

        internal override void SetCommitHook(SQLiteCommitCallback func)
        {
            UnsafeNativeMethods.sqlite3_commit_hook((IntPtr) this._sql, func, IntPtr.Zero);
        }

        internal override void SetPassword(byte[] passwordBytes)
        {
            int errorCode = UnsafeNativeMethods.sqlite3_key((IntPtr) this._sql, passwordBytes, passwordBytes.Length);
            if (errorCode > 0)
            {
                throw new SQLiteException(errorCode, this.SQLiteLastError());
            }
        }

        internal override void SetRollbackHook(SQLiteRollbackCallback func)
        {
            UnsafeNativeMethods.sqlite3_rollback_hook((IntPtr) this._sql, func, IntPtr.Zero);
        }

        internal override void SetTimeout(int nTimeoutMS)
        {
            int errorCode = UnsafeNativeMethods.sqlite3_busy_timeout((IntPtr) this._sql, nTimeoutMS);
            if (errorCode > 0)
            {
                throw new SQLiteException(errorCode, this.SQLiteLastError());
            }
        }

        internal override void SetUpdateHook(SQLiteUpdateCallback func)
        {
            UnsafeNativeMethods.sqlite3_update_hook((IntPtr) this._sql, func, IntPtr.Zero);
        }

        internal override string SQLiteLastError()
        {
            return SQLiteBase.SQLiteLastError(this._sql);
        }

        internal override bool Step(SQLiteStatement stmt)
        {
            Random random = null;
            uint tickCount = (uint) Environment.TickCount;
            uint num3 = (uint) (stmt._command._commandTimeout * 0x3e8);
            while (true)
            {
                int errorCode = UnsafeNativeMethods.sqlite3_step((IntPtr) stmt._sqlite_stmt);
                switch (errorCode)
                {
                    case 100:
                        return true;

                    case 0x65:
                        return false;
                }
                if (errorCode > 0)
                {
                    int num4 = this.Reset(stmt);
                    if (num4 == 0)
                    {
                        throw new SQLiteException(errorCode, this.SQLiteLastError());
                    }
                    if (((num4 == 6) || (num4 == 5)) && (stmt._command != null))
                    {
                        if (random == null)
                        {
                            random = new Random();
                        }
                        if ((Environment.TickCount - tickCount) > num3)
                        {
                            throw new SQLiteException(num4, this.SQLiteLastError());
                        }
                        Thread.Sleep(random.Next(1, 150));
                    }
                }
            }
        }

        internal override int Changes
        {
            get
            {
                return UnsafeNativeMethods.sqlite3_changes((IntPtr) this._sql);
            }
        }

        internal static string SQLiteVersion
        {
            get
            {
                return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_libversion(), -1);
            }
        }

        internal override string Version
        {
            get
            {
                return SQLiteVersion;
            }
        }
    }
}

