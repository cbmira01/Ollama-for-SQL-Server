using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlClrApiExecutor
{
    public class CommandExecutor
    {
        [SqlFunction(DataAccess = DataAccessKind.None)]
        public static SqlString ExecuteApiCommand(SqlString apiUrl, SqlString requestBody)
        {
            // SQLCLR logic here.
        }
    }
}

