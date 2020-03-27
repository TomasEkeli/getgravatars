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
    static class Program
    {
        const string base_url = "https://www.gravatar.com/avatar";
        const string query_string = "s=256&d=identicon";

        static string output_to = "output";

        static async Task Main(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Console.WriteLine();
                Console.WriteLine(
@"Usage: getgravatars {inputfile} {outputdirectory (optional)}

This program downloads the gravatar images of e-mail addresses given in the inputfile
and stores them as the ""full-name.jpg"" in the output direcotory given (defaults to ""output"")

The inputfile should contain emails and full names, one on each line

Example:
author.name@company.com|Author Name
author2.othername@company.com|Author2 Othername
");
                return;
            }

            var input_file = args[0];

            output_to = args.Length > 1
                ? args[1]
                : output_to;

            var output_directory = new DirectoryInfo(output_to);
            if (!output_directory.Exists)
            {
                Console.WriteLine($@"
Creating output directory {output_directory}"
                );
                Directory.CreateDirectory(output_to);
            }

            Console.WriteLine($@"
Reading {input_file}"
            );

            var all_lines_input = await File
                .ReadAllLinesAsync(input_file)
                .ConfigureAwait(false);

            var distinct = all_lines_input
                .Select(ToAvatarInfo)
                .GroupBy(_ => _.file_name)
                .Select(_ => _.First());

            Console.WriteLine($@"
Found {distinct.Count()} avatars to download

Downloading:"
            );

            using var client = new HttpClient();

            foreach (var info in distinct)
            {
                Console.WriteLine($"{info.link} -> \t{info.file_name}");

                await download_and_save_image(info, client)
                    .ConfigureAwait(false);
            }

            Console.WriteLine($@"
Done. {distinct.Count()} gravatar images have been placed in
{output_directory.FullName}
"
            );
        }

        static async Task download_and_save_image(
            gravatar_info info,
            HttpClient client
        )
        {
            var image_bytes = await client
                .GetByteArrayAsync(info.link)
                .ConfigureAwait(false);

            await File
                .WriteAllBytesAsync(
                    info.file_name,
                    image_bytes
                )
                .ConfigureAwait(false);
        }

        static gravatar_info ToAvatarInfo(string input)
        {
            var args_split = input.Split("|");

            var email = new MailAddress(args_split[0]);

            var filename = args_split.Length > 1
                ? $"{output_to}/{args_split[1]}.jpg"
                : "gravatar.jpg";

            var gravatar_id = gravatar_id_of(email);

            var link = new Uri($"{base_url}/{gravatar_id}?{query_string}");

            return new gravatar_info
            {
                address = email,
                file_name = filename,
                link = link
            };
        }

        static string gravatar_id_of(MailAddress email)
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

        public struct gravatar_info
        {
            public MailAddress address;
            public string file_name;
            public Uri link;
        }
    }
}
