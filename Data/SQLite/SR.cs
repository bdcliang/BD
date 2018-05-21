namespace System.Data.SQLite
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Resources;

    [DebuggerNonUserCode]
    internal class SR
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceMan;

        internal SR()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        internal static string DataTypes
        {
            get
            {
                return ResourceManager.GetString("DataTypes", resourceCulture);
            }
        }

        internal static string Keywords
        {
            get
            {
                return ResourceManager.GetString("Keywords", resourceCulture);
            }
        }

        internal static string MetaDataCollections
        {
            get
            {
                return ResourceManager.GetString("MetaDataCollections", resourceCulture);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("System.Data.SQLite.SR", typeof(System.Data.SQLite.SR).Assembly);
                    resourceMan = manager;
                }
                return resourceMan;
            }
        }
    }
}

