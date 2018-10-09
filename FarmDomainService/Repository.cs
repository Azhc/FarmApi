using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using NHibernate.Linq;
using NHibernate;

//using System.ServiceModel;
//using System.ServiceModel.Web;
//using System.ServiceModel.Activation;
//using WcfNHibernate;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace FarmDomain.Repositories
{
    /// <summary>
    /// 仓储类
    /// </summary>
    /// <typeparam name="T">NHibernate实体类</typeparam>
    public class Repository<T> : IRepository<T>
    {
        #region 实现IRepository接口
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="item">T类型的实例</param>
        public void Add(T item)
        {
            ISession session = SessionHelper.GetSession();
            session.Save(item);
            session.Flush();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="item">T类型的实例</param>
        public void Delete(T item)
        {
            ISession session = SessionHelper.GetSession();
            session.Delete(item);
            session.Flush();
        }

        /// <summary>
        /// 更改
        /// </summary>
        /// <param name="item">T类型的实例</param>
        public void Update(T item)
        {
            ISession session = SessionHelper.GetSession();
            session.Update(item);
            session.Flush();
        }

        /// <summary>
        /// 添加或者删除
        /// </summary>
        /// <param name="item">T类型的实例</param>
        public void AddOrUpdate(T item)
        {
            ISession session = SessionHelper.GetSession();
            session.SaveOrUpdate(item);
            session.Flush();
        }
        #endregion

        #region 实现IEnumerable接口
        public IEnumerator<T> GetEnumerator()
        {
            return SessionHelper.GetSession().Query<T>().AsEnumerable().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        #region 实现IQueryable接口
        public Type ElementType
        {
            get { return SessionHelper.GetSession().Query<T>().ElementType; }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get { return SessionHelper.GetSession().Query<T>().Expression; }
        }

        public IQueryProvider Provider
        {
            get { return SessionHelper.GetSession().Query<T>().Provider; }
        }
        #endregion

        /// <summary>
        /// 执行SQL语句，返回Object集合
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>object集合</returns>
        public IList<object> ExcuteSql(string sql)
        {
            ISession session = SessionHelper.GetSession();
            return session.CreateSQLQuery(sql).List<object>();
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="cmdText">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public DataTable ExecuteQuery(string cmdText, params SqlParameter[] parameters)
        {
            string connStr = ConfigurationManager.ConnectionStrings["sql"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, conn, null, CommandType.Text, cmdText, parameters);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                        return ds.Tables[0];
                    }
                }
            }
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="cmdText">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public DataTable ExecuteQuery(CommandType type, string cmdText, params SqlParameter[] parameters)
        {
            string connStr = ConfigurationManager.ConnectionStrings["sql"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, conn, null, type, cmdText, parameters);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                        return ds.Tables[0];
                    }
                }
            }
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="cmdText">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public int ExecuteNoneQuery(string cmdText, params SqlParameter[] parameters)
        {
            int result = 0;
            string connStr = ConfigurationManager.ConnectionStrings["sql"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, conn, null, CommandType.Text, cmdText, parameters);
                    result = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlList"></param>
        /// <param name="parasList"></param>
        public bool ExecuteNonQueryByTrans(IList<string> sqlList, IList<SqlParameter[]> parasList)
        {
            bool result = true;
            if (sqlList == null || sqlList.Count == 0) return result;
            string connStr = ConfigurationManager.ConnectionStrings["sql"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                SqlTransaction tran = conn.BeginTransaction();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.Transaction = tran;
                    int index = 0;
                    try
                    {
                        foreach (string sql in sqlList)
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sql;
                            if (parasList != null && parasList.Count > index && parasList[index] != null)
                            {
                                foreach (SqlParameter parm in parasList[index])
                                {
                                    cmd.Parameters.Add(parm);
                                }
                            }
                            cmd.ExecuteNonQuery();
                            index++;
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        result = false;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 准备执行一个命令
        /// </summary>
        /// <param name="cmd">sql命令</param>
        /// <param name="conn">Sql连接</param>
        /// <param name="trans">Sql事务</param>
        /// <param name="cmdType">命令类型例如 存储过程或者文本</param>
        /// <param name="cmdText">命令文本,例如：Select * from Products</param>
        /// <param name="cmdParms">执行命令的参数</param>
        private void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans,
            CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }

        /// <summary>
        /// 返回当查询条件为IN时的字符串
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public string GetInParameterSql(IList<string> arr)
        {
            StringBuilder builder = new StringBuilder();
            if (arr.Count == 0)
            {
                builder.Append("''");
            }
            else
            {
                builder.Append("'");
                foreach (string str in arr)
                {
                    builder.Append(str).Append("','");
                }
                builder.Remove(builder.Length - 2, 2);
            }
            return builder.ToString();
        }
    }
}
