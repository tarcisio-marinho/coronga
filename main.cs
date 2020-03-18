using System;
using coronga.crypto;
using coronga.server;

namespace coronga
{
    class Entry
    {
        static void Main(string[] args)
        {
            // Client c = new Client();
            // c.main(args);

            Server s = new Server();
            s.main(args);
        }
    }
}
