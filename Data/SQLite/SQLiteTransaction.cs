namespace System.Data.SQLite
{
    using System;
    using System.Data;
    using System.Data.Common;

    public sealed class SQLiteTransaction : DbTransaction
    {
        internal SQLiteConnection _cnn;
        private System.Data.IsolationLevel _level;
        internal long _version;

        internal SQLiteTransaction(SQLiteConnection connection, bool deferredLock)
        {
            this._cnn = connection;
            this._version = this._cnn._version;
            this._level = deferredLock ? System.Data.IsolationLevel.ReadCommitted : System.Data.IsolationLevel.Serializable;
            if (this._cnn._transactionLevel++ == 0)
            {
                try
                {
                    using (SQLiteCommand command = this._cnn.CreateCommand())
                    {
                        if (!deferredLock)
                        {
                            command.CommandText = "BEGIN IMMEDIATE";
                        }
                        else
                        {
                            command.CommandText = "BEGIN";
                        }
                        command.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException)
                {
                    this._cnn._transactionLevel--;
                    this._cnn = null;
                    throw;
                }
            }
        }

        public override void Commit()
        {
            this.IsValid(true);
            if ((this._cnn._transactionLevel - 1) == 0)
            {
                using (SQLiteCommand command = this._cnn.CreateCommand())
                {
                    command.CommandText = "COMMIT";
                    command.ExecuteNonQuery();
                }
            }
            this._cnn._transactionLevel--;
            this._cnn = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this)
                {
                    if (this.IsValid(false))
                    {
                        this.Rollback();
                    }
                    this._cnn = null;
                }
            }
            base.Dispose(disposing);
        }

        internal static void IssueRollback(SQLiteConnection cnn)
        {
            using (SQLiteCommand command = cnn.CreateCommand())
            {
                command.CommandText = "ROLLBACK";
                command.ExecuteNonQuery();
            }
        }

        internal bool IsValid(bool throwError)
        {
            if (this._cnn == null)
            {
                if (throwError)
                {
                    throw new ArgumentNullException("No connection associated with this transaction");
                }
                return false;
            }
            if (this._cnn._transactionLevel == 0)
            {
                if (throwError)
                {
                    throw new SQLiteException(0x15, "No transaction is active on this connection");
                }
                return false;
            }
            if (this._cnn._version != this._version)
            {
                if (throwError)
                {
                    throw new SQLiteException(0x15, "The connection was closed and re-opened, changes were rolled back");
                }
                return false;
            }
            if (this._cnn.State == ConnectionState.Open)
            {
                return true;
            }
            if (throwError)
            {
                throw new SQLiteException(0x15, "Connection was closed");
            }
            return false;
        }

        public override void Rollback()
        {
            this.IsValid(true);
            IssueRollback(this._cnn);
            this._cnn._transactionLevel = 0;
            this._cnn = null;
        }

        public SQLiteConnection Connection
        {
            get
            {
                return this._cnn;
            }
        }

        protected override System.Data.Common.DbConnection DbConnection
        {
            get
            {
                return this.Connection;
            }
        }

        public override System.Data.IsolationLevel IsolationLevel
        {
            get
            {
                return this._level;
            }
        }
    }
}

