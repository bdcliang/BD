namespace System.Data.SQLite
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [DefaultProperty("DataSource"), DefaultMember("Item")]
    public sealed class SQLiteConnectionStringBuilder : DbConnectionStringBuilder
    {
        private Hashtable _properties;

        public SQLiteConnectionStringBuilder()
        {
            this.Initialize(null);
        }

        public SQLiteConnectionStringBuilder(string connectionString)
        {
            this.Initialize(connectionString);
        }

        private void FallbackGetProperties(Hashtable propertyList)
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this, true))
            {
                if ((descriptor.Name != "ConnectionString") && !propertyList.ContainsKey(descriptor.DisplayName))
                {
                    propertyList.Add(descriptor.DisplayName, descriptor);
                }
            }
        }

        private void Initialize(string cnnString)
        {
            this._properties = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            try
            {
                base.GetProperties(this._properties);
            }
            catch (NotImplementedException)
            {
                this.FallbackGetProperties(this._properties);
            }
            if (!string.IsNullOrEmpty(cnnString))
            {
                base.ConnectionString = cnnString;
            }
        }

        public override bool TryGetValue(string keyword, out object value)
        {
            bool flag = base.TryGetValue(keyword, out value);
            if (this._properties.ContainsKey(keyword))
            {
                PropertyDescriptor descriptor = this._properties[keyword] as PropertyDescriptor;
                if (descriptor == null)
                {
                    return flag;
                }
                if (flag)
                {
                    if (descriptor.PropertyType == typeof(bool))
                    {
                        value = SQLiteConvert.ToBoolean(value);
                        return flag;
                    }
                    value = TypeDescriptor.GetConverter(descriptor.PropertyType).ConvertFrom(value);
                    return flag;
                }
                DefaultValueAttribute attribute = descriptor.Attributes[typeof(DefaultValueAttribute)] as DefaultValueAttribute;
                if (attribute != null)
                {
                    value = attribute.Value;
                    flag = true;
                }
            }
            return flag;
        }

        [Browsable(true), DefaultValue(true)]
        public bool BinaryGUID
        {
            get
            {
                object obj2;
                this.TryGetValue("binaryguid", out obj2);
                return SQLiteConvert.ToBoolean(obj2);
            }
            set
            {
                this["binaryguid"] = value;
            }
        }

        [DisplayName("Cache Size"), DefaultValue(0x7d0), Browsable(true)]
        public int CacheSize
        {
            get
            {
                object obj2;
                this.TryGetValue("cache size", out obj2);
                return Convert.ToInt32(obj2, CultureInfo.CurrentCulture);
            }
            set
            {
                this["cache size"] = value;
            }
        }

        [DisplayName("Data Source"), Browsable(true), DefaultValue("")]
        public string DataSource
        {
            get
            {
                object obj2;
                this.TryGetValue("data source", out obj2);
                return obj2.ToString();
            }
            set
            {
                this["data source"] = value;
            }
        }

        [DefaultValue(1), Browsable(true)]
        public SQLiteDateFormats DateTimeFormat
        {
            get
            {
                object obj2;
                this.TryGetValue("datetimeformat", out obj2);
                if (obj2 is string)
                {
                    return (SQLiteDateFormats) TypeDescriptor.GetConverter(typeof(SQLiteDateFormats)).ConvertFrom(obj2);
                }
                return (SQLiteDateFormats) obj2;
            }
            set
            {
                this["datetimeformat"] = value;
            }
        }

        [DisplayName("Default Isolation Level"), Browsable(true), DefaultValue(0x100000)]
        public IsolationLevel DefaultIsolationLevel
        {
            get
            {
                object obj2;
                this.TryGetValue("default isolationlevel", out obj2);
                if (obj2 is string)
                {
                    return (IsolationLevel) TypeDescriptor.GetConverter(typeof(IsolationLevel)).ConvertFrom(obj2);
                }
                return (IsolationLevel) obj2;
            }
            set
            {
                this["default isolationlevel"] = value;
            }
        }

        [DefaultValue(30), Browsable(true), DisplayName("Default Timeout")]
        public int DefaultTimeout
        {
            get
            {
                object obj2;
                this.TryGetValue("default timeout", out obj2);
                return Convert.ToInt32(obj2, CultureInfo.CurrentCulture);
            }
            set
            {
                this["default timeout"] = value;
            }
        }

        [Browsable(true), DefaultValue(true)]
        public bool Enlist
        {
            get
            {
                object obj2;
                this.TryGetValue("enlist", out obj2);
                return SQLiteConvert.ToBoolean(obj2);
            }
            set
            {
                this["enlist"] = value;
            }
        }

        [DefaultValue(false), Browsable(true)]
        public bool FailIfMissing
        {
            get
            {
                object obj2;
                this.TryGetValue("failifmissing", out obj2);
                return SQLiteConvert.ToBoolean(obj2);
            }
            set
            {
                this["failifmissing"] = value;
            }
        }

        [DefaultValue(0), DisplayName("Journal Mode"), Browsable(true)]
        public SQLiteJournalModeEnum JournalMode
        {
            get
            {
                object obj2;
                this.TryGetValue("journal mode", out obj2);
                if (obj2 is string)
                {
                    return (SQLiteJournalModeEnum) TypeDescriptor.GetConverter(typeof(SQLiteJournalModeEnum)).ConvertFrom(obj2);
                }
                return (SQLiteJournalModeEnum) obj2;
            }
            set
            {
                this["journal mode"] = value;
            }
        }

        [DisplayName("Legacy Format"), DefaultValue(false), Browsable(true)]
        public bool LegacyFormat
        {
            get
            {
                object obj2;
                this.TryGetValue("legacy format", out obj2);
                return SQLiteConvert.ToBoolean(obj2);
            }
            set
            {
                this["legacy format"] = value;
            }
        }

        [Browsable(true), DefaultValue(0), DisplayName("Max Page Count")]
        public int MaxPageCount
        {
            get
            {
                object obj2;
                this.TryGetValue("max page count", out obj2);
                return Convert.ToInt32(obj2, CultureInfo.CurrentCulture);
            }
            set
            {
                this["max page count"] = value;
            }
        }

        [DisplayName("Page Size"), DefaultValue(0x400), Browsable(true)]
        public int PageSize
        {
            get
            {
                object obj2;
                this.TryGetValue("page size", out obj2);
                return Convert.ToInt32(obj2, CultureInfo.CurrentCulture);
            }
            set
            {
                this["page size"] = value;
            }
        }

        [DefaultValue(""), PasswordPropertyText(true), Browsable(true)]
        public string Password
        {
            get
            {
                object obj2;
                this.TryGetValue("password", out obj2);
                return obj2.ToString();
            }
            set
            {
                this["password"] = value;
            }
        }

        [DefaultValue(false), Browsable(true)]
        public bool Pooling
        {
            get
            {
                object obj2;
                this.TryGetValue("pooling", out obj2);
                return SQLiteConvert.ToBoolean(obj2);
            }
            set
            {
                this["pooling"] = value;
            }
        }

        [DefaultValue(false), DisplayName("Read Only"), Browsable(true)]
        public bool ReadOnly
        {
            get
            {
                object obj2;
                this.TryGetValue("read only", out obj2);
                return SQLiteConvert.ToBoolean(obj2);
            }
            set
            {
                this["read only"] = value;
            }
        }

        [Browsable(true), DefaultValue(0), DisplayName("Synchronous")]
        public SynchronizationModes SyncMode
        {
            get
            {
                object obj2;
                this.TryGetValue("synchronous", out obj2);
                if (obj2 is string)
                {
                    return (SynchronizationModes) TypeDescriptor.GetConverter(typeof(SynchronizationModes)).ConvertFrom(obj2);
                }
                return (SynchronizationModes) obj2;
            }
            set
            {
                this["synchronous"] = value;
            }
        }

        [Browsable(false)]
        public string Uri
        {
            get
            {
                object obj2;
                this.TryGetValue("uri", out obj2);
                return obj2.ToString();
            }
            set
            {
                this["uri"] = value;
            }
        }

        [Browsable(true), DefaultValue(false)]
        public bool UseUTF16Encoding
        {
            get
            {
                object obj2;
                this.TryGetValue("useutf16encoding", out obj2);
                return SQLiteConvert.ToBoolean(obj2);
            }
            set
            {
                this["useutf16encoding"] = value;
            }
        }

        [Browsable(true), DefaultValue(3)]
        public int Version
        {
            get
            {
                object obj2;
                this.TryGetValue("version", out obj2);
                return Convert.ToInt32(obj2, CultureInfo.CurrentCulture);
            }
            set
            {
                if (value != 3)
                {
                    throw new NotSupportedException();
                }
                this["version"] = value;
            }
        }
    }
}

