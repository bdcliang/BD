namespace System.Data.SQLite
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;

    public sealed class SQLiteCommandBuilder : DbCommandBuilder
    {
        public SQLiteCommandBuilder() : this(null)
        {
        }

        public SQLiteCommandBuilder(SQLiteDataAdapter adp)
        {
            this.QuotePrefix = "[";
            this.QuoteSuffix = "]";
            this.DataAdapter = adp;
        }

        protected override void ApplyParameterInfo(DbParameter parameter, DataRow row, StatementType statementType, bool whereClause)
        {
            SQLiteParameter parameter2 = (SQLiteParameter) parameter;
            parameter2.DbType = (DbType) row[SchemaTableColumn.ProviderType];
        }

        public SQLiteCommand GetDeleteCommand()
        {
            return (SQLiteCommand) base.GetDeleteCommand();
        }

        public SQLiteCommand GetDeleteCommand(bool useColumnsForParameterNames)
        {
            return (SQLiteCommand) base.GetDeleteCommand(useColumnsForParameterNames);
        }

        public SQLiteCommand GetInsertCommand()
        {
            return (SQLiteCommand) base.GetInsertCommand();
        }

        public SQLiteCommand GetInsertCommand(bool useColumnsForParameterNames)
        {
            return (SQLiteCommand) base.GetInsertCommand(useColumnsForParameterNames);
        }

        protected override string GetParameterName(int parameterOrdinal)
        {
            return string.Format(CultureInfo.InvariantCulture, "@param{0}", new object[] { parameterOrdinal });
        }

        protected override string GetParameterName(string parameterName)
        {
            return string.Format(CultureInfo.InvariantCulture, "@{0}", new object[] { parameterName });
        }

        protected override string GetParameterPlaceholder(int parameterOrdinal)
        {
            return this.GetParameterName(parameterOrdinal);
        }

        protected override DataTable GetSchemaTable(DbCommand sourceCommand)
        {
            using (IDataReader reader = sourceCommand.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly))
            {
                DataTable schemaTable = reader.GetSchemaTable();
                if (this.HasSchemaPrimaryKey(schemaTable))
                {
                    this.ResetIsUniqueSchemaColumn(schemaTable);
                }
                return schemaTable;
            }
        }

        public SQLiteCommand GetUpdateCommand()
        {
            return (SQLiteCommand) base.GetUpdateCommand();
        }

        public SQLiteCommand GetUpdateCommand(bool useColumnsForParameterNames)
        {
            return (SQLiteCommand) base.GetUpdateCommand(useColumnsForParameterNames);
        }

        private bool HasSchemaPrimaryKey(DataTable schema)
        {
            DataColumn column = schema.Columns[SchemaTableColumn.IsKey];
            foreach (DataRow row in schema.Rows)
            {
                if ((bool) row[column])
                {
                    return true;
                }
            }
            return false;
        }

        public override string QuoteIdentifier(string unquotedIdentifier)
        {
            if ((!string.IsNullOrEmpty(this.QuotePrefix) && !string.IsNullOrEmpty(this.QuoteSuffix)) && !string.IsNullOrEmpty(unquotedIdentifier))
            {
                return (this.QuotePrefix + unquotedIdentifier.Replace(this.QuoteSuffix, this.QuoteSuffix + this.QuoteSuffix) + this.QuoteSuffix);
            }
            return unquotedIdentifier;
        }

        private void ResetIsUniqueSchemaColumn(DataTable schema)
        {
            DataColumn column = schema.Columns[SchemaTableColumn.IsUnique];
            DataColumn column2 = schema.Columns[SchemaTableColumn.IsKey];
            foreach (DataRow row in schema.Rows)
            {
                if (!((bool) row[column2]))
                {
                    row[column] = false;
                }
            }
            schema.AcceptChanges();
        }

        private void RowUpdatingEventHandler(object sender, RowUpdatingEventArgs e)
        {
            base.RowUpdatingHandler(e);
        }

        protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
        {
            if (adapter == base.DataAdapter)
            {
                ((SQLiteDataAdapter) adapter).RowUpdating -= new EventHandler<RowUpdatingEventArgs>(this.RowUpdatingEventHandler);
            }
            else
            {
                ((SQLiteDataAdapter) adapter).RowUpdating += new EventHandler<RowUpdatingEventArgs>(this.RowUpdatingEventHandler);
            }
        }

        public override string UnquoteIdentifier(string quotedIdentifier)
        {
            if (((!string.IsNullOrEmpty(this.QuotePrefix) && !string.IsNullOrEmpty(this.QuoteSuffix)) && !string.IsNullOrEmpty(quotedIdentifier)) && (quotedIdentifier.StartsWith(this.QuotePrefix, StringComparison.InvariantCultureIgnoreCase) && quotedIdentifier.EndsWith(this.QuoteSuffix, StringComparison.InvariantCultureIgnoreCase)))
            {
                return quotedIdentifier.Substring(this.QuotePrefix.Length, quotedIdentifier.Length - (this.QuotePrefix.Length + this.QuoteSuffix.Length)).Replace(this.QuoteSuffix + this.QuoteSuffix, this.QuoteSuffix);
            }
            return quotedIdentifier;
        }

        [Browsable(false)]
        public override System.Data.Common.CatalogLocation CatalogLocation
        {
            get
            {
                return base.CatalogLocation;
            }
            set
            {
                base.CatalogLocation = value;
            }
        }

        [Browsable(false)]
        public override string CatalogSeparator
        {
            get
            {
                return base.CatalogSeparator;
            }
            set
            {
                base.CatalogSeparator = value;
            }
        }

        public SQLiteDataAdapter DataAdapter
        {
            get
            {
                return (SQLiteDataAdapter) base.DataAdapter;
            }
            set
            {
                base.DataAdapter = value;
            }
        }

        [DefaultValue("["), Browsable(false)]
        public override string QuotePrefix
        {
            get
            {
                return base.QuotePrefix;
            }
            set
            {
                base.QuotePrefix = value;
            }
        }

        [Browsable(false)]
        public override string QuoteSuffix
        {
            get
            {
                return base.QuoteSuffix;
            }
            set
            {
                base.QuoteSuffix = value;
            }
        }

        [Browsable(false)]
        public override string SchemaSeparator
        {
            get
            {
                return base.SchemaSeparator;
            }
            set
            {
                base.SchemaSeparator = value;
            }
        }
    }
}

