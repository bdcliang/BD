namespace System.Data.SQLite
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;

    public sealed class SQLiteDataReader : DbDataReader
    {
        private SQLiteStatement _activeStatement;
        private int _activeStatementIndex;
        private SQLiteCommand _command;
        private CommandBehavior _commandBehavior;
        internal bool _disposeCommand;
        private int _fieldCount;
        private SQLiteType[] _fieldTypeArray;
        private SQLiteKeyReader _keyInfo;
        private int _readingState;
        private int _rowsAffected;
        internal long _version;

        internal SQLiteDataReader(SQLiteCommand cmd, CommandBehavior behave)
        {
            this._command = cmd;
            this._version = this._command.Connection._version;
            this._commandBehavior = behave;
            this._activeStatementIndex = -1;
            this._activeStatement = null;
            this._rowsAffected = -1;
            this._fieldCount = 0;
            if (this._command != null)
            {
                this.NextResult();
            }
        }

        internal void Cancel()
        {
            this._version = 0L;
        }

        private void CheckClosed()
        {
            if (this._command == null)
            {
                throw new InvalidOperationException("DataReader has been closed");
            }
            if (this._version == 0L)
            {
                throw new SQLiteException(4, "Execution was aborted by the user");
            }
            if ((this._command.Connection.State != ConnectionState.Open) || (this._command.Connection._version != this._version))
            {
                throw new InvalidOperationException("Connection was closed, statement was terminated");
            }
        }

        private void CheckValidRow()
        {
            if (this._readingState != 0)
            {
                throw new InvalidOperationException("No current row");
            }
        }

        public override void Close()
        {
            try
            {
                if (this._command != null)
                {
                    try
                    {
                        try
                        {
                            if (this._version != 0L)
                            {
                                try
                                {
                                    while (this.NextResult())
                                    {
                                    }
                                }
                                catch
                                {
                                }
                            }
                            this._command.ClearDataReader();
                        }
                        finally
                        {
                            if (((this._commandBehavior & CommandBehavior.CloseConnection) != CommandBehavior.Default) && (this._command.Connection != null))
                            {
                                this._command.Connection.Close();
                            }
                        }
                    }
                    finally
                    {
                        if (this._disposeCommand)
                        {
                            this._command.Dispose();
                        }
                    }
                }
                this._command = null;
                this._activeStatement = null;
                this._fieldTypeArray = null;
            }
            finally
            {
                if (this._keyInfo != null)
                {
                    this._keyInfo.Dispose();
                    this._keyInfo = null;
                }
            }
        }

        public override bool GetBoolean(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetBoolean(i - this.VisibleFieldCount);
            }
            this.VerifyType(i, DbType.Boolean);
            return Convert.ToBoolean(this.GetValue(i), CultureInfo.CurrentCulture);
        }

        public override byte GetByte(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetByte(i - this.VisibleFieldCount);
            }
            this.VerifyType(i, DbType.Byte);
            return Convert.ToByte(this._activeStatement._sql.GetInt32(this._activeStatement, i));
        }

        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetBytes(i - this.VisibleFieldCount, fieldOffset, buffer, bufferoffset, length);
            }
            this.VerifyType(i, DbType.Binary);
            return this._activeStatement._sql.GetBytes(this._activeStatement, i, (int) fieldOffset, buffer, bufferoffset, length);
        }

        public override char GetChar(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetChar(i - this.VisibleFieldCount);
            }
            this.VerifyType(i, DbType.SByte);
            return Convert.ToChar(this._activeStatement._sql.GetInt32(this._activeStatement, i));
        }

        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetChars(i - this.VisibleFieldCount, fieldoffset, buffer, bufferoffset, length);
            }
            this.VerifyType(i, DbType.String);
            return this._activeStatement._sql.GetChars(this._activeStatement, i, (int) fieldoffset, buffer, bufferoffset, length);
        }

        public override string GetDataTypeName(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetDataTypeName(i - this.VisibleFieldCount);
            }
            SQLiteType sQLiteType = this.GetSQLiteType(i);
            if (sQLiteType.Type == DbType.Object)
            {
                return SQLiteConvert.SQLiteTypeToType(sQLiteType).Name;
            }
            return this._activeStatement._sql.ColumnType(this._activeStatement, i, out sQLiteType.Affinity);
        }

        public override DateTime GetDateTime(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetDateTime(i - this.VisibleFieldCount);
            }
            this.VerifyType(i, DbType.DateTime);
            return this._activeStatement._sql.GetDateTime(this._activeStatement, i);
        }

        public override decimal GetDecimal(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetDecimal(i - this.VisibleFieldCount);
            }
            this.VerifyType(i, DbType.Decimal);
            return decimal.Parse(this._activeStatement._sql.GetText(this._activeStatement, i), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        }

        public override double GetDouble(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetDouble(i - this.VisibleFieldCount);
            }
            this.VerifyType(i, DbType.Double);
            return this._activeStatement._sql.GetDouble(this._activeStatement, i);
        }

        public override IEnumerator GetEnumerator()
        {
            return new DbEnumerator(this, (this._commandBehavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection);
        }

        public override Type GetFieldType(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetFieldType(i - this.VisibleFieldCount);
            }
            return SQLiteConvert.SQLiteTypeToType(this.GetSQLiteType(i));
        }

        public override float GetFloat(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetFloat(i - this.VisibleFieldCount);
            }
            this.VerifyType(i, DbType.Single);
            return Convert.ToSingle(this._activeStatement._sql.GetDouble(this._activeStatement, i));
        }

        public override Guid GetGuid(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetGuid(i - this.VisibleFieldCount);
            }
            if (this.VerifyType(i, DbType.Guid) == TypeAffinity.Blob)
            {
                byte[] bDest = new byte[0x10];
                this._activeStatement._sql.GetBytes(this._activeStatement, i, 0, bDest, 0, 0x10);
                return new Guid(bDest);
            }
            return new Guid(this._activeStatement._sql.GetText(this._activeStatement, i));
        }

        public override short GetInt16(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetInt16(i - this.VisibleFieldCount);
            }
            this.VerifyType(i, DbType.Int16);
            return Convert.ToInt16(this._activeStatement._sql.GetInt32(this._activeStatement, i));
        }

        public override int GetInt32(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetInt32(i - this.VisibleFieldCount);
            }
            this.VerifyType(i, DbType.Int32);
            return this._activeStatement._sql.GetInt32(this._activeStatement, i);
        }

        public override long GetInt64(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetInt64(i - this.VisibleFieldCount);
            }
            this.VerifyType(i, DbType.Int64);
            return this._activeStatement._sql.GetInt64(this._activeStatement, i);
        }

        public override string GetName(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetName(i - this.VisibleFieldCount);
            }
            return this._activeStatement._sql.ColumnName(this._activeStatement, i);
        }

        public override int GetOrdinal(string name)
        {
            this.CheckClosed();
            int ordinal = this._activeStatement._sql.ColumnIndex(this._activeStatement, name);
            if ((ordinal == -1) && (this._keyInfo != null))
            {
                ordinal = this._keyInfo.GetOrdinal(name);
                if (ordinal > -1)
                {
                    ordinal += this.VisibleFieldCount;
                }
            }
            return ordinal;
        }

        public override DataTable GetSchemaTable()
        {
            return this.GetSchemaTable(true, false);
        }

        internal DataTable GetSchemaTable(bool wantUniqueInfo, bool wantDefaultValue)
        {
            this.CheckClosed();
            DataTable tbl = new DataTable("SchemaTable");
            DataTable schema = null;
            string str2 = "";
            string str3 = "";
            string str4 = "";
            tbl.Locale = CultureInfo.InvariantCulture;
            tbl.Columns.Add(SchemaTableColumn.ColumnName, typeof(string));
            tbl.Columns.Add(SchemaTableColumn.ColumnOrdinal, typeof(int));
            tbl.Columns.Add(SchemaTableColumn.ColumnSize, typeof(int));
            tbl.Columns.Add(SchemaTableColumn.NumericPrecision, typeof(short));
            tbl.Columns.Add(SchemaTableColumn.NumericScale, typeof(short));
            tbl.Columns.Add(SchemaTableColumn.IsUnique, typeof(bool));
            tbl.Columns.Add(SchemaTableColumn.IsKey, typeof(bool));
            tbl.Columns.Add(SchemaTableOptionalColumn.BaseServerName, typeof(string));
            tbl.Columns.Add(SchemaTableOptionalColumn.BaseCatalogName, typeof(string));
            tbl.Columns.Add(SchemaTableColumn.BaseColumnName, typeof(string));
            tbl.Columns.Add(SchemaTableColumn.BaseSchemaName, typeof(string));
            tbl.Columns.Add(SchemaTableColumn.BaseTableName, typeof(string));
            tbl.Columns.Add(SchemaTableColumn.DataType, typeof(Type));
            tbl.Columns.Add(SchemaTableColumn.AllowDBNull, typeof(bool));
            tbl.Columns.Add(SchemaTableColumn.ProviderType, typeof(int));
            tbl.Columns.Add(SchemaTableColumn.IsAliased, typeof(bool));
            tbl.Columns.Add(SchemaTableColumn.IsExpression, typeof(bool));
            tbl.Columns.Add(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool));
            tbl.Columns.Add(SchemaTableOptionalColumn.IsRowVersion, typeof(bool));
            tbl.Columns.Add(SchemaTableOptionalColumn.IsHidden, typeof(bool));
            tbl.Columns.Add(SchemaTableColumn.IsLong, typeof(bool));
            tbl.Columns.Add(SchemaTableOptionalColumn.IsReadOnly, typeof(bool));
            tbl.Columns.Add(SchemaTableOptionalColumn.ProviderSpecificDataType, typeof(Type));
            tbl.Columns.Add(SchemaTableOptionalColumn.DefaultValue, typeof(object));
            tbl.Columns.Add("DataTypeName", typeof(string));
            tbl.Columns.Add("CollationType", typeof(string));
            tbl.BeginLoadData();
            for (int i = 0; i < this._fieldCount; i++)
            {
                string str6;
                bool flag;
                bool flag2;
                bool flag3;
                DataRow row = tbl.NewRow();
                DbType typ = this.GetSQLiteType(i).Type;
                row[SchemaTableColumn.ColumnName] = this.GetName(i);
                row[SchemaTableColumn.ColumnOrdinal] = i;
                row[SchemaTableColumn.ColumnSize] = SQLiteConvert.DbTypeToColumnSize(typ);
                row[SchemaTableColumn.NumericPrecision] = SQLiteConvert.DbTypeToNumericPrecision(typ);
                row[SchemaTableColumn.NumericScale] = SQLiteConvert.DbTypeToNumericScale(typ);
                row[SchemaTableColumn.ProviderType] = this.GetSQLiteType(i).Type;
                row[SchemaTableColumn.IsLong] = false;
                row[SchemaTableColumn.AllowDBNull] = true;
                row[SchemaTableOptionalColumn.IsReadOnly] = false;
                row[SchemaTableOptionalColumn.IsRowVersion] = false;
                row[SchemaTableColumn.IsUnique] = false;
                row[SchemaTableColumn.IsKey] = false;
                row[SchemaTableOptionalColumn.IsAutoIncrement] = false;
                row[SchemaTableColumn.DataType] = this.GetFieldType(i);
                row[SchemaTableOptionalColumn.IsHidden] = false;
                str4 = this._command.Connection._sql.ColumnOriginalName(this._activeStatement, i);
                if (!string.IsNullOrEmpty(str4))
                {
                    row[SchemaTableColumn.BaseColumnName] = str4;
                }
                row[SchemaTableColumn.IsExpression] = string.IsNullOrEmpty(str4);
                row[SchemaTableColumn.IsAliased] = string.Compare(this.GetName(i), str4, true, CultureInfo.InvariantCulture) != 0;
                string str = this._command.Connection._sql.ColumnTableName(this._activeStatement, i);
                if (!string.IsNullOrEmpty(str))
                {
                    row[SchemaTableColumn.BaseTableName] = str;
                }
                str = this._command.Connection._sql.ColumnDatabaseName(this._activeStatement, i);
                if (!string.IsNullOrEmpty(str))
                {
                    row[SchemaTableOptionalColumn.BaseCatalogName] = str;
                }
                string dataType = null;
                if (string.IsNullOrEmpty(str4))
                {
                    goto Label_09B7;
                }
                this._command.Connection._sql.ColumnMetaData((string) row[SchemaTableOptionalColumn.BaseCatalogName], (string) row[SchemaTableColumn.BaseTableName], str4, out dataType, out str6, out flag, out flag2, out flag3);
                if (flag || flag2)
                {
                    row[SchemaTableColumn.AllowDBNull] = false;
                }
                row[SchemaTableColumn.IsKey] = flag2;
                row[SchemaTableOptionalColumn.IsAutoIncrement] = flag3;
                row["CollationType"] = str6;
                string[] strArray = dataType.Split(new char[] { '(' });
                if (strArray.Length > 1)
                {
                    dataType = strArray[0];
                    strArray = strArray[1].Split(new char[] { ')' });
                    if (strArray.Length > 1)
                    {
                        strArray = strArray[0].Split(new char[] { ',', '.' });
                        if ((this.GetSQLiteType(i).Type == DbType.String) || (this.GetSQLiteType(i).Type == DbType.Binary))
                        {
                            row[SchemaTableColumn.ColumnSize] = Convert.ToInt32(strArray[0], CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            row[SchemaTableColumn.NumericPrecision] = Convert.ToInt32(strArray[0], CultureInfo.InvariantCulture);
                            if (strArray.Length > 1)
                            {
                                row[SchemaTableColumn.NumericScale] = Convert.ToInt32(strArray[1], CultureInfo.InvariantCulture);
                            }
                        }
                    }
                }
                if (wantDefaultValue)
                {
                    using (SQLiteCommand command = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "PRAGMA [{0}].TABLE_INFO([{1}])", new object[] { row[SchemaTableOptionalColumn.BaseCatalogName], row[SchemaTableColumn.BaseTableName] }), this._command.Connection))
                    {
                        using (DbDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (string.Compare((string) row[SchemaTableColumn.BaseColumnName], reader.GetString(1), true, CultureInfo.InvariantCulture) == 0)
                                {
                                    if (!reader.IsDBNull(4))
                                    {
                                        row[SchemaTableOptionalColumn.DefaultValue] = reader[4];
                                    }
                                    goto Label_075E;
                                }
                            }
                        }
                    }
                }
            Label_075E:
                if (wantUniqueInfo)
                {
                    if ((((string) row[SchemaTableOptionalColumn.BaseCatalogName]) != str2) || (((string) row[SchemaTableColumn.BaseTableName]) != str3))
                    {
                        str2 = (string) row[SchemaTableOptionalColumn.BaseCatalogName];
                        str3 = (string) row[SchemaTableColumn.BaseTableName];
                        string[] restrictionValues = new string[4];
                        restrictionValues[0] = (string) row[SchemaTableOptionalColumn.BaseCatalogName];
                        restrictionValues[2] = (string) row[SchemaTableColumn.BaseTableName];
                        schema = this._command.Connection.GetSchema("Indexes", restrictionValues);
                    }
                    foreach (DataRow row2 in schema.Rows)
                    {
                        string[] strArray3 = new string[5];
                        strArray3[0] = (string) row[SchemaTableOptionalColumn.BaseCatalogName];
                        strArray3[2] = (string) row[SchemaTableColumn.BaseTableName];
                        strArray3[3] = (string) row2["INDEX_NAME"];
                        DataTable table3 = this._command.Connection.GetSchema("IndexColumns", strArray3);
                        foreach (DataRow row3 in table3.Rows)
                        {
                            if (string.Compare((string) row3["COLUMN_NAME"], str4, true, CultureInfo.InvariantCulture) == 0)
                            {
                                if ((table3.Rows.Count == 1) && !((bool) row[SchemaTableColumn.AllowDBNull]))
                                {
                                    row[SchemaTableColumn.IsUnique] = row2["UNIQUE"];
                                }
                                if (((table3.Rows.Count != 1) || !((bool) row2["PRIMARY_KEY"])) || (string.IsNullOrEmpty(dataType) || (string.Compare(dataType, "integer", true, CultureInfo.InvariantCulture) != 0)))
                                {
                                }
                                break;
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(dataType))
                {
                    TypeAffinity affinity;
                    dataType = this._activeStatement._sql.ColumnType(this._activeStatement, i, out affinity);
                }
                if (!string.IsNullOrEmpty(dataType))
                {
                    row["DataTypeName"] = dataType;
                }
            Label_09B7:
                tbl.Rows.Add(row);
            }
            if (this._keyInfo != null)
            {
                this._keyInfo.AppendSchemaTable(tbl);
            }
            tbl.AcceptChanges();
            tbl.EndLoadData();
            return tbl;
        }

        private SQLiteType GetSQLiteType(int i)
        {
            if (this._fieldTypeArray == null)
            {
                this._fieldTypeArray = new SQLiteType[this.VisibleFieldCount];
            }
            if (this._fieldTypeArray[i] == null)
            {
                this._fieldTypeArray[i] = new SQLiteType();
            }
            SQLiteType type = this._fieldTypeArray[i];
            if (type.Affinity == TypeAffinity.Uninitialized)
            {
                type.Type = SQLiteConvert.TypeNameToDbType(this._activeStatement._sql.ColumnType(this._activeStatement, i, out type.Affinity));
                return type;
            }
            type.Affinity = this._activeStatement._sql.ColumnAffinity(this._activeStatement, i);
            return type;
        }

        public override string GetString(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetString(i - this.VisibleFieldCount);
            }
            this.VerifyType(i, DbType.String);
            return this._activeStatement._sql.GetText(this._activeStatement, i);
        }

        public override object GetValue(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.GetValue(i - this.VisibleFieldCount);
            }
            SQLiteType sQLiteType = this.GetSQLiteType(i);
            return this._activeStatement._sql.GetValue(this._activeStatement, i, sQLiteType);
        }

        public override int GetValues(object[] values)
        {
            int fieldCount = this.FieldCount;
            if (values.Length < fieldCount)
            {
                fieldCount = values.Length;
            }
            for (int i = 0; i < fieldCount; i++)
            {
                values[i] = this.GetValue(i);
            }
            return fieldCount;
        }

        public override bool IsDBNull(int i)
        {
            if ((i >= this.VisibleFieldCount) && (this._keyInfo != null))
            {
                return this._keyInfo.IsDBNull(i - this.VisibleFieldCount);
            }
            return this._activeStatement._sql.IsNull(this._activeStatement, i);
        }

        private void LoadKeyInfo()
        {
            if (this._keyInfo != null)
            {
                this._keyInfo.Dispose();
            }
            this._keyInfo = new SQLiteKeyReader(this._command.Connection, this, this._activeStatement);
        }

        public override bool NextResult()
        {
            this.CheckClosed();
            SQLiteStatement stmt = null;
        Label_0008:
            if ((this._activeStatement != null) && (stmt == null))
            {
                this._activeStatement._sql.Reset(this._activeStatement);
                if ((this._commandBehavior & CommandBehavior.SingleResult) != CommandBehavior.Default)
                {
                    while (true)
                    {
                        stmt = this._command.GetStatement(this._activeStatementIndex + 1);
                        if (stmt == null)
                        {
                            return false;
                        }
                        this._activeStatementIndex++;
                        stmt._sql.Step(stmt);
                        if (stmt._sql.ColumnCount(stmt) == 0)
                        {
                            if (this._rowsAffected == -1)
                            {
                                this._rowsAffected = 0;
                            }
                            this._rowsAffected += stmt._sql.Changes;
                        }
                        stmt._sql.Reset(stmt);
                    }
                }
            }
            stmt = this._command.GetStatement(this._activeStatementIndex + 1);
            if (stmt == null)
            {
                return false;
            }
            if (this._readingState < 1)
            {
                this._readingState = 1;
            }
            this._activeStatementIndex++;
            int num = stmt._sql.ColumnCount(stmt);
            if (((this._commandBehavior & CommandBehavior.SchemaOnly) == CommandBehavior.Default) || (num == 0))
            {
                if (stmt._sql.Step(stmt))
                {
                    this._readingState = -1;
                }
                else
                {
                    if (num == 0)
                    {
                        if (this._rowsAffected == -1)
                        {
                            this._rowsAffected = 0;
                        }
                        this._rowsAffected += stmt._sql.Changes;
                        stmt._sql.Reset(stmt);
                        goto Label_0008;
                    }
                    this._readingState = 1;
                }
            }
            this._activeStatement = stmt;
            this._fieldCount = num;
            this._fieldTypeArray = null;
            if ((this._commandBehavior & CommandBehavior.KeyInfo) != CommandBehavior.Default)
            {
                this.LoadKeyInfo();
            }
            return true;
        }

        public override bool Read()
        {
            this.CheckClosed();
            if (this._readingState == -1)
            {
                this._readingState = 0;
                return true;
            }
            if (this._readingState == 0)
            {
                if (((this._commandBehavior & CommandBehavior.SingleRow) == CommandBehavior.Default) && this._activeStatement._sql.Step(this._activeStatement))
                {
                    if (this._keyInfo != null)
                    {
                        this._keyInfo.Reset();
                    }
                    return true;
                }
                this._readingState = 1;
            }
            return false;
        }

        private TypeAffinity VerifyType(int i, DbType typ)
        {
            this.CheckClosed();
            this.CheckValidRow();
            TypeAffinity affinity = this.GetSQLiteType(i).Affinity;
            switch (affinity)
            {
                case TypeAffinity.Int64:
                    if (typ != DbType.Int16)
                    {
                        if (typ != DbType.Int32)
                        {
                            if (typ == DbType.Int64)
                            {
                                return affinity;
                            }
                            if (typ == DbType.Boolean)
                            {
                                return affinity;
                            }
                            if (typ == DbType.Byte)
                            {
                                return affinity;
                            }
                            if (typ == DbType.DateTime)
                            {
                                return affinity;
                            }
                            if (typ == DbType.Single)
                            {
                                return affinity;
                            }
                            if (typ == DbType.Double)
                            {
                                return affinity;
                            }
                            if (typ != DbType.Decimal)
                            {
                                break;
                            }
                        }
                        return affinity;
                    }
                    return affinity;

                case TypeAffinity.Double:
                    if (typ != DbType.Single)
                    {
                        if (typ != DbType.Double)
                        {
                            if (typ == DbType.Decimal)
                            {
                                return affinity;
                            }
                            if (typ != DbType.DateTime)
                            {
                                break;
                            }
                        }
                        return affinity;
                    }
                    return affinity;

                case TypeAffinity.Text:
                    if (typ != DbType.SByte)
                    {
                        if (typ != DbType.String)
                        {
                            if (typ == DbType.SByte)
                            {
                                return affinity;
                            }
                            if (typ == DbType.Guid)
                            {
                                return affinity;
                            }
                            if (typ == DbType.DateTime)
                            {
                                return affinity;
                            }
                            if (typ != DbType.Decimal)
                            {
                                break;
                            }
                        }
                        return affinity;
                    }
                    return affinity;

                case TypeAffinity.Blob:
                    if (typ != DbType.Guid)
                    {
                        if (typ == DbType.String)
                        {
                            return affinity;
                        }
                        if (typ == DbType.Binary)
                        {
                            return affinity;
                        }
                        break;
                    }
                    return affinity;
            }
            throw new InvalidCastException();
        }

        public override int Depth
        {
            get
            {
                this.CheckClosed();
                return 0;
            }
        }

        public override int FieldCount
        {
            get
            {
                this.CheckClosed();
                if (this._keyInfo == null)
                {
                    return this._fieldCount;
                }
                return (this._fieldCount + this._keyInfo.Count);
            }
        }

        public override bool HasRows
        {
            get
            {
                this.CheckClosed();
                return (this._readingState != 1);
            }
        }

        public override bool IsClosed
        {
            get
            {
                return (this._command == null);
            }
        }

        public override object this[string name]
        {
            get
            {
                return this.GetValue(this.GetOrdinal(name));
            }
        }

        public override object this[int i]
        {
            get
            {
                return this.GetValue(i);
            }
        }

        public override int RecordsAffected
        {
            get
            {
                if (this._rowsAffected >= 0)
                {
                    return this._rowsAffected;
                }
                return 0;
            }
        }

        public override int VisibleFieldCount
        {
            get
            {
                this.CheckClosed();
                return this._fieldCount;
            }
        }
    }
}

