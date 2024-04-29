using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using Npgsql;

class Worker
{
    static async Task Main(string[] args)

    {
        //variables de entorno
        string redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
        int redisPort = int.Parse(Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379");

        //configuracion de db
        string pgHost = Environment.GetEnvironmentVariable("PG_HOST") ?? "localhost";
        int pgPort = int.Parse(Environment.GetEnvironmentVariable("PG_PORT") ?? "5432");
        string pgUser = Environment.GetEnvironmentVariable("PG_USER") ?? "postgres";
        string pgPassword = Environment.GetEnvironmentVariable("PG_PASSWORD") ?? "password";
        string pgDatabase = Environment.GetEnvironmentVariable("PG_DATABASE") ?? "voting";


        var redis = ConnectionMultiplexer.Connect($"{redisHost}:{redisPort}");
        var db = redis.GetDatabase();

        // Conectarse a PostgreSQL
        string connectionString = $"Host={pgHost};Port={pgPort};Username={pgUser};Password={pgPassword};Database={pgDatabase}";
        using var conn = new NpgsqlConnection(connectionString);

        await conn.OpenAsync();

        // Crear la tabla si no existe
        var createTableCommand = new NpgsqlCommand(@"
            CREATE TABLE IF NOT EXISTS voting_results (
                option VARCHAR(50) PRIMARY KEY,
                count BIGINT
            )
        ", conn);
        await createTableCommand.ExecuteNonQueryAsync();

        // Obtener los votos de Redis
        var votes = await db.HashGetAllAsync("votes");

        // Procesar los votos
        foreach (var vote in votes)
        {
            string option = vote.Name;
            long count = (long)vote.Value;

            // Insertar o actualizar el resultado en la base de datos
            var insertOrUpdateCommand = new NpgsqlCommand(@"
                INSERT INTO voting_results (option, count)
                VALUES (@option, @count)
                ON CONFLICT(option) DO UPDATE 
                SET count = voting_results.count + @count
            ", conn);

            insertOrUpdateCommand.Parameters.AddWithValue("option", option);
            insertOrUpdateCommand.Parameters.AddWithValue("count", count);

            await insertOrUpdateCommand.ExecuteNonQueryAsync();
        }

        // Limpiar los votos de Redis
        await db.KeyDeleteAsync("votes");

        Console.WriteLine("Votos procesados y almacenados en PostgreSQL.");
    }
}
