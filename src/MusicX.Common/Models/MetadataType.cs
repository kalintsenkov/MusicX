﻿namespace MusicX.Common.Models
{
    // Follow name convention in https://en.wikipedia.org/wiki/ID3
    public enum MetadataType : short
    {
        Id = 1,
        Title = 2,
        Artist = 3,

        // Information
        Genre = 11,

        // Additional Information
        Year = 101,

        // Album information
        AlbumName = 1001,

        // Images
        ImageUrl = 2001,

        // Lyrics
        Lyrics = 3001,

        // Playable media
        YouTubeVideoUrl = 10001,
    }
}