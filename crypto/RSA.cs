using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace coronga.crypto
{
    public class RSA
    {
        readonly string pubKeyPath;
        readonly string priKeyPath;

        public RSA(string publicPath, string privPath){
            this.pubKeyPath = publicPath;
            this.priKeyPath = privPath;
        }
        
        public void CreateKeys()
        {
            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(2048);
            RSAParameters privKey = csp.ExportParameters(true);
            RSAParameters pubKey = csp.ExportParameters(false);
            string pubKeyString;
            {
                //we need some buffer
                var sw = new StringWriter();
                //we need a serializer
                var xs = new XmlSerializer(typeof(RSAParameters));
                //serialize the key into the stream
                xs.Serialize(sw, pubKey);
                //get the string from the stream
                pubKeyString = sw.ToString();
                File.WriteAllText(pubKeyPath, pubKeyString);
            }
            string privKeyString;
            {
                //we need some buffer
                var sw = new StringWriter();
                //we need a serializer
                var xs = new XmlSerializer(typeof(RSAParameters));
                //serialize the key into the stream
                xs.Serialize(sw, privKey);
                //get the string from the stream
                privKeyString = sw.ToString();
                File.WriteAllText(priKeyPath, privKeyString);
            }
        }
        public void EncryptFile(string filePath)
        {
            string pubKeyString;
            {
                using (StreamReader reader = new StreamReader(pubKeyPath))
                {
                    pubKeyString = reader.ReadToEnd();
                }
            }
            var sr = new StringReader(pubKeyString);
            var xs = new XmlSerializer(typeof(RSAParameters));
            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
            csp.ImportParameters((RSAParameters)xs.Deserialize(sr));
            byte[] bytesPlainTextData = File.ReadAllBytes(filePath);
            var bytesCipherText = csp.Encrypt(bytesPlainTextData, false);
            string encryptedText = Convert.ToBase64String(bytesCipherText);
            File.WriteAllText(filePath, encryptedText);
        }
        public void DecryptFile(string filePath)
        {
            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
            string privKeyString;
            {
                privKeyString = File.ReadAllText(priKeyPath);
                var sr = new StringReader(privKeyString);
                var xs = new XmlSerializer(typeof(RSAParameters));
                RSAParameters privKey = (RSAParameters)xs.Deserialize(sr);
                csp.ImportParameters(privKey);
            }
            string encryptedText;
            using (StreamReader reader = new StreamReader(filePath))
            {
                encryptedText = reader.ReadToEnd();
            }
            byte[] bytesCipherText = Convert.FromBase64String(encryptedText);
            byte[] bytesPlainTextData = csp.Decrypt(bytesCipherText, false);
            File.WriteAllBytes(filePath, bytesPlainTextData);
        }
    }
}
