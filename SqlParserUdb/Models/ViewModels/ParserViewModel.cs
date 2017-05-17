using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SqlParserUdb.Models.ViewModels
{
    public class ParserViewModel
    {
        public List<Database> Databases { get; set; }
    }

    public class Database
    {
        public string Name { get; set; }
        public List<string> Tables { get; set; }
    }
}