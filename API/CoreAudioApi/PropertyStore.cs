namespace CoreAudioApi
{
    using CoreAudioApi.Interfaces;
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class PropertyStore
    {
        private IPropertyStore _Store;

        internal PropertyStore(IPropertyStore store)
        {
            this._Store = store;
        }

        public bool Contains(Guid guid)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this.Get(i).fmtid == guid)
                {
                    return true;
                }
            }
            return false;
        }

        public PropertyKey Get(int index)
        {
            PropertyKey key;
            Marshal.ThrowExceptionForHR(this._Store.GetAt(index, out key));
            return key;
        }

        public PropVariant GetValue(int index)
        {
            PropVariant variant;
            PropertyKey key = this.Get(index);
            Marshal.ThrowExceptionForHR(this._Store.GetValue(ref key, out variant));
            return variant;
        }

        public int Count
        {
            get
            {
                int num;
                Marshal.ThrowExceptionForHR(this._Store.GetCount(out num));
                return num;
            }
        }

        public PropertyStoreProperty this[int index]
        {
            get
            {
                PropVariant variant;
                PropertyKey key = this.Get(index);
                Marshal.ThrowExceptionForHR(this._Store.GetValue(ref key, out variant));
                return new PropertyStoreProperty(key, variant);
            }
        }

        public PropertyStoreProperty this[Guid guid]
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    PropertyKey key = this.Get(i);
                    if (key.fmtid == guid)
                    {
                        PropVariant variant;
                        Marshal.ThrowExceptionForHR(this._Store.GetValue(ref key, out variant));
                        return new PropertyStoreProperty(key, variant);
                    }
                }
                return null;
            }
        }
    }
}

