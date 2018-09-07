﻿namespace MusicX.Services.DataProviders
{
    using System;
    using System.Net.Http;

    using MusicX.Common;
    using MusicX.Common.Models;

    public class Top40ChartsDataProvider
    {
        private const string SongInfoUrlFormat = "http://top40-charts.com/song.php?sid={0}";
        private const string SongVideoUrlFormat = "http://top40-charts.com/songs/media.php?sid={0}";
        private const string SongLyricsUrlFormat = "http://top40-charts.com/songs/lyrics.php?sid={0}";

        private readonly HttpClient http;

        public Top40ChartsDataProvider()
        {
            this.http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        }

        public SongAttributes GetSong(int id)
        {
            // Artist and song name
            var responseContent = this.ReadTextResponse(string.Format(SongInfoUrlFormat, id));
            var songTitle =
                responseContent.GetStringBetween(@"<td class=biggerblue height=30 valign=top>", "</td>")
                    .StripHtmlTags()
                    .Trim();

            var songArtist =
                responseContent.GetStringBetween(@"<td height=20 colspan=2 valign=top><font color=586973>", "</td>")
                    .StripHtmlTags()
                    .Trim();

            if (string.IsNullOrWhiteSpace(songTitle) || string.IsNullOrWhiteSpace(songArtist))
            {
                return null;
            }

            var attributes = new SongAttributes
                             {
                                 [SongMetadataType.Title] = songTitle,
                                 [SongMetadataType.Artist] = songArtist,
                             };

            // YouTube video
            var videoResponseContent = this.ReadTextResponse(string.Format(SongVideoUrlFormat, id));
            if (videoResponseContent.Contains(" src=\"http://www.youtube.com/embed/")
                && videoResponseContent.Contains("?autoplay=1&rel=0"))
            {
                var youTubeVideoId = videoResponseContent.GetStringBetween(
                    " src=\"http://www.youtube.com/embed/",
                    "?autoplay=1&rel=0");
                if (!string.IsNullOrWhiteSpace(youTubeVideoId))
                {
                    attributes[SongMetadataType.YouTubeVideoId] = youTubeVideoId;
                }
            }

            // Lyrics
            var lyricsResponseContent = this.ReadTextResponse(string.Format(SongLyricsUrlFormat, id));
            if (lyricsResponseContent.Contains("<table width=90% align=center><tr><td>")
                && lyricsResponseContent.Contains("</td></tr></table><img src=/images/spacer.gif height=1 width=1><BR><BR>"))
            {
                var lyrics = lyricsResponseContent.GetStringBetween(
                    "<table width=90% align=center><tr><td>",
                    "</td></tr></table><img src=/images/spacer.gif height=1 width=1><BR><BR>");
                if (!string.IsNullOrWhiteSpace(lyrics))
                {
                    attributes[SongMetadataType.Lyrics] = lyrics.Replace("\r", string.Empty)
                        .Replace("<br>\n", Environment.NewLine).StripHtmlTags().Replace("\\'", "'")
                        .Replace("\\\"", "\"").Trim();
                }
            }

            return attributes;
        }

        private string ReadTextResponse(string url)
        {
            var response = this.http.GetAsync(url).GetAwaiter().GetResult();
            var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return responseContent;
        }
    }
}
