namespace BD.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Globalization;
    using System.Reflection;
    using System.Data.SQLite;
    /// <summary>
    /// 列表变量类型
    /// </summary>
    public enum ColType
    {
        /// <summary>
        /// 文字
        /// </summary>
        Text,
        /// <summary>
        /// 日期
        /// </summary>
        DateTime,
        /// <summary>
        /// 整形
        /// </summary>
        Integer,
        /// <summary>
        /// 
        /// </summary>
        Decimal,
        /// <summary>
        /// 二进制大对象
        /// </summary>
        BLOB
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ColConstrains
    {
        /// <summary>
        /// 主键
        /// </summary>
        PrimaryKey,
        /// <summary>
        /// 自动增长
        /// </summary>
        AutoIncrement,
        /// <summary>
        /// 自动为非空
        /// </summary>
        NotNull,
        /// <summary>
        /// 默认值
        /// </summary>
        DefaultValue,
        /// <summary>
        /// 主键增长类型
        /// </summary>
        PrimaryKeyIncrement
    }
    /// <summary>
    /// 
    /// </summary>
    public class SQLiteHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbPath"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string BuildConnectString(string dbPath,string password="")
        {
            string connectionStr = string.Format("data source={0}",dbPath);
            if(password!="")
                connectionStr= string.Format("Data Source={0}; Password={1}", dbPath,password);
            return connectionStr;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbPath"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public SQLiteConnection BuildConnection(string dbPath, string password="")
        {
            if (!System.IO.File.Exists(dbPath)) System.IO.File.Create(dbPath);
            var connectionstr = BuildConnectString(dbPath,password);
            return new SQLiteConnection(connectionstr);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SQLiteConnection BuildConnection()
        {
            string path = Environment.CurrentDirectory+"/tmp.db";
            var connectionstr = BuildConnectString(path);
            return new SQLiteConnection(connectionstr);
        }

        SQLiteCommand cmd = null;
        /// <summary>
        /// 静态执行数据库操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbPath"></param>
        /// <param name="action"></param>
        public static void SafeExecute(string dbPath,Action<SQLiteHelper> action)
        {
            string connectStr = string.Format(@"data source={0}",dbPath);
            using (SQLiteConnection conn = new SQLiteConnection(connectStr))
            {               
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    SQLiteHelper sh = new SQLiteHelper(cmd);
                   // var transaction = conn.BeginTransaction();
                    action?.Invoke(sh);
                   // transaction.Commit();
                    conn.Close();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        public SQLiteHelper(SQLiteCommand command)
        {
            cmd = command;
        }

        #region DB Info
        /// <summary>
        /// 获取表的状态
        /// </summary>
        /// <returns></returns>
        public DataTable GetTableStatus()
        {
            return Select("SELECT * FROM sqlite_master;");
        }
        /// <summary>
        /// 获取表的List
        /// </summary>
        /// <returns></returns>
        public DataTable GetTableList()
        {
            DataTable dt = GetTableStatus();
            DataTable dt2 = new DataTable();
            dt2.Columns.Add("Tables");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string t = dt.Rows[i]["name"] + "";
                if (t != "sqlite_sequence")
                    dt2.Rows.Add(t);
            }
            return dt2;
        }
        /// <summary>
        /// 获取列的信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable GetColumnStatus(string tableName)
        {
            return Select(string.Format("PRAGMA table_info(`{0}`);", tableName));
        }
        /// <summary>
        /// 显示数据库
        /// </summary>
        /// <returns></returns>
        public DataTable ShowDatabase()
        {
            return Select("PRAGMA database_list;");
        }

        #endregion

        #region Query
        /// <summary>
        /// 开始业务处理
        /// </summary>
        public void BeginTransaction()
        {
            cmd.CommandText = "begin transaction;";
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// 提交业务处理
        /// </summary>
        public void Commit()
        {
            cmd.CommandText = "commit;";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 滚回
        /// </summary>
        public void Rollback()
        {
            cmd.CommandText = "rollback";
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// 执行sql查询语句
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable Select(string sql)
        {
            return Select(sql, new List<SQLiteParameter>());
        }
        /// <summary>
        /// 执行带参数的查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dicParameters"></param>
        /// <returns></returns>
        public DataTable Select(string sql, Dictionary<string, object> dicParameters = null)
        {
            List<SQLiteParameter> lst = GetParametersList(dicParameters);
            return Select(sql, lst);
        }
        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <param name="sql">update Table1 set photo = @photo where ID = '0'</param>
        /// <param name="parameters">["@photo","value"]</param>
        /// <returns></returns>
        public DataTable Select(string sql, IEnumerable<SQLiteParameter> parameters = null)
        {
            cmd.CommandText = sql;
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
            }
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql"></param>
        public void Execute(string sql)
        {
            Execute(sql, new List<SQLiteParameter>());
        }
        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dicParameters"></param>
        public void Execute(string sql, Dictionary<string, object> dicParameters = null)
        {
            List<SQLiteParameter> lst = GetParametersList(dicParameters);
            Execute(sql, lst);
        }
        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public void Execute(string sql, IEnumerable<SQLiteParameter> parameters = null)
        {
            cmd.CommandText = sql;
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
            }
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// 执行Scalar
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql)
        {
            cmd.CommandText = sql;
            return cmd.ExecuteScalar();
        }
        /// <summary>
        /// 执行Scalar
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dicParameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, Dictionary<string, object> dicParameters = null)
        {
            List<SQLiteParameter> lst = GetParametersList(dicParameters);
            return ExecuteScalar(sql, lst);
        }
        /// <summary>
        /// 执行Scalar
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, IEnumerable<SQLiteParameter> parameters = null)
        {
            cmd.CommandText = sql;
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
            }
            return cmd.ExecuteScalar();
        }
        /// <summary>
        /// 执行Scalar
        /// </summary>
        /// <typeparam name="dataType"></typeparam>
        /// <param name="sql"></param>
        /// <param name="dicParameters"></param>
        /// <returns></returns>
        public dataType ExecuteScalar<dataType>(string sql, Dictionary<string, object> dicParameters = null)
        {
            List<SQLiteParameter> lst = null;
            if (dicParameters != null)
            {
                lst = new List<SQLiteParameter>();
                foreach (KeyValuePair<string, object> kv in dicParameters)
                {
                    lst.Add(new SQLiteParameter(kv.Key, kv.Value));
                }
            }
            return ExecuteScalar<dataType>(sql, lst);
        }
        /// <summary>
        /// 执行Scalar
        /// </summary>
        /// <typeparam name="dataType"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public dataType ExecuteScalar<dataType>(string sql, IEnumerable<SQLiteParameter> parameters = null)
        {
            cmd.CommandText = sql;
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
            }
            return (dataType)Convert.ChangeType(cmd.ExecuteScalar(), typeof(dataType));
        }
        /// <summary>
        /// 执行Scalar
        /// </summary>
        /// <typeparam name="dataType"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public dataType ExecuteScalar<dataType>(string sql)
        {
            cmd.CommandText = sql;
            return (dataType)Convert.ChangeType(cmd.ExecuteScalar(), typeof(dataType));
        }

        private List<SQLiteParameter> GetParametersList(Dictionary<string, object> dicParameters)
        {
            List<SQLiteParameter> lst = new List<SQLiteParameter>();
            if (dicParameters != null)
            {
                foreach (KeyValuePair<string, object> kv in dicParameters)
                {
                    lst.Add(new SQLiteParameter(kv.Key, kv.Value));
                }
            }
            return lst;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Escape(string data)
        {
            data = data.Replace("'", "''");
            data = data.Replace("\\", "\\\\");
            return data;
        }
        /// <summary>
        /// 数据库插入
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dic"></param>
        public void Insert(string tableName, Dictionary<string, object> dic)
        {
            StringBuilder sbCol = new System.Text.StringBuilder();
            StringBuilder sbVal = new System.Text.StringBuilder();

            foreach (KeyValuePair<string, object> kv in dic)
            {
                if (sbCol.Length == 0)
                {
                    sbCol.Append("insert into ");
                    sbCol.Append(tableName);
                    sbCol.Append("(");
                }
                else
                {
                    sbCol.Append(",");
                }

                sbCol.Append("`");
                sbCol.Append(kv.Key);
                sbCol.Append("`");

                if (sbVal.Length == 0)
                {
                    sbVal.Append(" values(");
                }
                else
                {
                    sbVal.Append(", ");
                }

                sbVal.Append("@v");
                sbVal.Append(kv.Key);
            }

            sbCol.Append(") ");
            sbVal.Append(");");

            cmd.CommandText = sbCol.ToString() + sbVal.ToString();

            foreach (KeyValuePair<string, object> kv in dic)
            {
                cmd.Parameters.AddWithValue("@v" + kv.Key, kv.Value);
            }

            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// 数据库更新
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dicData"></param>
        /// <param name="colCond"></param>
        /// <param name="varCond"></param>
        public void Update(string tableName, Dictionary<string, object> dicData, string colCond, object varCond)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic[colCond] = varCond;
            Update(tableName, dicData, dic);
        }
        /// <summary>
        /// 数据库更新
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dicData">数据字典</param>
        /// <param name="dicCond">条件字典</param>
        public void Update(string tableName, Dictionary<string, object> dicData, Dictionary<string, object> dicCond)
        {
            if (dicData.Count == 0)
                throw new Exception("dicData is empty.");

            StringBuilder sbData = new System.Text.StringBuilder();

            Dictionary<string, object> _dicTypeSource = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> kv1 in dicData)
            {
                _dicTypeSource[kv1.Key] = null;
            }

            foreach (KeyValuePair<string, object> kv2 in dicCond)
            {
                if (!_dicTypeSource.ContainsKey(kv2.Key))
                    _dicTypeSource[kv2.Key] = null;
            }
            sbData.Append("update `");
            sbData.Append(tableName);
            sbData.Append("` set ");
            bool firstRecord = true;
            foreach (KeyValuePair<string, object> kv in dicData)
            {
                if (firstRecord)
                    firstRecord = false;
                else
                    sbData.Append(",");
                sbData.Append("`");
                sbData.Append(kv.Key);
                sbData.Append("` = ");
                sbData.Append("@v");
                sbData.Append(kv.Key);
            }
            sbData.Append(" where ");
            firstRecord = true;
            foreach (KeyValuePair<string, object> kv in dicCond)
            {
                if (firstRecord)
                    firstRecord = false;
                else
                {
                    sbData.Append(" and ");
                }
                sbData.Append("`");
                sbData.Append(kv.Key);
                sbData.Append("` = ");

                sbData.Append("@c");
                sbData.Append(kv.Key);
            }

            sbData.Append(";");
            cmd.CommandText = sbData.ToString();
            foreach (KeyValuePair<string, object> kv in dicData)
            {
                cmd.Parameters.AddWithValue("@v" + kv.Key, kv.Value);
            }
            foreach (KeyValuePair<string, object> kv in dicCond)
            {
                cmd.Parameters.AddWithValue("@c" + kv.Key, kv.Value);
            }
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// 最后插入的rowid
        /// </summary>
        /// <returns></returns>
        public long LastInsertRowId()
        {
            return ExecuteScalar<long>("select last_insert_rowid();");
        }

        #endregion

        #region Utilities
       
        /// <summary>
        /// 创建数据表
        /// </summary>
        /// <param name="table"></param>
        public void CreateTable(SQLiteTable table)
        {
            StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("create table if not exists `");
            sb.Append(table.TableName);
            sb.AppendLine("`(");
            bool firstRecord = true;
            foreach (SQLiteColumn col in table.Columns)
            {
                if (col.ColumnName.Trim().Length == 0)
                {
                    throw new Exception("Column name cannot be blank.");
                }

                if (firstRecord)
                    firstRecord = false;
                else
                    sb.AppendLine(",");

                sb.Append(col.ColumnName);
                sb.Append(" ");

                if (col.AutoIncrement)
                {

                    sb.Append("integer primary key autoincrement");
                    continue;
                }

                switch (col.ColDataType)
                {
                    case ColType.Text:
                        sb.Append("text"); break;
                    case ColType.Integer:
                        sb.Append("integer"); break;
                    case ColType.Decimal:
                        sb.Append("decimal"); break;
                    case ColType.DateTime:
                        sb.Append("datetime"); break;
                    case ColType.BLOB:
                        sb.Append("blob"); break;
                }

                if (col.PrimaryKey)
                    sb.Append(" primary key");
                else if (col.NotNull)
                    sb.Append(" not null");
                else if (col.DefaultValue.Length > 0)
                {
                    sb.Append(" default ");

                    if (col.DefaultValue.Contains(" ") || col.ColDataType == ColType.Text || col.ColDataType == ColType.DateTime)
                    {
                        sb.Append("'");
                        sb.Append(col.DefaultValue);
                        sb.Append("'");
                    }
                    else
                    {
                        sb.Append(col.DefaultValue);
                    }
                }
            }

            sb.AppendLine(");");

            cmd.CommandText = sb.ToString();
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// 重命名数据表
        /// </summary>
        /// <param name="tableFrom"></param>
        /// <param name="tableTo"></param>
        public void RenameTable(string tableFrom, string tableTo)
        {
            cmd.CommandText = string.Format("alter table `{0}` rename to `{1}`;", tableFrom, tableTo);
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// 复制数据表
        /// </summary>
        /// <param name="tableFrom"></param>
        /// <param name="tableTo"></param>
        public void CopyAllData(string tableFrom, string tableTo)
        {
            DataTable dt1 = Select(string.Format("select * from `{0}` where 1 = 2;", tableFrom));
            DataTable dt2 = Select(string.Format("select * from `{0}` where 1 = 2;", tableTo));

            Dictionary<string, bool> dic = new Dictionary<string, bool>();

            foreach (DataColumn dc in dt1.Columns)
            {
                if (dt2.Columns.Contains(dc.ColumnName))
                {
                    if (!dic.ContainsKey(dc.ColumnName))
                    {
                        dic[dc.ColumnName] = true;
                    }
                }
            }

            foreach (DataColumn dc in dt2.Columns)
            {
                if (dt1.Columns.Contains(dc.ColumnName))
                {
                    if (!dic.ContainsKey(dc.ColumnName))
                    {
                        dic[dc.ColumnName] = true;
                    }
                }
            }

            StringBuilder sb = new System.Text.StringBuilder();

            foreach (KeyValuePair<string, bool> kv in dic)
            {
                if (sb.Length > 0)
                    sb.Append(",");

                sb.Append("`");
                sb.Append(kv.Key);
                sb.Append("`");
            }

            StringBuilder sb2 = new System.Text.StringBuilder();
            sb2.Append("insert into `");
            sb2.Append(tableTo);
            sb2.Append("`(");
            sb2.Append(sb.ToString());
            sb2.Append(") select ");
            sb2.Append(sb.ToString());
            sb2.Append(" from `");
            sb2.Append(tableFrom);
            sb2.Append("`;");

            cmd.CommandText = sb2.ToString();
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// 抛弃数据表
        /// </summary>
        /// <param name="table"></param>
        public void DropTable(string table)
        {
            cmd.CommandText = string.Format("drop table if exists `{0}`", table);
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetTable"></param>
        /// <param name="newStructure"></param>
        public void UpdateTableStructure(string targetTable, SQLiteTable newStructure)
        {
            newStructure.TableName = targetTable + "_temp";

            CreateTable(newStructure);

            CopyAllData(targetTable, newStructure.TableName);

            DropTable(targetTable);

            RenameTable(newStructure.TableName, targetTable);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="alias"></param>
        public void AttachDatabase(string database, string alias)
        {
            Execute(string.Format("attach '{0}' as {1};", database, alias));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alias"></param>
        public void DetachDatabase(string alias)
        {
            Execute(string.Format("detach {0};", alias));
        }

        #endregion

        #region 模型EF

        /// <summary>
        /// 根据Model的类型来创建Table
        /// </summary>
        /// <param name="model"></param>
        public void CreateTable<TObject>(TObject model)
        {
            var modelType = model.GetType();
            var tablename = modelType.Name;
            DropTable("_Table_" + tablename);//如果有旧的数据表则抛弃
            ModelMetadata metadata = new ModelMetadata(modelType);
            StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("create table if not exists `");
            sb.Append("_Table_" + tablename);
            sb.AppendLine("`(");
            var properys = modelType.GetProperties();
            bool firstRecord = true;
            foreach (var item in properys)
            {
                var Name = item.Name;
                var valueType = item.PropertyType.Name;
                if (firstRecord)
                    firstRecord = false;
                else
                    sb.AppendLine(",");
                sb.Append(Name);
                sb.Append(" ");
                if (metadata.GetPropertyAttrValue(Name) == "primarykeyincrement")//如果找到主键
                {
                    sb.Append("integer primary key autoincrement");
                    continue;
                }
                switch (valueType.ToLower())
                {
                    case "string":
                        sb.Append("text"); break;
                    case "int32":
                        sb.Append("integer"); break;
                    case "decimal":
                        sb.Append("decimal"); break;
                    case "datetime":
                        sb.Append("datetime"); break;
                    case "blob":
                        sb.Append("blob"); break;
                }
                if (metadata.GetPropertyAttrValue(Name) == "primarykey")
                    sb.Append(" primary key");
                else if (metadata.GetPropertyAttrValue(Name) == "notnull")
                    sb.Append(" not null");
            }
            sb.AppendLine(");");
            cmd.CommandText = sb.ToString();
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 根据条件查询结果
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="model">实体</param>
        /// <param name="dicParameters">条件字典</param>
        /// <param name="conditonal">条件符号</param>
        /// <returns></returns>
        public List<TObject> Select<TObject>(TObject model, Dictionary<string, object> dicParameters = null, string conditonal = "and")
        {
            string conditionStr = CombineParameters(dicParameters, conditonal);
            return Select(model, conditionStr);
        }
        /// <summary>
        /// 根据条件conditional组合参数
        /// </summary>
        /// <param name="dicParameters"></param>
        /// <param name="conditional">条件</param>
        /// <returns></returns>
        private string CombineParameters(Dictionary<string, object> dicParameters, string conditional)
        {
            if (dicParameters.Count < 1) return "";
            string sql = " ";
            foreach (KeyValuePair<string, object> item in dicParameters)
            {
                sql += item.Key + "=" + item.Value + " " + conditional + " ";
            }
            string last = sql.Substring(0, sql.Length - 2 - conditional.Length);
            return sql;
        }
        /// <summary>
        /// 返回对象的数据List泛型
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conditonal">查询条件"ID=10"</param>
        public List<TObject> Select<TObject>(TObject model, string conditonal = "")
        {
            var type = model.GetType();
            var sql = "";
            if (conditonal != "")
                sql = string.Format("select * from `_Table_{0}` where {1};", type.Name, conditonal);
            else
                sql = string.Format("select * from `_Table_{0}`;", type.Name);
            var datatable = Select(sql, new List<SQLiteParameter>());
            var modelDic = GetModelParametersDic(model);
            List<TObject> modelDataCollection = new List<TObject>();
            foreach (DataRow dr2 in datatable.Rows)
            {
                string json = "{";
                foreach (var item in modelDic.Keys)
                {
                    json += "\"" + item + "\":\"" + dr2[item] + "\",";
                }
                var last = json.Substring(0, json.Length - 1);
                last += "}";
                //var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<TObject>(last);
                //modelDataCollection.Add(obj);
            }
            return modelDataCollection;
        }
        /// <summary>
        /// 根据Model 插入数据库
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="model"></param>
        public void Insert<TObject>(TObject model)
        {
            var type = model.GetType();
            var tablename = "_Table_" + type.Name;
            var modelDic = GetModelParametersDic(model);
            Insert(tablename, modelDic);
        }
        /// <summary>
        /// 插入列表Model
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="modellist"></param>
        public void Insert<TObject>(List<TObject> modellist)
        {
            BeginTransaction();
            modellist.ForEach(model => {
                Insert(model);
            });
            Commit();
        }
        /// <summary>
        /// 根据Model类型和条件更新数据库
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="model">输入的Model</param>
        /// <param name="dicCond">条件字典</param>
        public void Update<TObject>(TObject model, Dictionary<string, object> dicCond)
        {
            var type = model.GetType();
            var tablename = "_Table_" + type.Name;
            var modelDic = GetModelParametersDicIgnoreNull(model);
            Update(tablename, modelDic, dicCond);
        }
        /// <summary>
        /// 根据字典集合删除数据库
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="model"></param>
        /// <param name="dicCond">条件字典</param>
        public bool Delete<TObject>(TObject model, Dictionary<string, object> dicCond)
        {
            var type = model.GetType();
            var tablename = "_Table_" + type.Name;
            var modelDic = GetModelParametersDic(model);

            List<string> typeList = new List<string>(modelDic.Keys);
            string cond = "";
            foreach (KeyValuePair<string, object> item in dicCond)
            {
                if (typeList.Contains(item.Key))
                {
                    cond += item.Key + "=" + item.Value + " and ";
                }
            }
            if (cond.Length < 6) return false;
            var last = cond.Substring(0, cond.Length - 5);
            string sql = string.Format("delete from {0} where {1};", tablename, last);
            cmd.CommandText = sql;
            return cmd.ExecuteNonQuery() > 0 ? true : false;
        }
        /// <summary>
        /// 获取参数字典[key,value]
        /// </summary>
        /// <param name="model"></param>
        public Dictionary<string, object> GetModelParametersDic(object model)
        {
            var type = model.GetType();
            var properys = type.GetProperties();
            Dictionary<string, object> dic = new Dictionary<string, object>();

            foreach (var item in properys)
            {
                var name = item.Name;
                var value = item.GetValue(model, null);
                dic.Add(name, value);
            }
            return dic;
        }

        /// <summary>
        /// 获取参数字典,忽略值为空的属性[key,value]
        /// </summary>
        /// <param name="model"></param>
        public Dictionary<string, object> GetModelParametersDicIgnoreNull(object model)
        {
            var type = model.GetType();
            var properys = type.GetProperties();
            Dictionary<string, object> dic = new Dictionary<string, object>();

            foreach (var item in properys)
            {
                var name = item.Name;
                var value = item.GetValue(model, null);
                if (null != value)
                    dic.Add(name, value);
            }
            return dic;
        }
        #endregion
    }
}