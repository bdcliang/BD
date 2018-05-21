namespace System.Data.SQLite
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.InteropServices;

    internal sealed class SQLiteKeyReader : IDisposable
    {
        private bool _isValid;
        private KeyInfo[] _keyInfo;
        private SQLiteStatement _stmt;

        internal SQLiteKeyReader(SQLiteConnection cnn, SQLiteDataReader reader, SQLiteStatement stmt)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            Dictionary<string, List<string>> dictionary2 = new Dictionary<string, List<string>>();
            List<KeyInfo> list2 = new List<KeyInfo>();
            this._stmt = stmt;
            using (DataTable table = cnn.GetSchema("Catalogs"))
            {
                foreach (DataRow row in table.Rows)
                {
                    dictionary.Add((string) row["CATALOG_NAME"], Convert.ToInt32(row["ID"]));
                }
            }
            using (DataTable table2 = reader.GetSchemaTable(false, false))
            {
                foreach (DataRow row2 in table2.Rows)
                {
                    if (row2[SchemaTableOptionalColumn.BaseCatalogName] != DBNull.Value)
                    {
                        List<string> list;
                        string key = (string) row2[SchemaTableOptionalColumn.BaseCatalogName];
                        string item = (string) row2[SchemaTableColumn.BaseTableName];
                        if (!dictionary2.ContainsKey(key))
                        {
                            list = new List<string>();
                            dictionary2.Add(key, list);
                        }
                        else
                        {
                            list = dictionary2[key];
                        }
                        if (!list.Contains(item))
                        {
                            list.Add(item);
                        }
                    }
                }
                foreach (KeyValuePair<string, List<string>> pair in dictionary2)
                {
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        string str3 = pair.Value[i];
                        DataRow row3 = null;
                        string[] restrictionValues = new string[3];
                        restrictionValues[0] = pair.Key;
                        restrictionValues[2] = str3;
                        using (DataTable table3 = cnn.GetSchema("Indexes", restrictionValues))
                        {
                            for (int j = 0; (j < 2) && (row3 == null); j++)
                            {
                                foreach (DataRow row4 in table3.Rows)
                                {
                                    if ((j == 0) && ((bool) row4["PRIMARY_KEY"]))
                                    {
                                        row3 = row4;
                                        break;
                                    }
                                    if ((j == 1) && ((bool) row4["UNIQUE"]))
                                    {
                                        row3 = row4;
                                        break;
                                    }
                                }
                            }
                            if (row3 == null)
                            {
                                pair.Value.RemoveAt(i);
                                i--;
                            }
                            else
                            {
                                string[] strArray3 = new string[3];
                                strArray3[0] = pair.Key;
                                strArray3[2] = str3;
                                using (DataTable table4 = cnn.GetSchema("Tables", strArray3))
                                {
                                    int database = dictionary[pair.Key];
                                    int rootPage = Convert.ToInt32(table4.Rows[0]["TABLE_ROOTPAGE"]);
                                    int num5 = stmt._sql.GetCursorForTable(stmt, database, rootPage);
                                    string[] strArray4 = new string[4];
                                    strArray4[0] = pair.Key;
                                    strArray4[2] = str3;
                                    strArray4[3] = (string) row3["INDEX_NAME"];
                                    using (DataTable table5 = cnn.GetSchema("IndexColumns", strArray4))
                                    {
                                        KeyQuery query = null;
                                        List<string> list3 = new List<string>();
                                        for (int k = 0; k < table5.Rows.Count; k++)
                                        {
                                            bool flag = true;
                                            foreach (DataRow row5 in table2.Rows)
                                            {
                                                if ((!row5.IsNull(SchemaTableColumn.BaseColumnName) && (((string) row5[SchemaTableColumn.BaseColumnName]) == ((string) table5.Rows[k]["COLUMN_NAME"]))) && ((((string) row5[SchemaTableColumn.BaseTableName]) == str3) && (((string) row5[SchemaTableOptionalColumn.BaseCatalogName]) == pair.Key)))
                                                {
                                                    table5.Rows.RemoveAt(k);
                                                    k--;
                                                    flag = false;
                                                    break;
                                                }
                                            }
                                            if (flag)
                                            {
                                                list3.Add((string) table5.Rows[k]["COLUMN_NAME"]);
                                            }
                                        }
                                        if ((((string) row3["INDEX_NAME"]) != ("sqlite_master_PK_" + str3)) && (list3.Count > 0))
                                        {
                                            string[] array = new string[list3.Count];
                                            list3.CopyTo(array);
                                            query = new KeyQuery(cnn, pair.Key, str3, array);
                                        }
                                        for (int m = 0; m < table5.Rows.Count; m++)
                                        {
                                            string str4 = (string) table5.Rows[m]["COLUMN_NAME"];
                                            KeyInfo info = new KeyInfo {
                                                rootPage = rootPage,
                                                cursor = num5,
                                                database = database,
                                                databaseName = pair.Key,
                                                tableName = str3,
                                                columnName = str4,
                                                query = query,
                                                column = m
                                            };
                                            list2.Add(info);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            this._keyInfo = new KeyInfo[list2.Count];
            list2.CopyTo(this._keyInfo);
        }

        internal void AppendSchemaTable(DataTable tbl)
        {
            KeyQuery query = null;
            for (int i = 0; i < this._keyInfo.Length; i++)
            {
                if ((this._keyInfo[i].query == null) || (this._keyInfo[i].query != query))
                {
                    query = this._keyInfo[i].query;
                    if (query == null)
                    {
                        DataRow row = tbl.NewRow();
                        row[SchemaTableColumn.ColumnName] = this._keyInfo[i].columnName;
                        row[SchemaTableColumn.ColumnOrdinal] = tbl.Rows.Count;
                        row[SchemaTableColumn.ColumnSize] = 8;
                        row[SchemaTableColumn.NumericPrecision] = 0xff;
                        row[SchemaTableColumn.NumericScale] = 0xff;
                        row[SchemaTableColumn.ProviderType] = DbType.Int64;
                        row[SchemaTableColumn.IsLong] = false;
                        row[SchemaTableColumn.AllowDBNull] = false;
                        row[SchemaTableOptionalColumn.IsReadOnly] = false;
                        row[SchemaTableOptionalColumn.IsRowVersion] = false;
                        row[SchemaTableColumn.IsUnique] = false;
                        row[SchemaTableColumn.IsKey] = true;
                        row[SchemaTableColumn.DataType] = typeof(long);
                        row[SchemaTableOptionalColumn.IsHidden] = true;
                        row[SchemaTableColumn.BaseColumnName] = this._keyInfo[i].columnName;
                        row[SchemaTableColumn.IsExpression] = false;
                        row[SchemaTableColumn.IsAliased] = false;
                        row[SchemaTableColumn.BaseTableName] = this._keyInfo[i].tableName;
                        row[SchemaTableOptionalColumn.BaseCatalogName] = this._keyInfo[i].databaseName;
                        row[SchemaTableOptionalColumn.IsAutoIncrement] = true;
                        row["DataTypeName"] = "integer";
                        tbl.Rows.Add(row);
                    }
                    else
                    {
                        query.Sync(0L);
                        using (DataTable table = query._reader.GetSchemaTable())
                        {
                            foreach (DataRow row2 in table.Rows)
                            {
                                object[] itemArray = row2.ItemArray;
                                DataRow row3 = tbl.Rows.Add(itemArray);
                                row3[SchemaTableOptionalColumn.IsHidden] = true;
                                row3[SchemaTableColumn.ColumnOrdinal] = tbl.Rows.Count - 1;
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            this._stmt = null;
            if (this._keyInfo != null)
            {
                for (int i = 0; i < this._keyInfo.Length; i++)
                {
                    if (this._keyInfo[i].query != null)
                    {
                        this._keyInfo[i].query.Dispose();
                    }
                }
                this._keyInfo = null;
            }
        }

        internal bool GetBoolean(int i)
        {
            this.Sync(i);
            if (this._keyInfo[i].query == null)
            {
                throw new InvalidCastException();
            }
            return this._keyInfo[i].query._reader.GetBoolean(this._keyInfo[i].column);
        }

        internal byte GetByte(int i)
        {
            this.Sync(i);
            if (this._keyInfo[i].query == null)
            {
                throw new InvalidCastException();
            }
            return this._keyInfo[i].query._reader.GetByte(this._keyInfo[i].column);
        }

        internal long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            this.Sync(i);
            if (this._keyInfo[i].query == null)
            {
                throw new InvalidCastException();
            }
            return this._keyInfo[i].query._reader.GetBytes(this._keyInfo[i].column, fieldOffset, buffer, bufferoffset, length);
        }

        internal char GetChar(int i)
        {
            this.Sync(i);
            if (this._keyInfo[i].query == null)
            {
                throw new InvalidCastException();
            }
            return this._keyInfo[i].query._reader.GetChar(this._keyInfo[i].column);
        }

        internal long GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length)
        {
            this.Sync(i);
            if (this._keyInfo[i].query == null)
            {
                throw new InvalidCastException();
            }
            return this._keyInfo[i].query._reader.GetChars(this._keyInfo[i].column, fieldOffset, buffer, bufferoffset, length);
        }

        internal string GetDataTypeName(int i)
        {
            this.Sync();
            if (this._keyInfo[i].query != null)
            {
                return this._keyInfo[i].query._reader.GetDataTypeName(this._keyInfo[i].column);
            }
            return "integer";
        }

        internal DateTime GetDateTime(int i)
        {
            this.Sync(i);
            if (this._keyInfo[i].query == null)
            {
                throw new InvalidCastException();
            }
            return this._keyInfo[i].query._reader.GetDateTime(this._keyInfo[i].column);
        }

        internal decimal GetDecimal(int i)
        {
            this.Sync(i);
            if (this._keyInfo[i].query == null)
            {
                throw new InvalidCastException();
            }
            return this._keyInfo[i].query._reader.GetDecimal(this._keyInfo[i].column);
        }

        internal double GetDouble(int i)
        {
            this.Sync(i);
            if (this._keyInfo[i].query == null)
            {
                throw new InvalidCastException();
            }
            return this._keyInfo[i].query._reader.GetDouble(this._keyInfo[i].column);
        }

        internal Type GetFieldType(int i)
        {
            this.Sync();
            if (this._keyInfo[i].query != null)
            {
                return this._keyInfo[i].query._reader.GetFieldType(this._keyInfo[i].column);
            }
            return typeof(long);
        }

        internal float GetFloat(int i)
        {
            this.Sync(i);
            if (this._keyInfo[i].query == null)
            {
                throw new InvalidCastException();
            }
            return this._keyInfo[i].query._reader.GetFloat(this._keyInfo[i].column);
        }

        internal Guid GetGuid(int i)
        {
            this.Sync(i);
            if (this._keyInfo[i].query == null)
            {
                throw new InvalidCastException();
            }
            return this._keyInfo[i].query._reader.GetGuid(this._keyInfo[i].column);
        }

        internal short GetInt16(int i)
        {
            this.Sync(i);
            if (this._keyInfo[i].query != null)
            {
                return this._keyInfo[i].query._reader.GetInt16(this._keyInfo[i].column);
            }
            long rowIdForCursor = this._stmt._sql.GetRowIdForCursor(this._stmt, this._keyInfo[i].cursor);
            if (rowIdForCursor == 0L)
            {
                throw new InvalidCastException();
            }
            return Convert.ToInt16(rowIdForCursor);
        }

        internal int GetInt32(int i)
        {
            this.Sync(i);
            if (this._keyInfo[i].query != null)
            {
                return this._keyInfo[i].query._reader.GetInt32(this._keyInfo[i].column);
            }
            long rowIdForCursor = this._stmt._sql.GetRowIdForCursor(this._stmt, this._keyInfo[i].cursor);
            if (rowIdForCursor == 0L)
            {
                throw new InvalidCastException();
            }
            return Convert.ToInt32(rowIdForCursor);
        }

        internal long GetInt64(int i)
        {
            this.Sync(i);
            if (this._keyInfo[i].query != null)
            {
                return this._keyInfo[i].query._reader.GetInt64(this._keyInfo[i].column);
            }
            long rowIdForCursor = this._stmt._sql.GetRowIdForCursor(this._stmt, this._keyInfo[i].cursor);
            if (rowIdForCursor == 0L)
            {
                throw new InvalidCastException();
            }
            return Convert.ToInt64(rowIdForCursor);
        }

        internal string GetName(int i)
        {
            return this._keyInfo[i].columnName;
        }

        internal int GetOrdinal(string name)
        {
            for (int i = 0; i < this._keyInfo.Length; i++)
            {
                if (string.Compare(name, this._keyInfo[i].columnName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        internal string GetString(int i)
        {
            this.Sync(i);
            if (this._keyInfo[i].query == null)
            {
                throw new InvalidCastException();
            }
            return this._keyInfo[i].query._reader.GetString(this._keyInfo[i].column);
        }

        internal object GetValue(int i)
        {
            if (this._keyInfo[i].cursor == -1)
            {
                return DBNull.Value;
            }
            this.Sync(i);
            if (this._keyInfo[i].query != null)
            {
                return this._keyInfo[i].query._reader.GetValue(this._keyInfo[i].column);
            }
            if (this.IsDBNull(i))
            {
                return DBNull.Value;
            }
            return this.GetInt64(i);
        }

        internal bool IsDBNull(int i)
        {
            if (this._keyInfo[i].cursor == -1)
            {
                return true;
            }
            this.Sync(i);
            if (this._keyInfo[i].query != null)
            {
                return this._keyInfo[i].query._reader.IsDBNull(this._keyInfo[i].column);
            }
            return (this._stmt._sql.GetRowIdForCursor(this._stmt, this._keyInfo[i].cursor) == 0L);
        }

        internal void Reset()
        {
            this._isValid = false;
            if (this._keyInfo != null)
            {
                for (int i = 0; i < this._keyInfo.Length; i++)
                {
                    if (this._keyInfo[i].query != null)
                    {
                        this._keyInfo[i].query.IsValid = false;
                    }
                }
            }
        }

        internal void Sync()
        {
            if (!this._isValid)
            {
                KeyQuery query = null;
                for (int i = 0; i < this._keyInfo.Length; i++)
                {
                    if ((this._keyInfo[i].query == null) || (this._keyInfo[i].query != query))
                    {
                        query = this._keyInfo[i].query;
                        if (query != null)
                        {
                            query.Sync(this._stmt._sql.GetRowIdForCursor(this._stmt, this._keyInfo[i].cursor));
                        }
                    }
                }
                this._isValid = true;
            }
        }

        internal void Sync(int i)
        {
            this.Sync();
            if (this._keyInfo[i].cursor == -1)
            {
                throw new InvalidCastException();
            }
        }

        internal int Count
        {
            get
            {
                if (this._keyInfo != null)
                {
                    return this._keyInfo.Length;
                }
                return 0;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyInfo
        {
            internal string databaseName;
            internal string tableName;
            internal string columnName;
            internal int database;
            internal int rootPage;
            internal int cursor;
            internal SQLiteKeyReader.KeyQuery query;
            internal int column;
        }

        private sealed class KeyQuery : IDisposable
        {
            private SQLiteCommand _command;
            internal SQLiteDataReader _reader;

            internal KeyQuery(SQLiteConnection cnn, string database, string table, params string[] columns)
            {
                using (SQLiteCommandBuilder builder = new SQLiteCommandBuilder())
                {
                    this._command = cnn.CreateCommand();
                    for (int i = 0; i < columns.Length; i++)
                    {
                        columns[i] = builder.QuoteIdentifier(columns[i]);
                    }
                }
                this._command.CommandText = string.Format("SELECT {0} FROM [{1}].[{2}] WHERE ROWID = ?", string.Join(",", columns), database, table);
                this._command.Parameters.AddWithValue(null, 0L);
            }

            public void Dispose()
            {
                this.IsValid = false;
                if (this._command != null)
                {
                    this._command.Dispose();
                }
                this._command = null;
            }

            internal void Sync(long rowid)
            {
                this.IsValid = false;
                this._command.Parameters[0].Value = rowid;
                this._reader = this._command.ExecuteReader();
                this._reader.Read();
            }

            internal bool IsValid
            {
                get
                {
                    return (this._reader != null);
                }
                set
                {
                    if (value)
                    {
                        throw new ArgumentException();
                    }
                    if (this._reader != null)
                    {
                        this._reader.Dispose();
                        this._reader = null;
                    }
                }
            }
        }
    }
}

