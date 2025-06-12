using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces;
using Shared.Models;
using Shared.Models.Dtos;

namespace AdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminPostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ILogger<AdminPostsController> _logger;

        public AdminPostsController(
            IPostService postService,
            ILogger<AdminPostsController> logger)
        {
            _postService = postService;
            _logger = logger;
        }

        /// <summary>
        /// Get all posts (admin view with more details)
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
                _logger.LogError(ex, "Error fetching posts in admin");
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
        /// Create a new post with media upload
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PostDto>> CreatePost([FromForm] CreatePostDto dto)
        {
            try
            {
                var post = await _postService.CreatePostAsync(dto);
                return CreatedAtAction(nameof(GetPost), new { id = post.Id }, new { success = true, data = post });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                return StatusCode(500, new { success = false, error = "Internal server error" });
            }
        }

        /// <summary>
        /// Update an existing post
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<PostDto>> UpdatePost(Guid id, [FromForm] UpdatePostDto dto)
        {
            try
            {
                var post = await _postService.UpdatePostAsync(id, dto);
                return Ok(new { success = true, data = post });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { success = false, error = "Post not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId}", id);
                return StatusCode(500, new { success = false, error = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a post
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            try
            {
                await _postService.DeletePostAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { success = false, error = "Post not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId}", id);
                return StatusCode(500, new { success = false, error = "Internal server error" });
            }
        }
    }
} 