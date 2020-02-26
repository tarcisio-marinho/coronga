using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using coronga.crypto;

namespace coronga.server
{
    public class Server
    {
        public readonly string address = "127.0.0.1";
        public IPEndPoint remoteEP;
        private Socket mainSock;
        private Socket sock;
        private RSA rsa;
        private RSA clientRSA;
        private AES aes;
        private byte[] buffer = new byte[4096];
        public void createServer()
        {
            IPHostEntry host = Dns.GetHostEntry(this.address);
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
            this.mainSock = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            mainSock.Bind(localEndPoint);
            mainSock.Listen(10);
            Console.WriteLine("Waiting for a connection...");
            this.sock = this.mainSock.Accept();
        }
        public void main(string[] args)
        {
            this.rsa = new RSA();
            this.createServer();
            this.exchangeRSAKeys();
            this.exchangeAESKey();
            this.exchangeMsgs();
            try
            {
                byte[] bytes = null;

                int bytesRec = this.sock.Receive(bytes);

                sock.Send(new byte[]{255, 255});
                sock.Shutdown(SocketShutdown.Both);
                sock.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        private void exchangeMsgs()
        {
            // while (true)
            {
                var msg = "teste_send";
                var encryptedMsg = this.aes.Encrypt(Encoding.ASCII.GetBytes(msg));
                this.sock.Send(encryptedMsg);
            }
        }
        public void exchangeRSAKeys()
        {
            int bytesReceived = this.sock.Receive(buffer);
            this.clientRSA = new RSA(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
            this.sock.Send(this.rsa.PubKey);
            Console.WriteLine("keys exchanged, secure connection created");
        }

        public void exchangeAESKey()
        {
            var key = RandomBytes.Generate(32);
            var IV = RandomBytes.Generate(32);
            this.aes = new AES(key, IV);
            this.mainSock.Send(key);
            this.mainSock.Send(IV);
            Console.WriteLine("AES key exchanged");
        }
    }
}