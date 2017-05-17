using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SqlParserUdb.Models
{
    public enum ParseType
    {
        CreateDatabase = 1,
        DropDatabase = 2,
        CreateTable = 3,
        DropTable = 4,
        Select = 5,
        Insert = 6,
        Update = 7,
        Delete = 8
    }
}