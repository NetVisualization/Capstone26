using ClickHouse.Driver.ADO;
using ClickHouse.Driver.Copy;
using System.Data;
using ClickHouse.Driver.Utility;


var connectionString = "Host=100.111.112.111;Protocol=http;Database=net;Username=capstone;Password=boogle";

using (var connection = new ClickHouseConnection(connectionString))
{
    connection.Open();
    Console.WriteLine("Connected");
    
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = "CREATE TABLE IF NOT EXISTS net.test (id Int64, name String) ENGINE = Memory";
            command.ExecuteNonQuery();
 
            command.AddParameter("id", "Int64", 1);
            command.AddParameter("name", "String", "test1");
            command.CommandText = "INSERT INTO net.test (id, name) VALUES ({id:Int64}, {name:String})";
            command.ExecuteNonQuery();
            
        }
    
}