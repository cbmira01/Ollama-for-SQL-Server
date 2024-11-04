using System.Data;

namespace OllamaSqlClr.DataAccess
{
    public interface IDatabaseExecutor
    {
        DataTable ExecuteQuery(string query);

        void ExecuteNonQuery(string commandText);

        DataTable ExecuteWrappedQuery(string query);
    }
}
