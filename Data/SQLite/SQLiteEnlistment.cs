namespace System.Data.SQLite
{
    using System;
    using System.Transactions;

    internal class SQLiteEnlistment : IEnlistmentNotification
    {
        internal bool _disposeConnection;
        internal Transaction _scope;
        internal SQLiteTransaction _transaction;

        internal SQLiteEnlistment(SQLiteConnection cnn, Transaction scope)
        {
            this._transaction = cnn.BeginTransaction();
            this._scope = scope;
            this._disposeConnection = false;
            this._scope.EnlistVolatile(this, EnlistmentOptions.None);
        }

        private void Cleanup(SQLiteConnection cnn)
        {
            if (this._disposeConnection)
            {
                cnn.Dispose();
            }
            this._transaction = null;
            this._scope = null;
        }

        public void Commit(Enlistment enlistment)
        {
            SQLiteConnection cnn = this._transaction.Connection;
            cnn._enlistment = null;
            try
            {
                this._transaction.IsValid(true);
                this._transaction.Connection._transactionLevel = 1;
                this._transaction.Commit();
                enlistment.Done();
            }
            finally
            {
                this.Cleanup(cnn);
            }
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            if (!this._transaction.IsValid(false))
            {
                preparingEnlistment.ForceRollback();
            }
            else
            {
                preparingEnlistment.Prepared();
            }
        }

        public void Rollback(Enlistment enlistment)
        {
            SQLiteConnection cnn = this._transaction.Connection;
            cnn._enlistment = null;
            try
            {
                this._transaction.Rollback();
                enlistment.Done();
            }
            finally
            {
                this.Cleanup(cnn);
            }
        }
    }
}

