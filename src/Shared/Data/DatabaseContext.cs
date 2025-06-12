using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Shared.Data;

public class DatabaseContext
{
    private readonly string _connectionString;

    public DatabaseContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException("DefaultConnection string is missing");
    }

    public IDbConnection CreateConnection()
        => new NpgsqlConnection(_connectionString);
} 