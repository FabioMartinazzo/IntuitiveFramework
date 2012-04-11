using System;
using System.IO;
using System.Security.Cryptography;

namespace IntuitiveFramework.Models
{
    public class AESEncriptor
    {
        private string _plainText;
        private byte[] _key;
        private byte[] _initializatorVector;
        private byte[] _cipherText;

        public string PlainText { set { _plainText = value; } }
        public byte[] Key { set { _key = value; } }
        public byte[] InitializatorVector { set { _initializatorVector = value; } }
        public byte[] CipherText { set { _cipherText = value; } }

        public bool encriptarAES(out byte[] encriptedData)
        {
            try
            {
                RijndaelManaged aesAlg = new RijndaelManaged();
                aesAlg.Key = _key;
                aesAlg.IV = _initializatorVector;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                MemoryStream msEncrypt = new MemoryStream();
                CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write | CryptoStreamMode.Read);
                StreamWriter swEncrypt = new StreamWriter(csEncrypt);
                swEncrypt.Write(_plainText);
                
                swEncrypt.Close();
                csEncrypt.Close();
                msEncrypt.Close();

                aesAlg.Clear();
                
                encriptedData = msEncrypt.ToArray();

                return true;
            }
            catch
            {
                encriptedData = null;
                return false;
            }
        }

        public bool decriptarAES(out string decriptedData)
        {
            try
            {
                RijndaelManaged aesAlg = new RijndaelManaged();
                aesAlg.Key = _key;
                aesAlg.IV = _initializatorVector;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                MemoryStream msDecrypt = new MemoryStream(_cipherText);
                CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                StreamReader srDecrypt = new StreamReader(csDecrypt);

                decriptedData = srDecrypt.ReadToEnd();

                srDecrypt.Close();
                csDecrypt.Close();
                msDecrypt.Close();

                aesAlg.Clear();

                return true;
            }
            catch
            {
                decriptedData = null;
                return false;
            }
        }
    }
}
