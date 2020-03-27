using CommandLine;

namespace getgravatars
{
    public class run_options
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
            HelpText = "Directory to output gravatar images to (default is \"output\")")
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
}
