using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces;
using Shared.Models;

namespace FrontApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ILogger<PostsController> _logger;

        public PostsController(
            IPostService postService,
            ILogger<PostsController> logger)
        {
            _postService = postService;
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
                var posts = await _postService.GetAllPostsAsync();
                return Ok(new { success = true, data = posts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching posts");
                return StatusCode(500, new { success = false, error = "Internal server error" });
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
                var post = await _postService.GetPostByIdAsync(id);
                if (post == null)
                {
                    return NotFound(new { success = false, error = "Post not found" });
                }
                return Ok(new { success = true, data = post });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching post {PostId}", id);
                return StatusCode(500, new { success = false, error = "Internal server error" });
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
                //var result = await _postService.GetAllPostsAsync(page, pageSize);
                var result = await _postService.GetAllPostsAsync();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paged posts");
                return StatusCode(500, new { success = false, error = "Internal server error" });
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