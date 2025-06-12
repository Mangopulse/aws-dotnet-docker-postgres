using System;

namespace Shared.Models;

public class Media
{
    public Guid Id { get; set; }
    public string AwsS3Path { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 