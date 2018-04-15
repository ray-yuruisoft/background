using Dapper;
using background.Tools;
using log4net;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DapperWrapper
{
    public abstract class DapperBase<T> where T : class, new()
    {

        public abstract string connectionString { get; }
        public abstract string tableName { get; set; }

        #region private

        private static readonly ILog log = LogManager.GetLogger(typeof(DapperBase<T>));
        private static MySqlConnection Context;
        private string _tableName { get { return tableName?.ToLower(); } }
        private string autoTableName(string tableName)
        {
            if (String.IsNullOrWhiteSpace(tableName))
            {
                var classlike = likeTableName("Class", "Data");
                var Tlike = likeTableName("", "Model");
                if (classlike != null || Tlike != null)
                {
                    if (classlike == Tlike)
                    {
                        tableName = classlike;
                    }
                    else
                    {
                        if (classlike != null && Tlike != null)
                        {
                            return null;
                            throw new Exception("can not confirm table name.");
                        }

                        if (classlike != null)
                        {
                            tableName = classlike;
                        }
                        else
                        {
                            tableName = Tlike;
                        }
                    }
                }
            }
            return tableName;

        }
        private string likeTableName(string type, string suffix)
        {
            string original = null;
            if (type == "Class")
            {
                original = this.GetType().Name.Replace("`1", "");
            }
            else
            {
                original = typeof(T).Name;
            }
            string lower = original.ToLower();
            int pos = lower.LastIndexOf(suffix.ToLower());
            if (pos == -1 || pos == 0) { return null; }
            return original.Substring(0, pos);
        }

        #endregion

        public DapperBase()
        {
            tableName = autoTableName(tableName);
        }

        #region 数据库连接创建

        #region 线程非安全，手动释放，建立长连接

        public void Init()
        {
            string str = ConfigHelper.GetAppSettings(connectionString);
            Context = new MySql.Data.MySqlClient.MySqlConnection(str);
        }
        public static MySqlConnection getContext()
        {
            return Context;
        }
        /// <summary>
        /// 使用dapper不需要考虑conn是否连接，在执行dapper时自行判断open状态，如果没有打开它会自己打开。
        /// </summary>
        public static void Open()
        {
            Context.Open();
        }
        /// <summary>
        /// 释放数据库连接
        /// </summary>
        public static void Dispose()
        {
            Context.Dispose();
            Context = null;
        }

        #endregion

        #region 线程安全，自动释放，建立短连接

        private object obj = new object();
        public MySqlConnection AutoContext()
        {
            lock (obj)
            {
                if (Context == null)
                {
                    Init();
                }
                return Context;
            }
        }

        #endregion

        #endregion

        #region Select

        private string SelectstringBuiler(string sqlStr)
        {
            var sqlstr = sqlStr?.ToLower();
            if (sqlstr == null)
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach (var item in typeof(T).GetProperties())
                {
                    stringBuilder.Append(item.Name + ",");
                }
                sqlstr = string.Format("select {0} from {1}", stringBuilder.Remove(stringBuilder.Length - 1, 1).ToString(), tableName);
            }
            else
            {
                if (sqlstr.IndexOf(_tableName) == -1)
                {
                    if (sqlstr.IndexOf("select") == -1)
                    {
                        sqlstr = string.Format("select {0} from {1}", sqlstr, tableName);
                    }
                    else
                    {
                        sqlstr = string.Format("{0} from {1}", sqlstr, tableName);
                    }
                }
                else
                {
                    sqlstr = sqlStr;
                }
            }
            return sqlstr;
        }
        public IEnumerable<T> SelectList(string sqlstr = null, params object[] parameters)
        {
            using (var context = AutoContext())
            {
                return SelectListPersistent(context, sqlstr, parameters);
            }
        }
        public T SelectSingle(string sqlstr = null)
        {
            using (var context = AutoContext())
            {
                return SelectSinglePersistent(context, sqlstr);
            }
        }

        public T SelectSinglePersistent(MySqlConnection context, string sqlstr = null)
        {
            return context.QueryFirstOrDefault<T>(SelectstringBuiler(sqlstr));
        }
        public IEnumerable<T> SelectListPersistent(MySqlConnection context, string sqlstr = null, params object[] parameters)
        {
            return context.Query<T>(SelectstringBuiler(sqlstr), parameters);
        }

        #endregion

        #region Insert

        public string InsertstringBuiler(Type type)
        {

            var dic = ReflectionHelper.GetProperties(type);
            var fields = "";
            var fieldsPar = "";
            foreach (var item in dic)
            {
                fields += item.Key + ",";
                fieldsPar += "@" + item.Key + ",";
            }
            fields = fields.Remove(fields.Length - 1);
            fieldsPar = fieldsPar.Remove(fieldsPar.Length - 1);
            return string.Format("insert into {0}({1}) values({2})", tableName, fields, fieldsPar);

        }
        public int Insert(T t)
        {
            using (var context = AutoContext())
            {
                return InsertPersistent(context, t);
            }
        }
        public int Insert(IEnumerable<T> list)
        {
            using (var context = AutoContext())
            {
                return InsertPersistent(context, list);
            }
        }

        public int InsertPersistent(MySqlConnection context, T t)
        {
            return context.Execute(InsertstringBuiler(typeof(T)), t);
        }
        public int InsertPersistent(MySqlConnection context, IEnumerable<T> list)
        {
            return context.Execute(InsertstringBuiler(typeof(T)), list);
        }

        #endregion

        #region Update

        public string UpdatestringBuilder(Type type)
        {
            var dic = ReflectionHelper.GetProperties(type);
            var fields = "";
            var whereString = " where ";
            foreach (var item in dic)
            {
                var customAttributes = item.Value.CustomAttributes;
                if (customAttributes.FirstOrDefault(c => c.AttributeType.Name == "PrimaryKey") == null)
                {
                    fields += string.Format(" {0} = @{0},", item.Key);
                }
                if (customAttributes.FirstOrDefault(c => c.AttributeType.Name == "Condition") != null)
                {
                    whereString += string.Format(" {0}=@{0} and", item.Key);
                }
            }
            fields = fields.Remove(fields.Length - 1);
            whereString = whereString.Remove(whereString.Length - 3);
            return string.Format("UPDATE {0} SET {1} {2}", tableName, fields, whereString);
        }
        public bool UpdatePersistent(MySqlConnection context, T t)
        {

            return context.Execute(UpdatestringBuilder(typeof(T)), t) > 0;

        }
        public bool Update(T t)
        {
            using (var context = AutoContext())
            {
                return UpdatePersistent(context, t);
            }
        }

        #endregion

        #region AutoSumUpdateOrInsert

        public string UpdateSumstringBuiler(Type type)
        {
            var dic = ReflectionHelper.GetProperties(type);
            var setString = "";
            var whereString = " where ";
            foreach (var item in dic)
            {//todo,需扩展下 string 和时间的
                var customAttributes = item.Value.CustomAttributes;
                if (customAttributes.FirstOrDefault(c => c.AttributeType.Name == "Cumulative") != null)
                {
                    setString += string.Format(" {0}={0}+@{0},", item.Key);
                }
                if (customAttributes.FirstOrDefault(c => c.AttributeType.Name == "Condition") != null)
                {
                    whereString += string.Format(" {0}=@{0} and", item.Key);
                }
            }

            setString = setString.Remove(setString.Length - 1);
            whereString = whereString.Remove(whereString.Length - 3);
            return string.Format("UPDATE {0} SET {1} {2}", tableName, setString, whereString);
        }
        public int AutoSumUpdateOrInsertPersistent(MySqlConnection context, T t)
        {

            var temp = UpdateSumstringBuiler(typeof(T));

            var updateReturn = context.Execute(temp, t);
            if (updateReturn == 0)
            {
                return InsertPersistent(context, t);
            }
            return updateReturn;
        }
        public int AutoSumUpdateOrInsert(T t)
        {
            using (var context = AutoContext())
            {
                return AutoSumUpdateOrInsertPersistent(context, t);
            }
        }

        #endregion


    }


}
