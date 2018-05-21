namespace System.Data.SQLite
{
    using System;

    internal interface ISQLiteSchemaExtensions
    {
        void BuildTempSchema(SQLiteConnection cnn);
    }
}

