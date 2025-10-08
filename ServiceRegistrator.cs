using Jellyfin.Plugin.CableCast.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.CableCast;

/// <summary>
/// Service registrator for CableCast plugin services.
/// </summary>
public class ServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<ChannelManager>();
        serviceCollection.AddSingleton<ProgramScheduler>();
        serviceCollection.AddHostedService<ChannelHostedService>();
    }
}