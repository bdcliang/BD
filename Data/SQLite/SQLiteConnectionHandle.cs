namespace System.Data.SQLite
{
    using System;
    using System.Runtime.InteropServices;

    internal class SQLiteConnectionHandle : CriticalHandle
    {
        internal SQLiteConnectionHandle() : base(IntPtr.Zero)
        {
        }

        private SQLiteConnectionHandle(IntPtr db) : this()
        {
            base.SetHandle(db);
        }

        public static implicit operator IntPtr(SQLiteConnectionHandle db)
        {
            return db.handle;
        }

        public static implicit operator SQLiteConnectionHandle(IntPtr db)
        {
            return new SQLiteConnectionHandle(db);
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                SQLiteBase.CloseConnection(this);
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

