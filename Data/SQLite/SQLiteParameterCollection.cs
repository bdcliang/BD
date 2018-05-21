namespace System.Data.SQLite
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;

    [Editor("Microsoft.VSDesigner.Data.Design.DBParametersEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ListBindable(false)]
    public sealed class SQLiteParameterCollection : DbParameterCollection
    {
        private SQLiteCommand _command;
        private List<SQLiteParameter> _parameterList;
        private bool _unboundFlag;

        internal SQLiteParameterCollection(SQLiteCommand cmd)
        {
            this._command = cmd;
            this._parameterList = new List<SQLiteParameter>();
            this._unboundFlag = true;
        }

        public int Add(SQLiteParameter parameter)
        {
            int index = -1;
            if (!string.IsNullOrEmpty(parameter.ParameterName))
            {
                index = this.IndexOf(parameter.ParameterName);
            }
            if (index == -1)
            {
                index = this._parameterList.Count;
                this._parameterList.Add(parameter);
            }
            this.SetParameter(index, parameter);
            return index;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int Add(object value)
        {
            return this.Add((SQLiteParameter) value);
        }

        public SQLiteParameter Add(string parameterName, DbType parameterType)
        {
            SQLiteParameter parameter = new SQLiteParameter(parameterName, parameterType);
            this.Add(parameter);
            return parameter;
        }

        public SQLiteParameter Add(string parameterName, DbType parameterType, int parameterSize)
        {
            SQLiteParameter parameter = new SQLiteParameter(parameterName, parameterType, parameterSize);
            this.Add(parameter);
            return parameter;
        }

        public SQLiteParameter Add(string parameterName, DbType parameterType, int parameterSize, string sourceColumn)
        {
            SQLiteParameter parameter = new SQLiteParameter(parameterName, parameterType, parameterSize, sourceColumn);
            this.Add(parameter);
            return parameter;
        }

        public void AddRange(SQLiteParameter[] values)
        {
            int length = values.Length;
            for (int i = 0; i < length; i++)
            {
                this.Add(values[i]);
            }
        }

        public override void AddRange(Array values)
        {
            int length = values.Length;
            for (int i = 0; i < length; i++)
            {
                this.Add((SQLiteParameter) values.GetValue(i));
            }
        }

        public SQLiteParameter AddWithValue(string parameterName, object value)
        {
            SQLiteParameter parameter = new SQLiteParameter(parameterName, value);
            this.Add(parameter);
            return parameter;
        }

        public override void Clear()
        {
            this._unboundFlag = true;
            this._parameterList.Clear();
        }

        public override bool Contains(object value)
        {
            return this._parameterList.Contains((SQLiteParameter) value);
        }

        public override bool Contains(string parameterName)
        {
            return (this.IndexOf(parameterName) != -1);
        }

        public override void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator GetEnumerator()
        {
            return this._parameterList.GetEnumerator();
        }

        protected override DbParameter GetParameter(int index)
        {
            return this._parameterList[index];
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            return this.GetParameter(this.IndexOf(parameterName));
        }

        public override int IndexOf(object value)
        {
            return this._parameterList.IndexOf((SQLiteParameter) value);
        }

        public override int IndexOf(string parameterName)
        {
            int count = this._parameterList.Count;
            for (int i = 0; i < count; i++)
            {
                if (string.Compare(parameterName, this._parameterList[i].ParameterName, true, CultureInfo.InvariantCulture) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public override void Insert(int index, object value)
        {
            this._unboundFlag = true;
            this._parameterList.Insert(index, (SQLiteParameter) value);
        }

        internal void MapParameters(SQLiteStatement activeStatement)
        {
            if ((this._unboundFlag && (this._parameterList.Count != 0)) && (this._command._statementList != null))
            {
                int num = 0;
                int num3 = -1;
                foreach (SQLiteParameter parameter in this._parameterList)
                {
                    int count;
                    num3++;
                    string parameterName = parameter.ParameterName;
                    if (parameterName == null)
                    {
                        parameterName = string.Format(CultureInfo.InvariantCulture, ";{0}", new object[] { num });
                        num++;
                    }
                    bool flag = false;
                    if (activeStatement == null)
                    {
                        count = this._command._statementList.Count;
                    }
                    else
                    {
                        count = 1;
                    }
                    SQLiteStatement statement = activeStatement;
                    int num2 = 0;
                    while (num2 < count)
                    {
                        flag = false;
                        if (statement == null)
                        {
                            statement = this._command._statementList[num2];
                        }
                        if ((statement._paramNames != null) && statement.MapParameter(parameterName, parameter))
                        {
                            flag = true;
                        }
                        statement = null;
                        num2++;
                    }
                    if (!flag)
                    {
                        parameterName = string.Format(CultureInfo.InvariantCulture, ";{0}", new object[] { num3 });
                        statement = activeStatement;
                        for (num2 = 0; num2 < count; num2++)
                        {
                            if (statement == null)
                            {
                                statement = this._command._statementList[num2];
                            }
                            if ((statement._paramNames != null) && statement.MapParameter(parameterName, parameter))
                            {
                                flag = true;
                            }
                            statement = null;
                        }
                    }
                }
                if (activeStatement == null)
                {
                    this._unboundFlag = false;
                }
            }
        }

        public override void Remove(object value)
        {
            this._unboundFlag = true;
            this._parameterList.Remove((SQLiteParameter) value);
        }

        public override void RemoveAt(int index)
        {
            this._unboundFlag = true;
            this._parameterList.RemoveAt(index);
        }

        public override void RemoveAt(string parameterName)
        {
            this.RemoveAt(this.IndexOf(parameterName));
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            this._unboundFlag = true;
            this._parameterList[index] = (SQLiteParameter) value;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            this.SetParameter(this.IndexOf(parameterName), value);
        }

        internal void Unbind()
        {
            this._unboundFlag = true;
        }

        public override int Count
        {
            get
            {
                return this._parameterList.Count;
            }
        }

        public override bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public SQLiteParameter this[string parameterName]
        {
            get
            {
                return (SQLiteParameter) this.GetParameter(parameterName);
            }
            set
            {
                this.SetParameter(parameterName, value);
            }
        }

        public SQLiteParameter this[int index]
        {
            get
            {
                return (SQLiteParameter) this.GetParameter(index);
            }
            set
            {
                this.SetParameter(index, value);
            }
        }

        public override object SyncRoot
        {
            get
            {
                return null;
            }
        }
    }
}

