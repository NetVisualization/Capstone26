using ClickHouse.Driver.ADO;
using ClickHouse.Driver.Copy;
using System.Data;
using ClickHouse.Driver.Utility;


var connectionString = "Host=100.111.112.111;Protocol=http;Database=net;Username=capstone;Password=boogle";

using (var connection = new ClickHouseConnection(connectionString))
{
    connection.Open();
    Console.WriteLine("Connected");
    
    using (var command = connection.CreateCommand())
    {
        command.AddParameter("id", "Int64", 10);
        command.CommandText = "USE net;";
        command.ExecuteNonQuery();
        command.CommandText = "SHOW TABLES;";
        var rowsAffected = command.ExecuteNonQuery();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine(reader.GetString(0));
        }
    }
}