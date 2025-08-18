namespace playlist_converter.Models.Youtube;

public class YoutubePlaylistResponse
{
    public string kind { get; set; }
    public string etag { get; set; }
    public string id { get; set; } // This directly matches your JSON response
    public Snippet snippet { get; set; }
}

public class Snippet
{
    public DateTime PublishedAt { get; set; }
    public string ChannelId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Thumbnails Thumbnails { get; set; }
    public string ChannelTitle { get; set; }
    public Localized Localized { get; set; }
}

public class Thumbnails
{
    public Thumbnail Default { get; set; }
    public Thumbnail Medium { get; set; }
    public Thumbnail High { get; set; }
}

public class Thumbnail
{
    public string Url { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class Localized
{
    public string Title { get; set; }
    public string Description { get; set; }
}