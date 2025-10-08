using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.CableCast.Services;

/// <summary>
/// Background service for managing channel operations.
/// </summary>
public class ChannelHostedService : IHostedService
{
    private readonly ILogger<ChannelHostedService> _logger;
    private readonly ChannelManager _channelManager;
    private readonly ProgramScheduler _programScheduler;
    private Timer? _scheduleUpdateTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelHostedService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="channelManager">The channel manager.</param>
    /// <param name="programScheduler">The program scheduler.</param>
    public ChannelHostedService(
        ILogger<ChannelHostedService> logger,
        ChannelManager channelManager,
        ProgramScheduler programScheduler)
    {
        _logger = logger;
        _channelManager = channelManager;
        _programScheduler = programScheduler;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CableCast Channel Hosted Service starting");
        
        // Load existing channels
        await _channelManager.LoadChannelsAsync();
        
        // Set up timer to update schedules every 30 minutes
        _scheduleUpdateTimer = new Timer(UpdateChannelSchedules, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
        
        _logger.LogInformation("CableCast Channel Hosted Service started");
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CableCast Channel Hosted Service stopping");
        
        _scheduleUpdateTimer?.Change(Timeout.Infinite, 0);
        _scheduleUpdateTimer?.Dispose();
        
        _logger.LogInformation("CableCast Channel Hosted Service stopped");
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates schedules for all channels.
    /// </summary>
    /// <param name="state">Timer state (unused).</param>
    private async void UpdateChannelSchedules(object? state)
    {
        try
        {
            _logger.LogDebug("Updating channel schedules");
            
            var channels = _channelManager.GetChannels();
            foreach (var channel in channels)
            {
                if (channel.IsEnabled)
                {
                    await _programScheduler.UpdateChannelScheduleAsync(channel);
                }
            }
            
            _logger.LogDebug("Channel schedules updated for {ChannelCount} channels", channels.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating channel schedules");
        }
    }
}