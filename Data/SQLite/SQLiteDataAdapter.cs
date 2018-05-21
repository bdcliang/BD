namespace System.Data.SQLite
{
    using System;
    using System.ComponentModel;
    using System.Data.Common;

    [DefaultEvent("RowUpdated"), Designer("Microsoft.VSDesigner.Data.VS.SqlDataAdapterDesigner, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ToolboxItem("SQLite.Designer.SQLiteDataAdapterToolboxItem, SQLite.Designer, Version=1.0.36.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139")]
    public sealed class SQLiteDataAdapter : DbDataAdapter
    {
        private static object _updatedEventPH = new object();
        private static object _updatingEventPH = new object();

        public event EventHandler<RowUpdatedEventArgs> RowUpdated
        {
            add
            {
                base.Events.AddHandler(_updatedEventPH, value);
            }
            remove
            {
                base.Events.RemoveHandler(_updatedEventPH, value);
            }
        }

        public event EventHandler<RowUpdatingEventArgs> RowUpdating
        {
            add
            {
                EventHandler<RowUpdatingEventArgs> mcd = (EventHandler<RowUpdatingEventArgs>) base.Events[_updatingEventPH];
                if ((mcd != null) && (value.Target is DbCommandBuilder))
                {
                    EventHandler<RowUpdatingEventArgs> handler2 = (EventHandler<RowUpdatingEventArgs>) FindBuilder(mcd);
                    if (handler2 != null)
                    {
                        base.Events.RemoveHandler(_updatingEventPH, handler2);
                    }
                }
                base.Events.AddHandler(_updatingEventPH, value);
            }
            remove
            {
                base.Events.RemoveHandler(_updatingEventPH, value);
            }
        }

        public SQLiteDataAdapter()
        {
        }

        public SQLiteDataAdapter(SQLiteCommand cmd)
        {
            this.SelectCommand = cmd;
        }

        public SQLiteDataAdapter(string commandText, SQLiteConnection connection)
        {
            this.SelectCommand = new SQLiteCommand(commandText, connection);
        }

        public SQLiteDataAdapter(string commandText, string connectionString)
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            this.SelectCommand = new SQLiteCommand(commandText, connection);
        }

        internal static Delegate FindBuilder(MulticastDelegate mcd)
        {
            if (mcd != null)
            {
                Delegate[] invocationList = mcd.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    if (invocationList[i].Target is DbCommandBuilder)
                    {
                        return invocationList[i];
                    }
                }
            }
            return null;
        }

        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            EventHandler<RowUpdatedEventArgs> handler = base.Events[_updatedEventPH] as EventHandler<RowUpdatedEventArgs>;
            if (handler != null)
            {
                handler(this, value);
            }
        }

        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
            EventHandler<RowUpdatingEventArgs> handler = base.Events[_updatingEventPH] as EventHandler<RowUpdatingEventArgs>;
            if (handler != null)
            {
                handler(this, value);
            }
        }

        [DefaultValue((string) null), Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public SQLiteCommand DeleteCommand
        {
            get
            {
                return (SQLiteCommand) base.DeleteCommand;
            }
            set
            {
                base.DeleteCommand = value;
            }
        }

        [Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue((string) null)]
        public SQLiteCommand InsertCommand
        {
            get
            {
                return (SQLiteCommand) base.InsertCommand;
            }
            set
            {
                base.InsertCommand = value;
            }
        }

        [Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue((string) null)]
        public SQLiteCommand SelectCommand
        {
            get
            {
                return (SQLiteCommand) base.SelectCommand;
            }
            set
            {
                base.SelectCommand = value;
            }
        }

        [Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue((string) null)]
        public SQLiteCommand UpdateCommand
        {
            get
            {
                return (SQLiteCommand) base.UpdateCommand;
            }
            set
            {
                base.UpdateCommand = value;
            }
        }
    }
}

