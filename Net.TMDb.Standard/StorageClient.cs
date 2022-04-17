using Newtonsoft.Json.Linq;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Gabriel.Cat.S.Extension;
#pragma warning disable 1591

namespace System.Net.TMDb
{
    public sealed class StorageClient : IImageStorage
    {
        public JObject Data { get; set; }

        public StorageClient(JObject result)
        {
            Data = result;
        }
        public JToken? First => Data?.First?.First;

        public Task<Uri> GetFullPath(string fileName,string? sizeParam=default)
        {
            string? baseUrl;
            string fullPathFile;
            if (Equals(sizeParam, default))
            {
                sizeParam = First?["backdrop_sizes"]?.Values<string>().Last();
            }
            baseUrl = First?["base_url"]?.Value<string>();
            fullPathFile =baseUrl+sizeParam+fileName;//no puedo hacerlo de otra manera...hay un bug en .Net ...
            return Task.FromResult(new Uri(fullPathFile));
           
        }

    }
}

#pragma warning restore 1591