using System.IO;
using System.Threading.Tasks;

namespace getgravatars
{
    public class file_handler
    {
        readonly logger _logger;

        public file_handler(logger logger)
        {
            _logger = logger;
        }

        public async Task<string[]> read(string file_name)
        {
            return await File
                .ReadAllLinesAsync(file_name)
                .ConfigureAwait(true);
        }

        public async Task write(string file_name, byte[] bytes)
        {
            await File
                .WriteAllBytesAsync(
                    file_name,
                    bytes
                )
                .ConfigureAwait(false);
        }

        public void ensure_that_directory_exists(string directory)
        {
            var output_directory = new DirectoryInfo(directory);
            if (!output_directory.Exists)
            {
                _logger.log($@"
Creating output directory {output_directory}"
                );
                Directory.CreateDirectory(directory);
            }
        }
    }
}
