using System.Data;
using Dapper;
using System.Text.Json;
using Shared.Data;
using Shared.Models;
using Shared.Interfaces;

namespace Shared.Repositories;

public class PostRepository : IPostRepository
{
    private readonly DatabaseContext _context;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };

    public PostRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Post>("SELECT * FROM posts ORDER BY created_at DESC");
    }

    public async Task<Post?> GetByIdAsync(Guid id)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            SELECT 
                id as Id,
                title as Title,
                media_id as MediaId,
                json_meta as JsonMeta,
                created_at as CreatedAt,
                updated_at as UpdatedAt,
                public_id as PublicId
            FROM posts 
            WHERE id = @Id";
        var post = await connection.QueryFirstOrDefaultAsync<Post>(sql, new { Id = id });
        return post;
    }

    public async Task<Post?> GetByPublicIdAsync(int publicId)
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Post>(
            "SELECT * FROM posts WHERE public_id = @PublicId",
            new { PublicId = publicId });
    }

    public async Task<Guid> CreateAsync(Post post)
    {
        if (post.Id == Guid.Empty)
        {
            post.Id = Guid.NewGuid();
        }

        using var connection = _context.CreateConnection();
        var sql = @"
            INSERT INTO posts (id, title, media_id, json_meta, created_at)
            VALUES (@Id, @Title, @MediaId, @JsonMeta::jsonb, @CreatedAt)
            RETURNING id";

        var jsonMeta = post.JsonMeta?.ToString() ?? "{}";
        // Normalize JSON formatting
        if (!string.IsNullOrEmpty(jsonMeta) && jsonMeta != "{}")
        {
            var obj = JsonSerializer.Deserialize<JsonDocument>(jsonMeta);
            jsonMeta = JsonSerializer.Serialize(obj, _jsonOptions);
        }

        var parameters = new
        {
            post.Id,
            post.Title,
            post.MediaId,
            JsonMeta = jsonMeta,
            CreatedAt = DateTime.UtcNow
        };

        Console.WriteLine($"SQL: {sql}");
        Console.WriteLine($"Parameters: Id={parameters.Id}, Title={parameters.Title}, MediaId={parameters.MediaId}, JsonMeta={parameters.JsonMeta}");

        var createdId = await connection.QuerySingleAsync<Guid>(sql, parameters);
        return createdId;
    }

    public async Task UpdateAsync(Post post)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            UPDATE posts SET title = @Title, media_id = @MediaId, json_meta = @JsonMeta::jsonb
            WHERE id = @Id";
        var parameters = new
        {
            post.Title,
            post.MediaId,
            JsonMeta = post.JsonMeta?.ToString(),
            post.Id
        };
        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM posts WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }
} 