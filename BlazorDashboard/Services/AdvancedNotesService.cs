using Dapper;
using Npgsql;
using System.Text;

namespace BlazorDashboard.Services
{
    public class AdvancedNotesService
    {
        private readonly NpgsqlDataSource _dataSource;
        public AdvancedNotesService(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public record NoteAdv(long Id, string Title, string? Content, DateTime CreatedAt, DateTime UpdatedAt, bool Archived);

        public class Query
        {
            public string? TitleContains { get; set; }
            public string? ContentContains { get; set; }
            public bool? Archived { get; set; }
            public DateTime? From { get; set; }
            public DateTime? To { get; set; }
            public string SortBy { get; set; } = "updated"; // updated | created | title
            public bool Desc { get; set; } = true;
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 20;
        }

        public async virtual Task<(IEnumerable<NoteAdv> Items, int total)> SearchAsync(Query query)
        {
            var sb = new StringBuilder("WHERE 1=1");
            var dp = new DynamicParameters();

            // 타이틀 필터
            if (!string.IsNullOrWhiteSpace(query.TitleContains))
            {
                sb.Append(" AND title ILIKE '%' || @title || '%'");
                dp.Add("title", query.TitleContains);
            }

            // 내용 필터
            if (!string.IsNullOrWhiteSpace(query.ContentContains))
            {
                sb.Append(" AND content ILIKE '%' || @content || '%'");
                dp.Add("content", query.ContentContains);
            }

            // 보관됨 필터
            if (query.Archived is not null)
            {
                sb.Append(" AND archived = @archived");
                dp.Add("archived", query.Archived.Value);
            }

            // 생성일 필터
            if (query.From is not null)
            {
                sb.Append(" AND created_at >= @from");
                dp.Add("from", query.From.Value);
            }

            if(query.To is not null)
            {
                sb.Append(" AND created_at <= @to");
                dp.Add("to", query.To.Value);
            }

            // 정렬
            var sortBy = query.SortBy switch
            {
                "created" => "created_at",
                "title" => "title",
                _ => "updated_at",
            };

            var orderBy = $"ORDER BY {sortBy} {(query.Desc ? "DESC" : "ASC")}";

            // 페이징
            var offset = (query.Page - 1) * query.PageSize;
            var pageSize = query.PageSize <= 0 ? 20 : Math.Min(100, query.PageSize);

            // 쿼리 조합
            var sql = $"""
                SELECT Id, Title, Content, Created_at, Updated_at, Archived
                FROM notes_adv
                {sb}
                {orderBy}
                LIMIT @pageSize OFFSET @offset;
                SELECT COUNT(*) FROM notes_adv {sb};
            """;
           
            dp.Add("pageSize", pageSize);
            dp.Add("offset", offset);

            await using var conn = await _dataSource.OpenConnectionAsync();
            using var multi = await conn.QueryMultipleAsync(sql, dp);
            var items = await multi.ReadAsync<NoteAdv>();
            var total = await multi.ReadSingleAsync<int>();

            return (items, total);
        }
        public async Task<long> CreateAsync(string title, string? content = null)
        {
            const string sql = """
                INSERT INTO notes_adv (title, content)
                VALUES (@title, @content)
                RETURNING id;
            """;
            await using var conn = await _dataSource.OpenConnectionAsync();
            return await conn.ExecuteScalarAsync<long>(sql, new { title, content });
        }

        public async Task<int> UpdateAsync(long id, string title, string? content, bool archived)
        {
            const string sql = """
                UPDATE notes_adv
                SET title = @title,
                    content = @content,
                    archived = @archived,
                    updated_at = now()
                WHERE id = @id;
            """;
            await using var conn = await _dataSource.OpenConnectionAsync();
            return await conn.ExecuteAsync(sql, new { id, title, content, archived });
        }

        public async Task<int> DeleteAsync(long id)
        {
            const string sql = "DELETE FROM notes_adv WHERE id = @id;";
            await using var conn = await _dataSource.OpenConnectionAsync();
            return await conn.ExecuteAsync(sql, new { id });
        }
        public async Task<NoteAdv?> GetAsync(long id)
        {
            const string sql = "SELECT * FROM notes_adv WHERE id = @id;";
            await using var conn = await _dataSource.OpenConnectionAsync();
            return await conn.QuerySingleOrDefaultAsync<NoteAdv>(sql, new { id });
        }
    }
}
