using Google.Apis.YouTube.v3.Data;

namespace playlist_converter.Models.Youtube;

public class YoutubeSearchVideo
{
    public string kind { get; set; }
    public string etag { get; set; }
    public string nextPageToken { get; set; }
    public string regionCode { get; set; }
    public PageInfo pageInfo { get; set; }
    public List<SearchResultItem> items { get; set; }
}
public class PageInfo
{
    public int totalResults { get; set; }
    public int resultsPerPage { get; set; }
}

public class SearchResultItem
{
    public string kind { get; set; }
    public string etag { get; set; }
    public SearchResultId id { get; set; }
    public SearchResultSnippet snippet { get; set; }
}

public class SearchResultId
{
    public string kind { get; set; }
    public string videoId { get; set; }
}

public class SearchResultSnippet
{
    public string publishedAt { get; set; }
    public string channelId { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public Thumbnail thumbnails { get; set; }
    public string channelTitle { get; set; }
    public string liveBroadcastContent { get; set; }
    public string publishTime { get; set; }
}