namespace System.Data.SQLite
{
    public class SQLiteFunctionEx : SQLiteFunction
    {
        protected CollationSequence GetCollationSequence()
        {
            return base._base.GetCollationSequence(this, base._context);
        }
    }
}

