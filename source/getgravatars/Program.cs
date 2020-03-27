using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace app
{
    public class Options
    {
        [Option(
            'i',
            "input",
            Required = true,
            HelpText = "The path to the input file")
        ]
        public string input_file { get; set; }

        [Option(
            'o',
            "output",
            Required = false,
            HelpText = "Direcotry to output gravatar images to (default is \"outoput\")")
        ]
        public string output_directory { get; set; } = "output";

        [Option(
            's',
            "size",
            Required = false,
            HelpText = "Size of images to download in pixels. Allowed values are 1-2048. Default is 256"
        )]
        public int size { get; set; } = 256;

        [Option(
            't',
            "type",
            Required = false,
            HelpText = @"Type of gravatar to download.
Known values are mp, identicon, monsterid, wavatar, retro, robohash and blank. Default is identicon."
        )]
        public string gravatar_type { get; set; } = "identicon";

        [Option(
            'v',
            "verbose",
            Required = false,
            HelpText = "Enable to get verbose output. Default is false."
        )]
        public bool verbose { get; set; }
    }

    static class Program
    {
        const string base_url = "https://www.gravatar.com/avatar";
        static string query_string = "s=256&d=identicon";

        const string usage_text = @"
Usage: getgravatars -i {inputfile} -o {outputdirectory (optional)} -t {gravatarType (optional)} -s {size (optional)}
allowed gravatarTypes are mp, identicon, monsterid, wavatar, retro, robohash and blank
if none is provided the identicon is used.

This program downloads the gravatar images of e-mail addresses given in the inputfile
and stores them as the ""full-name.jpg"" in the output direcotory given (defaults to ""output"")

The inputfile should contain emails and full names, one on each line

Example:
author.name@company.com|Author Name
author2.othername@company.com|Author2 Othername
";
        static bool verbose;
        static string output_to = "output";

        static async Task Main(string[] args)
        {
            await Parser
                .Default
                .ParseArguments<Options>(args)
                .MapResult(
                    async (Options o) =>
                    {
                        log($"running");

                        await run_with_options(o)
                            .ConfigureAwait(false);
                        log($"done");
                    }
                    ,
                    _ =>
                    {
                        write(usage_text);
                        return Task.FromResult(1);
                    }
                )
                .ConfigureAwait(false);
        }

        static void log(string message)
        {
            if (!verbose)
            {
                return;
            }

            write(message);
        }

        static void write(string message)
        {
            Console.WriteLine(message);
        }

        static async Task run_with_options(Options options)
        {
            verbose = options.verbose;
            output_to = options.output_directory;

            query_string = query_string_from(options);
            var output_directory = new DirectoryInfo(output_to);
            if (!output_directory.Exists)
            {
                log($@"
Creating output directory {output_directory}"
                );
                Directory.CreateDirectory(output_to);
            }

            log($@"
Reading {options.input_file}"
            );
            log("starting to read file");

            var all_lines_input = await File
                .ReadAllLinesAsync(options.input_file)
                .ConfigureAwait(true);

            log($"input is {all_lines_input.Length} lines long");

            try
            {
                var avatar_infos = all_lines_input
                    .Select(_ => ToAvatarInfo(_, options));

                log($"made {avatar_infos.Count()} avatar infos");

                var distinct = avatar_infos
                    .GroupBy(_ => _.file_name)
                    .Select(_ => _.First());

                log($"discarded duplicats avatars - {distinct.Count()} left");

                write($@"
Found {distinct.Count()} avatars to download"
                );

                log("creating client");
                using var client = new HttpClient();

                foreach (var info in distinct)
                {
                    log($"{info.link} -> \t{info.file_name}");

                    await download_and_save_image(info, client)
                        .ConfigureAwait(false);
                }

                write($@"
Done. {distinct.Count()} gravatar images have been placed in
{output_directory.FullName}
"
                );
             }
            catch (Exception ex)
            {
                log($"Exception: {ex.Message}");
                throw;
            }
        }

        static gravatar_info ToAvatarInfo(string line, Options options)
        {
            var line_split = line.Split("|");

            var email = new MailAddress(line_split[0]);

            var filename = line_split.Length > 1
                ? $"{output_to}/{line_split[1]}.jpg"
                : "gravatar.jpg";

            var gravatar_id = gravatar_id_of(email);

            var uri_builder = new UriBuilder(
                $"{base_url}/{gravatar_id}"
            )
            {
                Query = query_string_from(options)
            };

            return new gravatar_info
            {
                address = email,
                file_name = filename,
                link = uri_builder.Uri
            };
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

        static string query_string_from(Options options)
        {
            return $"s={options.size}&d={options.gravatar_type}";
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
