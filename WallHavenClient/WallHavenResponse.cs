using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WallHaven.WallHavenClient
{
    public class WallHavenResponse
    {
        [JsonPropertyName("data")] public List<Data> Data { get; set; } = new List<Data>();
        [JsonPropertyName("meta")] public Meta Meta { get; set; } = new Meta();
    }

    public class Meta
    {
        [JsonPropertyName("current_page")] public int CurrentPage { get; set; }

        [JsonPropertyName("last_page")] public int LastPage { get; set; }

        [JsonPropertyName("per_page")] public string PerPageStr { get; set; } = string.Empty;

        public int PerPage
        {
            get
            {
                int.TryParse(PerPageStr, out int perpage);
                return perpage;
            }
        }

        [JsonPropertyName("total")] public int Total { get; set; }

        [JsonPropertyName("query")] public object Query { get; set; } = new object();

        [JsonPropertyName("seed")] public object Seed { get; set; } = new object();
    }

    public class Avatar
    {
        [JsonPropertyName("200px")] public string PX200 { get; set; } = string.Empty;

        [JsonPropertyName("128px")] public string PX128 { get; set; } = string.Empty;

        [JsonPropertyName("32px")] public string PX32 { get; set; } = string.Empty;

        [JsonPropertyName("20px")] public string PX20 { get; set; } = string.Empty;
    }

    public class Uploader
    {
        [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;

        [JsonPropertyName("group")] public string Group { get; set; } = string.Empty;

        [JsonPropertyName("avatar")] public Avatar Avatar { get; set; } = new Avatar();
    }

    public class Thumbs
    {
        [JsonPropertyName("large")] public string Large { get; set; } = string.Empty;

        [JsonPropertyName("original")] public string Original { get; set; } = string.Empty;

        [JsonPropertyName("small")] public string Small { get; set; } = string.Empty;
    }

    public class Tag
    {
        [JsonPropertyName("id")] public int Id { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

        [JsonPropertyName("alias")] public string Alias { get; set; } = string.Empty;

        [JsonPropertyName("category_id")] public int CategoryId { get; set; }

        [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;

        [JsonPropertyName("purity")] public string Purity { get; set; } = string.Empty;

        [JsonPropertyName("created_at")] public string CreatedAt { get; set; } = string.Empty;
    }

    public class Data
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

        [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;

        [JsonPropertyName("short_url")] public string ShortUrl { get; set; } = string.Empty;

        [JsonPropertyName("uploader")] public Uploader Uploader { get; set; } = new Uploader();

        [JsonPropertyName("views")] public int Views { get; set; }

        [JsonPropertyName("favorites")] public int Favorites { get; set; }

        [JsonPropertyName("source")] public string Source { get; set; } = string.Empty;

        [JsonPropertyName("purity")] public string Purity { get; set; } = string.Empty;

        [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;

        [JsonPropertyName("dimension_x")] public int DimensionX { get; set; }

        [JsonPropertyName("dimension_y")] public int DimensionY { get; set; }

        [JsonPropertyName("resolution")] public string Resolution { get; set; } = string.Empty;

        [JsonPropertyName("ratio")] public string Ratio { get; set; } = string.Empty;

        [JsonPropertyName("file_size")] public int FileSize { get; set; }

        [JsonPropertyName("file_type")] public string FileType { get; set; } = string.Empty;

        [JsonPropertyName("created_at")] public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("colors")] public List<string> Colors { get; set; } = new List<string>();

        [JsonPropertyName("path")] public string Path { get; set; } = string.Empty;

        [JsonPropertyName("thumbs")] public Thumbs Thumbs { get; set; } = new Thumbs();

        [JsonPropertyName("tags")] public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}