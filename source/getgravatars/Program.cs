using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace app
{
    class Program
    {
        const string base_url = "https://www.gravatar.com/avatar";

        static string output_directory = "output";

        static async Task Main(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                System.Console.WriteLine();
                System.Console.WriteLine("Usage: getgravatars inputfile outputdirectory");
                System.Console.WriteLine("Duplicates will only be downloaded once");
                System.Console.WriteLine();
                System.Console.WriteLine("The inputfile should contain emails and full names, one on each line");
                System.Console.WriteLine("Like this:");
                System.Console.WriteLine();
                System.Console.WriteLine("author.name@company.com|Author Name");
                System.Console.WriteLine("author2.othername@company.com|Author2 Othername");
                System.Console.WriteLine();
                return;
            }

            var input_file = args[0];

            output_directory = args.Length > 1
                ? args[1]
                : output_directory;

            var dirInfo = new DirectoryInfo(output_directory);
            if (!dirInfo.Exists)
            {
                Directory.CreateDirectory(output_directory);
            }

            Console.WriteLine($"Reading {input_file}");

            var all_lines_input = await File
                .ReadAllLinesAsync(input_file)
                .ConfigureAwait(false);

            var distinct = all_lines_input
                .Distinct()
                .Select(ToAvatarInfo);

            Console.WriteLine($"Found {distinct.Count()} distinct avatars to download");

            using var client = new HttpClient();

            foreach (var info in distinct)
            {
                Console.WriteLine($"Downloading {info.FileName}");

                await DownloadAndSaveImage(info, client).ConfigureAwait(false);
            }

            Console.WriteLine("Done");
        }

        static async Task DownloadAndSaveImage(AvatarInfo info, HttpClient client)
        {
            var image_bytes = await client
                .GetByteArrayAsync(info.Link)
                .ConfigureAwait(false);

            await File
                .WriteAllBytesAsync(
                    info.FileName,
                    image_bytes
                )
                .ConfigureAwait(false);
        }

        static AvatarInfo ToAvatarInfo(string input)
        {
            var args_split = input.Split("|");

            var email = new MailAddress(args_split[0]);
            var filename = args_split.Length > 1
                ? $"{output_directory}/{args_split[1]}.jpg"
                : "gravatar.jpg";

            var hash = ToHash(email);

            var link = new Uri($"{base_url}/{hash}?s=96&d=identicon");

            return new AvatarInfo
            {
                Address = email,
                FileName = filename,
                Link = link
            };
        }

        static string ToHash(MailAddress email)
        {
            using var hashing_algorithm = MD5.Create();
            var hashed_bytes = hashing_algorithm
                .ComputeHash(
                    Encoding.UTF8.GetBytes(email.Address)
                );

            var as_strings = hashed_bytes
                .Select(_ => _.ToString("X2").ToLowerInvariant());

            return string.Join(null, as_strings);
        }

        public struct AvatarInfo
        {
            public MailAddress Address;
            public string FileName;
            public Uri Link;
        }
    }
}
