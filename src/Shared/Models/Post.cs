using System;
using System.Text.Json;
using System.Dynamic;

namespace Shared.Models;

public class Post
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? MediaId { get; set; }
    public int? PublicId { get; set; }
    private string _jsonMeta = "{}";
    public string JsonMeta
    {
        get => _jsonMeta;
        set => _jsonMeta = string.IsNullOrEmpty(value) ? "{}" : value;
    }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public void SetJsonMeta(object data)
    {
        var options = new JsonSerializerOptions { WriteIndented = false };
        JsonMeta = JsonSerializer.Serialize(data, options);
    }

    public T? GetJsonMeta<T>()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
            return JsonSerializer.Deserialize<T>(JsonMeta, options);
        }
        catch
        {
            return default;
        }
    }
} 