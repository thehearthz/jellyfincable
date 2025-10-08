using System;

namespace Jellyfin.Plugin.CableCast.Models;

/// <summary>
/// Represents a scheduled program entry.
/// </summary>
public class ScheduledProgram
{
    /// <summary>
    /// Gets or sets the unique identifier for this scheduled program.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Jellyfin item ID for the content.
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time for this program.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time for this program.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Gets or sets the title of the program.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the program.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether commercials should be inserted for this program.
    /// </summary>
    public bool AllowCommercials { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether pre-roll content should be played for this program.
    /// </summary>
    public bool AllowPreRoll { get; set; } = true;

    /// <summary>
    /// Gets or sets the channel ID this program belongs to.
    /// </summary>
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the program type.
    /// </summary>
    public ProgramType Type { get; set; } = ProgramType.Content;
}

/// <summary>
/// Defines the type of program.
/// </summary>
public enum ProgramType
{
    /// <summary>
    /// Regular content (movies, TV episodes).
    /// </summary>
    Content,

    /// <summary>
    /// Commercial content.
    /// </summary>
    Commercial,

    /// <summary>
    /// Pre-roll content.
    /// </summary>
    PreRoll,

    /// <summary>
    /// Filler content.
    /// </summary>
    Filler
}