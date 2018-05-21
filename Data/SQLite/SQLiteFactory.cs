namespace System.Data.SQLite
{
    using System;
    using System.Data.Common;
    using System.Reflection;
    using System.Security.Permissions;

    public sealed class SQLiteFactory : DbProviderFactory, IServiceProvider
    {
        private static Type _dbProviderServicesType = Type.GetType("System.Data.Common.DbProviderServices, System.Data.Entity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false);
        private static object _sqliteServices;
        public static readonly SQLiteFactory Instance = new SQLiteFactory();

        public override DbCommand CreateCommand()
        {
            return new SQLiteCommand();
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            return new SQLiteCommandBuilder();
        }

        public override DbConnection CreateConnection()
        {
            return new SQLiteConnection();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new SQLiteConnectionStringBuilder();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return new SQLiteDataAdapter();
        }

        public override DbParameter CreateParameter()
        {
            return new SQLiteParameter();
        }

        [ReflectionPermission(SecurityAction.Assert, MemberAccess=true)]
        private object GetSQLiteProviderServicesInstance()
        {
            if (_sqliteServices == null)
            {
                Type type = Type.GetType("System.Data.SQLite.SQLiteProviderServices, System.Data.SQLite.Linq, Version=2.0.38.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139", false);
                if (type != null)
                {
                    _sqliteServices = type.GetField("Instance", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(null);
                }
            }
            return _sqliteServices;
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            if ((serviceType != typeof(ISQLiteSchemaExtensions)) && ((_dbProviderServicesType == null) || (serviceType != _dbProviderServicesType)))
            {
                return null;
            }
            return this.GetSQLiteProviderServicesInstance();
        }
    }
}

