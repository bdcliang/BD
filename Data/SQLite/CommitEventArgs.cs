namespace System.Data.SQLite
{
    using System;

    public class CommitEventArgs : EventArgs
    {
        public bool AbortTransaction;

        internal CommitEventArgs()
        {
        }
    }
}

