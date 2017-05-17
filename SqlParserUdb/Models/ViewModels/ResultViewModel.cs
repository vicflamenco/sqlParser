using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace SqlParserUdb.Models.ViewModels
{
    public class ResultViewModel
    {
        public string SqlScript { get; set; }
        public DataTable Table { get; set; }
        public Result Result { get; set; }
        public string Syntax { get; set; }
    }
}