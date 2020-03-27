using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace getgravatars
{
    public class downloader : IDisposable
    {
        bool is_disposed;
        readonly HttpClient _client;
        readonly file_handler _file_handler;

        public downloader(
            file_handler file_handler,
            HttpClient client = null
        )
        {
            _client = client ?? new HttpClient();
            _file_handler = file_handler;
        }

        public async Task download_and_save_image(
            gravatar_info info
        )
        {
            if (is_disposed)
            {
                throw new ObjectDisposedException(nameof(downloader));
            }

            var image_bytes = await _client
                .GetByteArrayAsync(info.link)
                .ConfigureAwait(false);

            await _file_handler
                .write(info.file_name, image_bytes)
                .ConfigureAwait(false);
        }

        public void Dispose()
        {
            is_disposed = true;
            _client.Dispose();
        }
    }
}
