using System.Collections.Generic;

namespace Jellyfin.Plugin.CableCast.Models;

/// <summary>
/// Represents content filtering options for a channel.
/// </summary>
public class ContentFilter
{
    /// <summary>
    /// Gets or sets the included genres.
    /// </summary>
    public List<string> IncludedGenres { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the excluded genres.
    /// </summary>
    public List<string> ExcludedGenres { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the minimum rating (e.g., "G", "PG", "PG-13", "R").
    /// </summary>
    public string? MinRating { get; set; }

    /// <summary>
    /// Gets or sets the maximum rating.
    /// </summary>
    public string? MaxRating { get; set; }

    /// <summary>
    /// Gets or sets the minimum release year.
    /// </summary>
    public int? MinReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets the maximum release year.
    /// </summary>
    public int? MaxReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets the included content types (Movie, Episode, etc.).
    /// </summary>
    public List<string> IncludedContentTypes { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the excluded content types.
    /// </summary>
    public List<string> ExcludedContentTypes { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets a value indicating whether to include adult content.
    /// </summary>
    public bool IncludeAdultContent { get; set; } = false;
}