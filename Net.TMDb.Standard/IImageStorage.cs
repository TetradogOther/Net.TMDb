using Gabriel.Cat.S.Extension;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace System.Net.TMDb
{
    public interface IImageStorage
    {
        Task<Uri> GetFullPath(string pathRelFile, string? fileSizeParam = default);
        public Uri GetFullPathSync(string fileName, string? sizeParam = default)
        {
            Task<Uri> result = GetFullPath(fileName, sizeParam);
            result.Wait();
            return result.Result;
        }
        public async Task<Bitmap> GetImage(string fileName, string? sizeParam = default, string? saveFolder = default)
        {
            Uri fullPathFile;
            Bitmap result;
            if (Equals(saveFolder, default) || !File.Exists(saveFolder+fileName))
            {
                fullPathFile = await GetFullPath(fileName, sizeParam);
                result = fullPathFile.DownloadBitmap();
                if (!Equals(saveFolder, default))
                {
                    result.Save(saveFolder+ fileName,Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
            else
            {
                result = new Bitmap(saveFolder+ fileName);
            }
            return result;
        
        }
        public Bitmap GetImageSync(string fileName, string? sizeParam = default,string? saveFolder=default)
        {
            Task<Bitmap> result = GetImage(fileName,sizeParam,saveFolder);
            result.Wait();
            return result.Result;
        }
    }
}

#pragma warning restore 1591