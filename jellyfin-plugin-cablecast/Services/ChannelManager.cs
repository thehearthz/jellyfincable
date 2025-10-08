using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.CableCast.Models;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.CableCast.Services;

/// <summary>
/// Manages virtual cable TV channels.
/// </summary>
public class ChannelManager
{
    private readonly ILogger<ChannelManager> _logger;
    private readonly IApplicationPaths _applicationPaths;
    private readonly ILibraryManager _libraryManager;
    private readonly List<CableChannel> _channels = new List<CableChannel>();
    private readonly string _channelsFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelManager"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="applicationPaths">The application paths.</param>
    /// <param name="libraryManager">The library manager.</param>
    public ChannelManager(
        ILogger<ChannelManager> logger,
        IApplicationPaths applicationPaths,
        ILibraryManager libraryManager)
    {
        _logger = logger;
        _applicationPaths = applicationPaths;
        _libraryManager = libraryManager;
        _channelsFilePath = Path.Combine(_applicationPaths.PluginConfigurationsPath, "cablecast_channels.json");
    }

    /// <summary>
    /// Gets all available channels.
    /// </summary>
    /// <returns>A list of all cable channels.</returns>
    public List<CableChannel> GetChannels()
    {
        return _channels.ToList();
    }

    /// <summary>
    /// Gets a channel by its ID.
    /// </summary>
    /// <param name="channelId">The channel ID.</param>
    /// <returns>The channel if found, otherwise null.</returns>
    public CableChannel? GetChannel(string channelId)
    {
        return _channels.FirstOrDefault(c => c.Id == channelId);
    }

    /// <summary>
    /// Creates a new channel.
    /// </summary>
    /// <param name="channel">The channel to create.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<CableChannel> CreateChannelAsync(CableChannel channel)
    {
        if (string.IsNullOrEmpty(channel.Id))
        {
            channel.Id = Guid.NewGuid().ToString();
        }

        _channels.Add(channel);
        await SaveChannelsAsync();
        _logger.LogInformation("Created channel: {ChannelName} (ID: {ChannelId})", channel.Name, channel.Id);
        return channel;
    }

    /// <summary>
    /// Updates an existing channel.
    /// </summary>
    /// <param name="channel">The channel to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateChannelAsync(CableChannel channel)
    {
        var existingChannel = _channels.FirstOrDefault(c => c.Id == channel.Id);
        if (existingChannel != null)
        {
            var index = _channels.IndexOf(existingChannel);
            _channels[index] = channel;
            await SaveChannelsAsync();
            _logger.LogInformation("Updated channel: {ChannelName} (ID: {ChannelId})", channel.Name, channel.Id);
        }
    }

    /// <summary>
    /// Deletes a channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteChannelAsync(string channelId)
    {
        var channel = _channels.FirstOrDefault(c => c.Id == channelId);
        if (channel != null)
        {
            _channels.Remove(channel);
            await SaveChannelsAsync();
            _logger.LogInformation("Deleted channel: {ChannelName} (ID: {ChannelId})", channel.Name, channel.Id);
        }
    }

    /// <summary>
    /// Loads channels from storage.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LoadChannelsAsync()
    {
        try
        {
            if (File.Exists(_channelsFilePath))
            {
                var json = await File.ReadAllTextAsync(_channelsFilePath);
                var channels = JsonSerializer.Deserialize<List<CableChannel>>(json);
                if (channels != null)
                {
                    _channels.Clear();
                    _channels.AddRange(channels);
                    _logger.LogInformation("Loaded {ChannelCount} channels from storage", _channels.Count);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading channels from storage");
        }
    }

    /// <summary>
    /// Saves channels to storage.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SaveChannelsAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_channels, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_channelsFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving channels to storage");
        }
    }

    /// <summary>
    /// Gets content for a channel based on its library IDs and filters.
    /// </summary>
    /// <param name="channel">The channel to get content for.</param>
    /// <returns>A list of media items for the channel.</returns>
    public async Task<List<BaseItem>> GetChannelContentAsync(CableChannel channel)
    {
        var content = new List<BaseItem>();

        foreach (var libraryId in channel.LibraryIds)
        {
            var library = _libraryManager.GetItemById(libraryId);
            if (library != null)
            {
                var query = new InternalItemsQuery
                {
                    Parent = library,
                    IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Episode },
                    Recursive = true
                };

                var items = _libraryManager.GetItemList(query);
                content.AddRange(ApplyContentFilter(items, channel.ContentFilter));
            }
        }

        return content;
    }

    /// <summary>
    /// Applies content filters to a list of items.
    /// </summary>
    /// <param name="items">The items to filter.</param>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>The filtered list of items.</returns>
    private List<BaseItem> ApplyContentFilter(IEnumerable<BaseItem> items, ContentFilter? filter)
    {
        if (filter == null)
        {
            return items.ToList();
        }

        var filteredItems = items.AsEnumerable();

        // Filter by genres
        if (filter.IncludedGenres.Any())
        {
            filteredItems = filteredItems.Where(item => 
                item.Genres.Any(genre => filter.IncludedGenres.Contains(genre, StringComparer.OrdinalIgnoreCase)));
        }

        if (filter.ExcludedGenres.Any())
        {
            filteredItems = filteredItems.Where(item => 
                !item.Genres.Any(genre => filter.ExcludedGenres.Contains(genre, StringComparer.OrdinalIgnoreCase)));
        }

        // Filter by content type
        if (filter.IncludedContentTypes.Any())
        {
            filteredItems = filteredItems.Where(item => 
                filter.IncludedContentTypes.Contains(item.GetType().Name, StringComparer.OrdinalIgnoreCase));
        }

        if (filter.ExcludedContentTypes.Any())
        {
            filteredItems = filteredItems.Where(item => 
                !filter.ExcludedContentTypes.Contains(item.GetType().Name, StringComparer.OrdinalIgnoreCase));
        }

        // Filter by release year
        if (filter.MinReleaseYear.HasValue)
        {
            filteredItems = filteredItems.Where(item => 
                item.ProductionYear >= filter.MinReleaseYear.Value);
        }

        if (filter.MaxReleaseYear.HasValue)
        {
            filteredItems = filteredItems.Where(item => 
                item.ProductionYear <= filter.MaxReleaseYear.Value);
        }

        return filteredItems.ToList();
    }
}