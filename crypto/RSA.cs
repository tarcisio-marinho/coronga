using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace coronga.crypto
{
    public class RSA
    {
        private RSAParameters _privKey;

        private RSAParameters _pubKey;

        public byte[] PubKey
        {
            get
            {
                var sw = new StringWriter();
                var xs = new XmlSerializer(typeof(RSAParameters));
                xs.Serialize(sw, this._pubKey);
                return Encoding.ASCII.GetBytes(sw.ToString());
            }
        }

        public byte[] PrivKey
        {
            get
            {
                var sw = new StringWriter();
                var xs = new XmlSerializer(typeof(RSAParameters));
                xs.Serialize(sw, this._privKey);
                return Encoding.ASCII.GetBytes(sw.ToString());
            }
        }

        private RSACryptoServiceProvider serviceProvider;
        public RSA()
        {
            this.serviceProvider = new RSACryptoServiceProvider(2048);
            this._privKey = serviceProvider.ExportParameters(true);
            this._pubKey = serviceProvider.ExportParameters(false);
        }


        // when only public key provided by server
        public RSA(string publicKey)
        {
            this.serviceProvider = new RSACryptoServiceProvider(2048);

            var sr = new StringReader(publicKey);
            var xs = new XmlSerializer(typeof(RSAParameters));

            this.serviceProvider.ImportParameters((RSAParameters)xs.Deserialize(sr));
        }

        public void storeKeys(string path)
        {
            string pubKeyString;
            {
                var sw = new StringWriter();
                var xs = new XmlSerializer(typeof(RSAParameters));
                xs.Serialize(sw, this._pubKey);
                pubKeyString = sw.ToString();
                File.WriteAllText(path + "public.key", pubKeyString);
            }
            string privKeyString;
            {
                var sw = new StringWriter();
                var xs = new XmlSerializer(typeof(RSAParameters));
                xs.Serialize(sw, this._privKey);
                privKeyString = sw.ToString();
                File.WriteAllText(path + "private.key", privKeyString);
            }
        }

        public byte[] Encrypt(string plainTextContent)
        {
            return this.serviceProvider.Encrypt(Encoding.ASCII.GetBytes(plainTextContent), false);
        }

        public string Decrypt(byte[] encryptedContent)
        {
            var decryptedContent = this.serviceProvider.Decrypt(encryptedContent, false);
            return Encoding.UTF8.GetString(decryptedContent, 0, decryptedContent.Length);
        }
    }
}
