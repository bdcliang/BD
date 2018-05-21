namespace System.Data.SQLite
{
    using System;
    using System.Runtime.InteropServices;

    internal class SQLiteStatementHandle : CriticalHandle
    {
        internal SQLiteStatementHandle() : base(IntPtr.Zero)
        {
        }

        private SQLiteStatementHandle(IntPtr stmt) : this()
        {
            base.SetHandle(stmt);
        }

        public static implicit operator IntPtr(SQLiteStatementHandle stmt)
        {
            return stmt.handle;
        }

        public static implicit operator SQLiteStatementHandle(IntPtr stmt)
        {
            return new SQLiteStatementHandle(stmt);
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                SQLiteBase.FinalizeStatement(this);
            }
            catch (SQLiteException)
            {
            }
            return true;
        }

        public override bool IsInvalid
        {
            get
            {
                return (base.handle == IntPtr.Zero);
            }
        }
    }
}

