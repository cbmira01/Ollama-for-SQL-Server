using OllamaSqlClr.DataAccess;
using System.Data.SqlClient;
using System.Data;

namespace OllamaSqlClr.Helpers
{
    // Handle SQL queries

    public class SqlQuery : ISqlQuery
    {
        public readonly IDatabaseExecutor _dbExecutor;

        public SqlQuery(IDatabaseExecutor dbExecutor)
        {
            _dbExecutor = dbExecutor;
        }

        public DataTable ExecuteProcedure(string name)
        {
            DataTable resultTable = new DataTable();
            string executeProcedureCommand = $"EXEC {name};";

            using (var cmd = new System.Data.SqlClient.SqlCommand(executeProcedureCommand, _dbExecutor.GetConnection()))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                adapter.Fill(resultTable);
            }

            return resultTable;
        }
    }
}
