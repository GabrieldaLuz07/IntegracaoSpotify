using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IntegracaoSpotify;
using SpotifyAPI.Web;

namespace IntegracaoSpotify
{
    class SpotifyDownload : IMusicDownloader
    {
        private SpotifyClient spotifyClient;
        private YouTubeDownload youtubeDownloader;

        public SpotifyDownload(string clientId, string clientSecret)
        {
            var config = SpotifyClientConfig.CreateDefault();
            var request = new ClientCredentialsRequest(clientId, clientSecret);
            var response = new OAuthClient(config).RequestToken(request).Result;
            spotifyClient = new SpotifyClient(config.WithToken(response.AccessToken));

            youtubeDownloader = new YouTubeDownload();
        }

        public async Task DownloadAsync(string input, string directory)
        {
            if (IsSpotifyLink(input))
            {
                if (IsSpotifyTrackLink(input))
                {
                    var trackId = ExtractSpotifyId(input);
                    var track = await spotifyClient.Tracks.Get(trackId);
                    var query = $"{track.Name} {track.Artists[0].Name}";
                    Console.WriteLine($"Baixando música do Spotify: {track.Name} - {track.Artists[0].Name}");
                    await youtubeDownloader.DownloadAsync(SanitizeFileName(query), directory);
                }
                else if (IsSpotifyPlaylistLink(input))
                {
                    var playlistId = ExtractSpotifyId(input);
                    var playlist = await spotifyClient.Playlists.Get(playlistId);
                    Console.WriteLine($"Playlist encontrada: {playlist.Name}");

                    var playlistTracks = await spotifyClient.Playlists.GetItems(playlistId);

                    foreach (var item in playlistTracks.Items)
                    {
                        if (item.Track is FullTrack track)
                        {
                            var query = $"{track.Name} {track.Artists[0].Name}";
                            Console.WriteLine($"Baixando música da playlist: {track.Name} - {track.Artists[0].Name}");
                            await youtubeDownloader.DownloadAsync(SanitizeFileName(query), directory);
                        }
                    }
                    Console.WriteLine($"Todas as músicas da playlist '{playlist.Name}' foram baixadas com sucesso.");
                }
            }
            else
            {
                await SearchAndDownload(input, directory);
            }
        }

        private string SanitizeFileName(string fileName)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars());
            return Regex.Replace(fileName, $"[{Regex.Escape(invalidChars)}]", "_");
        }

        private bool IsSpotifyLink(string input)
        {
            return input.Contains("open.spotify.com");
        }

        private bool IsSpotifyTrackLink(string input)
        {
            return input.Contains("/track/");
        }

        private bool IsSpotifyPlaylistLink(string input)
        {
            return input.Contains("/playlist/");
        }

        private string ExtractSpotifyId(string url)
        {
            var match = Regex.Match(url, @"(?:track|playlist)/([a-zA-Z0-9]+)");
            return match.Groups[1].Value;
        }

        private async Task SearchAndDownload(string name, string directory)
        {
            Console.WriteLine($"Procurando no Spotify: {name}");

            var searchRequest = new SearchRequest(SearchRequest.Types.Track | SearchRequest.Types.Playlist, name);
            var searchResult = await spotifyClient.Search.Item(searchRequest);

            if (searchResult.Playlists.Items.Count > 0)
            {
                var playlist = searchResult.Playlists.Items[0];
                Console.WriteLine($"Playlist encontrada: {playlist.Name}");

                var playlistTracks = await spotifyClient.Playlists.GetItems(playlist.Id);

                foreach (var item in playlistTracks.Items)
                {
                    if (item.Track is FullTrack track)
                    {
                        var query = $"{track.Name} {track.Artists[0].Name}";
                        Console.WriteLine($"Baixando música da playlist: {track.Name} - {track.Artists[0].Name}");
                        await youtubeDownloader.DownloadAsync(SanitizeFileName(query), directory);
                    }
                }
                Console.WriteLine($"Todas as músicas da playlist '{playlist.Name}' foram baixadas com sucesso.");
            }
            else if (searchResult.Tracks.Items.Count > 0)
            {
                var track = searchResult.Tracks.Items[0];
                var query = $"{track.Name} {track.Artists[0].Name}";

                Console.WriteLine($"Música encontrada no Spotify: {track.Name} - {track.Artists[0].Name}");
                await youtubeDownloader.DownloadAsync(SanitizeFileName(query), directory);
            }
            else
            {
                Console.WriteLine($"Nenhum resultado encontrado no Spotify para {name}");
            }
        }
    }
}
