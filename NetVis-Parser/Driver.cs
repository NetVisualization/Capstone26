using System;
using System.Threading.Tasks;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Utility;

internal class DatabaseConnection
{
    public const int NODE_EXP_SECONDS = 150;
    public const int PACKET_EXP_SECONDS = 30;

    private ClickHouseConnection _connection;

    public DatabaseConnection(string host, string port, string dbName)
    {
        // clickhouse connection info
        string connectionString = $"Host={host};Database={dbName};Username=capstone;Password=boogle";
        // Create a new client connected to the server
        _connection = new ClickHouseConnection(connectionString);
        _connection.Open();
    }
    public ClickHouseCommand CreateCommand()
    {
        return _connection.CreateCommand();
    }
}

namespace TestHarness
{
    internal class Program
    {
        private const string DB_NAME = "net";
        private const string DB_HOST = "10.200.1.13";
        private const string DB_PORT = "8123";
        static async Task Main(string[] args)
        {
            DatabaseConnection db = new DatabaseConnection(DB_HOST, DB_PORT, DB_NAME);
            var cmd = db.CreateCommand();
            cmd.CommandText = "SELECT version()";
            var version = await cmd.ExecuteScalarAsync();
            Console.WriteLine($"Connected to ClickHouse version: {version}");

            // Ask user how where they want to get data from
            Console.WriteLine("Which source from which source?");
            Console.WriteLine("0) Static Capture from WiFi Network");
            Console.WriteLine("1) Local .pcap file");
            Console.WriteLine("2) Live Network Capture");
            Console.WriteLine("\nWarning: proceeding will destroy any previous data stored in the database.");

            string dataSource = Console.ReadLine();

            if (dataSource == "1")
            {
                Console.WriteLine("Enter .pcap filename (in NetVis-Code):");
                string fname = Console.ReadLine();
            }
        }
    }
}
 