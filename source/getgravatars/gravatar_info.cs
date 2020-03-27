using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace getgravatars
{
    public struct gravatar_info
    {
        public MailAddress address;
        public string file_name;
        public Uri link;
    }
}
