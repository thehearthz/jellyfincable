using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.CableCast.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.CableCast.Services;

/// <summary>
/// Handles programming schedule generation and management.
/// </summary>
public class ProgramScheduler
{
    private readonly ILogger<ProgramScheduler> _logger;
    private readonly ILibraryManager _libraryManager;
    private readonly ChannelManager _channelManager;
    private readonly Random _random = new Random();

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgramScheduler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="channelManager">The channel manager.</param>
    public ProgramScheduler(
        ILogger<ProgramScheduler> logger,
        ILibraryManager libraryManager,
        ChannelManager channelManager)
    {
        _logger = logger;
        _libraryManager = libraryManager;
        _channelManager = channelManager;
    }

    /// <summary>
    /// Generates a programming schedule for a channel.
    /// </summary>
    /// <param name="channel">The channel to generate programming for.</param>
    /// <param name="startTime">The start time for the schedule.</param>
    /// <param name="duration">The duration of the schedule in hours.</param>
    /// <returns>A list of scheduled programs.</returns>
    public async Task<List<ScheduledProgram>> GenerateScheduleAsync(CableChannel channel, DateTime startTime, int duration)
    {
        var schedule = new List<ScheduledProgram>();
        var content = await _channelManager.GetChannelContentAsync(channel);
        
        if (!content.Any())
        {
            _logger.LogWarning("No content available for channel {ChannelName}", channel.Name);
            return schedule;
        }

        var currentTime = startTime;
        var endTime = startTime.AddHours(duration);
        var config = Plugin.Instance?.Configuration;

        while (currentTime < endTime)
        {
            // Add pre-roll if enabled
            if (config?.EnablePreRoll == true && ShouldInsertPreRoll())
            {
                var preRoll = await GetPreRollContentAsync();
                if (preRoll != null)
                {
                    var preRollProgram = CreateScheduledProgram(preRoll, currentTime, ProgramType.PreRoll, channel.Id);
                    schedule.Add(preRollProgram);
                    currentTime = preRollProgram.EndTime;
                }
            }

            // Add main content
            var mainContent = GetRandomContent(content);
            if (mainContent != null)
            {
                var mainProgram = CreateScheduledProgram(mainContent, currentTime, ProgramType.Content, channel.Id);
                schedule.Add(mainProgram);
                currentTime = mainProgram.EndTime;

                // Add commercial if enabled
                if (config?.EnableCommercials == true && ShouldInsertCommercial(config.CommercialProbability))
                {
                    var commercial = await GetCommercialContentAsync();
                    if (commercial != null)
                    {
                        var commercialProgram = CreateScheduledProgram(commercial, currentTime, ProgramType.Commercial, channel.Id);
                        schedule.Add(commercialProgram);
                        currentTime = commercialProgram.EndTime;
                    }
                }
            }
            else
            {
                // No more content available, break the loop
                break;
            }
        }

        _logger.LogInformation("Generated schedule with {ProgramCount} programs for channel {ChannelName}", 
            schedule.Count, channel.Name);
        
        return schedule;
    }

    /// <summary>
    /// Gets the current program for a channel at a specific time.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="currentTime">The current time.</param>
    /// <returns>The current scheduled program or null if none found.</returns>
    public ScheduledProgram? GetCurrentProgram(CableChannel channel, DateTime currentTime)
    {
        return channel.ScheduledPrograms
            .FirstOrDefault(p => p.StartTime <= currentTime && p.EndTime > currentTime);
    }

    /// <summary>
    /// Gets the next program for a channel after a specific time.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="currentTime">The current time.</param>
    /// <returns>The next scheduled program or null if none found.</returns>
    public ScheduledProgram? GetNextProgram(CableChannel channel, DateTime currentTime)
    {
        return channel.ScheduledPrograms
            .Where(p => p.StartTime > currentTime)
            .OrderBy(p => p.StartTime)
            .FirstOrDefault();
    }

    /// <summary>
    /// Updates the schedule for a channel to ensure continuous programming.
    /// </summary>
    /// <param name="channel">The channel to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateChannelScheduleAsync(CableChannel channel)
    {
        var now = DateTime.UtcNow;
        var lastProgram = channel.ScheduledPrograms
            .OrderBy(p => p.EndTime)
            .LastOrDefault();

        var scheduleEndTime = lastProgram?.EndTime ?? now;
        
        // If we need more programming (less than buffer time remaining)
        var bufferMinutes = Plugin.Instance?.Configuration?.ChannelBufferMinutes ?? 60;
        if (scheduleEndTime < now.AddMinutes(bufferMinutes))
        {
            var newSchedule = await GenerateScheduleAsync(channel, scheduleEndTime, 24); // Generate 24 hours ahead
            channel.ScheduledPrograms.AddRange(newSchedule);
            
            // Remove old programs (older than 1 hour)
            var cutoffTime = now.AddHours(-1);
            channel.ScheduledPrograms.RemoveAll(p => p.EndTime < cutoffTime);
            
            await _channelManager.UpdateChannelAsync(channel);
        }
    }

    /// <summary>
    /// Creates a scheduled program from a media item.
    /// </summary>
    /// <param name="item">The media item.</param>
    /// <param name="startTime">The start time for the program.</param>
    /// <param name="programType">The type of program.</param>
    /// <param name="channelId">The channel ID.</param>
    /// <returns>A scheduled program.</returns>
    private ScheduledProgram CreateScheduledProgram(BaseItem item, DateTime startTime, ProgramType programType, string channelId)
    {
        var duration = TimeSpan.FromTicks(item.RunTimeTicks ?? TimeSpan.FromMinutes(30).Ticks);
        
        return new ScheduledProgram
        {
            Id = Guid.NewGuid().ToString(),
            ItemId = item.Id.ToString(),
            StartTime = startTime,
            EndTime = startTime.Add(duration),
            Title = item.Name,
            Description = item.Overview,
            Type = programType,
            ChannelId = channelId,
            AllowCommercials = programType == ProgramType.Content,
            AllowPreRoll = programType == ProgramType.Content
        };
    }

    /// <summary>
    /// Gets random content from the available items.
    /// </summary>
    /// <param name="content">The available content.</param>
    /// <returns>A random content item or null if none available.</returns>
    private BaseItem? GetRandomContent(List<BaseItem> content)
    {
        if (!content.Any())
        {
            return null;
        }

        var config = Plugin.Instance?.Configuration;
        if (config != null)
        {
            // Filter by duration
            var filteredContent = content.Where(item =>
            {
                var durationMinutes = TimeSpan.FromTicks(item.RunTimeTicks ?? 0).TotalMinutes;
                return durationMinutes >= config.MinContentDuration && durationMinutes <= config.MaxContentDuration;
            }).ToList();

            if (filteredContent.Any())
            {
                content = filteredContent;
            }
        }

        return content[_random.Next(content.Count)];
    }

    /// <summary>
    /// Gets pre-roll content from the configured library.
    /// </summary>
    /// <returns>A pre-roll content item or null if none available.</returns>
    private async Task<BaseItem?> GetPreRollContentAsync()
    {
        var config = Plugin.Instance?.Configuration;
        if (string.IsNullOrEmpty(config?.PreRollLibraryPath))
        {
            return null;
        }

        // Implementation would get items from the pre-roll library
        // For now, return null as placeholder
        return await Task.FromResult<BaseItem?>(null);
    }

    /// <summary>
    /// Gets commercial content from the configured library.
    /// </summary>
    /// <returns>A commercial content item or null if none available.</returns>
    private async Task<BaseItem?> GetCommercialContentAsync()
    {
        var config = Plugin.Instance?.Configuration;
        if (string.IsNullOrEmpty(config?.CommercialLibraryPath))
        {
            return null;
        }

        // Implementation would get items from the commercial library
        // For now, return null as placeholder
        return await Task.FromResult<BaseItem?>(null);
    }

    /// <summary>
    /// Determines if a commercial should be inserted based on probability.
    /// </summary>
    /// <param name="probability">The probability of inserting a commercial (0.0 to 1.0).</param>
    /// <returns>True if a commercial should be inserted, false otherwise.</returns>
    private bool ShouldInsertCommercial(double probability)
    {
        return _random.NextDouble() < probability;
    }

    /// <summary>
    /// Determines if a pre-roll should be inserted.
    /// </summary>
    /// <returns>True if a pre-roll should be inserted, false otherwise.</returns>
    private bool ShouldInsertPreRoll()
    {
        // For now, insert pre-roll with 20% probability
        return _random.NextDouble() < 0.2;
    }
}