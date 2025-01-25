using System.Data;
using System.Data.SqlClient;

namespace OllamaSqlClr.DataAccess
{
    public interface IDatabaseExecutor
    {

        DataTable ExecuteQuery(string query);

        void ExecuteNonQuery(string commandText);

        void ExecuteNonQuery(string commandText, SqlParameter[] parameters);

        bool TryQuery(string query);

    }
}
