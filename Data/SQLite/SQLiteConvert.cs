﻿namespace System.Data.SQLite
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    public abstract class SQLiteConvert
    {
        private static Type[] _affinitytotype = new Type[] { typeof(object), typeof(long), typeof(double), typeof(string), typeof(byte[]), typeof(object), typeof(DateTime), typeof(object) };
        internal SQLiteDateFormats _datetimeFormat;
        private static string[] _datetimeFormats = new string[] { 
            "THHmmss", "THHmm", "HH:mm:ss", "HH:mm", "HH:mm:ss.FFFFFFF", "yy-MM-dd", "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss.FFFFFFF", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-MM-ddTHH:mm:ss.FFFFFFF", "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss", "yyyyMMddHHmmss", "yyyyMMddHHmm", "yyyyMMddTHHmmssFFFFFFF", 
            "yyyyMMdd"
         };
        private static SQLiteTypeNames[] _dbtypeNames = new SQLiteTypeNames[] { new SQLiteTypeNames("INTEGER", DbType.Int64), new SQLiteTypeNames("TINYINT", DbType.Byte), new SQLiteTypeNames("INT", DbType.Int32), new SQLiteTypeNames("VARCHAR", DbType.AnsiString), new SQLiteTypeNames("NVARCHAR", DbType.String), new SQLiteTypeNames("CHAR", DbType.AnsiStringFixedLength), new SQLiteTypeNames("NCHAR", DbType.StringFixedLength), new SQLiteTypeNames("FLOAT", DbType.Double), new SQLiteTypeNames("REAL", DbType.Single), new SQLiteTypeNames("BIT", DbType.Boolean), new SQLiteTypeNames("DECIMAL", DbType.Decimal), new SQLiteTypeNames("DATETIME", DbType.DateTime), new SQLiteTypeNames("BLOB", DbType.Binary), new SQLiteTypeNames("UNIQUEIDENTIFIER", DbType.Guid), new SQLiteTypeNames("SMALLINT", DbType.Int16) };
        private static int[] _dbtypetocolumnsize = new int[] { 
            0x7fffffff, 0x7fffffff, 1, 1, 8, 8, 8, 8, 8, 0x10, 2, 4, 8, 0x7fffffff, 1, 4, 
            0x7fffffff, 8, 2, 4, 8, 8, 0x7fffffff, 0x7fffffff, 0x7fffffff, 0x7fffffff
         };
        private static object[] _dbtypetonumericprecision = new object[] { 
            DBNull.Value, DBNull.Value, 3, DBNull.Value, 0x13, DBNull.Value, DBNull.Value, 0x35, 0x35, DBNull.Value, 5, 10, 0x13, DBNull.Value, 3, 0x18, 
            DBNull.Value, DBNull.Value, 5, 10, 0x13, 0x35, DBNull.Value, DBNull.Value, DBNull.Value
         };
        private static object[] _dbtypetonumericscale = new object[] { 
            DBNull.Value, DBNull.Value, 0, DBNull.Value, 4, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, 0, 0, 0, DBNull.Value, 0, DBNull.Value, 
            DBNull.Value, DBNull.Value, 0, 0, 0, 0, DBNull.Value, DBNull.Value, DBNull.Value
         };
        private static Type[] _dbtypeToType = new Type[] { 
            typeof(string), typeof(byte[]), typeof(byte), typeof(bool), typeof(decimal), typeof(DateTime), typeof(DateTime), typeof(decimal), typeof(double), typeof(Guid), typeof(short), typeof(int), typeof(long), typeof(object), typeof(sbyte), typeof(float), 
            typeof(string), typeof(DateTime), typeof(ushort), typeof(uint), typeof(ulong), typeof(double), typeof(string), typeof(string), typeof(string), typeof(string)
         };
        private static TypeAffinity[] _typecodeAffinities = new TypeAffinity[] { 
            TypeAffinity.Null, TypeAffinity.Blob, TypeAffinity.Null, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Int64, TypeAffinity.Double, TypeAffinity.Double, TypeAffinity.Double, 
            TypeAffinity.DateTime, TypeAffinity.Null, TypeAffinity.Text
         };
        private static SQLiteTypeNames[] _typeNames = new SQLiteTypeNames[] { 
            new SQLiteTypeNames("COUNTER", DbType.Int64), new SQLiteTypeNames("AUTOINCREMENT", DbType.Int64), new SQLiteTypeNames("IDENTITY", DbType.Int64), new SQLiteTypeNames("LONGTEXT", DbType.String), new SQLiteTypeNames("LONGCHAR", DbType.String), new SQLiteTypeNames("LONGVARCHAR", DbType.String), new SQLiteTypeNames("LONG", DbType.Int64), new SQLiteTypeNames("TINYINT", DbType.Byte), new SQLiteTypeNames("INTEGER", DbType.Int64), new SQLiteTypeNames("INT", DbType.Int32), new SQLiteTypeNames("VARCHAR", DbType.String), new SQLiteTypeNames("NVARCHAR", DbType.String), new SQLiteTypeNames("CHAR", DbType.String), new SQLiteTypeNames("NCHAR", DbType.String), new SQLiteTypeNames("TEXT", DbType.String), new SQLiteTypeNames("NTEXT", DbType.String), 
            new SQLiteTypeNames("STRING", DbType.String), new SQLiteTypeNames("DOUBLE", DbType.Double), new SQLiteTypeNames("FLOAT", DbType.Double), new SQLiteTypeNames("REAL", DbType.Single), new SQLiteTypeNames("BIT", DbType.Boolean), new SQLiteTypeNames("YESNO", DbType.Boolean), new SQLiteTypeNames("LOGICAL", DbType.Boolean), new SQLiteTypeNames("BOOL", DbType.Boolean), new SQLiteTypeNames("NUMERIC", DbType.Decimal), new SQLiteTypeNames("DECIMAL", DbType.Decimal), new SQLiteTypeNames("MONEY", DbType.Decimal), new SQLiteTypeNames("CURRENCY", DbType.Decimal), new SQLiteTypeNames("TIME", DbType.DateTime), new SQLiteTypeNames("DATE", DbType.DateTime), new SQLiteTypeNames("SMALLDATE", DbType.DateTime), new SQLiteTypeNames("BLOB", DbType.Binary), 
            new SQLiteTypeNames("BINARY", DbType.Binary), new SQLiteTypeNames("VARBINARY", DbType.Binary), new SQLiteTypeNames("IMAGE", DbType.Binary), new SQLiteTypeNames("GENERAL", DbType.Binary), new SQLiteTypeNames("OLEOBJECT", DbType.Binary), new SQLiteTypeNames("GUID", DbType.Guid), new SQLiteTypeNames("UNIQUEIDENTIFIER", DbType.Guid), new SQLiteTypeNames("MEMO", DbType.String), new SQLiteTypeNames("NOTE", DbType.String), new SQLiteTypeNames("SMALLINT", DbType.Int16), new SQLiteTypeNames("BIGINT", DbType.Int64)
         };
        private static DbType[] _typetodbtype = new DbType[] { 
            DbType.Object, DbType.Binary, DbType.Object, DbType.Boolean, DbType.SByte, DbType.SByte, DbType.Byte, DbType.Int16, DbType.UInt16, DbType.Int32, DbType.UInt32, DbType.Int64, DbType.UInt64, DbType.Single, DbType.Double, DbType.Decimal, 
            DbType.DateTime, DbType.Object, DbType.String
         };
        private static Encoding _utf8 = new UTF8Encoding();

        internal SQLiteConvert(SQLiteDateFormats fmt)
        {
            this._datetimeFormat = fmt;
        }

        internal static void ColumnToType(SQLiteStatement stmt, int i, SQLiteType typ)
        {
            typ.Type = TypeNameToDbType(stmt._sql.ColumnType(stmt, i, out typ.Affinity));
        }

        internal static int DbTypeToColumnSize(DbType typ)
        {
            return _dbtypetocolumnsize[(int) typ];
        }

        internal static object DbTypeToNumericPrecision(DbType typ)
        {
            return _dbtypetonumericprecision[(int) typ];
        }

        internal static object DbTypeToNumericScale(DbType typ)
        {
            return _dbtypetonumericscale[(int) typ];
        }

        internal static Type DbTypeToType(DbType typ)
        {
            return _dbtypeToType[(int) typ];
        }

        internal static string DbTypeToTypeName(DbType typ)
        {
            for (int i = 0; i < _dbtypeNames.Length; i++)
            {
                if (_dbtypeNames[i].dataType == typ)
                {
                    return _dbtypeNames[i].typeName;
                }
            }
            return string.Empty;
        }

        public static string[] Split(string source, char separator)
        {
            string str;
            char[] anyOf = new char[] { '"', separator };
            char[] chArray2 = new char[] { '"' };
            int startIndex = 0;
            List<string> list = new List<string>();
            while (source.Length > 0)
            {
                startIndex = source.IndexOfAny(anyOf, startIndex);
                if (startIndex == -1)
                {
                    break;
                }
                if (source[startIndex] == anyOf[0])
                {
                    startIndex = source.IndexOfAny(chArray2, startIndex + 1);
                    if (startIndex == -1)
                    {
                        break;
                    }
                    startIndex++;
                }
                else
                {
                    str = source.Substring(0, startIndex).Trim();
                    if (((str.Length > 1) && (str[0] == chArray2[0])) && (str[str.Length - 1] == str[0]))
                    {
                        str = str.Substring(1, str.Length - 2);
                    }
                    source = source.Substring(startIndex + 1).Trim();
                    if (str.Length > 0)
                    {
                        list.Add(str);
                    }
                    startIndex = 0;
                }
            }
            if (source.Length > 0)
            {
                str = source.Trim();
                if (((str.Length > 1) && (str[0] == chArray2[0])) && (str[str.Length - 1] == str[0]))
                {
                    str = str.Substring(1, str.Length - 2);
                }
                list.Add(str);
            }
            string[] array = new string[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        internal static Type SQLiteTypeToType(SQLiteType t)
        {
            if (t.Type == DbType.Object)
            {
                return _affinitytotype[(int) t.Affinity];
            }
            return DbTypeToType(t.Type);
        }

        public static bool ToBoolean(object source)
        {
            if (source is bool)
            {
                return (bool) source;
            }
            return ToBoolean(source.ToString());
        }

        public static bool ToBoolean(string source)
        {
            if (string.Compare(source, bool.TrueString, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            if (string.Compare(source, bool.FalseString, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }
            switch (source.ToLower())
            {
                case "yes":
                case "y":
                case "1":
                case "on":
                    return true;

                case "no":
                case "n":
                case "0":
                case "off":
                    return false;
            }
            throw new ArgumentException("source");
        }

        public DateTime ToDateTime(double julianDay)
        {
            return DateTime.FromOADate(julianDay - 2415018.5);
        }

        public DateTime ToDateTime(string dateText)
        {
            switch (this._datetimeFormat)
            {
                case SQLiteDateFormats.Ticks:
                    return new DateTime(Convert.ToInt64(dateText, CultureInfo.InvariantCulture));

                case SQLiteDateFormats.JulianDay:
                    return this.ToDateTime(Convert.ToDouble(dateText, CultureInfo.InvariantCulture));
            }
            return DateTime.ParseExact(dateText, _datetimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
        }

        internal DateTime ToDateTime(IntPtr ptr, int len)
        {
            return this.ToDateTime(this.ToString(ptr, len));
        }

        public double ToJulianDay(DateTime value)
        {
            return (value.ToOADate() + 2415018.5);
        }

        public string ToString(DateTime dateValue)
        {
            switch (this._datetimeFormat)
            {
                case SQLiteDateFormats.Ticks:
                    return dateValue.Ticks.ToString(CultureInfo.InvariantCulture);

                case SQLiteDateFormats.JulianDay:
                    return this.ToJulianDay(dateValue).ToString(CultureInfo.InvariantCulture);
            }
            return dateValue.ToString(_datetimeFormats[7], CultureInfo.InvariantCulture);
        }

        public virtual string ToString(IntPtr nativestring, int nativestringlen)
        {
            return UTF8ToString(nativestring, nativestringlen);
        }

        public byte[] ToUTF8(DateTime dateTimeValue)
        {
            return ToUTF8(this.ToString(dateTimeValue));
        }

        public static byte[] ToUTF8(string sourceText)
        {
            int index = _utf8.GetByteCount(sourceText) + 1;
            byte[] bytes = new byte[index];
            index = _utf8.GetBytes(sourceText, 0, sourceText.Length, bytes, 0);
            bytes[index] = 0;
            return bytes;
        }

        internal static DbType TypeNameToDbType(string Name)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                int length = _typeNames.Length;
                for (int i = 0; i < length; i++)
                {
                    if (string.Compare(Name, 0, _typeNames[i].typeName, 0, _typeNames[i].typeName.Length, true, CultureInfo.InvariantCulture) == 0)
                    {
                        return _typeNames[i].dataType;
                    }
                }
            }
            return DbType.Object;
        }

        internal static TypeAffinity TypeToAffinity(Type typ)
        {
            TypeCode typeCode = Type.GetTypeCode(typ);
            if (typeCode != TypeCode.Object)
            {
                return _typecodeAffinities[(int) typeCode];
            }
            if ((typ != typeof(byte[])) && (typ != typeof(Guid)))
            {
                return TypeAffinity.Text;
            }
            return TypeAffinity.Blob;
        }

        internal static DbType TypeToDbType(Type typ)
        {
            TypeCode typeCode = Type.GetTypeCode(typ);
            if (typeCode != TypeCode.Object)
            {
                return _typetodbtype[(int) typeCode];
            }
            if (typ == typeof(byte[]))
            {
                return DbType.Binary;
            }
            if (typ == typeof(Guid))
            {
                return DbType.Guid;
            }
            return DbType.String;
        }

        public static string UTF8ToString(IntPtr nativestring, int nativestringlen)
        {
            if ((nativestringlen == 0) || (nativestring == IntPtr.Zero))
            {
                return "";
            }
            if (nativestringlen == -1)
            {
                do
                {
                    nativestringlen++;
                }
                while (Marshal.ReadByte(nativestring, nativestringlen) != 0);
            }
            byte[] destination = new byte[nativestringlen];
            Marshal.Copy(nativestring, destination, 0, nativestringlen);
            return _utf8.GetString(destination, 0, nativestringlen);
        }
    }
}

