using System.Data;
using Dapper;
using Shared.Data;
using Shared.Models;

namespace Shared.Repositories;

public class PostRepository : IPostRepository
{
    private readonly DatabaseContext _context;

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
        var sql = "SELECT * FROM posts WHERE id = @Id";
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

        await connection.ExecuteAsync(sql, post);
        return post.Id;
    }

    public async Task UpdateAsync(Post post)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            UPDATE posts 
            SET title = @Title,
                media_id = @MediaId,
                json_meta = @JsonMeta::jsonb,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, post);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(
            "DELETE FROM posts WHERE id = @Id",
            new { Id = id });
    }
} 