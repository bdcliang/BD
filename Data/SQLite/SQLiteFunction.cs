namespace System.Data.SQLite
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    public abstract class SQLiteFunction : IDisposable
    {
        internal SQLiteBase _base;
        private SQLiteCollation _CompareFunc;
        private SQLiteCollation _CompareFunc16;
        internal IntPtr _context;
        private Dictionary<long, AggregateData> _contextDataList = new Dictionary<long, AggregateData>();
        private SQLiteFinalCallback _FinalFunc;
        private SQLiteCallback _InvokeFunc;
        private static List<SQLiteFunctionAttribute> _registeredFunctions = new List<SQLiteFunctionAttribute>();
        private SQLiteCallback _StepFunc;

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        static SQLiteFunction()
        {
            try
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                int length = assemblies.Length;
                AssemblyName name = Assembly.GetCallingAssembly().GetName();
                for (int i = 0; i < length; i++)
                {
                    Type[] types;
                    bool flag = false;
                    try
                    {
                        AssemblyName[] referencedAssemblies = assemblies[i].GetReferencedAssemblies();
                        int num3 = referencedAssemblies.Length;
                        for (int k = 0; k < num3; k++)
                        {
                            if (referencedAssemblies[k].Name == name.Name)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            continue;
                        }
                        types = assemblies[i].GetTypes();
                    }
                    catch (ReflectionTypeLoadException exception)
                    {
                        types = exception.Types;
                    }
                    int num5 = types.Length;
                    for (int j = 0; j < num5; j++)
                    {
                        if (types[j] != null)
                        {
                            object[] customAttributes = types[j].GetCustomAttributes(typeof(SQLiteFunctionAttribute), false);
                            int num7 = customAttributes.Length;
                            for (int m = 0; m < num7; m++)
                            {
                                SQLiteFunctionAttribute item = customAttributes[m] as SQLiteFunctionAttribute;
                                if (item != null)
                                {
                                    item._instanceType = types[j];
                                    _registeredFunctions.Add(item);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        protected SQLiteFunction()
        {
        }

        internal static SQLiteFunction[] BindFunctions(SQLiteBase sqlbase)
        {
            List<SQLiteFunction> list = new List<SQLiteFunction>();
            foreach (SQLiteFunctionAttribute attribute in _registeredFunctions)
            {
                SQLiteFunction item = (SQLiteFunction) Activator.CreateInstance(attribute._instanceType);
                item._base = sqlbase;
                item._InvokeFunc = (attribute.FuncType == FunctionType.Scalar) ? new SQLiteCallback(item.ScalarCallback) : null;
                item._StepFunc = (attribute.FuncType == FunctionType.Aggregate) ? new SQLiteCallback(item.StepCallback) : null;
                item._FinalFunc = (attribute.FuncType == FunctionType.Aggregate) ? new SQLiteFinalCallback(item.FinalCallback) : null;
                item._CompareFunc = (attribute.FuncType == FunctionType.Collation) ? new SQLiteCollation(item.CompareCallback) : null;
                item._CompareFunc16 = (attribute.FuncType == FunctionType.Collation) ? new SQLiteCollation(item.CompareCallback16) : null;
                if (attribute.FuncType != FunctionType.Collation)
                {
                    sqlbase.CreateFunction(attribute.Name, attribute.Arguments, item is SQLiteFunctionEx, item._InvokeFunc, item._StepFunc, item._FinalFunc);
                }
                else
                {
                    sqlbase.CreateCollation(attribute.Name, item._CompareFunc, item._CompareFunc16);
                }
                list.Add(item);
            }
            SQLiteFunction[] array = new SQLiteFunction[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        public virtual int Compare(string param1, string param2)
        {
            return 0;
        }

        internal int CompareCallback(IntPtr ptr, int len1, IntPtr ptr1, int len2, IntPtr ptr2)
        {
            return this.Compare(System.Data.SQLite.SQLiteConvert.UTF8ToString(ptr1, len1), System.Data.SQLite.SQLiteConvert.UTF8ToString(ptr2, len2));
        }

        internal int CompareCallback16(IntPtr ptr, int len1, IntPtr ptr1, int len2, IntPtr ptr2)
        {
            return this.Compare(SQLite3_UTF16.UTF16ToString(ptr1, len1), SQLite3_UTF16.UTF16ToString(ptr2, len2));
        }

        internal object[] ConvertParams(int nArgs, IntPtr argsptr)
        {
            object[] objArray = new object[nArgs];
            IntPtr[] destination = new IntPtr[nArgs];
            Marshal.Copy(argsptr, destination, 0, nArgs);
            for (int i = 0; i < nArgs; i++)
            {
                switch (this._base.GetParamValueType(destination[i]))
                {
                    case TypeAffinity.Int64:
                        objArray[i] = this._base.GetParamValueInt64(destination[i]);
                        break;

                    case TypeAffinity.Double:
                        objArray[i] = this._base.GetParamValueDouble(destination[i]);
                        break;

                    case TypeAffinity.Text:
                        objArray[i] = this._base.GetParamValueText(destination[i]);
                        break;

                    case TypeAffinity.Blob:
                    {
                        int nLength = (int) this._base.GetParamValueBytes(destination[i], 0, null, 0, 0);
                        byte[] bDest = new byte[nLength];
                        this._base.GetParamValueBytes(destination[i], 0, bDest, 0, nLength);
                        objArray[i] = bDest;
                        break;
                    }
                    case TypeAffinity.Null:
                        objArray[i] = DBNull.Value;
                        break;

                    case TypeAffinity.DateTime:
                        objArray[i] = this._base.ToDateTime(this._base.GetParamValueText(destination[i]));
                        break;
                }
            }
            return objArray;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (KeyValuePair<long, AggregateData> pair in this._contextDataList)
                {
                    IDisposable disposable = pair.Value._data as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
                this._contextDataList.Clear();
                this._InvokeFunc = null;
                this._StepFunc = null;
                this._FinalFunc = null;
                this._CompareFunc = null;
                this._base = null;
                this._contextDataList = null;
            }
        }

        public virtual object Final(object contextData)
        {
            return null;
        }

        internal void FinalCallback(IntPtr context)
        {
            long key = (long) this._base.AggregateContext(context);
            object contextData = null;
            if (this._contextDataList.ContainsKey(key))
            {
                contextData = this._contextDataList[key]._data;
                this._contextDataList.Remove(key);
            }
            this._context = context;
            this.SetReturnValue(context, this.Final(contextData));
            IDisposable disposable = contextData as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        public virtual object Invoke(object[] args)
        {
            return null;
        }

        public static void RegisterFunction(Type typ)
        {
            object[] customAttributes = typ.GetCustomAttributes(typeof(SQLiteFunctionAttribute), false);
            int length = customAttributes.Length;
            for (int i = 0; i < length; i++)
            {
                SQLiteFunctionAttribute item = customAttributes[i] as SQLiteFunctionAttribute;
                if (item != null)
                {
                    item._instanceType = typ;
                    _registeredFunctions.Add(item);
                }
            }
        }

        internal void ScalarCallback(IntPtr context, int nArgs, IntPtr argsptr)
        {
            this._context = context;
            this.SetReturnValue(context, this.Invoke(this.ConvertParams(nArgs, argsptr)));
        }

        private void SetReturnValue(IntPtr context, object returnValue)
        {
            if ((returnValue == null) || (returnValue == DBNull.Value))
            {
                this._base.ReturnNull(context);
            }
            else
            {
                Type typ = returnValue.GetType();
                if (typ == typeof(DateTime))
                {
                    this._base.ReturnText(context, this._base.ToString((DateTime) returnValue));
                }
                else
                {
                    Exception exception = returnValue as Exception;
                    if (exception != null)
                    {
                        this._base.ReturnError(context, exception.Message);
                    }
                    else
                    {
                        switch (System.Data.SQLite.SQLiteConvert.TypeToAffinity(typ))
                        {
                            case TypeAffinity.Int64:
                                this._base.ReturnInt64(context, Convert.ToInt64(returnValue, CultureInfo.CurrentCulture));
                                return;

                            case TypeAffinity.Double:
                                this._base.ReturnDouble(context, Convert.ToDouble(returnValue, CultureInfo.CurrentCulture));
                                return;

                            case TypeAffinity.Text:
                                this._base.ReturnText(context, returnValue.ToString());
                                return;

                            case TypeAffinity.Blob:
                                this._base.ReturnBlob(context, (byte[]) returnValue);
                                return;

                            case TypeAffinity.Null:
                                this._base.ReturnNull(context);
                                return;
                        }
                    }
                }
            }
        }

        public virtual void Step(object[] args, int stepNumber, ref object contextData)
        {
        }

        internal void StepCallback(IntPtr context, int nArgs, IntPtr argsptr)
        {
            AggregateData data;
            long key = (long) this._base.AggregateContext(context);
            if (!this._contextDataList.TryGetValue(key, out data))
            {
                data = new AggregateData();
                this._contextDataList[key] = data;
            }
            try
            {
                this._context = context;
                this.Step(this.ConvertParams(nArgs, argsptr), data._count, ref data._data);
            }
            finally
            {
                data._count++;
            }
        }

        public System.Data.SQLite.SQLiteConvert SQLiteConvert
        {
            get
            {
                return this._base;
            }
        }

        private class AggregateData
        {
            internal int _count = 1;
            internal object _data;
        }
    }
}

