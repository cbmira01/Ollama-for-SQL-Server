using System.Data;
using System.Data.SqlClient;

namespace OllamaSqlClr.DataAccess
{
    public interface IDatabaseExecutor
    {
        string ConnectionString { get; }

        DataTable ExecuteQuery(string query);

        void ExecuteNonQuery(string commandText);

        void ExecuteNonQuery(string commandText, SqlParameter[] parameters);

    }
}
