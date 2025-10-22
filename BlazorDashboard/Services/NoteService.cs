using Dapper;
using Npgsql;

namespace BlazorDashboard.Services
{
    public class NoteService
    {
        private readonly NpgsqlDataSource _dataSource;
        public NoteService(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public record Note(long Id, string Title, DateTime CreatedAt);

        public async Task<IEnumerable<Note>> GetNotesAsync()
        {
            const string sql = """
                SELECT Id, Title, Created_at
                FROM notes
                ORDER BY Created_at DESC
                LIMIT 100;
            """;

            await using var conn = await _dataSource.OpenConnectionAsync();
            return await conn.QueryAsync<Note>(sql);
        }
        public async Task<long> AddNoteAsync(string title)
        {
            const string sql = """
                INSERT INTO notes (Title)
                VALUES (@Title)
                RETURNING Id;
            """;

            await using var conn = await _dataSource.OpenConnectionAsync();
            return await conn.ExecuteScalarAsync<long>(sql, new { Title = title });
        }
        public async Task<int> DeleteNoteAsync(long id)
        {
            const string sql = """
                DELECT FROM notes
                WHERE Id = @Id;
            """;

            await using var conn = await _dataSource.OpenConnectionAsync();
            return await conn.ExecuteAsync(sql, new { Id = id });
        }
    }
}
