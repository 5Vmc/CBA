namespace Babu.Config
{
    public interface IConfigLoader
    {
        DataTable LoadTable(string tableName);

        System.Threading.Tasks.Task<DataTable> LoadTableAsync(string tableName);
    }


}