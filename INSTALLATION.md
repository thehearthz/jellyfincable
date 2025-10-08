# CableCast Plugin Installation Guide

## Overview

CableCast is a Jellyfin plugin that creates virtual cable TV channels with continuous programming from your media library. This guide covers installation methods and setup.

## Installation Methods

### Method 1: Custom Repository (Recommended)

1. **Add Custom Repository**
   - Open Jellyfin Admin Dashboard
   - Go to **Plugins** > **Repositories**
   - Click **Add Repository**
   - Enter the repository URL: `https://your-domain.com/repository/manifest.json`
   - Name: `CableCast Repository`

2. **Install Plugin**
   - Go to **Plugins** > **Catalog**
   - Find **CableCast** in the list
   - Click **Install**
   - Restart Jellyfin Server when prompted

### Method 2: Manual Installation

1. **Download Plugin**
   - Download `Jellyfin.Plugin.CableCast.dll` from the releases

2. **Install Plugin**
   ```bash
   # Create plugin directory
   mkdir -p /var/lib/jellyfin/plugins/CableCast/

   # Copy plugin DLL
   cp Jellyfin.Plugin.CableCast.dll /var/lib/jellyfin/plugins/CableCast/

   # Set correct permissions
   chown -R jellyfin:jellyfin /var/lib/jellyfin/plugins/CableCast/
   chmod 644 /var/lib/jellyfin/plugins/CableCast/Jellyfin.Plugin.CableCast.dll
   ```

3. **Restart Jellyfin**
   ```bash
   sudo systemctl restart jellyfin
   # or
   sudo service jellyfin restart
   ```

## Verification

1. **Check Plugin Status**
   - Open Jellyfin Admin Dashboard
   - Go to **Plugins** > **My Plugins**
   - Verify **CableCast** appears in the list

2. **Access Configuration**
   - Click on **CableCast** in the plugin list
   - Configure settings as needed
   - Click **Save**

## Configuration

### Basic Settings

1. **Commercial Settings**
   - Enable/disable commercial insertion
   - Set commercial probability (0.0 - 1.0)
   - Configure commercial library path

2. **Pre-Roll Settings**
   - Enable/disable pre-roll content
   - Set pre-roll library path

3. **Content Settings**
   - Set minimum/maximum content duration
   - Configure programming type

4. **Channel Management**
   - Use API endpoints to create/manage channels
   - Access via `/api/cablecast/` endpoints

## API Usage

### Authentication
All API calls require Jellyfin authentication headers.

### Create Channel Example
```bash
curl -X POST "http://jellyfin-server:8096/api/cablecast/channels" \
  -H "Authorization: MediaBrowser Token=YOUR_API_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Classic Movies",
    "number": 101,
    "description": "24/7 Classic Movies",
    "libraryIds": ["YOUR_LIBRARY_ID"],
    "isEnabled": true,
    "programmingType": 0
  }'
```

### Get Channels
```bash
curl -X GET "http://jellyfin-server:8096/api/cablecast/channels" \
  -H "Authorization: MediaBrowser Token=YOUR_API_TOKEN"
```

### Get Current Program
```bash
curl -X GET "http://jellyfin-server:8096/api/cablecast/channels/CHANNEL_ID/current" \
  -H "Authorization: MediaBrowser Token=YOUR_API_TOKEN"
```

## Troubleshooting

### Plugin Not Loading
1. Check Jellyfin logs: `/var/log/jellyfin/jellyfin.log`
2. Verify plugin file permissions
3. Ensure Jellyfin version compatibility (10.9.0+)

### API Not Working
1. Verify authentication token
2. Check that Jellyfin is running on correct port
3. Ensure API endpoints are prefixed with `/api/`

### No Content in Channels
1. Verify library IDs are correct
2. Check content filters aren't too restrictive
3. Ensure libraries contain compatible content (Movies/Episodes)

### Background Service Issues
1. Check Jellyfin logs for errors
2. Verify channel configuration
3. Restart Jellyfin service

## Uninstallation

### Manual Removal
```bash
# Remove plugin directory
rm -rf /var/lib/jellyfin/plugins/CableCast/

# Restart Jellyfin
sudo systemctl restart jellyfin
```

### Repository Removal
1. Go to **Plugins** > **My Plugins**
2. Find **CableCast** and click **Uninstall**
3. Remove custom repository if no longer needed
4. Restart Jellyfin

## Support

For issues or questions:
- Check the plugin logs in Jellyfin Admin Dashboard
- Review API documentation in README.md
- Report bugs on the project repository

## File Locations

- **Plugin Directory**: `/var/lib/jellyfin/plugins/CableCast/`
- **Configuration**: `/var/lib/jellyfin/config/plugins/CableCast/`
- **Logs**: `/var/log/jellyfin/jellyfin.log`
- **Channel Data**: `/var/lib/jellyfin/config/plugins/cablecast_channels.json`

## Requirements

- **Jellyfin Server**: 10.9.0 or later
- **Platform**: Linux, Windows, macOS
- **.NET Runtime**: 8.0 (included with Jellyfin)
- **Storage**: Minimal (plugin is ~61KB)
- **Memory**: Minimal overhead for background scheduling