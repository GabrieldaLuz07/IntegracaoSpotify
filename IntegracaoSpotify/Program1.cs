using System;
using System.Threading.Tasks;

namespace IntegracaoSpotify
{
    class Program1
    {
        static async Task Main(string[] args)
        {
            string spotifyClientId = "d68a06ff14914e8996fe9c2d978724c4";
            string spotifyClientSecret = "c4a309de56624139ba2eeda4cea19251";

            string continuar = "s";

            while (continuar.ToLower() == "s")
            {
                Console.WriteLine("Digite o nome da música, playlist ou o link:");
                string input = Console.ReadLine();

                Console.WriteLine("Digite o diretório onde a música será salva:");
                string directory = Console.ReadLine();

                Console.WriteLine("Digite a plataforma (youtube/spotify):");
                string platform = Console.ReadLine();

                try
                {
                    var musicDownload = new MusicDownload(platform, spotifyClientId, spotifyClientSecret);
                    await musicDownload.DownloadMusic(input, directory);
                    Console.WriteLine("Download finalizado!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro: {ex.Message}");
                }

                Console.WriteLine("Deseja baixar outra música ou playlist? (s/n)");
                continuar = Console.ReadLine();
            }

            Console.WriteLine("Encerrando o programa.");
        }
    }
}
