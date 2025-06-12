using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces;
using Shared.Models;

namespace FrontApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostRepository _postRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly ILogger<PostsController> _logger;

        public PostsController(
            IPostRepository postRepository,
            IMediaRepository mediaRepository,
            ILogger<PostsController> logger)
        {
            _postRepository = postRepository;
            _mediaRepository = mediaRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all posts with their media information
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts()
        {
            try
            {
                var posts = await _postRepository.GetAllAsync();
                var postDtos = posts.Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    PublicId = p.PublicId,
                    MediaUrl = p.Media?.AwsS3Path,
                    CreatedAt = p.CreatedAt
                }).ToList();

                return Ok(postDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching posts");
                return StatusCode(500, "Internal server error while fetching posts");
            }
        }

        /// <summary>
        /// Get a specific post by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetPost(Guid id)
        {
            try
            {
                var post = await _postRepository.GetByIdAsync(id);
                if (post == null)
                {
                    return NotFound($"Post with ID {id} not found");
                }

                var postDto = new PostDto
                {
                    Id = post.Id,
                    Title = post.Title,
                    PublicId = post.PublicId,
                    MediaUrl = post.Media?.AwsS3Path,
                    CreatedAt = post.CreatedAt,
                    JsonMeta = post.JsonMeta
                };

                return Ok(postDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching post {PostId}", id);
                return StatusCode(500, "Internal server error while fetching post");
            }
        }

        /// <summary>
        /// Get posts with pagination
        /// </summary>
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<PostDto>>> GetPagedPosts(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var allPosts = await _postRepository.GetAllAsync();
                var totalCount = allPosts.Count();
                
                var posts = allPosts
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PostDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        PublicId = p.PublicId,
                        MediaUrl = p.Media?.AwsS3Path,
                        CreatedAt = p.CreatedAt
                    }).ToList();

                var result = new PagedResult<PostDto>
                {
                    Items = posts,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paged posts");
                return StatusCode(500, "Internal server error while fetching posts");
            }
        }
    }

    // DTOs for API responses
    public class PostDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int PublicId { get; set; }
        public string? MediaUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? JsonMeta { get; set; }
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
} 