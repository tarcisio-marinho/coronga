using System;
using coronga.crypto;

namespace coronga
{
    class Entry
    {
        static void Main(string[] args)
        {
            RSA rsa = new RSA("pub.key","priv.key");
            rsa.CreateKeys();
            rsa.EncryptFile("TODO");
            rsa.DecryptFile("TODO");
        }
    }
}
