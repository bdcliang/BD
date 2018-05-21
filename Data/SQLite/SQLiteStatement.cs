namespace System.Data.SQLite
{
    using System;
    using System.Data;
    using System.Globalization;

    internal sealed class SQLiteStatement : IDisposable
    {
        internal SQLiteCommand _command;
        internal string[] _paramNames;
        internal SQLiteParameter[] _paramValues;
        internal SQLiteBase _sql;
        internal SQLiteStatementHandle _sqlite_stmt;
        internal string _sqlStatement;
        private string[] _types;
        internal int _unnamedParameters;

        internal SQLiteStatement(SQLiteBase sqlbase, SQLiteStatementHandle stmt, string strCommand, SQLiteStatement previous)
        {
            this._sql = sqlbase;
            this._sqlite_stmt = stmt;
            this._sqlStatement = strCommand;
            int num = 0;
            int num2 = this._sql.Bind_ParamCount(this);
            if (num2 > 0)
            {
                if (previous != null)
                {
                    num = previous._unnamedParameters;
                }
                this._paramNames = new string[num2];
                this._paramValues = new SQLiteParameter[num2];
                for (int i = 0; i < num2; i++)
                {
                    string str = this._sql.Bind_ParamName(this, i + 1);
                    if (string.IsNullOrEmpty(str))
                    {
                        str = string.Format(CultureInfo.InvariantCulture, ";{0}", new object[] { num });
                        num++;
                        this._unnamedParameters++;
                    }
                    this._paramNames[i] = str;
                    this._paramValues[i] = null;
                }
            }
        }

        private void BindParameter(int index, SQLiteParameter param)
        {
            if (param == null)
            {
                throw new SQLiteException(1, "Insufficient parameters supplied to the command");
            }
            object obj2 = param.Value;
            DbType dbType = param.DbType;
            if (Convert.IsDBNull(obj2) || (obj2 == null))
            {
                this._sql.Bind_Null(this, index);
            }
            else
            {
                if (dbType == DbType.Object)
                {
                    dbType = SQLiteConvert.TypeToDbType(obj2.GetType());
                }
                switch (dbType)
                {
                    case DbType.Binary:
                        this._sql.Bind_Blob(this, index, (byte[]) obj2);
                        return;

                    case DbType.Byte:
                    case DbType.Boolean:
                    case DbType.Int16:
                    case DbType.Int32:
                    case DbType.SByte:
                    case DbType.UInt16:
                    case DbType.UInt32:
                        this._sql.Bind_Int32(this, index, Convert.ToInt32(obj2, CultureInfo.CurrentCulture));
                        return;

                    case DbType.Currency:
                    case DbType.Double:
                    case DbType.Single:
                        this._sql.Bind_Double(this, index, Convert.ToDouble(obj2, CultureInfo.CurrentCulture));
                        return;

                    case DbType.Date:
                    case DbType.DateTime:
                    case DbType.Time:
                        this._sql.Bind_DateTime(this, index, Convert.ToDateTime(obj2, CultureInfo.CurrentCulture));
                        return;

                    case DbType.Decimal:
                        this._sql.Bind_Text(this, index, Convert.ToDecimal(obj2, CultureInfo.CurrentCulture).ToString(CultureInfo.InvariantCulture));
                        return;

                    case DbType.Guid:
                        if (!this._command.Connection._binaryGuid)
                        {
                            this._sql.Bind_Text(this, index, obj2.ToString());
                            return;
                        }
                        this._sql.Bind_Blob(this, index, ((Guid) obj2).ToByteArray());
                        return;

                    case DbType.Int64:
                    case DbType.UInt64:
                        this._sql.Bind_Int64(this, index, Convert.ToInt64(obj2, CultureInfo.CurrentCulture));
                        return;
                }
                this._sql.Bind_Text(this, index, obj2.ToString());
            }
        }

        internal void BindParameters()
        {
            if (this._paramNames != null)
            {
                int length = this._paramNames.Length;
                for (int i = 0; i < length; i++)
                {
                    this.BindParameter(i + 1, this._paramValues[i]);
                }
            }
        }

        public void Dispose()
        {
            if (this._sqlite_stmt != null)
            {
                this._sqlite_stmt.Dispose();
            }
            this._sqlite_stmt = null;
            this._paramNames = null;
            this._paramValues = null;
            this._sql = null;
            this._sqlStatement = null;
        }

        internal bool MapParameter(string s, SQLiteParameter p)
        {
            if (this._paramNames != null)
            {
                int indexA = 0;
                if ((s.Length > 0) && (":$@;".IndexOf(s[0]) == -1))
                {
                    indexA = 1;
                }
                int length = this._paramNames.Length;
                for (int i = 0; i < length; i++)
                {
                    if (string.Compare(this._paramNames[i], indexA, s, 0, Math.Max(this._paramNames[i].Length - indexA, s.Length), true, CultureInfo.InvariantCulture) == 0)
                    {
                        this._paramValues[i] = p;
                        return true;
                    }
                }
            }
            return false;
        }

        internal void SetTypes(string typedefs)
        {
            int num = typedefs.IndexOf("TYPES", 0, StringComparison.OrdinalIgnoreCase);
            if (num == -1)
            {
                throw new ArgumentOutOfRangeException();
            }
            string[] strArray = typedefs.Substring(num + 6).Replace(" ", "").Replace(";", "").Replace("\"", "").Replace("[", "").Replace("]", "").Replace("`", "").Split(new char[] { ',', '\r', '\n', '\t' });
            for (int i = 0; i < strArray.Length; i++)
            {
                if (string.IsNullOrEmpty(strArray[i]))
                {
                    strArray[i] = null;
                }
            }
            this._types = strArray;
        }

        internal string[] TypeDefinitions
        {
            get
            {
                return this._types;
            }
        }
    }
}

