using System.Data;

namespace OllamaSqlClr.Helpers
{
    public interface ISqlCommandHelper
    {
        string CreateProcedureFromQuery(string query);
        DataTable RunProcedure(string name);
        (bool success, string message) DropProcedure(string name);
    }
}

