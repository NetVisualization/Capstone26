using MongoDB.Driver;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.IsisMtt.X509;
using ClickHouse.Ado;
using System.Data.Common;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections;
using MongoDB.Driver.Core.Events;

namespace NetCapture
{
    internal class Driver
    {
        private const string DB_NAME = "net";
        private const string DB_HOST = "10.200.1.13";
        private const string DB_PORT = "8123";
        private const string DB_USER = "capstone";
        private const string DB_PASS = "boogle";

        public static async Task Main(string[] args)
        {
            try
            {

                DBConnection database = new DBConnection();
                await database.connect(DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASS);

                var version = await database.ExecuteCommand("SELECT version()");
                Console.WriteLine($"Connection to Clickhouse {version}");

                int result = await database.getNodeCountAsync();
                Console.WriteLine($"There are {result} nodes");

                List<DBConnection.Node> nodes = database.getNodesAfter(new DateTime(1970, 01, 01));
                List<DBConnection.Connection> conns = database.getConnectionsAfter(new DateTime(1970, 01, 01));

                var nodeType = nodes[0].GetType();
                var fields = nodeType.GetFields();
                foreach (var node in nodes)
                {
                    foreach (var field in fields)
                    {
                        object fieldVal = field.GetValue(node);
                        Console.WriteLine(fieldVal);
                    }
                }
                
                var connType = conns[0].GetType();
                fields = connType.GetFields();
                foreach (var conn in conns) {
                    foreach (var field in fields)
                    {
                        object fieldVal = field.GetValue(conn);
                        Console.WriteLine(fieldVal);
                    }    
                }
                

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
