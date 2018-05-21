namespace System.Data.SQLite
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;

    [Designer("SQLite.Designer.SQLiteCommandDesigner, SQLite.Designer, Version=1.0.36.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139"), ToolboxItem(true)]
    public sealed class SQLiteCommand : DbCommand, ICloneable
    {
        private WeakReference _activeReader;
        private SQLiteConnection _cnn;
        private string _commandText;
        internal int _commandTimeout;
        private bool _designTimeVisible;
        private SQLiteParameterCollection _parameterCollection;
        internal string _remainingText;
        internal List<SQLiteStatement> _statementList;
        private SQLiteTransaction _transaction;
        private UpdateRowSource _updateRowSource;
        private long _version;

        public SQLiteCommand() : this(null, null)
        {
        }

        private SQLiteCommand(SQLiteCommand source) : this(source.CommandText, source.Connection, source.Transaction)
        {
            this.CommandTimeout = source.CommandTimeout;
            this.DesignTimeVisible = source.DesignTimeVisible;
            this.UpdatedRowSource = source.UpdatedRowSource;
            foreach (SQLiteParameter parameter in source._parameterCollection)
            {
                this.Parameters.Add(parameter.Clone());
            }
        }

        public SQLiteCommand(SQLiteConnection connection) : this(null, connection, null)
        {
        }

        public SQLiteCommand(string commandText) : this(commandText, null, null)
        {
        }

        public SQLiteCommand(string commandText, SQLiteConnection connection) : this(commandText, connection, null)
        {
        }

        public SQLiteCommand(string commandText, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            this._statementList = null;
            this._activeReader = null;
            this._commandTimeout = 30;
            this._parameterCollection = new SQLiteParameterCollection(this);
            this._designTimeVisible = true;
            this._updateRowSource = UpdateRowSource.None;
            this._transaction = null;
            if (commandText != null)
            {
                this.CommandText = commandText;
            }
            if (connection != null)
            {
                this.DbConnection = connection;
                this._commandTimeout = connection.DefaultTimeout;
            }
            if (transaction != null)
            {
                this.Transaction = transaction;
            }
        }

        internal SQLiteStatement BuildNextCommand()
        {
            SQLiteStatement item = null;
            SQLiteStatement statement2;
            try
            {
                if (this._statementList == null)
                {
                    this._remainingText = this._commandText;
                }
                item = this._cnn._sql.Prepare(this._cnn, this._remainingText, (this._statementList == null) ? null : this._statementList[this._statementList.Count - 1], (uint) (this._commandTimeout * 0x3e8), out this._remainingText);
                if (item != null)
                {
                    item._command = this;
                    if (this._statementList == null)
                    {
                        this._statementList = new List<SQLiteStatement>();
                    }
                    this._statementList.Add(item);
                    this._parameterCollection.MapParameters(item);
                    item.BindParameters();
                }
                statement2 = item;
            }
            catch (Exception)
            {
                if (item != null)
                {
                    if (this._statementList.Contains(item))
                    {
                        this._statementList.Remove(item);
                    }
                    item.Dispose();
                }
                this._remainingText = null;
                throw;
            }
            return statement2;
        }

        public override void Cancel()
        {
            if (this._activeReader != null)
            {
                SQLiteDataReader target = this._activeReader.Target as SQLiteDataReader;
                if (target != null)
                {
                    target.Cancel();
                }
            }
        }

        internal void ClearCommands()
        {
            if (this._activeReader != null)
            {
                SQLiteDataReader target = null;
                try
                {
                    target = this._activeReader.Target as SQLiteDataReader;
                }
                catch
                {
                }
                if (target != null)
                {
                    target.Close();
                }
                this._activeReader = null;
            }
            if (this._statementList != null)
            {
                int count = this._statementList.Count;
                for (int i = 0; i < count; i++)
                {
                    this._statementList[i].Dispose();
                }
                this._statementList = null;
                this._parameterCollection.Unbind();
            }
        }

        internal void ClearDataReader()
        {
            this._activeReader = null;
        }

        public object Clone()
        {
            return new SQLiteCommand(this);
        }

        protected override DbParameter CreateDbParameter()
        {
            return this.CreateParameter();
        }

        public SQLiteParameter CreateParameter()
        {
            return new SQLiteParameter();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                SQLiteDataReader target = null;
                if (this._activeReader != null)
                {
                    try
                    {
                        target = this._activeReader.Target as SQLiteDataReader;
                    }
                    catch
                    {
                    }
                }
                if (target != null)
                {
                    target._disposeCommand = true;
                    this._activeReader = null;
                }
                else
                {
                    this.Connection = null;
                    this._parameterCollection.Clear();
                    this._commandText = null;
                }
            }
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return this.ExecuteReader(behavior);
        }

        public override int ExecuteNonQuery()
        {
            using (SQLiteDataReader reader = this.ExecuteReader(CommandBehavior.SingleRow | CommandBehavior.SingleResult))
            {
                while (reader.NextResult())
                {
                }
                return reader.RecordsAffected;
            }
        }

        public SQLiteDataReader ExecuteReader()
        {
            return this.ExecuteReader(CommandBehavior.Default);
        }

        public SQLiteDataReader ExecuteReader(CommandBehavior behavior)
        {
            this.InitializeForReader();
            SQLiteDataReader target = new SQLiteDataReader(this, behavior);
            this._activeReader = new WeakReference(target, false);
            return target;
        }

        public override object ExecuteScalar()
        {
            using (SQLiteDataReader reader = this.ExecuteReader(CommandBehavior.SingleRow | CommandBehavior.SingleResult))
            {
                if (reader.Read())
                {
                    return reader[0];
                }
            }
            return null;
        }

        internal SQLiteStatement GetStatement(int index)
        {
            if (this._statementList == null)
            {
                return this.BuildNextCommand();
            }
            if (index == this._statementList.Count)
            {
                if (!string.IsNullOrEmpty(this._remainingText))
                {
                    return this.BuildNextCommand();
                }
                return null;
            }
            SQLiteStatement statement = this._statementList[index];
            statement.BindParameters();
            return statement;
        }

        private void InitializeForReader()
        {
            if ((this._activeReader != null) && this._activeReader.IsAlive)
            {
                throw new InvalidOperationException("DataReader already active on this command");
            }
            if (this._cnn == null)
            {
                throw new InvalidOperationException("No connection associated with this command");
            }
            if (this._cnn.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Database is not open");
            }
            if (this._cnn._version != this._version)
            {
                this._version = this._cnn._version;
                this.ClearCommands();
            }
            this._parameterCollection.MapParameters(null);
        }

        public override void Prepare()
        {
        }

        [RefreshProperties(RefreshProperties.All), Editor("Microsoft.VSDesigner.Data.SQL.Design.SqlCommandTextEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue("")]
        public override string CommandText
        {
            get
            {
                return this._commandText;
            }
            set
            {
                if (this._commandText != value)
                {
                    if ((this._activeReader != null) && this._activeReader.IsAlive)
                    {
                        throw new InvalidOperationException("Cannot set CommandText while a DataReader is active");
                    }
                    this.ClearCommands();
                    this._commandText = value;
                    SQLiteConnection connection1 = this._cnn;
                }
            }
        }

        [DefaultValue(30)]
        public override int CommandTimeout
        {
            get
            {
                return this._commandTimeout;
            }
            set
            {
                this._commandTimeout = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), DefaultValue(1)]
        public override System.Data.CommandType CommandType
        {
            get
            {
                return System.Data.CommandType.Text;
            }
            set
            {
                if (value != System.Data.CommandType.Text)
                {
                    throw new NotSupportedException();
                }
            }
        }

        [Editor("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue((string) null)]
        public SQLiteConnection Connection
        {
            get
            {
                return this._cnn;
            }
            set
            {
                if ((this._activeReader != null) && this._activeReader.IsAlive)
                {
                    throw new InvalidOperationException("Cannot set Connection while a DataReader is active");
                }
                if (this._cnn != null)
                {
                    this.ClearCommands();
                }
                this._cnn = value;
                if (this._cnn != null)
                {
                    this._version = this._cnn._version;
                }
            }
        }

        protected override System.Data.Common.DbConnection DbConnection
        {
            get
            {
                return this.Connection;
            }
            set
            {
                this.Connection = (SQLiteConnection) value;
            }
        }

        protected override System.Data.Common.DbParameterCollection DbParameterCollection
        {
            get
            {
                return this.Parameters;
            }
        }

        protected override System.Data.Common.DbTransaction DbTransaction
        {
            get
            {
                return this.Transaction;
            }
            set
            {
                this.Transaction = (SQLiteTransaction) value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DefaultValue(true), DesignOnly(true), Browsable(false)]
        public override bool DesignTimeVisible
        {
            get
            {
                return this._designTimeVisible;
            }
            set
            {
                this._designTimeVisible = value;
                TypeDescriptor.Refresh(this);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public SQLiteParameterCollection Parameters
        {
            get
            {
                return this._parameterCollection;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SQLiteTransaction Transaction
        {
            get
            {
                return this._transaction;
            }
            set
            {
                if (this._cnn != null)
                {
                    if ((this._activeReader != null) && this._activeReader.IsAlive)
                    {
                        throw new InvalidOperationException("Cannot set Transaction while a DataReader is active");
                    }
                    if ((value != null) && (value._cnn != this._cnn))
                    {
                        throw new ArgumentException("Transaction is not associated with the command's connection");
                    }
                    this._transaction = value;
                }
                else
                {
                    this.Connection = value.Connection;
                    this._transaction = value;
                }
            }
        }

        [DefaultValue(0)]
        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return this._updateRowSource;
            }
            set
            {
                this._updateRowSource = value;
            }
        }
    }
}

