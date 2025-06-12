using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces;
using Shared.Models;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository _postRepository;
    private readonly IMediaRepository _mediaRepository;
    private readonly IS3Service _s3Service;

    public PostsController(
        IPostRepository postRepository,
        IMediaRepository mediaRepository,
        IS3Service s3Service)
    {
        _postRepository = postRepository;
        _mediaRepository = mediaRepository;
        _s3Service = s3Service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
    {
        var posts = await _postRepository.GetAllAsync();
        return Ok(posts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> GetPost(Guid id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
        {
            return NotFound();
        }
        return Ok(post);
    }

    [HttpPost]
    public async Task<ActionResult<Post>> CreatePost([FromForm] string title, [FromForm] IFormFile media)
    {
        if (media == null || media.Length == 0)
        {
            return BadRequest("Media file is required");
        }

        // Upload media to S3
        using var stream = media.OpenReadStream();
        var s3Path = await _s3Service.UploadFileAsync(stream, media.FileName, media.ContentType);

        // Create media record
        var mediaEntity = new Media
        {
            AwsS3Path = s3Path
        };
        mediaEntity = await _mediaRepository.CreateAsync(mediaEntity);

        // Create post
        var post = new Post
        {
            Title = title,
            MediaId = mediaEntity.Id
        };

        post = await _postRepository.CreateAsync(post);
        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] Post post)
    {
        if (id != post.Id)
        {
            return BadRequest();
        }

        var existingPost = await _postRepository.GetByIdAsync(id);
        if (existingPost == null)
        {
            return NotFound();
        }

        await _postRepository.UpdateAsync(post);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        // Delete media from S3
        var media = await _mediaRepository.GetByIdAsync(post.MediaId);
        if (media != null)
        {
            await _s3Service.DeleteFileAsync(media.AwsS3Path);
            await _mediaRepository.DeleteAsync(media.Id);
        }

        await _postRepository.DeleteAsync(id);
        return NoContent();
    }
} 