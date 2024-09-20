using IntegracaoSpotify;
using System;
using System.Threading.Tasks;

namespace IntegracaoSpotify
{
    class MusicDownload
    {
        private IMusicDownloader downloader;

        public MusicDownload(string platform, string clientId = "", string clientSecret = "")
        {
            switch (platform.ToLower())
            {
                case "youtube":
                    downloader = new YouTubeDownload();
                    break;
                case "spotify":
                    downloader = new SpotifyDownload(clientId, clientSecret);
                    break;
                default:
                    throw new ArgumentException("Plataforma não suportada.");
            }
        }

        public async Task DownloadMusic(string name, string directory)
        {
            await downloader.DownloadAsync(name, directory);
        }
    }
}
