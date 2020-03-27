using System.IO;
using System.Threading.Tasks;

namespace getgravatars
{
    public class file_handler
    {
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
                Directory.CreateDirectory(directory);
            }
        }
    }
}
