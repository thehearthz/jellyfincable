using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.CableCast.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether commercials should be inserted between content.
    /// </summary>
    public bool EnableCommercials { get; set; } = true;

    /// <summary>
    /// Gets or sets the commercial insert probability (0.0 to 1.0).
    /// </summary>
    public double CommercialProbability { get; set; } = 0.3;

    /// <summary>
    /// Gets or sets a value indicating whether pre-roll content should be played.
    /// </summary>
    public bool EnablePreRoll { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum duration in minutes for content to be included in programming.
    /// </summary>
    public int MinContentDuration { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum duration in minutes for content to be included in programming.
    /// </summary>
    public int MaxContentDuration { get; set; } = 180;

    /// <summary>
    /// Gets or sets the channel buffer size in minutes.
    /// </summary>
    public int ChannelBufferMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets a value indicating whether to use schedule-based programming.
    /// </summary>
    public bool UseScheduledProgramming { get; set; } = true;

    /// <summary>
    /// Gets or sets the commercial library path for commercial content.
    /// </summary>
    public string? CommercialLibraryPath { get; set; }

    /// <summary>
    /// Gets or sets the pre-roll library path for pre-roll content.
    /// </summary>
    public string? PreRollLibraryPath { get; set; }
}