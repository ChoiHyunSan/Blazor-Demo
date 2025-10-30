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
    
        public async Task<bool> SendMailAsync(List<int> users, Mail mail)
        {
            if(users is null || users.Count == 0)
            {
                return false;
            }

            if(mail is null || mail.itemCount <= 0)
            {
                return false;
            }

            const string sql = """
                INSERT INTO mails (user_id, title, body, item_id, item_count)
                VALUES (@uid, @title, @body, @itemId, @itemCount)
            """;

            await using var conn = await _dataSource.OpenConnectionAsync();
            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                foreach(var uid in users)
                {
                    await conn.ExecuteAsync(sql, new
                    {
                        uid = uid,
                        title = mail.title,
                        body = mail.body,
                        itemId = mail.itemId,
                        itemCount = mail.itemCount
                    }, tx);   
                }

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                return false;
            }
        }
    }
}
