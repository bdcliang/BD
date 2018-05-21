namespace CoreAudioApi
{
    using System;

    public class PropertyStoreProperty
    {
        private PropertyKey _PropertyKey;
        private PropVariant _PropValue;

        internal PropertyStoreProperty(PropertyKey key, PropVariant value)
        {
            this._PropertyKey = key;
            this._PropValue = value;
        }

        public PropertyKey Key
        {
            get
            {
                return this._PropertyKey;
            }
        }

        public object Value
        {
            get
            {
                return this._PropValue.Value;
            }
        }
    }
}

