using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegracaoSpotify
{
    interface IMusicDownloader
    {
        Task DownloadAsync(string name, string directory);
    }
}
