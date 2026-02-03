using System;
using UnityEngine;
using System.Collections.Generic;
using ClickHouse.Client.ADO;
using System.Data.Common;
using System.Threading.Tasks;

public class DBConnection : IDisposable
{
    private ClickHouseConnection _connection;
    public bool IsConnected => (_connection != null) && (_connection.State == System.Data.ConnectionState.Open);
    
    //NodeSpawnerScript nodeSpawnerScript;
    //public GameObject nodeSpawner;

    /// <summary>
    /// Connect to the Clickhouse Database
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    /// <param name="dbName"></param>
    /// <param name="user"></param>
    /// <param name="pass"></param>
    /// <returns></returns>
    public async Task connect(string host, string port, string dbName, string user, string pass)
    {
        // clickhouse connection info
        string connectionString = $"Host={host};Database={dbName};Port={port};Username={user};Password={pass}";
        _connection = new ClickHouseConnection(connectionString);
        await _connection.OpenAsync();
        Debug.Log($"Connected to the {dbName} database.");
    }

    /// <summary>
    /// idk chat wanted me to overload this method
    /// </summary>
    /// <param name="commandText"></param>
    /// <returns></returns>
    public async Task<object> ExecuteScalarAsync(string commandText)
    {
        EnsureConnection();
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = commandText;
        return await cmd.ExecuteScalarAsync();
    }

    public async Task<object> ExecuteCommand(string text)
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = text;
        return await cmd.ExecuteScalarAsync();
    }

    public async Task<object> ExecuteNonQuery(string text)
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = text;
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<DbDataReader> ExecuteReader(string commandText, Dictionary<string, object> parameters = null)
    {
        EnsureConnection();
        var cmd = _connection.CreateCommand();
        cmd.CommandText = commandText;

        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                cmd.Parameters.Add(new ClickHouse.Client.ADO.Parameters.ClickHouseDbParameter
                {
                    ParameterName = kvp.Key,
                    Value = kvp.Value
                });
            }
        }

        // Note: The caller is responsible for disposing this reader!
        return await cmd.ExecuteReaderAsync();
    }

    private void EnsureConnection()
    {
        if (_connection == null)
            throw new InvalidOperationException("Database connection is not initialized.");
    }

    // Standard IDisposable implementation
    public void Dispose()
    {
        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
            _connection = null;
            Debug.Log("[DatabaseClient] Connection closed.");
        }
    }

    /// <summary>
    /// Get the total number of nodes in the database.
    /// </summary>
    public async Task<int> getNodeCountAsync()
    {
        var result = await ExecuteCommand("SELECT COUNT(*) FROM nodes;");
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Get the total number of connections in the database.
    /// </summary>
    public async Task<int> getConnectionCountAsync()
    {
        var result = await ExecuteCommand("SELECT COUNT(*) FROM connections;");
        return Convert.ToInt32(result);
    }
}