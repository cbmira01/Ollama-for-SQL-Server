using System.Data;

namespace OllamaSqlClr.Helpers
{
    public interface ISqlQuery
    {
        DataTable ExecuteProcedure(string name);
    }
}
