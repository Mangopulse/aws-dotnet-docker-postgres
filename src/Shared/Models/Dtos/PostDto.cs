using System;

namespace Shared.Models.Dtos;

public class PostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? MediaId { get; set; }
    public string JsonMeta { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}

public class CreatePostDto
{
    public string Title { get; set; } = string.Empty;
    public Guid? MediaId { get; set; }
    public string JsonMeta { get; set; } = "{}";
}

public class UpdatePostDto
{
    public string Title { get; set; } = string.Empty;
    public Guid? MediaId { get; set; }
    public string JsonMeta { get; set; } = "{}";
} 