using DockerX.Domain.Entities;
using DockerX.Domain.Interfaces;
using DockerX.Infrastructure.Data;
using Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;

namespace DockerX.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PostRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT p.*, m.* 
                FROM Posts p 
                LEFT JOIN Media m ON p.Id = m.PostId";
            
            var postDict = new Dictionary<int, Post>();
            
            await connection.QueryAsync<Post, Media, Post>(
                sql,
                (post, media) =>
                {
                    if (!postDict.TryGetValue(post.Id, out var postEntry))
                    {
                        postEntry = post;
                        postEntry.Media = new List<Media>();
                        postDict.Add(post.Id, postEntry);
                    }
                    
                    if (media != null)
                    {
                        postEntry.Media.Add(media);
                    }
                    
                    return postEntry;
                },
                splitOn: "Id"
            );
            
            return postDict.Values;
        }

        public async Task<Post> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT p.*, m.* 
                FROM Posts p 
                LEFT JOIN Media m ON p.Id = m.PostId 
                WHERE p.Id = @Id";
            
            var postDict = new Dictionary<int, Post>();
            
            await connection.QueryAsync<Post, Media, Post>(
                sql,
                (post, media) =>
                {
                    if (!postDict.TryGetValue(post.Id, out var postEntry))
                    {
                        postEntry = post;
                        postEntry.Media = new List<Media>();
                        postDict.Add(post.Id, postEntry);
                    }
                    
                    if (media != null)
                    {
                        postEntry.Media.Add(media);
                    }
                    
                    return postEntry;
                },
                new { Id = id },
                splitOn: "Id"
            );
            
            return postDict.Values.FirstOrDefault();
        }

        public async Task<Post> AddAsync(Post post)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO Posts (Title, Content, CreatedAt, UpdatedAt) 
                VALUES (@Title, @Content, @CreatedAt, @UpdatedAt) 
                RETURNING *";
            
            return await connection.QuerySingleAsync<Post>(sql, post);
        }

        public async Task UpdateAsync(Post post)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Posts 
                SET Title = @Title, 
                    Content = @Content, 
                    UpdatedAt = @UpdatedAt 
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, post);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM Posts WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
} 