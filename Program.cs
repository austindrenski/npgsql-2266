using System;
using System.Linq;
using Npgsql;

namespace npgsql_2266
{
    class Program
    {
        static string _connectionString;
        static string _commandString;
        static string[] _parameters;

        static void Main(string[] args)
        {
            _connectionString = $"Host=localhost;Port=5432;Username={args[0]};Password={args[1]};";

            Setup(8_000);

            while (true)
            {
                Run();
            }

            // ReSharper disable once FunctionNeverReturns
        }

        static void Setup(int parameters)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();

                cmd.CommandText = "DROP TABLE IF EXISTS prepare_test;";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE prepare_test ( c0 INT );";
                cmd.ExecuteNonQuery();
            }

            _parameters = Enumerable.Range(0, parameters).Select(i => $"@p{i}").ToArray();
            _commandString = $"INSERT INTO prepare_test VALUES {string.Join(", ", _parameters.Select(x => $"({x})"))};";
        }

        static async void Run()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            using (var cmd = new NpgsqlCommand(_commandString, conn))
            {
                foreach (var p in _parameters)
                {
                    cmd.Parameters.AddWithValue(p, 0);
                }

                await conn.OpenAsync();
                await cmd.PrepareAsync();
                await cmd.ExecuteNonQueryAsync();
                await Console.Out.WriteLineAsync("Command executed.");
            }
        }
    }
}