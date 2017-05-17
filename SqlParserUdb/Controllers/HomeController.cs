using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SqlParserUdb.Models;
using SqlParserUdb.Models.ViewModels;
using System.Data;
using System.Data.SqlClient;

namespace SqlParserUdb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Parser parser = new Parser("");
            var lstDatabases = new List<Database>();

            if (parser.Connect())
            {
                DataTable databases = parser.GetAllDatabases();

                foreach (DataRow row in databases.Rows)
                {
                    var lstTables = new List<string>();

                    var tempParser = new Parser(row["name"].ToString());

                    tempParser.Connect();
                    var name = row["name"].ToString();
                    var tables = tempParser.GetTablesFromDatabase(name);
                    var tables2 = tables.Rows;

                    foreach (DataRow table in tables2)
                    {
                        lstTables.Add(table["TABLE_NAME"].ToString());
                    }
                    tempParser.Disconnect();
                    tempParser = null;

                    lstDatabases.Add(new Database { Name = row["name"].ToString(), Tables = lstTables });
                }
                    
            }

            return View(new ParserViewModel { Databases = lstDatabases });
        }

        //[HttpPost]
        //public ActionResult Parse(string selectDatabase, string textSqlScript, ParseType parseType)
        //{
        //    Parser parser = new Parser(selectDatabase);
        //    DataTable result = new DataTable();
        //    if (parser.Connect())
        //        result = parser.ExecuteQuery(textSqlScript);
        //    return View(new ResultViewModel { SqlScript = textSqlScript, Table = result });
        //}

        public ActionResult ParseCreateDatabase(string textSqlScript)
        {
            textSqlScript = textSqlScript.ToUpper();
            Parser parser = new Parser();
            DataTable dataTable = new DataTable();
            Result result = parser.Parse(textSqlScript, ParseType.CreateDatabase);

            if (result.Success)
                if (parser.Connect())
                    dataTable = parser.ExecuteQuery(textSqlScript);

            return View("Parse", new ResultViewModel 
                {
                    SqlScript = textSqlScript, 
                    Table = dataTable, 
                    Result = result,
                    Syntax = "CREATE DATABASE <identificador>"
                });
        }

        public ActionResult ParseDropDatabase(string textSqlScript)
        {
            textSqlScript = textSqlScript.ToUpper();
            Parser parser = new Parser();
            DataTable dataTable = new DataTable();
            Result result = parser.Parse(textSqlScript, ParseType.DropDatabase);

            if (result.Success)
                if (parser.Connect())
                    dataTable = parser.ExecuteQuery(textSqlScript);

            return View("Parse", new ResultViewModel
            {
                SqlScript = textSqlScript,
                Table = dataTable,
                Result = result,
                Syntax = "DROP DATABASE <identificador>"
            });
        }

        public ActionResult ParseDropTable(string textSqlScript, string selectDatabase)
        {
            textSqlScript = textSqlScript.ToUpper();
            Parser parser = new Parser(selectDatabase);
            DataTable dataTable = new DataTable();
            Result result = parser.Parse(textSqlScript, ParseType.DropTable);

            if (result.Success)
                if (parser.Connect())
                    dataTable = parser.ExecuteQuery(textSqlScript);

            return View("Parse", new ResultViewModel
            {
                SqlScript = textSqlScript,
                Table = dataTable,
                Result = result,
                Syntax = "DROP TABLE <identificador>"
            });
        }


        public ActionResult ParseCreateTable(string textSqlScript, string selectDatabase)
        {
            textSqlScript = textSqlScript.ToUpper();
            Parser parser = new Parser(selectDatabase);
            DataTable dataTable = new DataTable();
            Result result = parser.Parse(textSqlScript, ParseType.CreateTable);

            if (parser.Connect())
                dataTable = parser.ExecuteQuery(textSqlScript);


            return View("Parse", new ResultViewModel
            {
                SqlScript = textSqlScript,
                Table = dataTable,
                Result = (parser.Errors.Any()) ? new Result { ErrorMessage = "Análisis sintáctico falló", Success = false } : new Result { Success = true },
                Syntax = "CREATE TABLE <identificador> ( <campo1> <tipo1> [ , <campo2> <tipo2> ... ] )"
            });
        }

        public ActionResult ParseSelect(string textSqlScript, string selectDatabase)
        {
            textSqlScript = textSqlScript.ToUpper();
            Parser parser = new Parser(selectDatabase);
            DataTable dataTable = new DataTable();
            Result result = parser.Parse(textSqlScript, ParseType.Select);

            if (parser.Connect())
                dataTable = parser.ExecuteQuery(textSqlScript);

            return View("Parse", new ResultViewModel
            {
                SqlScript = textSqlScript,
                Table = dataTable,
                Result = (parser.Errors.Any()) ? new Result { ErrorMessage = "Análisis sintáctico falló", Success = false } : new Result { Success = true },
                Syntax = "SELECT <campos> FROM <identificador> WHERE <condiciones> GROUP BY <identificador> <ASC>|<DES>"
            });
        }

        public ActionResult ParseUpdate(string textSqlScript, string selectDatabase)
        {
            textSqlScript = textSqlScript.ToUpper();
            Parser parser = new Parser(selectDatabase);
            DataTable dataTable = new DataTable();
            Result result = parser.Parse(textSqlScript, ParseType.Update);

            if (parser.Connect())
                dataTable = parser.ExecuteQuery(textSqlScript);

            return View("Parse", new ResultViewModel
            {
                SqlScript = textSqlScript,
                Table = dataTable,
                Result = (parser.Errors.Any()) ? new Result { ErrorMessage = "Análisis sintáctico falló", Success = false } : new Result { Success = true },
                Syntax = "UPDATE <identificador> SET <asignaciones>"
            });
        }

        public ActionResult ParseInsert(string textSqlScript, string selectDatabase)
        {
            textSqlScript = textSqlScript.ToUpper();
            Parser parser = new Parser(selectDatabase);
            DataTable dataTable = new DataTable();
            Result result = parser.Parse(textSqlScript, ParseType.Insert);

            if (parser.Connect())
                dataTable = parser.ExecuteQuery(textSqlScript);

            return View("Parse", new ResultViewModel
            {
                SqlScript = textSqlScript,
                Table = dataTable,
                Result = (parser.Errors.Any()) ? new Result { ErrorMessage = "Análisis sintáctico falló", Success = false } : new Result { Success = true },
                Syntax = "INSERT INTO <identificador> VALUES ( <campo1> [ , <campo2> ... ] )"
            });
        }

        public ActionResult ParseDelete(string textSqlScript, string selectDatabase)
        {
            textSqlScript = textSqlScript.ToUpper();
            Parser parser = new Parser(selectDatabase);
            DataTable dataTable = new DataTable();
            Result result = parser.Parse(textSqlScript, ParseType.Delete);

            if (parser.Connect())
                dataTable = parser.ExecuteQuery(textSqlScript);

            return View("Parse", new ResultViewModel
            {
                SqlScript = textSqlScript,
                Table = dataTable,
                Result = (parser.Errors.Any()) ? new Result { ErrorMessage = "Análisis sintáctico falló", Success = false } : new Result { Success = true },
                Syntax = "DELETE FROM <identificador> WHERE <condiciones>"
            });
        }
    }
}