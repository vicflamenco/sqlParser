using System;
using System.Windows.Input;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;

namespace SqlParserUdb.Models
{
    public class SqlHandler
    {
        #region Commands

        static SqlHandler()
        {

        }
        #endregion

        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataAdapter adapter;
        private List<SqlError> errors;

        public string ConnectionString
        {
            get { return conn.ConnectionString; }
            private set { conn.ConnectionString = value; }
        }

        public bool IsConnected
        {
            get { return conn.State == ConnectionState.Open; }
        }

        public SqlHandler()
        {
            conn = new SqlConnection();
            cmd = new SqlCommand("", conn);
            adapter = new SqlDataAdapter(cmd);
            errors = new List<SqlError>(5);

            ConnectionString = "Data Source=; Initial Catalog=; Integrated Security=SSPI";
            conn.FireInfoMessageEventOnUserErrors = true; //when true, the SqlCommand object will not throw an Exception when errors occur
            conn.InfoMessage += new SqlInfoMessageEventHandler(conn_InfoMessage);
        }

        public void Connect(string connStr)
        {
            if (IsConnected)
                Disconnect();
            conn.ConnectionString = connStr;
            conn.Open();
        }

        public void Disconnect()
        {
            conn.Close();
        }

        public DataTable Execute(string sqlText, out SqlError[] errorsArray)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Can not execute Sql query while the connection is closed!");

            errors.Clear();
            cmd.CommandText = sqlText;
            DataTable tbl = new DataTable();
            adapter.Fill(tbl);
            errorsArray = errors.ToArray();
            return tbl;
        }

        public SqlError[] Parse(string sqlText)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Can not parse Sql query while the connection is closed!");

            errors.Clear();
            cmd.CommandText = "SET PARSEONLY ON";
            cmd.ExecuteNonQuery();

            cmd.CommandText = sqlText;
            cmd.ExecuteNonQuery(); //conn_InfoMessage is invoked for every error, e.g. 2 times for 2 errors
            
            cmd.CommandText = "SET PARSEONLY OFF";
            cmd.ExecuteNonQuery();

            return errors.ToArray();
        }

        private void conn_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            //ensure that all errors are caught
            SqlError[] errorsFound = new SqlError[e.Errors.Count];
            e.Errors.CopyTo(errorsFound, 0);
            errors.AddRange(errorsFound);
        }
    }
}
