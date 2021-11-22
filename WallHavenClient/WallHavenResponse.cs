using Newtonsoft.Json;
using System.Collections.Generic;

namespace WallHaven.WallHavenClient
{
    public class WallHavenResponse
    {
        [JsonProperty("data")] public List<Data> Data { get; set; } = new List<Data>();
        [JsonProperty("meta")] public Meta Meta { get; set; } = new Meta();
    }

    public class Meta
    {
        [JsonProperty("current_page")] public int CurrentPage { get; set; }

        [JsonProperty("last_page")] public int LastPage { get; set; }

        [JsonProperty("per_page")] public string PerPageStr { get; set; } = string.Empty;

        public int PerPage
        {
            get
            {
                int.TryParse(PerPageStr, out int perpage);
                return perpage;
            }
        }

        [JsonProperty("total")] public int Total { get; set; }

        [JsonProperty("query")] public object Query { get; set; } = new object();

        [JsonProperty("seed")] public object Seed { get; set; } = new object();
    }

    public class Avatar
    {
        [JsonProperty("200px")] public string PX200 { get; set; } = string.Empty;

        [JsonProperty("128px")] public string PX128 { get; set; } = string.Empty;

        [JsonProperty("32px")] public string PX32 { get; set; } = string.Empty;

        [JsonProperty("20px")] public string PX20 { get; set; } = string.Empty;
    }

    public class Uploader
    {
        [JsonProperty("username")] public string Username { get; set; } = string.Empty;

        [JsonProperty("group")] public string Group { get; set; } = string.Empty;

        [JsonProperty("avatar")] public Avatar Avatar { get; set; } = new Avatar();
    }

    public class Thumbs
    {
        [JsonProperty("large")] public string Large { get; set; } = string.Empty;

        [JsonProperty("original")] public string Original { get; set; } = string.Empty;

        [JsonProperty("small")] public string Small { get; set; } = string.Empty;
    }

    public class Tag
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; } = string.Empty;

        [JsonProperty("alias")] public string Alias { get; set; } = string.Empty;

        [JsonProperty("category_id")] public int CategoryId { get; set; }

        [JsonProperty("category")] public string Category { get; set; } = string.Empty;

        [JsonProperty("purity")] public string Purity { get; set; } = string.Empty;

        [JsonProperty("created_at")] public string CreatedAt { get; set; } = string.Empty;
    }

    public class Data
    {
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;

        [JsonProperty("url")] public string Url { get; set; } = string.Empty;

        [JsonProperty("short_url")] public string ShortUrl { get; set; } = string.Empty;

        [JsonProperty("uploader")] public Uploader Uploader { get; set; } = new Uploader();

        [JsonProperty("views")] public int Views { get; set; }

        [JsonProperty("favorites")] public int Favorites { get; set; }

        [JsonProperty("source")] public string Source { get; set; } = string.Empty;

        [JsonProperty("purity")] public string Purity { get; set; } = string.Empty;

        [JsonProperty("category")] public string Category { get; set; } = string.Empty;

        [JsonProperty("dimension_x")] public int DimensionX { get; set; }

        [JsonProperty("dimension_y")] public int DimensionY { get; set; }

        [JsonProperty("resolution")] public string Resolution { get; set; } = string.Empty;

        [JsonProperty("ratio")] public string Ratio { get; set; } = string.Empty;

        [JsonProperty("file_size")] public int FileSize { get; set; }

        [JsonProperty("file_type")] public string FileType { get; set; } = string.Empty;

        [JsonProperty("created_at")] public string CreatedAt { get; set; } = string.Empty;

        [JsonProperty("colors")] public List<string> Colors { get; set; } = new List<string>();

        [JsonProperty("path")] public string Path { get; set; } = string.Empty;

        [JsonProperty("thumbs")] public Thumbs Thumbs { get; set; } = new Thumbs();
        public string ThumbsUrl
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Thumbs.Small)) return Thumbs.Small;
                if (!string.IsNullOrWhiteSpace(Thumbs.Original)) return Thumbs.Original;
                if (!string.IsNullOrWhiteSpace(Thumbs.Large)) return Thumbs.Large;
                return Path;
            }
        }

        [JsonProperty("tags")] public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}