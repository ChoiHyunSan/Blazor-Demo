using BlazorDashboard.Models;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Npgsql;

namespace BlazorDashboard.Services
{
    public class UserMailService
    {
        private readonly NpgsqlDataSource _dataSource;

        public UserMailService(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task<List<User>> GetUsersAsync()
        {
            const string sql = """
                SELECT Id, Name
                FROM users
                ORDER BY Id ASC
            """;

            await using var conn = await _dataSource.OpenConnectionAsync();
            var result = await conn.QueryAsync<User>(sql);

            return result.AsList();
        } 
    

    }
}
