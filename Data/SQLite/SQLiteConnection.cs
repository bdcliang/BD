namespace System.Data.SQLite
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Transactions;

    public sealed class SQLiteConnection : DbConnection, ICloneable
    {
        internal bool _binaryGuid;
        private SQLiteCommitCallback _commitCallback;
        private ConnectionState _connectionState;
        private string _connectionString;
        private const string _dataDirectory = "|DataDirectory|";
        private string _dataSource;
        private System.Data.IsolationLevel _defaultIsolation;
        private int _defaultTimeout;
        internal SQLiteEnlistment _enlistment;
        private const string _masterdb = "sqlite_master";
        private byte[] _password;
        private SQLiteRollbackCallback _rollbackCallback;
        internal SQLiteBase _sql;
        private const string _tempmasterdb = "sqlite_temp_master";
        internal int _transactionLevel;
        private SQLiteUpdateCallback _updateCallback;
        internal long _version;

        private event SQLiteCommitHandler _commitHandler;

        private event EventHandler _rollbackHandler;

        private event SQLiteUpdateEventHandler _updateHandler;

        public event SQLiteCommitHandler Commit
        {
            add
            {
                if (this._commitHandler == null)
                {
                    this._commitCallback = new SQLiteCommitCallback(this.CommitCallback);
                    if (this._sql != null)
                    {
                        this._sql.SetCommitHook(this._commitCallback);
                    }
                }
                this._commitHandler = (SQLiteCommitHandler) Delegate.Combine(this._commitHandler, value);
            }
            remove
            {
                this._commitHandler = (SQLiteCommitHandler) Delegate.Remove(this._commitHandler, value);
                if (this._commitHandler == null)
                {
                    if (this._sql != null)
                    {
                        this._sql.SetCommitHook(null);
                    }
                    this._commitCallback = null;
                }
            }
        }

        public event EventHandler RollBack
        {
            add
            {
                if (this._rollbackHandler == null)
                {
                    this._rollbackCallback = new SQLiteRollbackCallback(this.RollbackCallback);
                    if (this._sql != null)
                    {
                        this._sql.SetRollbackHook(this._rollbackCallback);
                    }
                }
                this._rollbackHandler = (EventHandler) Delegate.Combine(this._rollbackHandler, value);
            }
            remove
            {
                this._rollbackHandler = (EventHandler) Delegate.Remove(this._rollbackHandler, value);
                if (this._rollbackHandler == null)
                {
                    if (this._sql != null)
                    {
                        this._sql.SetRollbackHook(null);
                    }
                    this._rollbackCallback = null;
                }
            }
        }

        public event StateChangeEventHandler StateChange;

        public event SQLiteUpdateEventHandler Update
        {
            add
            {
                if (this._updateHandler == null)
                {
                    this._updateCallback = new SQLiteUpdateCallback(this.UpdateCallback);
                    if (this._sql != null)
                    {
                        this._sql.SetUpdateHook(this._updateCallback);
                    }
                }
                this._updateHandler = (SQLiteUpdateEventHandler) Delegate.Combine(this._updateHandler, value);
            }
            remove
            {
                this._updateHandler = (SQLiteUpdateEventHandler) Delegate.Remove(this._updateHandler, value);
                if (this._updateHandler == null)
                {
                    if (this._sql != null)
                    {
                        this._sql.SetUpdateHook(null);
                    }
                    this._updateCallback = null;
                }
            }
        }

        public SQLiteConnection() : this("")
        {
        }

        public SQLiteConnection(SQLiteConnection connection) : this(connection.ConnectionString)
        {
            if (connection.State == ConnectionState.Open)
            {
                this.Open();
                using (DataTable table = connection.GetSchema("Catalogs"))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        string strA = row[0].ToString();
                        if ((string.Compare(strA, "main", true, CultureInfo.InvariantCulture) != 0) && (string.Compare(strA, "temp", true, CultureInfo.InvariantCulture) != 0))
                        {
                            using (SQLiteCommand command = this.CreateCommand())
                            {
                                command.CommandText = string.Format(CultureInfo.InvariantCulture, "ATTACH DATABASE '{0}' AS [{1}]", new object[] { row[1], row[0] });
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        public SQLiteConnection(string connectionString)
        {
            this._defaultTimeout = 30;
            this._sql = null;
            this._connectionState = ConnectionState.Closed;
            this._connectionString = "";
            this._transactionLevel = 0;
            this._version = 0L;
            if (connectionString != null)
            {
                this.ConnectionString = connectionString;
            }
        }

        protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel)
        {
            if (this._connectionState != ConnectionState.Open)
            {
                throw new InvalidOperationException();
            }
            if (isolationLevel == System.Data.IsolationLevel.Unspecified)
            {
                isolationLevel = this._defaultIsolation;
            }
            if ((isolationLevel != System.Data.IsolationLevel.Serializable) && (isolationLevel != System.Data.IsolationLevel.ReadCommitted))
            {
                throw new ArgumentException("isolationLevel");
            }
            return new SQLiteTransaction(this, isolationLevel != System.Data.IsolationLevel.Serializable);
        }

        public SQLiteTransaction BeginTransaction()
        {
            return (SQLiteTransaction) this.BeginDbTransaction(this._defaultIsolation);
        }

        [Obsolete("Use one of the standard BeginTransaction methods, this one will be removed soon")]
        public SQLiteTransaction BeginTransaction(bool deferredLock)
        {
            return (SQLiteTransaction) this.BeginDbTransaction(!deferredLock ? System.Data.IsolationLevel.Serializable : System.Data.IsolationLevel.ReadCommitted);
        }

        public SQLiteTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            return (SQLiteTransaction) this.BeginDbTransaction(isolationLevel);
        }

        [Obsolete("Use one of the standard BeginTransaction methods, this one will be removed soon")]
        public SQLiteTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel, bool deferredLock)
        {
            return (SQLiteTransaction) this.BeginDbTransaction(!deferredLock ? System.Data.IsolationLevel.Serializable : System.Data.IsolationLevel.ReadCommitted);
        }

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public void ChangePassword(string newPassword)
        {
            this.ChangePassword(string.IsNullOrEmpty(newPassword) ? null : Encoding.UTF8.GetBytes(newPassword));
        }

        public void ChangePassword(byte[] newPassword)
        {
            if (this._connectionState != ConnectionState.Open)
            {
                throw new InvalidOperationException("Database must be opened before changing the password.");
            }
            this._sql.ChangePassword(newPassword);
        }

        public static void ClearAllPools()
        {
            SQLiteConnectionPool.ClearAllPools();
        }

        public static void ClearPool(SQLiteConnection connection)
        {
            if (connection._sql != null)
            {
                connection._sql.ClearPool();
            }
        }

        public object Clone()
        {
            return new SQLiteConnection(this);
        }

        public override void Close()
        {
            if (this._sql != null)
            {
                if (this._enlistment != null)
                {
                    SQLiteConnection connection = new SQLiteConnection {
                        _sql = this._sql,
                        _transactionLevel = this._transactionLevel,
                        _enlistment = this._enlistment,
                        _connectionState = this._connectionState,
                        _version = this._version
                    };
                    connection._enlistment._transaction._cnn = connection;
                    connection._enlistment._disposeConnection = true;
                    this._sql = null;
                    this._enlistment = null;
                }
                if (this._sql != null)
                {
                    this._sql.Close();
                }
                this._sql = null;
                this._transactionLevel = 0;
            }
            this.OnStateChange(ConnectionState.Closed);
        }

        private int CommitCallback(IntPtr parg)
        {
            CommitEventArgs e = new CommitEventArgs();
            this._commitHandler(this, e);
            if (!e.AbortTransaction)
            {
                return 0;
            }
            return 1;
        }

        [Obsolete("This functionality is being removed from a future version of the SQLite provider")]
        public static void CompressFile(string databaseFileName)
        {
            System.Data.SQLite.UnsafeNativeMethods.sqlite3_compressfile(databaseFileName);
        }

        public SQLiteCommand CreateCommand()
        {
            return new SQLiteCommand(this);
        }

        protected override DbCommand CreateDbCommand()
        {
            return this.CreateCommand();
        }

        public static void CreateFile(string databaseFileName)
        {
            File.Create(databaseFileName).Close();
        }

        [Obsolete("This functionality is being removed from a future version of the SQLite provider")]
        public static void DecompressFile(string databaseFileName)
        {
            System.Data.SQLite.UnsafeNativeMethods.sqlite3_decompressfile(databaseFileName);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.Close();
            }
        }

        public override void EnlistTransaction(Transaction transaction)
        {
            if ((this._transactionLevel > 0) && (transaction != null))
            {
                throw new ArgumentException("Unable to enlist in transaction, a local transaction already exists");
            }
            if ((this._enlistment != null) && (transaction != this._enlistment._scope))
            {
                throw new ArgumentException("Already enlisted in a transaction");
            }
            this._enlistment = new SQLiteEnlistment(this, transaction);
        }

        private string ExpandFileName(string sourceFile)
        {
            if (!string.IsNullOrEmpty(sourceFile))
            {
                if (sourceFile.StartsWith("|DataDirectory|", StringComparison.OrdinalIgnoreCase))
                {
                    string data = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                    if (string.IsNullOrEmpty(data))
                    {
                        data = AppDomain.CurrentDomain.BaseDirectory;
                    }
                    if ((sourceFile.Length > "|DataDirectory|".Length) && ((sourceFile["|DataDirectory|".Length] == Path.DirectorySeparatorChar) || (sourceFile["|DataDirectory|".Length] == Path.AltDirectorySeparatorChar)))
                    {
                        sourceFile = sourceFile.Remove("|DataDirectory|".Length, 1);
                    }
                    sourceFile = Path.Combine(data, sourceFile.Substring("|DataDirectory|".Length));
                }
                sourceFile = Path.GetFullPath(sourceFile);
            }
            return sourceFile;
        }

        internal static string FindKey(SortedList<string, string> items, string key, string defValue)
        {
            string str;
            if (items.TryGetValue(key, out str))
            {
                return str;
            }
            return defValue;
        }

        public override DataTable GetSchema()
        {
            return this.GetSchema("MetaDataCollections", null);
        }

        public override DataTable GetSchema(string collectionName)
        {
            return this.GetSchema(collectionName, new string[0]);
        }

        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            if (this._connectionState != ConnectionState.Open)
            {
                throw new InvalidOperationException();
            }
            string[] array = new string[5];
            if (restrictionValues == null)
            {
                restrictionValues = new string[0];
            }
            restrictionValues.CopyTo(array, 0);
            switch (collectionName.ToUpper(CultureInfo.InvariantCulture))
            {
                case "METADATACOLLECTIONS":
                    return Schema_MetaDataCollections();

                case "DATASOURCEINFORMATION":
                    return this.Schema_DataSourceInformation();

                case "DATATYPES":
                    return this.Schema_DataTypes();

                case "COLUMNS":
                case "TABLECOLUMNS":
                    return this.Schema_Columns(array[0], array[2], array[3]);

                case "INDEXES":
                    return this.Schema_Indexes(array[0], array[2], array[3]);

                case "TRIGGERS":
                    return this.Schema_Triggers(array[0], array[2], array[3]);

                case "INDEXCOLUMNS":
                    return this.Schema_IndexColumns(array[0], array[2], array[3], array[4]);

                case "TABLES":
                    return this.Schema_Tables(array[0], array[2], array[3]);

                case "VIEWS":
                    return this.Schema_Views(array[0], array[2]);

                case "VIEWCOLUMNS":
                    return this.Schema_ViewColumns(array[0], array[2], array[3]);

                case "FOREIGNKEYS":
                    return this.Schema_ForeignKeys(array[0], array[2], array[3]);

                case "CATALOGS":
                    return this.Schema_Catalogs(array[0]);

                case "RESERVEDWORDS":
                    return Schema_ReservedWords();
            }
            throw new NotSupportedException();
        }

        internal static string MapUriPath(string path)
        {
            if (path.StartsWith("file://"))
            {
                return path.Substring(7);
            }
            if (path.StartsWith("file:"))
            {
                return path.Substring(5);
            }
            if (!path.StartsWith("/"))
            {
                throw new InvalidOperationException("Invalid connection string: invalid URI");
            }
            return path;
        }

        internal void OnStateChange(ConnectionState newState)
        {
            ConnectionState originalState = this._connectionState;
            this._connectionState = newState;
            if ((this.StateChange != null) && (originalState != newState))
            {
                StateChangeEventArgs e = new StateChangeEventArgs(originalState, newState);
                this.StateChange(this, e);
            }
        }

        public override void Open()
        {
            if (this._connectionState != ConnectionState.Closed)
            {
                throw new InvalidOperationException();
            }
            this.Close();
            SortedList<string, string> items = ParseConnectionString(this._connectionString);
            if (Convert.ToInt32(FindKey(items, "Version", "3"), CultureInfo.InvariantCulture) != 3)
            {
                throw new NotSupportedException("Only SQLite Version 3 is supported at this time");
            }
            string str = FindKey(items, "Data Source", "");
            if (string.IsNullOrEmpty(str))
            {
                str = FindKey(items, "Uri", "");
                if (string.IsNullOrEmpty(str))
                {
                    throw new ArgumentException("Data Source cannot be empty.  Use :memory: to open an in-memory database");
                }
                str = MapUriPath(str);
            }
            if (string.Compare(str, ":MEMORY:", true, CultureInfo.InvariantCulture) == 0)
            {
                str = ":memory:";
            }
            else
            {
                str = this.ExpandFileName(str);
            }
            try
            {
                bool usePool = SQLiteConvert.ToBoolean(FindKey(items, "Pooling", bool.FalseString));
                bool flag2 = SQLiteConvert.ToBoolean(FindKey(items, "UseUTF16Encoding", bool.FalseString));
                int maxPoolSize = Convert.ToInt32(FindKey(items, "Max Pool Size", "100"));
                this._defaultTimeout = Convert.ToInt32(FindKey(items, "Default Timeout", "30"), CultureInfo.CurrentCulture);
                this._defaultIsolation = (System.Data.IsolationLevel) Enum.Parse(typeof(System.Data.IsolationLevel), FindKey(items, "Default IsolationLevel", "Serializable"), true);
                if ((this._defaultIsolation != System.Data.IsolationLevel.Serializable) && (this._defaultIsolation != System.Data.IsolationLevel.ReadCommitted))
                {
                    throw new NotSupportedException("Invalid Default IsolationLevel specified");
                }
                SQLiteDateFormats fmt = (SQLiteDateFormats) Enum.Parse(typeof(SQLiteDateFormats), FindKey(items, "DateTimeFormat", "ISO8601"), true);
                if (flag2)
                {
                    this._sql = new SQLite3_UTF16(fmt);
                }
                else
                {
                    this._sql = new SQLite3(fmt);
                }
                SQLiteOpenFlagsEnum none = SQLiteOpenFlagsEnum.None;
                if (!SQLiteConvert.ToBoolean(FindKey(items, "FailIfMissing", bool.FalseString)))
                {
                    none |= SQLiteOpenFlagsEnum.Create;
                }
                if (SQLiteConvert.ToBoolean(FindKey(items, "Read Only", bool.FalseString)))
                {
                    none |= SQLiteOpenFlagsEnum.ReadOnly;
                }
                else
                {
                    none |= SQLiteOpenFlagsEnum.ReadWrite;
                }
                this._sql.Open(str, none, maxPoolSize, usePool);
                this._binaryGuid = SQLiteConvert.ToBoolean(FindKey(items, "BinaryGUID", bool.TrueString));
                string str2 = FindKey(items, "Password", null);
                if (!string.IsNullOrEmpty(str2))
                {
                    this._sql.SetPassword(Encoding.UTF8.GetBytes(str2));
                }
                else if (this._password != null)
                {
                    this._sql.SetPassword(this._password);
                }
                this._password = null;
                this._dataSource = Path.GetFileNameWithoutExtension(str);
                this.OnStateChange(ConnectionState.Open);
                this._version += 1L;
                using (SQLiteCommand command = this.CreateCommand())
                {
                    string str3;
                    if (str != ":memory:")
                    {
                        str3 = FindKey(items, "Page Size", "1024");
                        if (Convert.ToInt32(str3, CultureInfo.InvariantCulture) != 0x400)
                        {
                            command.CommandText = string.Format(CultureInfo.InvariantCulture, "PRAGMA page_size={0}", new object[] { str3 });
                            command.ExecuteNonQuery();
                        }
                    }
                    str3 = FindKey(items, "Max Page Count", "0");
                    if (Convert.ToInt32(str3, CultureInfo.InvariantCulture) != 0)
                    {
                        command.CommandText = string.Format(CultureInfo.InvariantCulture, "PRAGMA max_page_count={0}", new object[] { str3 });
                        command.ExecuteNonQuery();
                    }
                    str3 = FindKey(items, "Legacy Format", bool.FalseString);
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, "PRAGMA legacy_file_format={0}", new object[] { SQLiteConvert.ToBoolean(str3) ? "ON" : "OFF" });
                    command.ExecuteNonQuery();
                    str3 = FindKey(items, "Synchronous", "Normal");
                    if (string.Compare(str3, "Full", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        command.CommandText = string.Format(CultureInfo.InvariantCulture, "PRAGMA synchronous={0}", new object[] { str3 });
                        command.ExecuteNonQuery();
                    }
                    str3 = FindKey(items, "Cache Size", "2000");
                    if (Convert.ToInt32(str3, CultureInfo.InvariantCulture) != 0x7d0)
                    {
                        command.CommandText = string.Format(CultureInfo.InvariantCulture, "PRAGMA cache_size={0}", new object[] { str3 });
                        command.ExecuteNonQuery();
                    }
                    str3 = FindKey(items, "Journal Mode", "Delete");
                    if (string.Compare(str3, "Default", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        command.CommandText = string.Format(CultureInfo.InvariantCulture, "PRAGMA journal_mode={0}", new object[] { str3 });
                        command.ExecuteNonQuery();
                    }
                }
                if (this._commitHandler != null)
                {
                    this._sql.SetCommitHook(this._commitCallback);
                }
                if (this._updateHandler != null)
                {
                    this._sql.SetUpdateHook(this._updateCallback);
                }
                if (this._rollbackHandler != null)
                {
                    this._sql.SetRollbackHook(this._rollbackCallback);
                }
                if ((Transaction.Current != null) && SQLiteConvert.ToBoolean(FindKey(items, "Enlist", bool.TrueString)))
                {
                    this.EnlistTransaction(Transaction.Current);
                }
            }
            catch (SQLiteException)
            {
                this.Close();
                throw;
            }
        }

        internal static SortedList<string, string> ParseConnectionString(string connectionString)
        {
            string source = connectionString;
            SortedList<string, string> list = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);
            string[] strArray = SQLiteConvert.Split(source, ';');
            int length = strArray.Length;
            for (int i = 0; i < length; i++)
            {
                string[] strArray2 = SQLiteConvert.Split(strArray[i], '=');
                if (strArray2.Length != 2)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Invalid ConnectionString format for parameter \"{0}\"", new object[] { (strArray2.Length > 0) ? strArray2[0] : "null" }));
                }
                list.Add(strArray2[0], strArray2[1]);
            }
            return list;
        }

        private void RollbackCallback(IntPtr parg)
        {
            this._rollbackHandler(this, EventArgs.Empty);
        }

        private DataTable Schema_Catalogs(string strCatalog)
        {
            DataTable table = new DataTable("Catalogs") {
                Locale = CultureInfo.InvariantCulture
            };
            table.Columns.Add("CATALOG_NAME", typeof(string));
            table.Columns.Add("DESCRIPTION", typeof(string));
            table.Columns.Add("ID", typeof(long));
            table.BeginLoadData();
            using (SQLiteCommand command = new SQLiteCommand("PRAGMA database_list", this))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if ((string.Compare(reader.GetString(1), strCatalog, true, CultureInfo.InvariantCulture) == 0) || (strCatalog == null))
                        {
                            DataRow row = table.NewRow();
                            row["CATALOG_NAME"] = reader.GetString(1);
                            row["DESCRIPTION"] = reader.GetString(2);
                            row["ID"] = reader.GetInt64(0);
                            table.Rows.Add(row);
                        }
                    }
                }
            }
            table.AcceptChanges();
            table.EndLoadData();
            return table;
        }

        private DataTable Schema_Columns(string strCatalog, string strTable, string strColumn)
        {
            DataTable table = new DataTable("Columns") {
                Locale = CultureInfo.InvariantCulture
            };
            table.Columns.Add("TABLE_CATALOG", typeof(string));
            table.Columns.Add("TABLE_SCHEMA", typeof(string));
            table.Columns.Add("TABLE_NAME", typeof(string));
            table.Columns.Add("COLUMN_NAME", typeof(string));
            table.Columns.Add("COLUMN_GUID", typeof(Guid));
            table.Columns.Add("COLUMN_PROPID", typeof(long));
            table.Columns.Add("ORDINAL_POSITION", typeof(int));
            table.Columns.Add("COLUMN_HASDEFAULT", typeof(bool));
            table.Columns.Add("COLUMN_DEFAULT", typeof(string));
            table.Columns.Add("COLUMN_FLAGS", typeof(long));
            table.Columns.Add("IS_NULLABLE", typeof(bool));
            table.Columns.Add("DATA_TYPE", typeof(string));
            table.Columns.Add("TYPE_GUID", typeof(Guid));
            table.Columns.Add("CHARACTER_MAXIMUM_LENGTH", typeof(int));
            table.Columns.Add("CHARACTER_OCTET_LENGTH", typeof(int));
            table.Columns.Add("NUMERIC_PRECISION", typeof(int));
            table.Columns.Add("NUMERIC_SCALE", typeof(int));
            table.Columns.Add("DATETIME_PRECISION", typeof(long));
            table.Columns.Add("CHARACTER_SET_CATALOG", typeof(string));
            table.Columns.Add("CHARACTER_SET_SCHEMA", typeof(string));
            table.Columns.Add("CHARACTER_SET_NAME", typeof(string));
            table.Columns.Add("COLLATION_CATALOG", typeof(string));
            table.Columns.Add("COLLATION_SCHEMA", typeof(string));
            table.Columns.Add("COLLATION_NAME", typeof(string));
            table.Columns.Add("DOMAIN_CATALOG", typeof(string));
            table.Columns.Add("DOMAIN_NAME", typeof(string));
            table.Columns.Add("DESCRIPTION", typeof(string));
            table.Columns.Add("PRIMARY_KEY", typeof(bool));
            table.Columns.Add("EDM_TYPE", typeof(string));
            table.Columns.Add("AUTOINCREMENT", typeof(bool));
            table.Columns.Add("UNIQUE", typeof(bool));
            table.BeginLoadData();
            if (string.IsNullOrEmpty(strCatalog))
            {
                strCatalog = "main";
            }
            string str = (string.Compare(strCatalog, "temp", true, CultureInfo.InvariantCulture) == 0) ? "sqlite_temp_master" : "sqlite_master";
            using (SQLiteCommand command = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'table' OR [type] LIKE 'view'", new object[] { strCatalog, str }), this))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (string.IsNullOrEmpty(strTable) || (string.Compare(strTable, reader.GetString(2), true, CultureInfo.InvariantCulture) == 0))
                        {
                            try
                            {
                                using (SQLiteCommand command2 = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}]", new object[] { strCatalog, reader.GetString(2) }), this))
                                {
                                    using (SQLiteDataReader reader2 = command2.ExecuteReader(CommandBehavior.SchemaOnly))
                                    {
                                        using (DataTable table2 = reader2.GetSchemaTable(true, true))
                                        {
                                            foreach (DataRow row2 in table2.Rows)
                                            {
                                                if ((string.Compare(row2[SchemaTableColumn.ColumnName].ToString(), strColumn, true, CultureInfo.InvariantCulture) == 0) || (strColumn == null))
                                                {
                                                    DataRow row = table.NewRow();
                                                    row["NUMERIC_PRECISION"] = row2[SchemaTableColumn.NumericPrecision];
                                                    row["NUMERIC_SCALE"] = row2[SchemaTableColumn.NumericScale];
                                                    row["TABLE_NAME"] = reader.GetString(2);
                                                    row["COLUMN_NAME"] = row2[SchemaTableColumn.ColumnName];
                                                    row["TABLE_CATALOG"] = strCatalog;
                                                    row["ORDINAL_POSITION"] = row2[SchemaTableColumn.ColumnOrdinal];
                                                    row["COLUMN_HASDEFAULT"] = row2[SchemaTableOptionalColumn.DefaultValue] != DBNull.Value;
                                                    row["COLUMN_DEFAULT"] = row2[SchemaTableOptionalColumn.DefaultValue];
                                                    row["IS_NULLABLE"] = row2[SchemaTableColumn.AllowDBNull];
                                                    row["DATA_TYPE"] = row2["DataTypeName"].ToString().ToLower(CultureInfo.InvariantCulture);
                                                    row["EDM_TYPE"] = SQLiteConvert.DbTypeToTypeName((DbType) row2[SchemaTableColumn.ProviderType]).ToString().ToLower(CultureInfo.InvariantCulture);
                                                    row["CHARACTER_MAXIMUM_LENGTH"] = row2[SchemaTableColumn.ColumnSize];
                                                    row["TABLE_SCHEMA"] = row2[SchemaTableColumn.BaseSchemaName];
                                                    row["PRIMARY_KEY"] = row2[SchemaTableColumn.IsKey];
                                                    row["AUTOINCREMENT"] = row2[SchemaTableOptionalColumn.IsAutoIncrement];
                                                    row["COLLATION_NAME"] = row2["CollationType"];
                                                    row["UNIQUE"] = row2[SchemaTableColumn.IsUnique];
                                                    table.Rows.Add(row);
                                                }
                                            }
                                        }
                                        continue;
                                    }
                                }
                            }
                            catch (SQLiteException)
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            table.AcceptChanges();
            table.EndLoadData();
            return table;
        }

        private DataTable Schema_DataSourceInformation()
        {
            DataTable table = new DataTable("DataSourceInformation") {
                Locale = CultureInfo.InvariantCulture
            };
            table.Columns.Add(DbMetaDataColumnNames.CompositeIdentifierSeparatorPattern, typeof(string));
            table.Columns.Add(DbMetaDataColumnNames.DataSourceProductName, typeof(string));
            table.Columns.Add(DbMetaDataColumnNames.DataSourceProductVersion, typeof(string));
            table.Columns.Add(DbMetaDataColumnNames.DataSourceProductVersionNormalized, typeof(string));
            table.Columns.Add(DbMetaDataColumnNames.GroupByBehavior, typeof(int));
            table.Columns.Add(DbMetaDataColumnNames.IdentifierPattern, typeof(string));
            table.Columns.Add(DbMetaDataColumnNames.IdentifierCase, typeof(int));
            table.Columns.Add(DbMetaDataColumnNames.OrderByColumnsInSelect, typeof(bool));
            table.Columns.Add(DbMetaDataColumnNames.ParameterMarkerFormat, typeof(string));
            table.Columns.Add(DbMetaDataColumnNames.ParameterMarkerPattern, typeof(string));
            table.Columns.Add(DbMetaDataColumnNames.ParameterNameMaxLength, typeof(int));
            table.Columns.Add(DbMetaDataColumnNames.ParameterNamePattern, typeof(string));
            table.Columns.Add(DbMetaDataColumnNames.QuotedIdentifierPattern, typeof(string));
            table.Columns.Add(DbMetaDataColumnNames.QuotedIdentifierCase, typeof(int));
            table.Columns.Add(DbMetaDataColumnNames.StatementSeparatorPattern, typeof(string));
            table.Columns.Add(DbMetaDataColumnNames.StringLiteralPattern, typeof(string));
            table.Columns.Add(DbMetaDataColumnNames.SupportedJoinOperators, typeof(int));
            table.BeginLoadData();
            DataRow row = table.NewRow();
            object[] objArray = new object[0x11];
            objArray[1] = "SQLite";
            objArray[2] = this._sql.Version;
            objArray[3] = this._sql.Version;
            objArray[4] = 3;
            objArray[5] = "(^\\[\\p{Lo}\\p{Lu}\\p{Ll}_@#][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Nd}@$#_]*$)|(^\\[[^\\]\\0]|\\]\\]+\\]$)|(^\\\"[^\\\"\\0]|\\\"\\\"+\\\"$)";
            objArray[6] = 1;
            objArray[7] = false;
            objArray[8] = "{0}";
            objArray[9] = @"@[\p{Lo}\p{Lu}\p{Ll}\p{Lm}_@#][\p{Lo}\p{Lu}\p{Ll}\p{Lm}\p{Nd}\uff3f_@#\$]*(?=\s+|$)";
            objArray[10] = 0xff;
            objArray[11] = @"^[\p{Lo}\p{Lu}\p{Ll}\p{Lm}_@#][\p{Lo}\p{Lu}\p{Ll}\p{Lm}\p{Nd}\uff3f_@#\$]*(?=\s+|$)";
            objArray[12] = @"(([^\[]|\]\])*)";
            objArray[13] = 1;
            objArray[14] = ";";
            objArray[15] = "'(([^']|'')*)'";
            objArray[0x10] = 15;
            row.ItemArray = objArray;
            table.Rows.Add(row);
            table.AcceptChanges();
            table.EndLoadData();
            return table;
        }

        private DataTable Schema_DataTypes()
        {
            DataTable table = new DataTable("DataTypes") {
                Locale = CultureInfo.InvariantCulture
            };
            table.Columns.Add("TypeName", typeof(string));
            table.Columns.Add("ProviderDbType", typeof(int));
            table.Columns.Add("ColumnSize", typeof(long));
            table.Columns.Add("CreateFormat", typeof(string));
            table.Columns.Add("CreateParameters", typeof(string));
            table.Columns.Add("DataType", typeof(string));
            table.Columns.Add("IsAutoIncrementable", typeof(bool));
            table.Columns.Add("IsBestMatch", typeof(bool));
            table.Columns.Add("IsCaseSensitive", typeof(bool));
            table.Columns.Add("IsFixedLength", typeof(bool));
            table.Columns.Add("IsFixedPrecisionScale", typeof(bool));
            table.Columns.Add("IsLong", typeof(bool));
            table.Columns.Add("IsNullable", typeof(bool));
            table.Columns.Add("IsSearchable", typeof(bool));
            table.Columns.Add("IsSearchableWithLike", typeof(bool));
            table.Columns.Add("IsLiteralSupported", typeof(bool));
            table.Columns.Add("LiteralPrefix", typeof(string));
            table.Columns.Add("LiteralSuffix", typeof(string));
            table.Columns.Add("IsUnsigned", typeof(bool));
            table.Columns.Add("MaximumScale", typeof(short));
            table.Columns.Add("MinimumScale", typeof(short));
            table.Columns.Add("IsConcurrencyType", typeof(bool));
            table.BeginLoadData();
            StringReader reader = new StringReader(System.Data.SQLite.SR.DataTypes);
            table.ReadXml(reader);
            reader.Close();
            table.AcceptChanges();
            table.EndLoadData();
            return table;
        }

        private DataTable Schema_ForeignKeys(string strCatalog, string strTable, string strKeyName)
        {
            DataTable table = new DataTable("ForeignKeys") {
                Locale = CultureInfo.InvariantCulture
            };
            table.Columns.Add("CONSTRAINT_CATALOG", typeof(string));
            table.Columns.Add("CONSTRAINT_SCHEMA", typeof(string));
            table.Columns.Add("CONSTRAINT_NAME", typeof(string));
            table.Columns.Add("TABLE_CATALOG", typeof(string));
            table.Columns.Add("TABLE_SCHEMA", typeof(string));
            table.Columns.Add("TABLE_NAME", typeof(string));
            table.Columns.Add("CONSTRAINT_TYPE", typeof(string));
            table.Columns.Add("IS_DEFERRABLE", typeof(bool));
            table.Columns.Add("INITIALLY_DEFERRED", typeof(bool));
            table.Columns.Add("FKEY_FROM_COLUMN", typeof(string));
            table.Columns.Add("FKEY_FROM_ORDINAL_POSITION", typeof(int));
            table.Columns.Add("FKEY_TO_CATALOG", typeof(string));
            table.Columns.Add("FKEY_TO_SCHEMA", typeof(string));
            table.Columns.Add("FKEY_TO_TABLE", typeof(string));
            table.Columns.Add("FKEY_TO_COLUMN", typeof(string));
            if (string.IsNullOrEmpty(strCatalog))
            {
                strCatalog = "main";
            }
            string str = (string.Compare(strCatalog, "temp", true, CultureInfo.InvariantCulture) == 0) ? "sqlite_temp_master" : "sqlite_master";
            table.BeginLoadData();
            using (SQLiteCommand command = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'table'", new object[] { strCatalog, str }), this))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (string.IsNullOrEmpty(strTable) || (string.Compare(strTable, reader.GetString(2), true, CultureInfo.InvariantCulture) == 0))
                        {
                            try
                            {
                                using (SQLiteCommandBuilder builder = new SQLiteCommandBuilder())
                                {
                                    using (SQLiteCommand command2 = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "PRAGMA [{0}].foreign_key_list([{1}])", new object[] { strCatalog, reader.GetString(2) }), this))
                                    {
                                        using (SQLiteDataReader reader2 = command2.ExecuteReader())
                                        {
                                            while (reader2.Read())
                                            {
                                                DataRow row = table.NewRow();
                                                row["CONSTRAINT_CATALOG"] = strCatalog;
                                                row["CONSTRAINT_NAME"] = string.Format(CultureInfo.InvariantCulture, "FK_{0}_{1}", new object[] { reader[2], reader2.GetInt32(0) });
                                                row["TABLE_CATALOG"] = strCatalog;
                                                row["TABLE_NAME"] = builder.UnquoteIdentifier(reader.GetString(2));
                                                row["CONSTRAINT_TYPE"] = "FOREIGN KEY";
                                                row["IS_DEFERRABLE"] = false;
                                                row["INITIALLY_DEFERRED"] = false;
                                                row["FKEY_FROM_COLUMN"] = builder.UnquoteIdentifier(reader2[3].ToString());
                                                row["FKEY_TO_CATALOG"] = strCatalog;
                                                row["FKEY_TO_TABLE"] = builder.UnquoteIdentifier(reader2[2].ToString());
                                                row["FKEY_TO_COLUMN"] = builder.UnquoteIdentifier(reader2[4].ToString());
                                                row["FKEY_FROM_ORDINAL_POSITION"] = reader2[1];
                                                if (string.IsNullOrEmpty(strKeyName) || (string.Compare(strKeyName, row["CONSTRAINT_NAME"].ToString(), true, CultureInfo.InvariantCulture) == 0))
                                                {
                                                    table.Rows.Add(row);
                                                }
                                            }
                                        }
                                        continue;
                                    }
                                }
                            }
                            catch (SQLiteException)
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            table.EndLoadData();
            table.AcceptChanges();
            return table;
        }

        private DataTable Schema_IndexColumns(string strCatalog, string strTable, string strIndex, string strColumn)
        {
            DataTable table = new DataTable("IndexColumns");
            List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
            table.Locale = CultureInfo.InvariantCulture;
            table.Columns.Add("CONSTRAINT_CATALOG", typeof(string));
            table.Columns.Add("CONSTRAINT_SCHEMA", typeof(string));
            table.Columns.Add("CONSTRAINT_NAME", typeof(string));
            table.Columns.Add("TABLE_CATALOG", typeof(string));
            table.Columns.Add("TABLE_SCHEMA", typeof(string));
            table.Columns.Add("TABLE_NAME", typeof(string));
            table.Columns.Add("COLUMN_NAME", typeof(string));
            table.Columns.Add("ORDINAL_POSITION", typeof(int));
            table.Columns.Add("INDEX_NAME", typeof(string));
            table.Columns.Add("COLLATION_NAME", typeof(string));
            table.Columns.Add("SORT_MODE", typeof(string));
            table.Columns.Add("CONFLICT_OPTION", typeof(int));
            if (string.IsNullOrEmpty(strCatalog))
            {
                strCatalog = "main";
            }
            string str = (string.Compare(strCatalog, "temp", true, CultureInfo.InvariantCulture) == 0) ? "sqlite_temp_master" : "sqlite_master";
            table.BeginLoadData();
            using (SQLiteCommand command = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'table'", new object[] { strCatalog, str }), this))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bool flag = false;
                        list.Clear();
                        if (string.IsNullOrEmpty(strTable) || (string.Compare(reader.GetString(2), strTable, true, CultureInfo.InvariantCulture) == 0))
                        {
                            DataRow row;
                            try
                            {
                                using (SQLiteCommand command2 = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "PRAGMA [{0}].table_info([{1}])", new object[] { strCatalog, reader.GetString(2) }), this))
                                {
                                    using (SQLiteDataReader reader2 = command2.ExecuteReader())
                                    {
                                        while (reader2.Read())
                                        {
                                            if (reader2.GetInt32(5) == 1)
                                            {
                                                list.Add(new KeyValuePair<int, string>(reader2.GetInt32(0), reader2.GetString(1)));
                                                if (string.Compare(reader2.GetString(2), "INTEGER", true, CultureInfo.InvariantCulture) == 0)
                                                {
                                                    flag = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (SQLiteException)
                            {
                            }
                            if ((list.Count == 1) && flag)
                            {
                                row = table.NewRow();
                                row["CONSTRAINT_CATALOG"] = strCatalog;
                                row["CONSTRAINT_NAME"] = string.Format(CultureInfo.InvariantCulture, "{1}_PK_{0}", new object[] { reader.GetString(2), str });
                                row["TABLE_CATALOG"] = strCatalog;
                                row["TABLE_NAME"] = reader.GetString(2);
                                KeyValuePair<int, string> pair = list[0];
                                row["COLUMN_NAME"] = pair.Value;
                                row["INDEX_NAME"] = row["CONSTRAINT_NAME"];
                                row["ORDINAL_POSITION"] = 0;
                                row["COLLATION_NAME"] = "BINARY";
                                row["SORT_MODE"] = "ASC";
                                row["CONFLICT_OPTION"] = 2;
                                if (string.IsNullOrEmpty(strIndex) || (string.Compare(strIndex, (string) row["INDEX_NAME"], true, CultureInfo.InvariantCulture) == 0))
                                {
                                    table.Rows.Add(row);
                                }
                            }
                            using (SQLiteCommand command3 = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{2}] WHERE [type] LIKE 'index' AND [tbl_name] LIKE '{1}'", new object[] { strCatalog, reader.GetString(2).Replace("'", "''"), str }), this))
                            {
                                using (SQLiteDataReader reader3 = command3.ExecuteReader())
                                {
                                    while (reader3.Read())
                                    {
                                        int num = 0;
                                        if (string.IsNullOrEmpty(strIndex) || (string.Compare(strIndex, reader3.GetString(1), true, CultureInfo.InvariantCulture) == 0))
                                        {
                                            try
                                            {
                                                using (SQLiteCommand command4 = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "PRAGMA [{0}].index_info([{1}])", new object[] { strCatalog, reader3.GetString(1) }), this))
                                                {
                                                    using (SQLiteDataReader reader4 = command4.ExecuteReader())
                                                    {
                                                        while (reader4.Read())
                                                        {
                                                            string str2;
                                                            int num2;
                                                            int num3;
                                                            row = table.NewRow();
                                                            row["CONSTRAINT_CATALOG"] = strCatalog;
                                                            row["CONSTRAINT_NAME"] = reader3.GetString(1);
                                                            row["TABLE_CATALOG"] = strCatalog;
                                                            row["TABLE_NAME"] = reader3.GetString(2);
                                                            row["COLUMN_NAME"] = reader4.GetString(2);
                                                            row["INDEX_NAME"] = reader3.GetString(1);
                                                            row["ORDINAL_POSITION"] = num;
                                                            this._sql.GetIndexColumnExtendedInfo(strCatalog, reader3.GetString(1), reader4.GetString(2), out num2, out num3, out str2);
                                                            if (!string.IsNullOrEmpty(str2))
                                                            {
                                                                row["COLLATION_NAME"] = str2;
                                                            }
                                                            row["SORT_MODE"] = (num2 == 0) ? "ASC" : "DESC";
                                                            row["CONFLICT_OPTION"] = num3;
                                                            num++;
                                                            if (string.IsNullOrEmpty(strColumn) || (string.Compare(strColumn, row["COLUMN_NAME"].ToString(), true, CultureInfo.InvariantCulture) == 0))
                                                            {
                                                                table.Rows.Add(row);
                                                            }
                                                        }
                                                    }
                                                }
                                                continue;
                                            }
                                            catch (SQLiteException)
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                }
                                continue;
                            }
                        }
                    }
                }
            }
            table.EndLoadData();
            table.AcceptChanges();
            return table;
        }

        private DataTable Schema_Indexes(string strCatalog, string strTable, string strIndex)
        {
            DataTable table = new DataTable("Indexes");
            List<int> list = new List<int>();
            table.Locale = CultureInfo.InvariantCulture;
            table.Columns.Add("TABLE_CATALOG", typeof(string));
            table.Columns.Add("TABLE_SCHEMA", typeof(string));
            table.Columns.Add("TABLE_NAME", typeof(string));
            table.Columns.Add("INDEX_CATALOG", typeof(string));
            table.Columns.Add("INDEX_SCHEMA", typeof(string));
            table.Columns.Add("INDEX_NAME", typeof(string));
            table.Columns.Add("PRIMARY_KEY", typeof(bool));
            table.Columns.Add("UNIQUE", typeof(bool));
            table.Columns.Add("CLUSTERED", typeof(bool));
            table.Columns.Add("TYPE", typeof(int));
            table.Columns.Add("FILL_FACTOR", typeof(int));
            table.Columns.Add("INITIAL_SIZE", typeof(int));
            table.Columns.Add("NULLS", typeof(int));
            table.Columns.Add("SORT_BOOKMARKS", typeof(bool));
            table.Columns.Add("AUTO_UPDATE", typeof(bool));
            table.Columns.Add("NULL_COLLATION", typeof(int));
            table.Columns.Add("ORDINAL_POSITION", typeof(int));
            table.Columns.Add("COLUMN_NAME", typeof(string));
            table.Columns.Add("COLUMN_GUID", typeof(Guid));
            table.Columns.Add("COLUMN_PROPID", typeof(long));
            table.Columns.Add("COLLATION", typeof(short));
            table.Columns.Add("CARDINALITY", typeof(decimal));
            table.Columns.Add("PAGES", typeof(int));
            table.Columns.Add("FILTER_CONDITION", typeof(string));
            table.Columns.Add("INTEGRATED", typeof(bool));
            table.Columns.Add("INDEX_DEFINITION", typeof(string));
            table.BeginLoadData();
            if (string.IsNullOrEmpty(strCatalog))
            {
                strCatalog = "main";
            }
            string str = (string.Compare(strCatalog, "temp", true, CultureInfo.InvariantCulture) == 0) ? "sqlite_temp_master" : "sqlite_master";
            using (SQLiteCommand command = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'table'", new object[] { strCatalog, str }), this))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bool flag = false;
                        list.Clear();
                        if (string.IsNullOrEmpty(strTable) || (string.Compare(reader.GetString(2), strTable, true, CultureInfo.InvariantCulture) == 0))
                        {
                            DataRow row;
                            try
                            {
                                using (SQLiteCommand command2 = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "PRAGMA [{0}].table_info([{1}])", new object[] { strCatalog, reader.GetString(2) }), this))
                                {
                                    using (SQLiteDataReader reader2 = command2.ExecuteReader())
                                    {
                                        while (reader2.Read())
                                        {
                                            if (reader2.GetInt32(5) == 1)
                                            {
                                                list.Add(reader2.GetInt32(0));
                                                if (string.Compare(reader2.GetString(2), "INTEGER", true, CultureInfo.InvariantCulture) == 0)
                                                {
                                                    flag = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (SQLiteException)
                            {
                            }
                            if ((list.Count == 1) && flag)
                            {
                                row = table.NewRow();
                                row["TABLE_CATALOG"] = strCatalog;
                                row["TABLE_NAME"] = reader.GetString(2);
                                row["INDEX_CATALOG"] = strCatalog;
                                row["PRIMARY_KEY"] = true;
                                row["INDEX_NAME"] = string.Format(CultureInfo.InvariantCulture, "{1}_PK_{0}", new object[] { reader.GetString(2), str });
                                row["UNIQUE"] = true;
                                if ((string.Compare((string) row["INDEX_NAME"], strIndex, true, CultureInfo.InvariantCulture) == 0) || (strIndex == null))
                                {
                                    table.Rows.Add(row);
                                }
                                list.Clear();
                            }
                            try
                            {
                                using (SQLiteCommand command3 = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "PRAGMA [{0}].index_list([{1}])", new object[] { strCatalog, reader.GetString(2) }), this))
                                {
                                    using (SQLiteDataReader reader3 = command3.ExecuteReader())
                                    {
                                        while (reader3.Read())
                                        {
                                            if ((string.Compare(reader3.GetString(1), strIndex, true, CultureInfo.InvariantCulture) != 0) && (strIndex != null))
                                            {
                                                continue;
                                            }
                                            row = table.NewRow();
                                            row["TABLE_CATALOG"] = strCatalog;
                                            row["TABLE_NAME"] = reader.GetString(2);
                                            row["INDEX_CATALOG"] = strCatalog;
                                            row["INDEX_NAME"] = reader3.GetString(1);
                                            row["UNIQUE"] = reader3.GetBoolean(2);
                                            row["PRIMARY_KEY"] = false;
                                            using (SQLiteCommand command4 = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{2}] WHERE [type] LIKE 'index' AND [name] LIKE '{1}'", new object[] { strCatalog, reader3.GetString(1).Replace("'", "''"), str }), this))
                                            {
                                                using (SQLiteDataReader reader4 = command4.ExecuteReader())
                                                {
                                                    while (reader4.Read())
                                                    {
                                                        if (!reader4.IsDBNull(4))
                                                        {
                                                            row["INDEX_DEFINITION"] = reader4.GetString(4);
                                                        }
                                                        goto Label_063E;
                                                    }
                                                }
                                            }
                                        Label_063E:
                                            if ((list.Count > 0) && reader3.GetString(1).StartsWith("sqlite_autoindex_" + reader.GetString(2), StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                using (SQLiteCommand command5 = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "PRAGMA [{0}].index_info([{1}])", new object[] { strCatalog, reader3.GetString(1) }), this))
                                                {
                                                    using (SQLiteDataReader reader5 = command5.ExecuteReader())
                                                    {
                                                        int num = 0;
                                                        while (reader5.Read())
                                                        {
                                                            if (!list.Contains(reader5.GetInt32(1)))
                                                            {
                                                                num = 0;
                                                                break;
                                                            }
                                                            num++;
                                                        }
                                                        if (num == list.Count)
                                                        {
                                                            row["PRIMARY_KEY"] = true;
                                                            list.Clear();
                                                        }
                                                    }
                                                }
                                            }
                                            table.Rows.Add(row);
                                        }
                                    }
                                }
                                continue;
                            }
                            catch (SQLiteException)
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            table.AcceptChanges();
            table.EndLoadData();
            return table;
        }

        private static DataTable Schema_MetaDataCollections()
        {
            DataTable table = new DataTable("MetaDataCollections") {
                Locale = CultureInfo.InvariantCulture
            };
            table.Columns.Add("CollectionName", typeof(string));
            table.Columns.Add("NumberOfRestrictions", typeof(int));
            table.Columns.Add("NumberOfIdentifierParts", typeof(int));
            table.BeginLoadData();
            StringReader reader = new StringReader(System.Data.SQLite.SR.MetaDataCollections);
            table.ReadXml(reader);
            reader.Close();
            table.AcceptChanges();
            table.EndLoadData();
            return table;
        }

        private static DataTable Schema_ReservedWords()
        {
            DataTable table = new DataTable("MetaDataCollections") {
                Locale = CultureInfo.InvariantCulture
            };
            table.Columns.Add("ReservedWord", typeof(string));
            table.Columns.Add("MaximumVersion", typeof(string));
            table.Columns.Add("MinimumVersion", typeof(string));
            table.BeginLoadData();
            foreach (string str in System.Data.SQLite.SR.Keywords.Split(new char[] { ',' }))
            {
                DataRow row = table.NewRow();
                row[0] = str;
                table.Rows.Add(row);
            }
            table.AcceptChanges();
            table.EndLoadData();
            return table;
        }

        private DataTable Schema_Tables(string strCatalog, string strTable, string strType)
        {
            DataTable table = new DataTable("Tables") {
                Locale = CultureInfo.InvariantCulture
            };
            table.Columns.Add("TABLE_CATALOG", typeof(string));
            table.Columns.Add("TABLE_SCHEMA", typeof(string));
            table.Columns.Add("TABLE_NAME", typeof(string));
            table.Columns.Add("TABLE_TYPE", typeof(string));
            table.Columns.Add("TABLE_ID", typeof(long));
            table.Columns.Add("TABLE_ROOTPAGE", typeof(int));
            table.Columns.Add("TABLE_DEFINITION", typeof(string));
            table.BeginLoadData();
            if (string.IsNullOrEmpty(strCatalog))
            {
                strCatalog = "main";
            }
            string str2 = (string.Compare(strCatalog, "temp", true, CultureInfo.InvariantCulture) == 0) ? "sqlite_temp_master" : "sqlite_master";
            using (SQLiteCommand command = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "SELECT [type], [name], [tbl_name], [rootpage], [sql], [rowid] FROM [{0}].[{1}] WHERE [type] LIKE 'table'", new object[] { strCatalog, str2 }), this))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string strB = reader.GetString(0);
                        if (string.Compare(reader.GetString(2), 0, "SQLITE_", 0, 7, true, CultureInfo.InvariantCulture) == 0)
                        {
                            strB = "SYSTEM_TABLE";
                        }
                        if (((string.Compare(strType, strB, true, CultureInfo.InvariantCulture) == 0) || (strType == null)) && ((string.Compare(reader.GetString(2), strTable, true, CultureInfo.InvariantCulture) == 0) || (strTable == null)))
                        {
                            DataRow row = table.NewRow();
                            row["TABLE_CATALOG"] = strCatalog;
                            row["TABLE_NAME"] = reader.GetString(2);
                            row["TABLE_TYPE"] = strB;
                            row["TABLE_ID"] = reader.GetInt64(5);
                            row["TABLE_ROOTPAGE"] = reader.GetInt32(3);
                            row["TABLE_DEFINITION"] = reader.GetString(4);
                            table.Rows.Add(row);
                        }
                    }
                }
            }
            table.AcceptChanges();
            table.EndLoadData();
            return table;
        }

        private DataTable Schema_Triggers(string catalog, string table, string triggerName)
        {
            DataTable table2 = new DataTable("Triggers") {
                Locale = CultureInfo.InvariantCulture
            };
            table2.Columns.Add("TABLE_CATALOG", typeof(string));
            table2.Columns.Add("TABLE_SCHEMA", typeof(string));
            table2.Columns.Add("TABLE_NAME", typeof(string));
            table2.Columns.Add("TRIGGER_NAME", typeof(string));
            table2.Columns.Add("TRIGGER_DEFINITION", typeof(string));
            table2.BeginLoadData();
            if (string.IsNullOrEmpty(table))
            {
                table = null;
            }
            if (string.IsNullOrEmpty(catalog))
            {
                catalog = "main";
            }
            string str = (string.Compare(catalog, "temp", true, CultureInfo.InvariantCulture) == 0) ? "sqlite_temp_master" : "sqlite_master";
            using (SQLiteCommand command = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "SELECT [type], [name], [tbl_name], [rootpage], [sql], [rowid] FROM [{0}].[{1}] WHERE [type] LIKE 'trigger'", new object[] { catalog, str }), this))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (((string.Compare(reader.GetString(1), triggerName, true, CultureInfo.InvariantCulture) == 0) || (triggerName == null)) && ((table == null) || (string.Compare(table, reader.GetString(2), true, CultureInfo.InvariantCulture) == 0)))
                        {
                            DataRow row = table2.NewRow();
                            row["TABLE_CATALOG"] = catalog;
                            row["TABLE_NAME"] = reader.GetString(2);
                            row["TRIGGER_NAME"] = reader.GetString(1);
                            row["TRIGGER_DEFINITION"] = reader.GetString(4);
                            table2.Rows.Add(row);
                        }
                    }
                }
            }
            table2.AcceptChanges();
            table2.EndLoadData();
            return table2;
        }

        private DataTable Schema_ViewColumns(string strCatalog, string strView, string strColumn)
        {
            DataTable table = new DataTable("ViewColumns") {
                Locale = CultureInfo.InvariantCulture
            };
            table.Columns.Add("VIEW_CATALOG", typeof(string));
            table.Columns.Add("VIEW_SCHEMA", typeof(string));
            table.Columns.Add("VIEW_NAME", typeof(string));
            table.Columns.Add("VIEW_COLUMN_NAME", typeof(string));
            table.Columns.Add("TABLE_CATALOG", typeof(string));
            table.Columns.Add("TABLE_SCHEMA", typeof(string));
            table.Columns.Add("TABLE_NAME", typeof(string));
            table.Columns.Add("COLUMN_NAME", typeof(string));
            table.Columns.Add("ORDINAL_POSITION", typeof(int));
            table.Columns.Add("COLUMN_HASDEFAULT", typeof(bool));
            table.Columns.Add("COLUMN_DEFAULT", typeof(string));
            table.Columns.Add("COLUMN_FLAGS", typeof(long));
            table.Columns.Add("IS_NULLABLE", typeof(bool));
            table.Columns.Add("DATA_TYPE", typeof(string));
            table.Columns.Add("CHARACTER_MAXIMUM_LENGTH", typeof(int));
            table.Columns.Add("NUMERIC_PRECISION", typeof(int));
            table.Columns.Add("NUMERIC_SCALE", typeof(int));
            table.Columns.Add("DATETIME_PRECISION", typeof(long));
            table.Columns.Add("CHARACTER_SET_CATALOG", typeof(string));
            table.Columns.Add("CHARACTER_SET_SCHEMA", typeof(string));
            table.Columns.Add("CHARACTER_SET_NAME", typeof(string));
            table.Columns.Add("COLLATION_CATALOG", typeof(string));
            table.Columns.Add("COLLATION_SCHEMA", typeof(string));
            table.Columns.Add("COLLATION_NAME", typeof(string));
            table.Columns.Add("PRIMARY_KEY", typeof(bool));
            table.Columns.Add("EDM_TYPE", typeof(string));
            table.Columns.Add("AUTOINCREMENT", typeof(bool));
            table.Columns.Add("UNIQUE", typeof(bool));
            if (string.IsNullOrEmpty(strCatalog))
            {
                strCatalog = "main";
            }
            string str2 = (string.Compare(strCatalog, "temp", true, CultureInfo.InvariantCulture) == 0) ? "sqlite_temp_master" : "sqlite_master";
            table.BeginLoadData();
            using (SQLiteCommand command = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'view'", new object[] { strCatalog, str2 }), this))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (string.IsNullOrEmpty(strView) || (string.Compare(strView, reader.GetString(2), true, CultureInfo.InvariantCulture) == 0))
                        {
                            using (SQLiteCommand command2 = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}]", new object[] { strCatalog, reader.GetString(2) }), this))
                            {
                                string source = reader.GetString(4).Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
                                int num = CultureInfo.InvariantCulture.CompareInfo.IndexOf(source, " AS ", CompareOptions.IgnoreCase);
                                if (num >= 0)
                                {
                                    using (SQLiteCommand command3 = new SQLiteCommand(source.Substring(num + 4), this))
                                    {
                                        using (SQLiteDataReader reader2 = command2.ExecuteReader(CommandBehavior.SchemaOnly))
                                        {
                                            using (SQLiteDataReader reader3 = command3.ExecuteReader(CommandBehavior.SchemaOnly))
                                            {
                                                using (DataTable table2 = reader2.GetSchemaTable(false, false))
                                                {
                                                    using (DataTable table3 = reader3.GetSchemaTable(false, false))
                                                    {
                                                        for (num = 0; num < table3.Rows.Count; num++)
                                                        {
                                                            DataRow row3 = table2.Rows[num];
                                                            DataRow row2 = table3.Rows[num];
                                                            if ((string.Compare(row3[SchemaTableColumn.ColumnName].ToString(), strColumn, true, CultureInfo.InvariantCulture) == 0) || (strColumn == null))
                                                            {
                                                                DataRow row = table.NewRow();
                                                                row["VIEW_CATALOG"] = strCatalog;
                                                                row["VIEW_NAME"] = reader.GetString(2);
                                                                row["TABLE_CATALOG"] = strCatalog;
                                                                row["TABLE_SCHEMA"] = row2[SchemaTableColumn.BaseSchemaName];
                                                                row["TABLE_NAME"] = row2[SchemaTableColumn.BaseTableName];
                                                                row["COLUMN_NAME"] = row2[SchemaTableColumn.BaseColumnName];
                                                                row["VIEW_COLUMN_NAME"] = row3[SchemaTableColumn.ColumnName];
                                                                row["COLUMN_HASDEFAULT"] = row3[SchemaTableOptionalColumn.DefaultValue] != DBNull.Value;
                                                                row["COLUMN_DEFAULT"] = row3[SchemaTableOptionalColumn.DefaultValue];
                                                                row["ORDINAL_POSITION"] = row3[SchemaTableColumn.ColumnOrdinal];
                                                                row["IS_NULLABLE"] = row3[SchemaTableColumn.AllowDBNull];
                                                                row["DATA_TYPE"] = row3["DataTypeName"];
                                                                row["EDM_TYPE"] = SQLiteConvert.DbTypeToTypeName((DbType) row3[SchemaTableColumn.ProviderType]).ToString().ToLower(CultureInfo.InvariantCulture);
                                                                row["CHARACTER_MAXIMUM_LENGTH"] = row3[SchemaTableColumn.ColumnSize];
                                                                row["TABLE_SCHEMA"] = row3[SchemaTableColumn.BaseSchemaName];
                                                                row["PRIMARY_KEY"] = row3[SchemaTableColumn.IsKey];
                                                                row["AUTOINCREMENT"] = row3[SchemaTableOptionalColumn.IsAutoIncrement];
                                                                row["COLLATION_NAME"] = row3["CollationType"];
                                                                row["UNIQUE"] = row3[SchemaTableColumn.IsUnique];
                                                                table.Rows.Add(row);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                continue;
                            }
                        }
                    }
                }
            }
            table.EndLoadData();
            table.AcceptChanges();
            return table;
        }

        private DataTable Schema_Views(string strCatalog, string strView)
        {
            DataTable table = new DataTable("Views") {
                Locale = CultureInfo.InvariantCulture
            };
            table.Columns.Add("TABLE_CATALOG", typeof(string));
            table.Columns.Add("TABLE_SCHEMA", typeof(string));
            table.Columns.Add("TABLE_NAME", typeof(string));
            table.Columns.Add("VIEW_DEFINITION", typeof(string));
            table.Columns.Add("CHECK_OPTION", typeof(bool));
            table.Columns.Add("IS_UPDATABLE", typeof(bool));
            table.Columns.Add("DESCRIPTION", typeof(string));
            table.Columns.Add("DATE_CREATED", typeof(DateTime));
            table.Columns.Add("DATE_MODIFIED", typeof(DateTime));
            table.BeginLoadData();
            if (string.IsNullOrEmpty(strCatalog))
            {
                strCatalog = "main";
            }
            string str2 = (string.Compare(strCatalog, "temp", true, CultureInfo.InvariantCulture) == 0) ? "sqlite_temp_master" : "sqlite_master";
            using (SQLiteCommand command = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'view'", new object[] { strCatalog, str2 }), this))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if ((string.Compare(reader.GetString(1), strView, true, CultureInfo.InvariantCulture) == 0) || string.IsNullOrEmpty(strView))
                        {
                            string source = reader.GetString(4).Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
                            int num = CultureInfo.InvariantCulture.CompareInfo.IndexOf(source, " AS ", CompareOptions.IgnoreCase);
                            if (num > -1)
                            {
                                source = source.Substring(num + 4).Trim();
                                DataRow row = table.NewRow();
                                row["TABLE_CATALOG"] = strCatalog;
                                row["TABLE_NAME"] = reader.GetString(2);
                                row["IS_UPDATABLE"] = false;
                                row["VIEW_DEFINITION"] = source;
                                table.Rows.Add(row);
                            }
                        }
                    }
                }
            }
            table.AcceptChanges();
            table.EndLoadData();
            return table;
        }

        public void SetPassword(string databasePassword)
        {
            this.SetPassword(string.IsNullOrEmpty(databasePassword) ? null : Encoding.UTF8.GetBytes(databasePassword));
        }

        public void SetPassword(byte[] databasePassword)
        {
            if (this._connectionState != ConnectionState.Closed)
            {
                throw new InvalidOperationException("Password can only be set before the database is opened.");
            }
            if ((databasePassword != null) && (databasePassword.Length == 0))
            {
                databasePassword = null;
            }
            this._password = databasePassword;
        }

        private void UpdateCallback(IntPtr puser, int type, IntPtr database, IntPtr table, long rowid)
        {
            this._updateHandler(this, new UpdateEventArgs(SQLiteConvert.UTF8ToString(database, -1), SQLiteConvert.UTF8ToString(table, -1), (UpdateEventType) type, rowid));
        }

        [Editor("SQLite.Designer.SQLiteConnectionStringEditor, SQLite.Designer, Version=1.0.36.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), RefreshProperties(RefreshProperties.All), DefaultValue("")]
        public override string ConnectionString
        {
            get
            {
                return this._connectionString;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                if (this._connectionState != ConnectionState.Closed)
                {
                    throw new InvalidOperationException();
                }
                this._connectionString = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Database
        {
            get
            {
                return "main";
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string DataSource
        {
            get
            {
                return this._dataSource;
            }
        }

        protected override System.Data.Common.DbProviderFactory DbProviderFactory
        {
            get
            {
                return SQLiteFactory.Instance;
            }
        }

        public int DefaultTimeout
        {
            get
            {
                return this._defaultTimeout;
            }
            set
            {
                this._defaultTimeout = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override string ServerVersion
        {
            get
            {
                if (this._connectionState != ConnectionState.Open)
                {
                    throw new InvalidOperationException();
                }
                return this._sql.Version;
            }
        }

        public static string SQLiteVersion
        {
            get
            {
                return SQLite3.SQLiteVersion;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ConnectionState State
        {
            get
            {
                return this._connectionState;
            }
        }
    }
}

