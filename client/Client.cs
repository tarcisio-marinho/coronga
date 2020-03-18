using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using coronga.crypto;

namespace coronga
{
    public class Main
    {
        public static void main(string[] args){
            Client c = new Client();
            c.main(args);
        }
    }
    public class Client
    {
        public readonly string address = "127.0.0.1";
        public IPEndPoint remoteEP;
        private Socket sock;
        private RSA rsa;
        private RSA serverRSA;
        private AES aes;
        private byte[] buffer = new byte[4096];
        public void main(string[] args)
        {
            this.rsa = new RSA();
            StartClient();
        }

        public void CreateSocket()
        {
            IPHostEntry host = Dns.GetHostEntry(this.address);
            IPAddress ipAddress = host.AddressList[0];
            this.remoteEP = new IPEndPoint(ipAddress, 11000);
            this.sock = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void destroySocket()
        {
            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
        }

        public void exchangeRSAKeys()
        {
            this.sock.Send(this.rsa.PubKey);
            int bytesReceived = this.sock.Receive(this.buffer);
            this.serverRSA = new RSA(Encoding.UTF8.GetString(this.buffer, 0, bytesReceived));
            Array.Clear(this.buffer, 0, this.buffer.Length);
            Console.WriteLine("keys exchanged, secure connection created");
        }

        public void exchangeAESKey()
        {
            int bytesReceived = this.sock.Receive(this.buffer);
            byte[] key = this.buffer;
            Array.Clear(this.buffer, 0, this.buffer.Length);
            int br = this.sock.Receive(this.buffer);
            byte[] IV = this.buffer;
            Array.Clear(this.buffer, 0, this.buffer.Length);
            this.aes = new AES(key, IV);
            Console.WriteLine("AES key exchanged");
        }

        public void StartClient()
        {
            // while (true)
            {
                try
                {
                    this.CreateSocket();
                }
                catch (Exception e)
                {
                    this.destroySocket();
                    Console.WriteLine("Error: ");
                    Console.WriteLine(e.ToString());
                    // continue;
                }
                try
                {
                    this.sock.Connect(this.remoteEP);
                    Console.WriteLine("Socket connected to {0}", this.sock.RemoteEndPoint.ToString());
                    this.exchangeRSAKeys();
                    this.exchangeAESKey();
                    this.exchangeMsgs();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
        }
        private void exchangeMsgs()
        {
            while (true)
            {
                int count = this.sock.Receive(this.buffer);
                Console.WriteLine("encrypted: ");
                Console.WriteLine(Encoding.UTF8.GetString(this.buffer, 0, this.buffer.Length));
                var plainMsg = this.aes.Decrypt(this.buffer);
                Array.Clear(this.buffer, 0, this.buffer.Length);
                Console.WriteLine("Received: ");
                Console.WriteLine(Encoding.UTF8.GetString(plainMsg, 0, plainMsg.Length));
            }
        }
    }
}