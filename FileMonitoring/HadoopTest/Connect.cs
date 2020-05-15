
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace MyFile
{
    class Connect
    {
        private static SqlConnection db;
        private const string connUrl = "server=192.168.130.121;database=HOMMES;uid=sa;pwd=895859@hxdz";

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        /// <returns></returns>
        public static SqlConnection getConn() {
            if (db == null) {
                db = new SqlConnection(connUrl);
            }
            return db;
        }

        /// <summary>
        /// 执行数据库非查询语句
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int ExecuteNoQuery(SqlConnection conn,string sql) {
            conn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            int iud = 0;
            iud = cmd.ExecuteNonQuery();
            conn.Close();
            return iud;
        }

        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataTable ExecuteQuey(SqlConnection conn, string sql) {
            conn.Open();
            SqlDataAdapter msda;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            DataTable dt = new DataTable();
            msda = new SqlDataAdapter(cmd);
            msda.Fill(dt);
            conn.Close();
            return dt;
        }

    }
}