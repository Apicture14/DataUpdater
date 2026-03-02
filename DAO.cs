using System;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace DataUpdater;

public class DAO
{
    private const string ConnectionString = "Server=localhost;Database=mydatabase;Uid=root;Pwd=root;Charset=utf8mb4";
    private readonly MySqlConnection _connection = new MySqlConnection(ConnectionString);
    public MySqlConnection Connection {get=>_connection;} 
    public DAO()
    {
        try
        {
            _connection.Open();
        }catch (Exception ex)
        {
            Console.WriteLine("Failed");
        }
    }
}
