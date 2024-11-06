using System.Data;

namespace OllamaSqlClr.Helpers
{
    public interface ISqlCommand
    {
        string CreateProcedureFromQuery(string query);
        DataTable RunTemporaryProcedure(string name);
        (bool success, string message) DropTemporaryProcedure(string name);
    }
}

