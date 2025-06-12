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
    public class MediaRepository : IMediaRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MediaRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<IEnumerable<Media>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT m.*, p.* 
                FROM Media m 
                LEFT JOIN Posts p ON m.PostId = p.Id";
            
            var mediaDict = new Dictionary<int, Media>();
            
            await connection.QueryAsync<Media, Post, Media>(
                sql,
                (media, post) =>
                {
                    if (!mediaDict.TryGetValue(media.Id, out var mediaEntry))
                    {
                        mediaEntry = media;
                        mediaEntry.Post = post;
                        mediaDict.Add(media.Id, mediaEntry);
                    }
                    return mediaEntry;
                },
                splitOn: "Id"
            );
            
            return mediaDict.Values;
        }

        public async Task<Media> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT m.*, p.* 
                FROM Media m 
                LEFT JOIN Posts p ON m.PostId = p.Id 
                WHERE m.Id = @Id";
            
            var mediaDict = new Dictionary<int, Media>();
            
            await connection.QueryAsync<Media, Post, Media>(
                sql,
                (media, post) =>
                {
                    if (!mediaDict.TryGetValue(media.Id, out var mediaEntry))
                    {
                        mediaEntry = media;
                        mediaEntry.Post = post;
                        mediaDict.Add(media.Id, mediaEntry);
                    }
                    return mediaEntry;
                },
                new { Id = id },
                splitOn: "Id"
            );
            
            return mediaDict.Values.FirstOrDefault();
        }

        public async Task<Media> AddAsync(Media media)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO Media (Url, Type, PostId, CreatedAt, UpdatedAt) 
                VALUES (@Url, @Type, @PostId, @CreatedAt, @UpdatedAt) 
                RETURNING *";
            
            return await connection.QuerySingleAsync<Media>(sql, media);
        }

        public async Task UpdateAsync(Media media)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Media 
                SET Url = @Url, 
                    Type = @Type, 
                    PostId = @PostId, 
                    UpdatedAt = @UpdatedAt 
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, media);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM Media WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
} 