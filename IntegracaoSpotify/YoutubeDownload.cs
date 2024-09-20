using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Playlists;
using IntegracaoSpotify;
using YoutubeExplode.Common;
using YoutubeExplode.Search;

namespace IntegracaoSpotify
{
    class YouTubeDownload : IMusicDownloader
    {
        private YoutubeClient youtubeClient;

        public YouTubeDownload()
        {
            youtubeClient = new YoutubeClient();
        }

        public async Task DownloadAsync(string input, string directory)
        {
            if (IsYouTubeLink(input))
            {
                if (IsYouTubePlaylistLink(input))
                {
                    var playlist = await youtubeClient.Playlists.GetAsync(input);
                    var videos = await youtubeClient.Playlists.GetVideosAsync(playlist.Id);

                    Console.WriteLine($"Baixando playlist do YouTube: {playlist.Title}");

                    foreach (var video in videos)
                    {
                        await DownloadVideoAsync(SanitizeFileName(video.Title), video.Id, directory);
                    }

                    Console.WriteLine($"Todas as músicas da playlist '{playlist.Title}' foram baixadas com sucesso.");
                }
                else
                {
                    var video = await youtubeClient.Videos.GetAsync(input);
                    await DownloadVideoAsync(SanitizeFileName(video.Title), video.Id, directory);
                }
            }
            else
            {
                await SearchAndDownload(input, directory);
            }
        }

        private async Task DownloadVideoAsync(string title, string videoId, string directory)
        {
            Console.WriteLine($"Baixando: {title}");

            var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoId);
            var audioStream = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            var filePath = Path.Combine(directory, $"{title}.mp3");

            await youtubeClient.Videos.Streams.DownloadAsync(audioStream, filePath);
            Console.WriteLine($"Música {title} baixada com sucesso no diretório {directory}");
        }

        private string SanitizeFileName(string fileName)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars());
            return Regex.Replace(fileName, $"[{Regex.Escape(invalidChars)}]", "_");
        }

        private bool IsYouTubeLink(string input)
        {
            return input.Contains("youtube.com") || input.Contains("youtu.be");
        }

        private bool IsYouTubePlaylistLink(string input)
        {
            return input.Contains("list=");
        }

        private async Task SearchAndDownload(string name, string directory)
        {
            Console.WriteLine($"Procurando música no YouTube: {name}");

            var videos = youtubeClient.Search.GetVideosAsync(name);
            VideoSearchResult? video = null;

            await foreach (var result in videos)
            {
                video = result;
                break;
            }

            if (video != null)
            {
                await DownloadVideoAsync(SanitizeFileName(video.Title), video.Id, directory);
            }
            else
            {
                Console.WriteLine("Nenhum vídeo encontrado no YouTube.");
            }
        }
    }
}
