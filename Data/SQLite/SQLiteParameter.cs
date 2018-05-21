namespace System.Data.SQLite
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;

    public sealed class SQLiteParameter : DbParameter, ICloneable
    {
        private int _dataSize;
        internal int _dbType;
        private bool _nullable;
        private bool _nullMapping;
        private object _objValue;
        private string _parameterName;
        private DataRowVersion _rowVersion;
        private string _sourceColumn;

        public SQLiteParameter() : this(null, ~System.Data.DbType.AnsiString, 0, null, DataRowVersion.Current)
        {
        }

        public SQLiteParameter(System.Data.DbType dbType) : this(null, dbType, 0, null, DataRowVersion.Current)
        {
        }

        private SQLiteParameter(SQLiteParameter source) : this(source.ParameterName, (System.Data.DbType) source._dbType, 0, source.Direction, source.IsNullable, 0, 0, source.SourceColumn, source.SourceVersion, source.Value)
        {
            this._nullMapping = source._nullMapping;
        }

        public SQLiteParameter(string parameterName) : this(parameterName, ~System.Data.DbType.AnsiString, 0, null, DataRowVersion.Current)
        {
        }

        public SQLiteParameter(System.Data.DbType parameterType, int parameterSize) : this(null, parameterType, parameterSize, null, DataRowVersion.Current)
        {
        }

        public SQLiteParameter(System.Data.DbType dbType, object value) : this(null, dbType, 0, null, DataRowVersion.Current)
        {
            this.Value = value;
        }

        public SQLiteParameter(System.Data.DbType dbType, string sourceColumn) : this(null, dbType, 0, sourceColumn, DataRowVersion.Current)
        {
        }

        public SQLiteParameter(string parameterName, System.Data.DbType dbType) : this(parameterName, dbType, 0, null, DataRowVersion.Current)
        {
        }

        public SQLiteParameter(string parameterName, object value) : this(parameterName, ~System.Data.DbType.AnsiString, 0, null, DataRowVersion.Current)
        {
            this.Value = value;
        }

        public SQLiteParameter(System.Data.DbType parameterType, int parameterSize, string sourceColumn) : this(null, parameterType, parameterSize, sourceColumn, DataRowVersion.Current)
        {
        }

        public SQLiteParameter(System.Data.DbType dbType, string sourceColumn, DataRowVersion rowVersion) : this(null, dbType, 0, sourceColumn, rowVersion)
        {
        }

        public SQLiteParameter(string parameterName, System.Data.DbType parameterType, int parameterSize) : this(parameterName, parameterType, parameterSize, null, DataRowVersion.Current)
        {
        }

        public SQLiteParameter(string parameterName, System.Data.DbType dbType, string sourceColumn) : this(parameterName, dbType, 0, sourceColumn, DataRowVersion.Current)
        {
        }

        public SQLiteParameter(System.Data.DbType parameterType, int parameterSize, string sourceColumn, DataRowVersion rowVersion) : this(null, parameterType, parameterSize, sourceColumn, rowVersion)
        {
        }

        public SQLiteParameter(string parameterName, System.Data.DbType parameterType, int parameterSize, string sourceColumn) : this(parameterName, parameterType, parameterSize, sourceColumn, DataRowVersion.Current)
        {
        }

        public SQLiteParameter(string parameterName, System.Data.DbType dbType, string sourceColumn, DataRowVersion rowVersion) : this(parameterName, dbType, 0, sourceColumn, rowVersion)
        {
        }

        public SQLiteParameter(string parameterName, System.Data.DbType parameterType, int parameterSize, string sourceColumn, DataRowVersion rowVersion)
        {
            this._parameterName = parameterName;
            this._dbType = (int) parameterType;
            this._sourceColumn = sourceColumn;
            this._rowVersion = rowVersion;
            this._objValue = null;
            this._dataSize = parameterSize;
            this._nullMapping = false;
            this._nullable = true;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public SQLiteParameter(string parameterName, System.Data.DbType parameterType, int parameterSize, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion rowVersion, object value) : this(parameterName, parameterType, parameterSize, sourceColumn, rowVersion)
        {
            this.Direction = direction;
            this.IsNullable = isNullable;
            this.Value = value;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public SQLiteParameter(string parameterName, System.Data.DbType parameterType, int parameterSize, ParameterDirection direction, byte precision, byte scale, string sourceColumn, DataRowVersion rowVersion, bool sourceColumnNullMapping, object value) : this(parameterName, parameterType, parameterSize, sourceColumn, rowVersion)
        {
            this.Direction = direction;
            this.SourceColumnNullMapping = sourceColumnNullMapping;
            this.Value = value;
        }

        public object Clone()
        {
            return new SQLiteParameter(this);
        }

        public override void ResetDbType()
        {
            this._dbType = -1;
        }

        [DbProviderSpecificTypeProperty(true), RefreshProperties(RefreshProperties.All)]
        public override System.Data.DbType DbType
        {
            get
            {
                if (this._dbType != -1)
                {
                    return (System.Data.DbType) this._dbType;
                }
                if ((this._objValue != null) && (this._objValue != DBNull.Value))
                {
                    return SQLiteConvert.TypeToDbType(this._objValue.GetType());
                }
                return System.Data.DbType.String;
            }
            set
            {
                this._dbType = (int) value;
            }
        }

        public override ParameterDirection Direction
        {
            get
            {
                return ParameterDirection.Input;
            }
            set
            {
                if (value != ParameterDirection.Input)
                {
                    throw new NotSupportedException();
                }
            }
        }

        public override bool IsNullable
        {
            get
            {
                return this._nullable;
            }
            set
            {
                this._nullable = value;
            }
        }

        public override string ParameterName
        {
            get
            {
                return this._parameterName;
            }
            set
            {
                this._parameterName = value;
            }
        }

        [DefaultValue(0)]
        public override int Size
        {
            get
            {
                return this._dataSize;
            }
            set
            {
                this._dataSize = value;
            }
        }

        public override string SourceColumn
        {
            get
            {
                return this._sourceColumn;
            }
            set
            {
                this._sourceColumn = value;
            }
        }

        public override bool SourceColumnNullMapping
        {
            get
            {
                return this._nullMapping;
            }
            set
            {
                this._nullMapping = value;
            }
        }

        public override DataRowVersion SourceVersion
        {
            get
            {
                return this._rowVersion;
            }
            set
            {
                this._rowVersion = value;
            }
        }

        [TypeConverter(typeof(StringConverter)), RefreshProperties(RefreshProperties.All)]
        public override object Value
        {
            get
            {
                return this._objValue;
            }
            set
            {
                this._objValue = value;
                if (((this._dbType == -1) && (this._objValue != null)) && (this._objValue != DBNull.Value))
                {
                    this._dbType = (int) SQLiteConvert.TypeToDbType(this._objValue.GetType());
                }
            }
        }
    }
}

