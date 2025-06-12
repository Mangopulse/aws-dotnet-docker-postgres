using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces;
using Shared.Models;
using API.Services;

namespace AdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminPostsController : ControllerBase
    {
        private readonly IPostRepository _postRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly S3Service _s3Service;
        private readonly ILogger<AdminPostsController> _logger;

        public AdminPostsController(
            IPostRepository postRepository,
            IMediaRepository mediaRepository,
            S3Service s3Service,
            ILogger<AdminPostsController> logger)
        {
            _postRepository = postRepository;
            _mediaRepository = mediaRepository;
            _s3Service = s3Service;
            _logger = logger;
        }

        /// <summary>
        /// Get all posts (admin view with more details)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminPostDto>>> GetPosts()
        {
            try
            {
                var posts = await _postRepository.GetAllAsync();
                var postDtos = posts.Select(p => new AdminPostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    PublicId = p.PublicId,
                    MediaId = p.MediaId,
                    MediaUrl = p.Media?.AwsS3Path,
                    JsonMeta = p.JsonMeta,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                return Ok(postDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching posts in admin");
                return StatusCode(500, "Internal server error while fetching posts");
            }
        }

        /// <summary>
        /// Create a new post with media upload
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AdminPostDto>> CreatePost([FromForm] CreatePostRequest request)
        {
            try
            {
                Guid? mediaId = null;

                // Handle media upload if provided
                if (request.MediaFile != null && request.MediaFile.Length > 0)
                {
                    // Validate file type
                    var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                    if (!allowedTypes.Contains(request.MediaFile.ContentType))
                    {
                        return BadRequest("Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed.");
                    }

                    // Validate file size (max 10MB)
                    if (request.MediaFile.Length > 10 * 1024 * 1024)
                    {
                        return BadRequest("File size too large. Maximum size is 10MB.");
                    }

                    // Upload to S3
                    using var stream = request.MediaFile.OpenReadStream();
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.MediaFile.FileName)}";
                    var s3Path = await _s3Service.UploadFileAsync(stream, fileName, request.MediaFile.ContentType);

                    // Create media record
                    var media = new Media
                    {
                        Id = Guid.NewGuid(),
                        AwsS3Path = s3Path,
                        CreatedAt = DateTime.UtcNow
                    };

                    var createdMedia = await _mediaRepository.CreateAsync(media);
                    mediaId = createdMedia.Id;
                }

                // Create post
                var post = new Post
                {
                    Title = request.Title,
                    MediaId = mediaId,
                    JsonMeta = request.JsonMeta ?? "{}"
                };

                var createdPost = await _postRepository.CreateAsync(post);

                // Return created post with media info
                var resultDto = new AdminPostDto
                {
                    Id = createdPost.Id,
                    Title = createdPost.Title,
                    PublicId = createdPost.PublicId,
                    MediaId = createdPost.MediaId,
                    MediaUrl = mediaId.HasValue ? (await _mediaRepository.GetByIdAsync(mediaId.Value))?.AwsS3Path : null,
                    JsonMeta = createdPost.JsonMeta,
                    CreatedAt = createdPost.CreatedAt,
                    UpdatedAt = createdPost.UpdatedAt
                };

                return CreatedAtAction(nameof(GetPost), new { id = createdPost.Id }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                return StatusCode(500, "Internal server error while creating post");
            }
        }

        /// <summary>
        /// Get a specific post by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminPostDto>> GetPost(Guid id)
        {
            try
            {
                var post = await _postRepository.GetByIdAsync(id);
                if (post == null)
                {
                    return NotFound($"Post with ID {id} not found");
                }

                var postDto = new AdminPostDto
                {
                    Id = post.Id,
                    Title = post.Title,
                    PublicId = post.PublicId,
                    MediaId = post.MediaId,
                    MediaUrl = post.Media?.AwsS3Path,
                    JsonMeta = post.JsonMeta,
                    CreatedAt = post.CreatedAt,
                    UpdatedAt = post.UpdatedAt
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
        /// Update an existing post
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<AdminPostDto>> UpdatePost(Guid id, [FromForm] UpdatePostRequest request)
        {
            try
            {
                var existingPost = await _postRepository.GetByIdAsync(id);
                if (existingPost == null)
                {
                    return NotFound($"Post with ID {id} not found");
                }

                // Update title if provided
                if (!string.IsNullOrWhiteSpace(request.Title))
                {
                    existingPost.Title = request.Title;
                }

                // Update JSON meta if provided
                if (!string.IsNullOrWhiteSpace(request.JsonMeta))
                {
                    existingPost.JsonMeta = request.JsonMeta;
                }

                // Handle new media upload if provided
                if (request.MediaFile != null && request.MediaFile.Length > 0)
                {
                    // Validate file
                    var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                    if (!allowedTypes.Contains(request.MediaFile.ContentType))
                    {
                        return BadRequest("Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed.");
                    }

                    if (request.MediaFile.Length > 10 * 1024 * 1024)
                    {
                        return BadRequest("File size too large. Maximum size is 10MB.");
                    }

                    // Upload new media to S3
                    using var stream = request.MediaFile.OpenReadStream();
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.MediaFile.FileName)}";
                    var s3Path = await _s3Service.UploadFileAsync(stream, fileName, request.MediaFile.ContentType);

                    // Create new media record
                    var newMedia = new Media
                    {
                        Id = Guid.NewGuid(),
                        AwsS3Path = s3Path,
                        CreatedAt = DateTime.UtcNow
                    };

                    var createdMedia = await _mediaRepository.CreateAsync(newMedia);
                    existingPost.MediaId = createdMedia.Id;

                    // TODO: Consider deleting old media from S3 if needed
                }

                var updatedPost = await _postRepository.UpdateAsync(existingPost);

                var resultDto = new AdminPostDto
                {
                    Id = updatedPost.Id,
                    Title = updatedPost.Title,
                    PublicId = updatedPost.PublicId,
                    MediaId = updatedPost.MediaId,
                    MediaUrl = updatedPost.Media?.AwsS3Path,
                    JsonMeta = updatedPost.JsonMeta,
                    CreatedAt = updatedPost.CreatedAt,
                    UpdatedAt = updatedPost.UpdatedAt
                };

                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId}", id);
                return StatusCode(500, "Internal server error while updating post");
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
                var post = await _postRepository.GetByIdAsync(id);
                if (post == null)
                {
                    return NotFound($"Post with ID {id} not found");
                }

                await _postRepository.DeleteAsync(id);

                // TODO: Consider deleting associated media from S3 if needed

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId}", id);
                return StatusCode(500, "Internal server error while deleting post");
            }
        }
    }

    // DTOs for admin operations
    public class AdminPostDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int PublicId { get; set; }
        public Guid? MediaId { get; set; }
        public string? MediaUrl { get; set; }
        public string? JsonMeta { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreatePostRequest
    {
        public string Title { get; set; } = string.Empty;
        public IFormFile? MediaFile { get; set; }
        public string? JsonMeta { get; set; }
    }

    public class UpdatePostRequest
    {
        public string? Title { get; set; }
        public IFormFile? MediaFile { get; set; }
        public string? JsonMeta { get; set; }
    }
} 