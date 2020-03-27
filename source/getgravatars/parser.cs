using System;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace getgravatars
{
    public class parser
    {
        const string base_url = "https://www.gravatar.com/avatar";
        readonly string _output_to;
        readonly string _query_string;

        public parser(string output_to, run_options options)
        {
            _output_to = output_to;
            _query_string = query_string_from(options);
        }

        public gravatar_info ToAvatarInfo(string line)
        {
            var line_split = line.Split("|");

            var email = new MailAddress(line_split[0]);

            var filename = line_split.Length > 1
                ? $"{_output_to}/{line_split[1]}.jpg"
                : "gravatar.jpg";

            var gravatar_id = gravatar_id_of(email);

            var uri_builder = new UriBuilder(
                $"{base_url}/{gravatar_id}"
            )
            {
                Query = _query_string
            };

            return new gravatar_info
            {
                address = email,
                file_name = filename,
                link = uri_builder.Uri
            };
        }

        public  string gravatar_id_of(MailAddress email)
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

        string query_string_from(run_options options)
        {
            return $"s={options.size}&d={options.gravatar_type}";
        }
    }
}
