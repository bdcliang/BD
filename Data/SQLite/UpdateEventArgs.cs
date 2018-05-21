namespace System.Data.SQLite
{
    using System;

    public class UpdateEventArgs : EventArgs
    {
        public readonly string Database;
        public readonly UpdateEventType Event;
        public readonly long RowId;
        public readonly string Table;

        internal UpdateEventArgs(string database, string table, UpdateEventType eventType, long rowid)
        {
            this.Database = database;
            this.Table = table;
            this.Event = eventType;
            this.RowId = rowid;
        }
    }
}

