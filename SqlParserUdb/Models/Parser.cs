using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlParserUdb.Models;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace SqlParserUdb.Models
{
    public class Parser
    {
        public SqlError[] Errors;
        private SqlHandler Handler;
        private List<string> _tokensCreateDatabase = new List<string> { "CREATE", "DATABASE" };
        private List<string> _tokensDropDatabase = new List<string> { "DROP", "DATABASE" };
        private List<string> _tokensDropTable = new List<string> { "DROP", "TABLE" };
        private List<string> _tokensCreateTable = new List<string> { "CREATE", "TABLE", "(", ")" };
        private List<string> _tokensSelect = new List<string> { "SELECT", "FROM", "WHERE", "ORDER", "BY", "=", ">", ">=", "<", "<=", "'", "," };
        private List<string> _tokensInsert = new List<string> { "INSERT", "INTO", "(", ")", "VALUES" , "'", ","};
        private List<string> _tokensUpdate = new List<string> { "UPDATE", "SET", "WHERE", "=", ">", ">=", "<", "<=", "'", "," };
        private List<string> _tokensDelete = new List<string> { "DELETE", "FROM", "WHERE", "=", ">", ">=", "<", "<=", "'", "," };

        private string _identifier = "[_a-zA-Z0-9]{1,50}";

        private string _dataType = "[BIT|MONEY|BIGINT|INT|DATETIME|DECIMAL|NVARCHAR([0-9]+)|VARCHAR([0-9]+)|CHAR([0-9]+)|NUMERIC|SMALLINT|SMALLMONEY|TINYINT|FLOAT|REAL|DATE|DATETIME2|SMALLDATETIME|TIME|CHAR|VARCHAR]";
        public string ConnectionString;
        
        public Parser(string DbName = "")
        
        {
            for (int i = 0; i < 999; i++ )
            {
                _tokensSelect.Add(i.ToString());
                _tokensInsert.Add(i.ToString());
                _tokensUpdate.Add(i.ToString());
                _tokensDelete.Add(i.ToString());
            }
            this.ConnectionString = string.Format("Data Source=VICTOR\\SQLEXPRESS; Initial Catalog={0}; Integrated Security=SSPI", DbName);
            this.Handler = new SqlHandler();
        }

        public bool Connect()
        {
            Handler.Connect(this.ConnectionString);
            return Handler.IsConnected;
        }
        public void Disconnect()
        {
            Handler.Connect(this.ConnectionString);
            
        }

        public DataTable ExecuteQuery(string Sql)
        {
            return Handler.Execute(Sql, out Errors);
        }

        public DataTable GetAllDatabases()
        {
            if (Handler.IsConnected) return Handler.Execute("SELECT name FROM master.dbo.sysdatabases", out Errors);
            return null;
        }

        public DataTable GetTablesFromDatabase(string database)
        {
            if (Handler.IsConnected) return Handler.Execute("SELECT * FROM information_schema.tables", out Errors);
            return null;
        }

        private List<string> GetTokens(string sql)
        {
            return sql.Split(' ').ToList();
        }

        private bool MatchesRegularExpression(string expression, string regularExpression)
        {
            Regex regex = new Regex(regularExpression);
            return regex.Match(expression).Success;
        }

        private bool IsIdentifier(string identifier)
        {
            string regularExpression = @"^[_a-zA-Z0-9][_a-zA-Z0-9]{1,50}$";
            return MatchesRegularExpression(identifier, regularExpression);
        }


        private Result LexicalAnalyzer(string sql, List<string> admittedTokens)
        {
            List<string> tokens = GetTokens(sql);

            foreach (var token in tokens)
                if (!admittedTokens.Contains(token) && !IsIdentifier(token) && !_dataType.Contains(token))
                    return new Result { Success = false, ErrorMessage = string.Format("Error léxico: Token no válido: {0}", token) };

            return new Result { Success = true };
        }

        private Result SyntaxAnalyzer(string sql, List<string> admittedTokens, string regularExpression)
        {
            if (MatchesRegularExpression(sql, regularExpression))
                return new Result { Success = true };

            List<string> missingTokens = admittedTokens.ToList();
            foreach (var token in GetTokens(sql))
                if (missingTokens.Contains(token))
                    missingTokens.Remove(token);

            string error = "";
            if (missingTokens.Any())
            {
                foreach (string missingToken in missingTokens)
                    error += missingToken + ", ";
                return new Result { Success = false, ErrorMessage = "Análisis sintáctico falló: Faltan: " + error.Substring(0, error.Length - 2)};
            }
            return new Result { Success = false, ErrorMessage = "Análisis semántico falló" };
        }

        private Result ParseCreateDatabase(string sql)
        {
            Result lexicalAnalyzerResult = LexicalAnalyzer(sql, _tokensCreateDatabase);
            if (!lexicalAnalyzerResult.Success)
                return lexicalAnalyzerResult;

            //return new Result { Success = true };
            return SyntaxAnalyzer(sql, _tokensCreateDatabase, @"^CREATE DATABASE " + _identifier + "$");
        }

        private Result ParseDropDatabase(string sql)
        {
            Result lexicalAnalyzerResult = LexicalAnalyzer(sql, _tokensDropDatabase);
            if (!lexicalAnalyzerResult.Success)
                return lexicalAnalyzerResult;

            //return new Result { Success = true };
            return SyntaxAnalyzer(sql, _tokensDropDatabase, @"^DROP DATABASE " + _identifier + "$");
        }


        private Result ParseDropTable(string sql)
        {
            Result lexicalAnalyzerResult = LexicalAnalyzer(sql, _tokensDropTable);
            if (!lexicalAnalyzerResult.Success)
                return lexicalAnalyzerResult;

            //return new Result { Success = true };
            return SyntaxAnalyzer(sql, _tokensDropTable, @"^DROP TABLE " + _identifier + "$");
        }

        private Result ParseCreateTable(string sql)
        {
            Result lexicalAnalyzerResult = LexicalAnalyzer(sql, _tokensCreateTable);
            if (!lexicalAnalyzerResult.Success)
                return lexicalAnalyzerResult;
           // return new Result { Success = true };
            string regular = string.Format(@"^CREATE TABLE {0} ( [{0} {1}]+[, {0} {1}]* )$", _identifier, _dataType);
            return SyntaxAnalyzer(sql, _tokensCreateTable, regular);
        }

        private Result ParseSelect(string sql)
        {
            Result lexicalAnalyzerResult = LexicalAnalyzer(sql, _tokensSelect);
            if (!lexicalAnalyzerResult.Success)
                return lexicalAnalyzerResult;

            return new Result { Success = true };
            //string regular = string.Format(@"^SELECT [{0}]+[,{0}]* FROM {0} WHERE {0}={0} $", _identifier);
            //return SyntaxAnalyzer(sql, _tokensSelect, regular);
        }

        private Result ParseInsert(string sql)
        {
            Result lexicalAnalyzerResult = LexicalAnalyzer(sql, _tokensInsert);
            if (!lexicalAnalyzerResult.Success)
                return lexicalAnalyzerResult;
            //return new Result { Success = true };
            string regular = string.Format(@"^INSERT INTO {0} VALUES ( [{0}]+[,{0}{1}]* )$", _identifier, _dataType);
            return SyntaxAnalyzer(sql, _tokensInsert, regular);
        }

        private Result ParseUpdate(string sql)
        {
            Result lexicalAnalyzerResult = LexicalAnalyzer(sql, _tokensUpdate);
            if (!lexicalAnalyzerResult.Success)
                return lexicalAnalyzerResult;
            return new Result { Success = true };
            //string regular = string.Format(@"^UPDATE {0} SET ( [{0}=[0-9]+]+[ ,{0}=[0-9]+]* ) WHERE {0}=[0-9]+$", _identifier);
            //return SyntaxAnalyzer(sql, _tokensUpdate, regular);
        }

        private Result ParseDelete(string sql)
        {
            Result lexicalAnalyzerResult = LexicalAnalyzer(sql, _tokensDelete);
            if (!lexicalAnalyzerResult.Success)
                return lexicalAnalyzerResult;
            return new Result { Success = true };
            //string regular = string.Format(@"^DELETE FROM {0} WHERE {0}=[0-9]+$", _identifier);
            //return SyntaxAnalyzer(sql, _tokensDelete, regular);
        }

        public Result Parse(string sql, ParseType parseType)
        {
            switch (parseType)
            {
                case ParseType.CreateDatabase: return ParseCreateDatabase(sql);
                case ParseType.DropDatabase: return ParseDropDatabase(sql);
                case ParseType.CreateTable: return ParseCreateTable(sql);
                case ParseType.DropTable: return ParseDropTable(sql);
                case ParseType.Select: return ParseSelect(sql);
                case ParseType.Insert: return ParseInsert(sql);
                case ParseType.Update: return ParseUpdate(sql);
                case ParseType.Delete: return ParseDelete(sql);
                default: return new Result();
            }
        }

    }
}