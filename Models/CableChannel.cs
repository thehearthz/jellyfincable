using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.CableCast.Models;

/// <summary>
/// Represents a virtual cable TV channel.
/// </summary>
public class CableChannel
{
    /// <summary>
    /// Gets or sets the unique identifier for the channel.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the channel name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the channel number.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the channel description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the channel logo URL.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Gets or sets the library IDs that provide content for this channel.
    /// </summary>
    public List<string> LibraryIds { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets a value indicating whether this channel is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the programming type for this channel.
    /// </summary>
    public ProgrammingType ProgrammingType { get; set; } = ProgrammingType.Continuous;

    /// <summary>
    /// Gets or sets the scheduled programs for this channel.
    /// </summary>
    public List<ScheduledProgram> ScheduledPrograms { get; set; } = new List<ScheduledProgram>();

    /// <summary>
    /// Gets or sets the content filters for this channel.
    /// </summary>
    public ContentFilter? ContentFilter { get; set; }
}

/// <summary>
/// Defines the programming type for a channel.
/// </summary>
public enum ProgrammingType
{
    /// <summary>
    /// Continuous loop programming.
    /// </summary>
    Continuous,

    /// <summary>
    /// Schedule-based programming.
    /// </summary>
    Scheduled
}