using System;
using System.Text.Json;

namespace Shared.Models;

public class Post
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid MediaId { get; set; }
    public int? PublicId { get; set; }
    public string JsonMeta { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 