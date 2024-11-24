using System.Data;

namespace OllamaSqlClr.Helpers
{
    public interface ISqlQueryHelper
    {
        DataTable ExecuteProcedure(string name);
    }
}
