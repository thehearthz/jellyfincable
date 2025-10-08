using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.CableCast.Models;
using Jellyfin.Plugin.CableCast.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.CableCast.Controllers;

/// <summary>
/// API controller for CableCast plugin functionality.
/// </summary>
[ApiController]
[Route("api/cablecast")]
[Authorize]
public class CableCastController : ControllerBase
{
    private readonly ILogger<CableCastController> _logger;
    private readonly ChannelManager _channelManager;
    private readonly ProgramScheduler _programScheduler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CableCastController"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="channelManager">The channel manager.</param>
    /// <param name="programScheduler">The program scheduler.</param>
    public CableCastController(
        ILogger<CableCastController> logger,
        ChannelManager channelManager,
        ProgramScheduler programScheduler)
    {
        _logger = logger;
        _channelManager = channelManager;
        _programScheduler = programScheduler;
    }

    /// <summary>
    /// Gets all available channels.
    /// </summary>
    /// <returns>A list of all cable channels.</returns>
    [HttpGet("channels")]
    public ActionResult<List<CableChannel>> GetChannels()
    {
        try
        {
            var channels = _channelManager.GetChannels();
            return Ok(channels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving channels");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets a specific channel by ID.
    /// </summary>
    /// <param name="channelId">The channel ID.</param>
    /// <returns>The requested channel.</returns>
    [HttpGet("channels/{channelId}")]
    public ActionResult<CableChannel> GetChannel([Required] string channelId)
    {
        try
        {
            var channel = _channelManager.GetChannel(channelId);
            if (channel == null)
            {
                return NotFound($"Channel with ID {channelId} not found");
            }

            return Ok(channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving channel {ChannelId}", channelId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Creates a new channel.
    /// </summary>
    /// <param name="channel">The channel data.</param>
    /// <returns>The created channel.</returns>
    [HttpPost("channels")]
    public async Task<ActionResult<CableChannel>> CreateChannel([Required][FromBody] CableChannel channel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdChannel = await _channelManager.CreateChannelAsync(channel);
            
            // Generate initial schedule
            var initialSchedule = await _programScheduler.GenerateScheduleAsync(createdChannel, DateTime.UtcNow, 24);
            createdChannel.ScheduledPrograms.AddRange(initialSchedule);
            await _channelManager.UpdateChannelAsync(createdChannel);
            
            return CreatedAtAction(nameof(GetChannel), new { channelId = createdChannel.Id }, createdChannel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating channel");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Updates an existing channel.
    /// </summary>
    /// <param name="channelId">The channel ID.</param>
    /// <param name="channel">The updated channel data.</param>
    /// <returns>The updated channel.</returns>
    [HttpPut("channels/{channelId}")]
    public async Task<ActionResult<CableChannel>> UpdateChannel([Required] string channelId, [Required][FromBody] CableChannel channel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingChannel = _channelManager.GetChannel(channelId);
            if (existingChannel == null)
            {
                return NotFound($"Channel with ID {channelId} not found");
            }

            channel.Id = channelId;
            await _channelManager.UpdateChannelAsync(channel);
            
            return Ok(channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating channel {ChannelId}", channelId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Deletes a channel.
    /// </summary>
    /// <param name="channelId">The channel ID.</param>
    /// <returns>A status indicating the result of the operation.</returns>
    [HttpDelete("channels/{channelId}")]
    public async Task<ActionResult> DeleteChannel([Required] string channelId)
    {
        try
        {
            var channel = _channelManager.GetChannel(channelId);
            if (channel == null)
            {
                return NotFound($"Channel with ID {channelId} not found");
            }

            await _channelManager.DeleteChannelAsync(channelId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting channel {ChannelId}", channelId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets the current program for a channel.
    /// </summary>
    /// <param name="channelId">The channel ID.</param>
    /// <returns>The current program.</returns>
    [HttpGet("channels/{channelId}/current")]
    public ActionResult<ScheduledProgram> GetCurrentProgram([Required] string channelId)
    {
        try
        {
            var channel = _channelManager.GetChannel(channelId);
            if (channel == null)
            {
                return NotFound($"Channel with ID {channelId} not found");
            }

            var currentProgram = _programScheduler.GetCurrentProgram(channel, DateTime.UtcNow);
            if (currentProgram == null)
            {
                return NotFound("No current program found");
            }

            return Ok(currentProgram);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current program for channel {ChannelId}", channelId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets the schedule for a channel.
    /// </summary>
    /// <param name="channelId">The channel ID.</param>
    /// <param name="startTime">The start time for the schedule (optional, defaults to now).</param>
    /// <param name="hours">The number of hours to retrieve (optional, defaults to 24).</param>
    /// <returns>The channel schedule.</returns>
    [HttpGet("channels/{channelId}/schedule")]
    public ActionResult<List<ScheduledProgram>> GetChannelSchedule(
        [Required] string channelId, 
        [FromQuery] DateTime? startTime = null, 
        [FromQuery] int hours = 24)
    {
        try
        {
            var channel = _channelManager.GetChannel(channelId);
            if (channel == null)
            {
                return NotFound($"Channel with ID {channelId} not found");
            }

            var start = startTime ?? DateTime.UtcNow;
            var end = start.AddHours(hours);
            
            var schedule = channel.ScheduledPrograms
                .Where(p => p.StartTime >= start && p.StartTime < end)
                .OrderBy(p => p.StartTime)
                .ToList();

            return Ok(schedule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedule for channel {ChannelId}", channelId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Regenerates the schedule for a channel.
    /// </summary>
    /// <param name="channelId">The channel ID.</param>
    /// <param name="hours">The number of hours to generate (optional, defaults to 24).</param>
    /// <returns>The regenerated schedule.</returns>
    [HttpPost("channels/{channelId}/regenerate-schedule")]
    public async Task<ActionResult<List<ScheduledProgram>>> RegenerateSchedule([Required] string channelId, [FromQuery] int hours = 24)
    {
        try
        {
            var channel = _channelManager.GetChannel(channelId);
            if (channel == null)
            {
                return NotFound($"Channel with ID {channelId} not found");
            }

            // Clear existing schedule
            channel.ScheduledPrograms.Clear();
            
            // Generate new schedule
            var newSchedule = await _programScheduler.GenerateScheduleAsync(channel, DateTime.UtcNow, hours);
            channel.ScheduledPrograms.AddRange(newSchedule);
            
            await _channelManager.UpdateChannelAsync(channel);
            
            return Ok(newSchedule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating schedule for channel {ChannelId}", channelId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets plugin configuration information.
    /// </summary>
    /// <returns>Plugin configuration details.</returns>
    [HttpGet("config")]
    public ActionResult GetConfiguration()
    {
        try
        {
            var config = Plugin.Instance?.Configuration;
            return Ok(new
            {
                EnableCommercials = config?.EnableCommercials ?? true,
                CommercialProbability = config?.CommercialProbability ?? 0.3,
                EnablePreRoll = config?.EnablePreRoll ?? true,
                MinContentDuration = config?.MinContentDuration ?? 5,
                MaxContentDuration = config?.MaxContentDuration ?? 180,
                ChannelBufferMinutes = config?.ChannelBufferMinutes ?? 60,
                UseScheduledProgramming = config?.UseScheduledProgramming ?? true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration");
            return StatusCode(500, "Internal server error");
        }
    }
}