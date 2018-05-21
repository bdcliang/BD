namespace System.Data.SQLite
{
    using System;
    using System.Data.Common;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class SQLiteException : DbException
    {
        private SQLiteErrorCode _errorCode;
        private static string[] _errorMessages = new string[] { 
            "SQLite OK", "SQLite error", "An internal logic error in SQLite", "Access permission denied", "Callback routine requested an abort", "The database file is locked", "A table in the database is locked", "malloc() failed", "Attempt to write a read-only database", "Operation terminated by sqlite3_interrupt()", "Some kind of disk I/O error occurred", "The database disk image is malformed", "Table or record not found", "Insertion failed because the database is full", "Unable to open the database file", "Database lock protocol error", 
            "Database is empty", "The database schema changed", "Too much data for one row of a table", "Abort due to constraint violation", "Data type mismatch", "Library used incorrectly", "Uses OS features not supported on host", "Authorization denied", "Auxiliary database format error", "2nd parameter to sqlite3_bind() out of range", "File opened that is not a database file"
         };

        public SQLiteException()
        {
        }

        public SQLiteException(string message) : base(message)
        {
        }

        public SQLiteException(int errorCode, string extendedInformation) : base(GetStockErrorMessage(errorCode, extendedInformation))
        {
            this._errorCode = (SQLiteErrorCode) errorCode;
        }

        private SQLiteException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SQLiteException(string message, Exception innerException) : base(message, innerException)
        {
        }

        private static string GetStockErrorMessage(int errorCode, string errorMessage)
        {
            if (errorMessage == null)
            {
                errorMessage = "";
            }
            if (errorMessage.Length > 0)
            {
                errorMessage = "\r\n" + errorMessage;
            }
            if ((errorCode < 0) || (errorCode >= _errorMessages.Length))
            {
                errorCode = 1;
            }
            return (_errorMessages[errorCode] + errorMessage);
        }

        public SQLiteErrorCode ErrorCode
        {
            get
            {
                return this._errorCode;
            }
        }
    }
}

