using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using coronga.crypto;

namespace coronga.server
{
    public class Main
    {
        public static void main(string [] args){
            Server s =  new Server();
            Console.WriteLine("entrou");
            s.main(args);
        }
    }
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
        }

        private void exchangeMsgs()
        {
            while (true)
            {
                var msg = "teste_send";
                var encryptedMsg = this.aes.Encrypt(Encoding.ASCII.GetBytes(msg));
                Console.WriteLine(encryptedMsg);
                this.sock.Send(encryptedMsg);
            }
        }
        public void exchangeRSAKeys()
        {
            int bytesReceived = this.sock.Receive(this.buffer);
            this.clientRSA = new RSA(Encoding.UTF8.GetString(this.buffer, 0, bytesReceived));
            Array.Clear(this.buffer, 0, this.buffer.Length);
            this.sock.Send(this.rsa.PubKey);
            Console.WriteLine("keys exchanged, secure connection created");
        }

        public void exchangeAESKey()
        {
            var key = RandomBytes.Generate(32);
            var IV = RandomBytes.Generate(16);
            this.aes = new AES(key, IV);
            this.sock.Send(key);
            this.sock.Send(IV);
            Console.WriteLine("AES key exchanged");
        }
    }
}