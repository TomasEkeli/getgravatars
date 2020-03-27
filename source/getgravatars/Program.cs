using System.Linq;
using System.Threading.Tasks;
using CommandLine;

namespace getgravatars
{
    static class program
    {
        static logger _logger;

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

        static async Task Main(string[] args)
        {
            _logger = new logger(false);

            await Parser
                .Default
                .ParseArguments<run_options>(args)
                .MapResult(
                    async (run_options o) =>
                    {
                        await run_with_options(o)
                            .ConfigureAwait(false);
                    }
                    ,
                    _ =>
                    {
                        _logger.write(usage_text);
                        return Task.FromResult(1);
                    }
                )
                .ConfigureAwait(false);
        }

        static async Task run_with_options(run_options options)
        {
            // no IoC container in a console app - have to make things myself
            _logger = new logger(options.verbose);
            var parser = new parser(options.output_directory, options);
            var file_handler = new file_handler(_logger);
            using var downloader = new downloader(file_handler);

            file_handler.ensure_that_directory_exists(options.output_directory);

            _logger.log($"Reading {options.input_file}");

            var all_lines_input = await file_handler
                .read(options.input_file)
                .ConfigureAwait(true);

            _logger.log($"input is {all_lines_input.Length} lines long");

            var avatar_infos = all_lines_input
                .Select(_ => parser.ToAvatarInfo(_));

            _logger.log($"made {avatar_infos.Count()} avatar infos");

            var distinct = avatar_infos
                .GroupBy(_ => _.file_name)
                .Select(_ => _.First());

            _logger.log($"discarded duplicats avatars - {distinct.Count()} left");

            _logger.write($@"
Found {distinct.Count()} avatars to download"
            );

            _logger.log("creating client");

            foreach (var info in distinct)
            {
                _logger.log($"{info.link} -> \t{info.file_name}");

                await downloader
                    .download_and_save_image(info)
                    .ConfigureAwait(false);
            }

            _logger.write("");
            _logger.write(
                $"Done. {distinct.Count()} gravatar images have been placed in"
            );
            _logger.write($"{options.output_directory}");
        }
    }
}
