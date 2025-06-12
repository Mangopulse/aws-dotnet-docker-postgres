using System.IO;
using System.Threading.Tasks;

namespace Shared.Storage
{
    public interface IStorageProvider
    {
        Task<UploadResult> UploadAsync(string container, string fileName, Stream content);
        Task<Stream> DownloadAsync(string container, string fileName);
        Task DeleteAsync(string container, string fileName);
        Task<string> GetUrlAsync(string container, string fileName);
    }

    public class UploadResult
    {
        public string FileName { get; set; }
        public string Url { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
    }
} 