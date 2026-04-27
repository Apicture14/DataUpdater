using System;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace DataUpdater.DAO;

public class DataAccessor
{
    private const string ConnectionString = "Server=localhost;Database=mydatabase;Uid=root;Pwd=root;Charset=utf8mb4";
    private readonly MySqlConnection _connection = new MySqlConnection(ConnectionString);
    public MySqlConnection Connection => _connection;
    public DataAccessor()
    {
        try
        {
            _connection.Open();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to connect to database: " + ex.Message);
        }
    }
    public bool CreateNewDataSheet(string sheetName)
    {
        string sql = """

                     """;
        throw new NotImplementedException();
    }
}
