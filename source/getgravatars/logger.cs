using System;

namespace getgravatars
{
    public class logger
    {
        readonly bool _verbose;

        public logger(bool verbose)
        {
            _verbose = verbose;
        }

        public void log(string message)
        {
            if (!_verbose)
            {
                return;
            }

            write(message);
        }

        public void write(string message)
        {
            Console.WriteLine(message);
        }
    }
}
