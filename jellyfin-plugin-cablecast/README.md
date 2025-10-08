# CableCast - Jellyfin Cable TV Channel Plugin

CableCast is a Jellyfin plugin that creates virtual cable TV channels from your media library. Experience continuous programming with features like commercial insertion, pre-roll content, and scheduled or continuous playback.

## Features

### Core Functionality
- **Virtual Cable Channels**: Create multiple virtual TV channels from your Jellyfin libraries
- **Continuous Programming**: 24/7 programming with seamless transitions between content
- **Schedule-Based Programming**: Time-based scheduling for specific content at specific times
- **Commercial Insertion**: Insert commercials between content with configurable probability
- **Pre-Roll Content**: Play trailers or announcements before main content
- **Content Filtering**: Filter content by genre, rating, release year, and content type

### Advanced Features
- **Multiple Programming Types**: Choose between continuous loop or scheduled programming per channel
- **Smart Scheduling**: Automatic schedule generation and maintenance
- **RESTful API**: Full API access for creating and managing channels programmatically
- **Background Services**: Automatic schedule updates and channel maintenance
- **Configurable Settings**: Extensive configuration options for all features

## Installation

### Prerequisites
- Jellyfin Server 10.9.0 or later
- .NET 8.0 runtime

### From Custom Repository (Recommended)

1. In Jellyfin Admin Dashboard, go to **Plugins** > **Repositories**
2. Add a new repository with URL: `https://your-custom-repo-url/manifest.json`
3. Browse the **Catalog** and install **CableCast**
4. Restart Jellyfin Server

### Manual Installation

1. Download the latest `Jellyfin.Plugin.CableCast.dll` from the releases
2. Create a folder named `CableCast` in your Jellyfin `plugins` directory
3. Copy the DLL file into the `CableCast` folder
4. Restart Jellyfin Server

## Configuration

### Plugin Settings

Access plugin settings through: **Admin Dashboard** > **Plugins** > **CableCast**

#### Commercial Settings
- **Enable Commercial Insertion**: Toggle commercial insertion on/off
- **Commercial Probability**: Set the probability of inserting commercials (0.0 = never, 1.0 = always)
- **Commercial Library Path**: Path to your commercial content library

#### Pre-Roll Settings
- **Enable Pre-Roll Content**: Toggle pre-roll content on/off
- **Pre-Roll Library Path**: Path to your pre-roll content library

#### Content Settings
- **Minimum Content Duration**: Minimum duration (in minutes) for content inclusion
- **Maximum Content Duration**: Maximum duration (in minutes) for content inclusion

#### Programming Settings
- **Use Schedule-Based Programming**: Enable time-based scheduling vs continuous loop
- **Channel Buffer**: How far ahead to maintain programming schedules (recommended: 60+ minutes)

## API Usage

CableCast provides a comprehensive REST API for channel management.

### Base URL
All API endpoints are prefixed with `/api/cablecast`

### Authentication
All API calls require Jellyfin authentication headers.

### Endpoints

#### Channel Management

**List All Channels**
```
GET /api/cablecast/channels
```

**Get Specific Channel**
```
GET /api/cablecast/channels/{channelId}
```

**Create New Channel**
```
POST /api/cablecast/channels
Content-Type: application/json

{
  "name": "My Movie Channel",
  "number": 101,
  "description": "24/7 Movies",
  "libraryIds": ["library-id-1", "library-id-2"],
  "isEnabled": true,
  "programmingType": 0, // 0 = Continuous, 1 = Scheduled
  "contentFilter": {
    "includedGenres": ["Action", "Comedy"],
    "minRating": "PG",
    "maxRating": "R",
    "minReleaseYear": 2000,
    "includeAdultContent": false
  }
}
```

**Update Channel**
```
PUT /api/cablecast/channels/{channelId}
Content-Type: application/json

{
  // Updated channel data
}
```

**Delete Channel**
```
DELETE /api/cablecast/channels/{channelId}
```

#### Programming

**Get Current Program**
```
GET /api/cablecast/channels/{channelId}/current
```

**Get Channel Schedule**
```
GET /api/cablecast/channels/{channelId}/schedule?startTime=2024-01-01T00:00:00Z&hours=24
```

**Regenerate Schedule**
```
POST /api/cablecast/channels/{channelId}/regenerate-schedule?hours=24
```

#### Configuration

**Get Plugin Configuration**
```
GET /api/cablecast/config
```

## Channel Creation Examples

### Basic Movie Channel
```json
{
  "name": "Classic Movies",
  "number": 101,
  "description": "Classic movies from the golden age of cinema",
  "libraryIds": ["movies-library-id"],
  "isEnabled": true,
  "programmingType": 0,
  "contentFilter": {
    "maxReleaseYear": 1980,
    "includedContentTypes": ["Movie"]
  }
}
```

### Kids TV Channel
```json
{
  "name": "Kids Zone",
  "number": 102,
  "description": "Safe content for children",
  "libraryIds": ["tv-shows-library-id", "kids-movies-library-id"],
  "isEnabled": true,
  "programmingType": 0,
  "contentFilter": {
    "maxRating": "G",
    "includedGenres": ["Animation", "Family"],
    "includeAdultContent": false
  }
}
```

### News & Documentary Channel
```json
{
  "name": "InfoChannel",
  "number": 103,
  "description": "News and documentaries",
  "libraryIds": ["documentaries-library-id"],
  "isEnabled": true,
  "programmingType": 1,
  "contentFilter": {
    "includedGenres": ["Documentary", "News"]
  }
}
```

## Content Organization

### Recommended Library Structure
```
Media Libraries/
├── Movies/
│   ├── Action Movies/
│   ├── Comedy Movies/
│   └── Classic Movies/
├── TV Shows/
│   ├── Drama Series/
│   ├── Comedy Series/
│   └── Kids Shows/
├── Commercials/
│   ├── Fake Commercials/
│   ├── Movie Trailers/
│   └── PSAs/
└── Pre-Roll/
    ├── Network Bumpers/
    ├── Coming Up Next/
    └── Channel Idents/
```

### Content Preparation Tips
1. **Organize by Genre**: Create separate libraries or folders for different content types
2. **Commercial Content**: Collect or create short commercial content (30 seconds - 2 minutes)
3. **Pre-Roll Content**: Prepare network identifiers, trailers, or announcements
4. **Metadata**: Ensure all content has proper metadata (genres, ratings, release years)

## Troubleshooting

### Common Issues

**Channels Not Appearing**
- Check that libraries are properly assigned to channels
- Verify content exists in the assigned libraries
- Ensure channel is enabled

**No Programming Generated**
- Check content duration filters (min/max duration)
- Verify content filters aren't too restrictive
- Check that libraries contain compatible content types

**Commercials Not Playing**
- Verify commercial library path is set correctly
- Check commercial probability setting (should be > 0.0)
- Ensure commercial library contains content

**Schedule Not Updating**
- Check background service is running
- Verify channel buffer time is adequate
- Look for errors in Jellyfin logs

### Logging

Enable debug logging for troubleshooting:
1. Go to **Admin Dashboard** > **Logs**
2. Set log level to **Debug**
3. Look for entries prefixed with `CableCast`

## Development

### Building from Source

Requirements:
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

```bash
git clone https://github.com/your-repo/jellyfin-plugin-cablecast.git
cd jellyfin-plugin-cablecast
dotnet build
```

### Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the GPL-3.0 License - see the [LICENSE](LICENSE) file for details.

## Support

- **Issues**: Report bugs on GitHub Issues
- **Discussions**: Join the community discussions
- **Documentation**: Check the wiki for additional help

## Changelog

### Version 1.0.0 (Initial Release)
- Virtual cable TV channel creation
- Continuous and scheduled programming modes
- Commercial insertion with configurable probability
- Pre-roll content support
- Content filtering by multiple criteria
- RESTful API for channel management
- Background service for automatic scheduling
- Web-based configuration interface