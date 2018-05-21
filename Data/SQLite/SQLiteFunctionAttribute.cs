namespace System.Data.SQLite
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited=false, AllowMultiple=true)]
    public sealed class SQLiteFunctionAttribute : Attribute
    {
        private int _arguments;
        private FunctionType _functionType;
        internal Type _instanceType;
        private string _name;

        public SQLiteFunctionAttribute()
        {
            this.Name = "";
            this.Arguments = -1;
            this.FuncType = FunctionType.Scalar;
        }

        public int Arguments
        {
            get
            {
                return this._arguments;
            }
            set
            {
                this._arguments = value;
            }
        }

        public FunctionType FuncType
        {
            get
            {
                return this._functionType;
            }
            set
            {
                this._functionType = value;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }
    }
}

